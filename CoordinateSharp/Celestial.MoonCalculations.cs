using System;
using System.Collections.Generic;

namespace CoordinateSharp {
  internal class MoonCalc {
    static readonly Double rad = Math.PI / 180; //For converting radians

    //obliquity of the ecliptic in radians based on standard equinox 2000.
    static readonly Double e = rad * 23.4392911;
    /// <summary>
    /// Gets Moon Times, Altitude and Azimuth
    /// </summary>
    /// <param name="date">Date</param>
    /// <param name="lat">Latitude</param>
    /// <param name="lng">Longitude</param>
    /// <param name="c">Celestial</param>
    public static void GetMoonTimes(DateTime date, Double lat, Double lng, Celestial c) {
      //Get current Moon Position to populate passed Alt / Azi for user specified date
      MoonPosition mp = GetMoonPosition(date, lat, lng, c);
      Double altRad = mp.Altitude / Math.PI * 180; //Convert alt to degrees
      c.moonAltitude = altRad - mp.ParallaxCorection; //Set altitude with adjusted parallax                
      c.moonAzimuth = mp.Azimuth / Math.PI * 180 + 180;  //Azimuth in degrees + 180 for E by N.

      ////New Iterations for Moon set / rise
      Boolean moonRise = false;
      Boolean moonSet = false;

      //Start at beginning of day
      DateTime t = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);

      //Get start of day Moon Pos
      MoonPosition moonPos = GetMoonPosition(t, lat, lng, c);
      Double alt1 = moonPos.Altitude - moonPos.ParallaxCorection * rad;

      DateTime? setTime = null;
      DateTime? riseTime = null;
      Double hz = -.3 * rad;//Horizon degrees at -.3 for appearant rise / set

      //Iterate for each hour of the day
      for (Int32 x = 1; x <= 24; x++) {
        moonPos = GetMoonPosition(t.AddHours(x), lat, lng, c);//Get the next hours altitude for comparison
        Double alt2 = moonPos.Altitude - moonPos.ParallaxCorection * rad;
        //If hour 1 is below horizon and hour 2 is above
        if (alt1 < hz && alt2 >= hz) {
          //Moon Rise Occurred
          moonRise = true;
          DateTime dt1 = t.AddHours(x - 1);
          moonPos = GetMoonPosition(dt1, lat, lng, c);//Get the next hours altitude for comparison
          Double altM1 = moonPos.Altitude - moonPos.ParallaxCorection * rad;
          //Iterate through each minute to determine at which minute the horizon is crossed.
          //Interpolation is more efficient, but yielded results with deviations up to 5 minutes. 
          //Investigate formula efficiency 
          for (Int32 y = 1; y <= 60; y++) {
            DateTime dt2 = t.AddHours(x - 1).AddMinutes(y);
            moonPos = GetMoonPosition(dt2, lat, lng, c);//Get the next hours altitude for comparison
            Double altM2 = moonPos.Altitude - moonPos.ParallaxCorection * rad;
            if (altM1 < hz && altM2 >= hz) {
              //interpolate seconds
              Double p = 60 * ((hz - altM1) / (altM2 - altM1));
              riseTime = dt1.AddMinutes(y - 1).AddSeconds(p);
              break;
            }
            altM1 = altM2;

          }
        }
        //if hour 2 is above horizon and hour 1 below
        if (alt1 >= hz && alt2 < hz) {
          //Moon Set Occured
          moonSet = true;
          DateTime dt1 = t.AddHours(x - 1);
          moonPos = GetMoonPosition(dt1, lat, lng, c);//Get the next hours altitude for comparison
          Double altM1 = moonPos.Altitude - moonPos.ParallaxCorection * rad;
          //Iterate through each minute to determine at which minute the horizon is crossed.
          //Interpolation is more efficient, but yielded results with deviations up to 5 minutes. 
          //Investigate formula efficiency 
          for (Int32 y = 1; y <= 60; y++) {
            DateTime dt2 = t.AddHours(x - 1).AddMinutes(y);
            moonPos = GetMoonPosition(dt2, lat, lng, c);//Get the next hours altitude for comparison
            Double altM2 = moonPos.Altitude - moonPos.ParallaxCorection * rad;
            if (altM1 >= hz && altM2 < hz) {
              //Interpolate seconds 
              Double p = 60 * ((hz - altM2) / (altM1 - altM2));
              setTime = dt1.AddMinutes(y).AddSeconds(-p);
              break;
            }
            altM1 = altM2;

          }
        }
        alt1 = alt2;
        if (moonRise && moonSet) { break; }
      }

      c.moonSet = setTime;
      c.moonRise = riseTime;
      if (moonRise && moonSet) { c.moonCondition = CelestialStatus.RiseAndSet; } else {
        if (!moonRise && !moonSet) {
          c.moonCondition = alt1 >= 0 ? CelestialStatus.UpAllDay : CelestialStatus.DownAllDay;
        }
        if (!moonRise && moonSet) { c.moonCondition = CelestialStatus.NoRise; }
        if (moonRise && !moonSet) { c.moonCondition = CelestialStatus.NoSet; }
      }
    }

    private static MoonPosition GetMoonPosition(DateTime date, Double lat, Double lng, Celestial cel) {
      //Set UTC date integrity
      date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, DateTimeKind.Utc);

      Double d = JulianConversions.GetJulian_Epoch2000(date);

      //Ch 47
      Double JDE = JulianConversions.GetJulian(date);//Get julian 

      Double T = (JDE - 2451545) / 36525; //Get dynamic time.
      Double[] LDMNF = Get_Moon_LDMNF(T);
      CelCoords c = GetMoonCoords(d, cel, LDMNF, T);
      Distance dist = GetMoonDistance(date);
      Double lw = rad * -lng;
      Double phi = rad * lat;

      Double H = rad * MeeusFormulas.Get_Sidereal_Time(JDE) - lw - c.Ra;

      Double ra = c.Ra; //Adjust current RA formula to avoid needless RAD conversions
      Double dec = c.Dec; //Adjust current RA formula to avoid needless RAD conversions

      //Adjust for parallax (low accuracry increases may not be worth cost)
      //Investigate
      Double pSinE = Get_pSinE(dec, dist.Meters) * Math.PI / 180;
      Double pCosE = Get_pCosE(dec, dist.Meters) * Math.PI / 180;
      Double cRA = Parallax_RA(dist.Meters, H, pCosE, dec, ra);
      Double tDEC = Parallax_Dec(dist.Meters, H, pCosE, pSinE, dec, cRA);
      //Double tRA = ra - cRA;
      dec = tDEC;
      //ra = tRA;

      //Get true altitude
      Double h = Altitude(H, phi, dec);

      // formula 14.1 of "Astronomical Algorithms" 2nd edition by Jean Meeus (Willmann-Bell, Richmond) 1998.
      Double pa = Math.Atan2(Math.Sin(H), Math.Tan(phi) * Math.Cos(dec) - Math.Sin(dec) * Math.Cos(H));

      //altitude correction for refraction
      h += AstroRefraction(h);

      MoonPosition mp = new MoonPosition {
        Azimuth = Azimuth(H, phi, dec),
        Altitude = h / Math.PI * 180,
        Distance = dist,
        ParallacticAngle = pa
      };

      Double horParal = 8.794 / (dist.Meters / 149.59787E6); // horizontal parallax (arcseconds), Meeus S. 263  
      Double p = Math.Asin(Math.Cos(h) * Math.Sin(horParal / 3600)); // parallax in altitude (degrees)
      p *= 1000;

      mp.ParallaxCorection = p;
      mp.Altitude *= rad;

      return mp;
    }
    private static CelCoords GetMoonCoords(Double _1, Celestial _2, Double[] LDMNF, Double t) {
      // Legacy function. Updated with Meeus Calcs for increased accuracy.
      // geocentric ecliptic coordinates of the moon
      // Meeus Ch 47
      Double[] cs = Get_Moon_Coordinates(LDMNF, t);

      Double l = cs[0]; // longitude
      Double b = cs[1]; // latitude          

      CelCoords mc = new CelCoords {
        Ra = RightAscension(l, b),
        Dec = Declination(l, b)
      };
      //Double ra = mc.ra / Math.PI * 180;
      //Double dec = mc.dec / Math.PI * 180;

      return mc;
    }

    public static void GetMoonIllumination(DateTime date, Celestial c, Double lat, Double lng) {
      date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, DateTimeKind.Utc);

      Double d = JulianConversions.GetJulian_Epoch2000(date);
      CelCoords s = GetSunCoords(d);
      Double JDE = JulianConversions.GetJulian(date);//Get julian 
      Double T = (JDE - 2451545) / 36525; //Get dynamic time.
      Double[] LDMNF = Get_Moon_LDMNF(T);

      CelCoords m = GetMoonCoords(d, c, LDMNF, T);

      Double sdist = 149598000,
            phi = Math.Acos(Math.Sin(s.Dec) * Math.Sin(m.Dec) + Math.Cos(s.Dec) * Math.Cos(m.Dec) * Math.Cos(s.Ra - m.Ra)),
            inc = Math.Atan2(sdist * Math.Sin(phi), m.Dist - sdist * Math.Cos(phi)),
            angle = Math.Atan2(Math.Cos(s.Dec) * Math.Sin(s.Ra - m.Ra), Math.Sin(s.Dec) * Math.Cos(m.Dec) -
                    Math.Cos(s.Dec) * Math.Sin(m.Dec) * Math.Cos(s.Ra - m.Ra));


      MoonIllum mi = new MoonIllum {
        Fraction = (1 + Math.Cos(inc)) / 2,
        Phase = 0.5 + 0.5 * inc * (angle < 0 ? -1 : 1) / Math.PI,
        Angle = angle
      };


      c.moonIllum = mi;

      String moonName = "";
      Int32 moonDate = 0;
      //GET PHASE NAME

      //CHECK MOON AT BEGINNING AT END OF DAY TO GET DAY PHASE
      DateTime dMon = new DateTime(date.Year, date.Month, 1);
      for (Int32 x = 1; x <= date.Day; x++) {
        DateTime nDate = new DateTime(dMon.Year, dMon.Month, x, 0, 0, 0, DateTimeKind.Utc);
        d = JulianConversions.GetJulian_Epoch2000(nDate);
        s = GetSunCoords(d);
        JDE = JulianConversions.GetJulian(nDate);//Get julian 
        T = (JDE - 2451545) / 36525; //Get dynamic time.
        LDMNF = Get_Moon_LDMNF(T);
        m = GetMoonCoords(d, c, LDMNF, T);

        phi = Math.Acos(Math.Sin(s.Dec) * Math.Sin(m.Dec) + Math.Cos(s.Dec) * Math.Cos(m.Dec) * Math.Cos(s.Ra - m.Ra));
        inc = Math.Atan2(sdist * Math.Sin(phi), m.Dist - sdist * Math.Cos(phi));
        angle = Math.Atan2(Math.Cos(s.Dec) * Math.Sin(s.Ra - m.Ra), Math.Sin(s.Dec) * Math.Cos(m.Dec) -
                Math.Cos(s.Dec) * Math.Sin(m.Dec) * Math.Cos(s.Ra - m.Ra));

        Double startPhase = 0.5 + 0.5 * inc * (angle < 0 ? -1 : 1) / Math.PI;

        nDate = new DateTime(dMon.Year, dMon.Month, x, 23, 59, 59, DateTimeKind.Utc);
        d = JulianConversions.GetJulian_Epoch2000(nDate);
        s = GetSunCoords(d);
        JDE = JulianConversions.GetJulian(nDate);//Get julian 
        T = (JDE - 2451545) / 36525; //Get dynamic time.
        LDMNF = Get_Moon_LDMNF(T);
        m = GetMoonCoords(d, c, LDMNF, T);

        phi = Math.Acos(Math.Sin(s.Dec) * Math.Sin(m.Dec) + Math.Cos(s.Dec) * Math.Cos(m.Dec) * Math.Cos(s.Ra - m.Ra));
        inc = Math.Atan2(sdist * Math.Sin(phi), m.Dist - sdist * Math.Cos(phi));
        angle = Math.Atan2(Math.Cos(s.Dec) * Math.Sin(s.Ra - m.Ra), Math.Sin(s.Dec) * Math.Cos(m.Dec) -
                Math.Cos(s.Dec) * Math.Sin(m.Dec) * Math.Cos(s.Ra - m.Ra));

        Double endPhase = 0.5 + 0.5 * inc * (angle < 0 ? -1 : 1) / Math.PI;
        //Determine Moon Name.
        if (startPhase <= .5 && endPhase >= .5) {
          moonDate = x;
          moonName = GetMoonName(dMon.Month, moonName);
        }
        //Get Moon Name (month, string);
        //Get Moon Phase Name          
        if (date.Day == x) {
          if (startPhase > endPhase) {
            mi.PhaseName = "New Moon";
            break;
          }
          if (startPhase <= .25 && endPhase >= .25) {
            mi.PhaseName = "First Quarter";
            break;
          }
          if (startPhase <= .5 && endPhase >= .5) {
            mi.PhaseName = "Full Moon";
            break;
          }
          if (startPhase <= .75 && endPhase >= .75) {
            mi.PhaseName = "Last Quarter";
            break;
          }

          if (startPhase > 0 && startPhase < .25 && endPhase > 0 && endPhase < .25) {
            mi.PhaseName = "Waxing Crescent";
            break;
          }
          if (startPhase > .25 && startPhase < .5 && endPhase > .25 && endPhase < .5) {
            mi.PhaseName = "Waxing Gibbous";
            break;
          }
          if (startPhase > .5 && startPhase < .75 && endPhase > .5 && endPhase < .75) {
            mi.PhaseName = "Waning Gibbous";
            break;
          }
          if (startPhase > .75 && startPhase < 1 && endPhase > .75 && endPhase < 1) {
            mi.PhaseName = "Waning Crescent";
            break;
          }
        }

      }
      c.AstrologicalSigns.MoonName = date.Day == moonDate ? moonName : "";
      CalculateLunarEclipse(date, lat, lng, c);

    }
    public static void CalculateLunarEclipse(DateTime date, Double lat, Double longi, Celestial c) {
      //Convert to Radian
      Double latR = lat * Math.PI / 180;
      Double longR = longi * Math.PI / 180;
      List<List<String>> se = LunarEclipseCalc.CalculateLunarEclipse(date, latR, longR);
      //RETURN FIRST AND LAST
      if (se.Count == 0) { return; }
      //FIND LAST AND NEXT ECLIPSE
      Int32 lastE = -1;
      Int32 nextE = -1;
      Int32 currentE = 0;
      DateTime lastDate = new DateTime();
      DateTime nextDate = new DateTime(3300, 1, 1);
      //Iterate to get last and next eclipse

      foreach (List<String> values in se) {
        DateTime ld = DateTime.ParseExact(values[0], "yyyy-MMM-dd", System.Globalization.CultureInfo.InvariantCulture);
        if (ld < date && ld > lastDate) { lastDate = ld; lastE = currentE; }
        if (ld >= date && ld < nextDate) { nextDate = ld; nextE = currentE; }
        currentE++;
      }
      //SET ECLIPSE DATA
      if (lastE >= 0) {
        c.LunarEclipse.LastEclipse = new LunarEclipseDetails(se[lastE]);
      }
      if (nextE >= 0) {
        c.LunarEclipse.NextEclipse = new LunarEclipseDetails(se[nextE]);
      }
    }

    private static String GetMoonName(Int32 month, String name) => name != "" ? "Blue Moon" : (month switch
    {
      1 => "Wolf Moon",
      2 => "Snow Moon",
      3 => "Worm Moon",
      4 => "Pink Moon",
      5 => "Flower Moon",
      6 => "Strawberry Moon",
      7 => "Buck Moon",
      8 => "Sturgeon Moon",
      9 => "Corn Moon",
      10 => "Hunters Moon",
      11 => "Beaver Moon",
      12 => "Cold Moon",
      _ => "",
    });
    public static void GetMoonDistance(DateTime date, Celestial c) {
      date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, DateTimeKind.Utc);

      c.moonDistance = GetMoonDistance(date);      //Updating distance formula    
    }
    //Moon Time Functions
    private static CelCoords GetSunCoords(Double d) {
      Double M = SolarMeanAnomaly(d),
                L = EclipticLongitude(M);
      CelCoords c = new CelCoords {
        Dec = Declination(L, 0),
        Ra = RightAscension(L, 0)
      };
      return c;
    }
    private static Double SolarMeanAnomaly(Double d) => rad * (357.5291 + 0.98560028 * d);
    private static Double EclipticLongitude(Double M) {
      Double C = rad * (1.9148 * Math.Sin(M) + 0.02 * Math.Sin(2 * M) + 0.0003 * Math.Sin(3 * M)), // equation of center
                P = rad * 102.9372; // perihelion of the Earth

      return M + C + P + Math.PI;
    }

    public static void GetMoonSign(DateTime date, Celestial c) {
      //Formulas taken from https://www.astrocal.co.uk/moon-sign-calculator/
      Double d = date.Day;
      Double m = date.Month;
      Double y = date.Year;
      Double hr = date.Hour;
      Double mi = date.Minute;

      Double f = hr + mi / 60;
      Double im = 12 * (y + 4800) + m - 3;
      Double j = (2 * (im - Math.Floor(im / 12) * 12) + 7 + 365 * im) / 12;
      j = Math.Floor(j) + d + Math.Floor(im / 48) - 32083;
      Double jd = j + Math.Floor(im / 4800) - Math.Floor(im / 1200) + 38;
      Double T = (jd - 2415020 + f / 24 - .5) / 36525;
      //Double ob = FNr(23.452294 - .0130125 * T);
      Double ll = 973563 + 1732564379 * T - 4 * T * T;
      Double g = 1012395 + 6189 * T;
      Double n = 933060 - 6962911 * T + 7.5 * T * T;
      Double g1 = 1203586 + 14648523 * T - 37 * T * T;
      d = 1262655 + 1602961611 * T - 5 * T * T;
      Double M = 3600;
      Double l = (ll - g1) / M;
      Double l1 = (ll - d - g) / M;
      f = (ll - n) / M;
      d /= M;
      y = 2 * d;
      Double ml = 22639.6 * FNs(l) - 4586.4 * FNs(l - y);
      ml = ml + 2369.9 * FNs(y) + 769 * FNs(2 * l) - 669 * FNs(l1);
      ml = ml - 411.6 * FNs(2 * f) - 212 * FNs(2 * l - y);
      ml = ml - 206 * FNs(l + l1 - y) + 192 * FNs(l + y);
      ml = ml - 165 * FNs(l1 - y) + 148 * FNs(l - l1) - 125 * FNs(d);
      ml = ml - 110 * FNs(l + l1) - 55 * FNs(2 * f - y);
      ml = ml - 45 * FNs(l + 2 * f) + 40 * FNs(l - 2 * f);
      //Double tn = n + 5392 * FNs(2 * f - y) - 541 * FNs(l1) - 442 * FNs(y);
      //tn = tn + 423 * FNs(2 * f) - 291 * FNs(2 * l - 2 * f);
      g = FNu(FNp(ll + ml));
      Double sign = Math.Floor(g / 30);
      //Double degree = g - sign * 30;
      sign += 1;

      c.AstrologicalSigns.MoonSign = (sign.ToString()) switch
      {
        "1" => "Aries",
        "2" => "Taurus",
        "3" => "Gemini",
        "4" => "Cancer",
        "5" => "Leo",
        "6" => "Virgo",
        "7" => "Libra",
        "8" => "Scorpio",
        "9" => "Sagitarius",
        "10" => "Capricorn",
        "11" => "Aquarius",
        "12" => "Pisces",
        _ => "Pisces",
      };
    }

    private static Double FNp(Double x) {
      Double sgn = x < 0 ? -1 : 1;
      return sgn * (Math.Abs(x) / 3600 / 360 - Math.Floor(Math.Abs(x) / 3600.0 / 360.0)) * 360;
    }
    private static Double FNu(Double x) => x - Math.Floor(x / 360) * 360;
    //private static Double FNr(Double x) => Math.PI / 180 * x;
    private static Double FNs(Double x) => Math.Sin(Math.PI / 180 * x);

    //v1.1.3 Formulas
    //The following formulas are either additions 
    //or conversions of SunCalcs formulas into Meeus

    /// <summary>
    /// Grabs Perigee or Apogee of Moon based on specified time.
    /// Results will return event just before, or just after specified DateTime
    /// </summary>
    /// <param name="d">DateTime</param>
    /// <param name="md">Event Type</param>
    /// <returns>PerigeeApogee</returns>
    private static PerigeeApogee MoonPerigeeOrApogee(DateTime d, MoonDistanceType md) {
      //Perigee & Apogee Algorithms from Jean Meeus Astronomical Algorithms Ch. 50

      //50.1
      //JDE = 2451534.6698 + 27.55454989 * k 
      //                     -0.0006691 * Math.Pow(T,2)
      //                     -0.000.01098 * Math.Pow(T,3)
      //                     -0.0000000052 * Math.Pow(T,4)

      //50.2
      //K approx = (yv - 1999.97)*13.2555
      //yv is the year + percentage of days that have occured in the year. 1998 Oct 1 is approx 1998.75
      //k ending in .0 represent perigee and .5 apogee. Anything > .5 is an error.

      //50.3
      //T = k/1325.55

      Double yt = 365; //days in year
      if (DateTime.IsLeapYear(d.Year)) { yt = 366; } //days in year if leap year
      Double f = d.DayOfYear / yt; //Get percentage of year that as passed
      Double yv = d.Year + f; //add percentage of year passed to year.
      Double k = (yv - 1999.97) * 13.2555; //find approximate k using formula 50.2

      //Set k decimal based on apogee or perigee
      k = md == MoonDistanceType.Apogee ? Math.Floor(k) + .5 : Math.Floor(k);

      //Find T using formula 50.3
      Double T = k / 1325.55;
      //Find JDE using formula 50.1
      Double JDE = 2451534.6698 + 27.55454989 * k -
                0.0006691 * Math.Pow(T, 2) -
                0.00001098 * Math.Pow(T, 3) -
                0.0000000052 * Math.Pow(T, 4);

      //Find Moon's mean elongation at time JDE.
      Double D = 171.9179 + 335.9106046 * k -
                0.0100383 * Math.Pow(T, 2) -
                0.00001156 * Math.Pow(T, 3) +
                0.000000055 * Math.Pow(T, 4);

      //Find Sun's mean anomaly at time JDE
      Double M = 347.3477 + 27.1577721 * k -
                0.0008130 * Math.Pow(T, 2) -
                0.0000010 * Math.Pow(T, 3);


      //Find Moon's argument of latitude at Time JDE
      Double F = 316.6109 + 364.5287911 * k -
                0.0125053 * Math.Pow(T, 2) -
                0.0000148 * Math.Pow(T, 3);

      //Normalize DMF to a 0-360 degree number
      D %= 360;
      if (D < 0) { D += 360; }
      M %= 360;
      if (M < 0) { M += 360; }
      F %= 360;
      if (F < 0) { F += 360; }

      //Convert DMF to radians
      D = D * Math.PI / 180;
      M = M * Math.PI / 180;
      F = F * Math.PI / 180;
      Double termsA = md == MoonDistanceType.Apogee ? MeeusTables.ApogeeTermsA(D, M, F, T) : MeeusTables.PerigeeTermsA(D, M, F, T);
      //Find Terms A from Table 50.A 
      JDE += termsA;
      Double termsB = md == MoonDistanceType.Apogee ? MeeusTables.ApogeeTermsB(D, M, F, T) : MeeusTables.PerigeeTermsB(D, M, F, T);
      //Convert julian back to date
      DateTime date = JulianConversions.GetDate_FromJulian(JDE).Value;
      //Obtain distance
      Distance dist = GetMoonDistance(date);

      PerigeeApogee ap = new PerigeeApogee(date, termsB, dist);
      return ap;
    }

    public static Perigee GetPerigeeEvents(DateTime d) {
      //Iterate in 15 day increments due to formula variations.
      //Determine closest events to date.
      //per1 is last date
      //per2 is next date

      //integrity for new date.
      if (d.Year <= 0001) { return new Perigee(new PerigeeApogee(new DateTime(), 0, new Distance(0)), new PerigeeApogee(new DateTime(), 0, new Distance(0))); }
      //Start at lowest increment
      PerigeeApogee per1 = MoonPerigeeOrApogee(d.AddDays(-45), MoonDistanceType.Perigee);
      PerigeeApogee per2 = MoonPerigeeOrApogee(d.AddDays(-45), MoonDistanceType.Perigee);

      for (Int32 x = -30; x <= 45; x += 15) {
        //used for comparison 
        PerigeeApogee t = MoonPerigeeOrApogee(d.AddDays(x), MoonDistanceType.Perigee);

        //Find the next pergiee after specified date           
        if (t.Date > per2.Date && t.Date >= d) {
          per2 = t;
          break;
        }
        //Find last perigee before specified date
        if (t.Date > per1.Date && t.Date < d) {
          per1 = t;
          per2 = t;
        }

      }
      return new Perigee(per1, per2);
    }
    public static Apogee GetApogeeEvents(DateTime d) {
      //Iterate in 5 month increments due to formula variations.
      //Determine closest events to date.
      //apo1 is last date
      //apo2 is next date

      //integrity for new date.
      if (d.Year <= 0001) { return new Apogee(new PerigeeApogee(new DateTime(), 0, new Distance(0)), new PerigeeApogee(new DateTime(), 0, new Distance(0))); }

      PerigeeApogee apo1 = MoonPerigeeOrApogee(d.AddDays(-45), MoonDistanceType.Apogee);
      PerigeeApogee apo2 = MoonPerigeeOrApogee(d.AddDays(-45), MoonDistanceType.Apogee);
      for (Int32 x = -30; x <= 45; x += 15) {
        PerigeeApogee t = MoonPerigeeOrApogee(d.AddDays(x), MoonDistanceType.Apogee);
        //Find next apogee after specified date
        if (t.Date > apo2.Date && t.Date >= d) {
          apo2 = t;
          break;
        }
        //Find last apogee before specified date
        if (t.Date > apo1.Date && t.Date < d) {
          apo1 = t;
          apo2 = t;
        }

      }
      return new Apogee(apo1, apo2);

    }

    /// <summary>
    /// Gets moon distance (Ch 47).
    /// </summary>
    /// <param name="d">DateTime</param>
    /// <returns>Distance</returns>
    public static Distance GetMoonDistance(DateTime d) {
      //Ch 47
      Double JDE = JulianConversions.GetJulian(d);//Get julian 
      Double T = (JDE - 2451545) / 36525; //Get dynamic time.

      Double[] values = Get_Moon_LDMNF(T);

      Double D = values[1];
      Double M = values[2];
      Double N = values[3];
      Double F = values[4];

      //Ch 47 distance formula
      Double dist = 385000.56 + MeeusTables.Moon_Periodic_Er(D, M, N, F, T) / 1000;
      return new Distance(dist);
    }

    /*private static Distance GetMoonDistance(DateTime d, Double[] values) {
      //Ch 47
      Double JDE = JulianConversions.GetJulian(d);//Get julian 
      Double T = (JDE - 2451545) / 36525; //Get dynamic time.        

      Double D = values[1];
      Double M = values[2];
      Double N = values[3];
      Double F = values[4];

      Double dist = 385000.56 + MeeusTables.Moon_Periodic_Er(D, M, N, F, T) / 1000;
      return new Distance(dist);
    }*/

    /// <summary>
    /// Gets Moon L, D, M, N, F values
    /// Ch. 47 
    /// </summary>
    /// <param name="T">Dynamic Time</param>
    /// <returns>double[] containing L,D,M,N,F</returns>
    static Double[] Get_Moon_LDMNF(Double T) {
      //T = dynamic time

      //Moon's mean longitude
      Double L = 218.316447 + 481267.88123421 * T -
                 .0015786 * Math.Pow(T, 2) + Math.Pow(T, 3) / 538841 -
                 Math.Pow(T, 4) / 65194000;

      //Moon's mean elongation 
      Double D = 297.8501921 + 445267.1114034 * T -
                0.0018819 * Math.Pow(T, 2) + Math.Pow(T, 3) / 545868 - Math.Pow(T, 4) / 113065000;
      //Sun's mean anomaly
      Double M = 357.5291092 + 35999.0502909 * T -
                .0001536 * Math.Pow(T, 2) + Math.Pow(T, 3) / 24490000;
      //Moon's mean anomaly
      Double N = 134.9633964 + 477198.8675055 * T + .0087414 * Math.Pow(T, 2) +
                Math.Pow(T, 3) / 69699 - Math.Pow(T, 4) / 14712000;
      //Moon's argument of latitude
      Double F = 93.2720950 + 483202.0175233 * T - .0036539 * Math.Pow(T, 2) - Math.Pow(T, 3) /
                3526000 + Math.Pow(T, 4) / 863310000;

      //Normalize DMF to a 0-360 degree number      
      D %= 360;
      if (D < 0) { D += 360; }
      M %= 360;
      if (M < 0) { M += 360; }
      N %= 360;
      if (N < 0) { N += 360; }
      F %= 360;
      if (F < 0) { F += 360; }

      //Convert DMF to radians

      D = D * Math.PI / 180;
      M = M * Math.PI / 180;
      N = N * Math.PI / 180;
      F = F * Math.PI / 180;

      return new Double[] { L, D, M, N, F };
    }
    /// <summary>
    /// Get moons lat/long in radians (Ch 47).
    /// </summary>
    /// <param name="LDMNF">L,D,M,N,F</param>
    /// <param name="T">Dynamic Time</param>
    /// <returns>Lat[0], Long[1]</returns>
    private static Double[] Get_Moon_Coordinates(Double[] LDMNF, Double T) {
      //Refence Ch 47.
      Double lat = LDMNF[0] + MeeusTables.Moon_Periodic_El(LDMNF[0], LDMNF[1], LDMNF[2], LDMNF[3], LDMNF[4], T) / 1000000;
      Double longi = MeeusTables.Moon_Periodic_Eb(LDMNF[0], LDMNF[1], LDMNF[2], LDMNF[3], LDMNF[4], T) / 1000000;
      lat %= 360;
      if (lat < 0) { lat += 360; }

      //Convert to radians
      Double l = rad * lat; // longitude
      Double b = rad * longi; // latitude

      return new Double[] { l, b };
    }

    /// <summary>
    /// Gets right Ascension of celestial object (Ch 13 Fig 13.3)
    /// </summary>
    /// <param name="l">latitude in radians</param>
    /// <param name="b">longitude in radian</param>
    /// <returns>Right Ascension</returns>
    private static Double RightAscension(Double l, Double b) =>
      //Ch 13 Fig 13.3
      //tan a = ( sin(l) * cos(e) - tan(b)-sin(e) ) / cons(l)
      //Converts to the following using Atan2 for 4 quadriatic regions
      Math.Atan2(Math.Sin(l) * Math.Cos(e) - Math.Tan(b) * Math.Sin(e), Math.Cos(l));
    /// <summary>
    /// Gets declination of celestial object (Ch 13 Fig 13.4)
    /// </summary>
    /// <param name="l">latitude in radians</param>
    /// <param name="b">longitude in radian</param>
    /// <returns>Declination</returns>
    private static Double Declination(Double l, Double b) =>
      //Ch 13 Fig 13.4
      //sin o =  sin(b) * cos(e) + cos(b)*sin(e) * sin(l)
      //Converts to the following using Asin
      Math.Asin(Math.Sin(b) * Math.Cos(e) + Math.Cos(b) * Math.Sin(e) * Math.Sin(l));

    static Double Parallax_Dec(Double distance, Double H, Double pCosE, Double pSinE, Double dec, Double cRA) {
      //Ch 40 (Correction for parallax
      //H - geocentric hour angle of the body (sidereal) IAW Ch 12
      Double pi = Math.Asin(Math.Sin(8.794 / distance)) * Math.PI / 180; // 40.1 in radians
      H = H * Math.PI / 180;
      //Directly to topocencric dec
      Double tDEC = Math.Atan2((Math.Sin(dec) - pSinE * Math.Sin(pi)) * Math.Cos(cRA), Math.Cos(dec) - pCosE * Math.Sin(pi) * Math.Cos(H));
      return tDEC;

    }
    static Double Parallax_RA(Double distance, Double H, Double pCosE, Double dec, Double _) {
      //ENSURE RADIANS

      //Ch 40 (Correction for parallax
      //H - geocentric hour angle of the body (sidereal) IAW Ch 12

      Double pi = Math.Asin(Math.Sin(8.794 / distance)) * Math.PI / 180; // 40.1


      //Convert to Radian
      Double t = -pCosE * Math.Sin(pi) * Math.Sin(H);
      Double b = Math.Cos(dec) - pCosE * Math.Sin(pi) * Math.Cos(H);
      Double cRA = Math.Atan2(t, b);
      return cRA;
      //Topocencric RA = RA - cRA
    }
    static Double Get_pSinE(Double dec, Double H) {
      //ASSUME WGS 84 FOR NOW
      //Double a = 6378.14;
      //Double f = 1 / 298.257;
      //Double b = a * (1 - f);
      Double ba = .99664719; // or 1-f
      Double u = ba * dec * Math.PI / 180;

      Double ps = ba * Math.Sin(u) + H / 6378140 * Math.Sin(dec);
      return ps;

    }
    static Double Get_pCosE(Double dec, Double H) {
      //ASSUME WGS 84 FOR NOW
      //Double a = 6378.14;
      //Double f = 1 / 298.257;
      //Double b = a * (1 - f);
      Double ba = .99664719; // or 1-f
      Double u = ba * dec * Math.PI / 180;

      Double ps = Math.Cos(u) + H / 6378140 * Math.Cos(dec);
      return ps;
    }

    static Double Azimuth(Double H, Double phi, Double dec) => Math.Atan2(Math.Sin(H), Math.Cos(H) * Math.Sin(phi) - Math.Tan(dec) * Math.Cos(phi));
    static Double Altitude(Double H, Double phi, Double dec) => Math.Asin(Math.Sin(phi) * Math.Sin(dec) + Math.Cos(phi) * Math.Cos(dec) * Math.Cos(H));
    static Double AstroRefraction(Double h) {
      //CH 16
      Double P = 1013.25; //Average pressure of earth
      Double T = 16; //Average temp of earth
      Double alt = h / Math.PI * 180;
      Double Ref = P * (.1594 + .0196 * alt + .00002 * Math.Pow(alt, 2)) / ((273 + T) * (1 + .505 * alt + .0845 * Math.Pow(alt, 2)));
      return Ref / 60;
    }
  }
}
