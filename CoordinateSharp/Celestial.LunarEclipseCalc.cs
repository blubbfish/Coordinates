using System;
using System.Collections.Generic;

namespace CoordinateSharp {
  //CURRENT ALTITUDE IS SET CONSTANT AT 100M. POSSIBLY NEED TO ADJUST TO ALLOW USER PASS.
  //Altitude adjustments appear to have minimal effect on eclipse timing. These were mainly used
  //to signify eclipses that had already started during rise and set times on the NASA calculator

  //SOME TIMES AND ALTS WERE RETURNED WITH COLOR AND STYLING. DETERMINE WHY AND ADJUST VALUE AS REQUIRED. SEARCH "WAS ITALIC".

  //ELLIPSOID ADJUSTMENT
  //6378140.0 Ellipsoid is used in the NASA Calculator
  //WGS84 Ellipsoid is 6378137.0. Adjustments to the ellipsoid appear to effect eclipse seconds in fractions.
  //This can be modified if need to allow users to pass custom number with the Coordinate SetDatum() functions.

  //CURRENT RANGE 1601-2600.
  internal class LunarEclipseCalc {
    public static List<List<String>> CalculateLunarEclipse(DateTime d, Double latRad, Double longRad) => Calculate(d, latRad, longRad);
    public static List<LunarEclipseDetails> CalculateLunarEclipse(DateTime d, Double latRad, Double longRad, Double[] events) {
      List<List<String>> evs = Calculate(d, latRad, longRad, events);
      List<LunarEclipseDetails> deetsList = new List<LunarEclipseDetails>();
      foreach (List<String> ls in evs) {
        LunarEclipseDetails deets = new LunarEclipseDetails(ls);
        deetsList.Add(deets);
      }
      return deetsList;
    }
    public static List<List<String>> CalculateLunarEclipse(DateTime d, Coordinate coord) => Calculate(d, coord.Latitude.ToRadians(), coord.Longitude.ToRadians());


    // CALCULATE!
    private static List<List<String>> Calculate(DateTime d, Double latRad, Double longRad, Double[] ev = null) {
      //DECLARE ARRAYS
      Double[] obsvconst = new Double[6];
      Double[] mid = new Double[41];
      Double[] p1 = new Double[41];
      Double[] u1 = new Double[41];
      Double[] u2 = new Double[41];
      Double[] u3 = new Double[41];
      Double[] u4 = new Double[41];
      Double[] p4 = new Double[41];

      Double[] el = ev ?? Eclipse.LunarData.LunarDateData(d);
      List<List<String>> events = new List<List<String>>();
      ReadData(latRad, longRad, obsvconst);

      for (Int32 i = 0; i < el.Length; i += 22) {
        if (el[5 + i] <= obsvconst[5]) {
          List<String> values = new List<String>();
          obsvconst[4] = i;
          GetAll(el, obsvconst, mid, p1, u1, u2, u3, u4, p4);
          // Is there an event...
          if (mid[5] != 1) {

            values.Add(GetDate(el, p1, obsvconst));

            if (el[5 + i] == 1) {
              values.Add("T");
            } else if (el[5 + i] == 2) {
              values.Add("P");
            } else {
              values.Add("N");
            }

            // Pen. Mag
            values.Add(el[3 + i].ToString());

            // Umbral Mag
            values.Add(el[4 + i].ToString());

            // P1
            values.Add(GetTime(el, p1, obsvconst));

            // P1 alt
            values.Add(GetAlt(p1));

            if (u1[5] == 1) {
              values.Add("-");
              values.Add("-");
            } else {
              // U1
              values.Add(GetTime(el, u1, obsvconst));

              // U1 alt
              values.Add(GetAlt(u1));
            }
            if (u2[5] == 1) {
              values.Add("-");
              values.Add("-");
            } else {
              // U2
              values.Add(GetTime(el, u2, obsvconst));

              // U2 alt
              values.Add(GetAlt(u2));
            }
            // mid

            values.Add(GetTime(el, mid, obsvconst));

            // mid alt

            values.Add(GetAlt(mid));

            if (u3[5] == 1) {
              values.Add("-");
              values.Add("-");
            } else {
              // u3
              values.Add(GetTime(el, u3, obsvconst));

              // u3 alt
              values.Add(GetAlt(u3));
            }
            if (u4[5] == 1) {
              values.Add("-");
              values.Add("-");
            } else {
              // u4
              values.Add(GetTime(el, u4, obsvconst));

              // u4 alt
              values.Add(GetAlt(u4));

            }
            // P4
            values.Add(GetTime(el, p4, obsvconst));

            // P4 alt
            values.Add(GetAlt(p4));
            events.Add(values);
          }
        }
      }
      return events;
    }
    // Read the data that's in the form, and populate the obsvconst array
    private static void ReadData(Double latRad, Double longRad, Double[] obsvconst) {

      // Get the latitude
      obsvconst[0] = latRad;

      // Get the longitude
      obsvconst[1] = -1 * longRad; //PASS REVERSE RADIAN.

      // Get the altitude
      obsvconst[2] = 100; //CHANGE TO ALLOW USER TO PASS.

      // Get the time zone
      obsvconst[3] = 0; //GMT TIME

      obsvconst[4] = 0; //INDEX

      //SET MAX ECLIPSE TYPE
      obsvconst[5] = 4;//4 is ALL Eclipses

    }
    // Populate the p1, u1, u2, mid, u3, u4 and p4 arrays
    private static void GetAll(Double[] elements, Double[] obsvconst, Double[] mid, Double[] p1, Double[] u1, Double[] u2, Double[] u3, Double[] u4, Double[] p4) {
      Int32 index = (Int32)obsvconst[4];
      p1[1] = elements[index + 9];
      PopulateCircumstances(elements, p1, obsvconst);
      mid[1] = elements[index + 12];
      PopulateCircumstances(elements, mid, obsvconst);
      p4[1] = elements[index + 15];
      PopulateCircumstances(elements, p4, obsvconst);
      if (elements[index + 5] < 3) {
        u1[1] = elements[index + 10];
        PopulateCircumstances(elements, u1, obsvconst);
        u4[1] = elements[index + 14];
        PopulateCircumstances(elements, u4, obsvconst);
        if (elements[index + 5] < 2) {
          u2[1] = elements[index + 11];
          u3[1] = elements[index + 13];
          PopulateCircumstances(elements, u2, obsvconst);
          PopulateCircumstances(elements, u3, obsvconst);
        } else {
          u2[5] = 1;
          u3[5] = 1;
        }
      } else {
        u1[5] = 1;
        u2[5] = 1;
        u3[5] = 1;
        u4[5] = 1;
      }
      if (p1[5] != 0 && u1[5] != 0 && u2[5] != 0 && mid[5] != 0 && u3[5] != 0 && u4[5] != 0 && p4[5] != 0) {
        mid[5] = 1;
      }
    }
    // Populate the circumstances array
    // entry condition - circumstances[1] must contain the correct value
    private static void PopulateCircumstances(Double[] elements, Double[] circumstances, Double[] obsvconst) {
      Double t, ra, dec, h;

      Int32 index = (Int32)obsvconst[4];
      t = circumstances[1];
      ra = elements[18 + index] * t + elements[17 + index];
      ra = ra * t + elements[16 + index];
      dec = elements[21 + index] * t + elements[20 + index];
      dec = dec * t + elements[19 + index];
      dec = dec * Math.PI / 180.0;
      circumstances[3] = dec;
      h = 15.0 * (elements[6 + index] + (t - elements[2 + index] / 3600.0) * 1.00273791) - ra;
      h = h * Math.PI / 180.0 - obsvconst[1];
      circumstances[2] = h;
      circumstances[4] = Math.Asin(Math.Sin(obsvconst[0]) * Math.Sin(dec) + Math.Cos(obsvconst[0]) * Math.Cos(dec) * Math.Cos(h));
      circumstances[4] -= Math.Asin(Math.Sin(elements[7 + index] * Math.PI / 180.0) * Math.Cos(circumstances[4]));
      if (circumstances[4] * 180.0 / Math.PI < elements[8 + index] - 0.5667) {
        circumstances[5] = 2;
      } else if (circumstances[4] < 0.0) {
        circumstances[4] = 0.0;
        circumstances[5] = 0;
      } else {
        circumstances[5] = 0;
      }
    }
    // Get the date of an event
    private static String GetDate(Double[] elements, Double[] circumstances, Double[] obsvconst) {
      String[] month = new String[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };//Month string array
      Double t, jd, a, b, c, d, e;
      Int32 index = (Int32)obsvconst[4];
      // Calculate the JD for noon (TDT) the day before the day that contains T0
      jd = Math.Floor(elements[index] - elements[1 + index] / 24.0);
      // Calculate the local time (ie the offset in hours since midnight TDT on the day containing T0).
      t = circumstances[1] + elements[1 + index] - obsvconst[3] - (elements[2 + index] - 30.0) / 3600.0;

      if (t < 0.0) {
        jd--;
      }
      if (t >= 24.0) {
        jd++;
      }
      if (jd >= 2299160.0) {
        a = Math.Floor((jd - 1867216.25) / 36524.25);
        a = jd + 1 + a - Math.Floor(a / 4.0);
      } else {
        a = jd;
      }
      b = a + 1525.0;
      c = Math.Floor((b - 122.1) / 365.25);
      d = Math.Floor(365.25 * c);
      e = Math.Floor((b - d) / 30.6001);
      d = b - d - Math.Floor(30.6001 * e);
      e = e < 13.5 ? e - 1 : e - 13;
      Double year;
      String ans;
      if (e > 2.5) {
        ans = c - 4716 + "-";
        year = c - 4716;
      } else {
        ans = c - 4715 + "-";
        year = c - 4715;
      }
      String m = month[(Int32)e - 1];
      ans += m + "-";
      if (d < 10) {
        ans += "0";
      }
      ans += d;
      //Leap Year Integrity Check

      if (m == "Feb" && d == 29 && !DateTime.IsLeapYear((Int32)year)) {
        ans = year.ToString() + "-Mar-01";
      }
      return ans;
    }
    // Get the time of an event
    private static String GetTime(Double[] elements, Double[] circumstances, Double[] obsvconst) {
      Double t;
      String ans = "";

      Int32 index = (Int32)obsvconst[4];
      t = circumstances[1] + elements[1 + index] - obsvconst[3] - (elements[2 + index] - 30.0) / 3600.0;
      if (t < 0.0) {
        t += 24.0;
      }
      if (t >= 24.0) {
        t -= 24.0;
      }
      if (t < 10.0) {
        ans += "0";
      }
      ans = ans + Math.Floor(t) + ":";
      t = t * 60.0 - 60.0 * Math.Floor(t);
      if (t < 10.0) {
        ans += "0";
      }
      ans += Math.Floor(t);
      if (circumstances[5] == 2) {
        return ans; //RETURNED IN ITAL DETERMINE WHY            
      } else {
        return ans;
      }
    }
    // Get the altitude
    private static String GetAlt(Double[] circumstances) {
      Double t;
      t = circumstances[4] * 180.0 / Math.PI;
      t = Math.Floor(t + 0.5);
      String ans;
      if (t < 0.0) {
        ans = "-";
        t = -t;
      } else {
        ans = "+";
      }
      if (t < 10.0) {
        ans += "0";
      }
      ans += t;
      if (circumstances[5] == 2) {
        return ans; //returned in italics determine why

      } else {
        return ans;
      }
    }
  }

}
