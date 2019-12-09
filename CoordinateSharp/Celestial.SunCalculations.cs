using System;
using System.Collections.Generic;

namespace CoordinateSharp {
  internal class SunCalc {
    public static void CalculateSunTime(Double lat, Double longi, DateTime date, Celestial c, Double _ = 0) {
      if (date.Year == 0001) { return; } //Return if date vaue hasn't been established.
      DateTime actualDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);

      ////Sun Time Calculations

      //Get Julian     
      Double lw = rad * -longi;
      Double phi = rad * lat;

      //Rise Set        
      DateTime?[] evDate = Get_Event_Time(lw, phi, -.8333, actualDate);
      c.sunRise = evDate[0];
      c.sunSet = evDate[1];

      c.sunCondition = CelestialStatus.RiseAndSet;
      //Azimuth and Altitude
      CalculateSunAngle(date, longi, lat, c);
      // neither sunrise nor sunset
      if (!c.SunRise.HasValue && !c.SunSet.HasValue) {
        c.sunCondition = c.SunAltitude < 0 ? CelestialStatus.DownAllDay : CelestialStatus.UpAllDay;
      }
      // sunrise or sunset
      else {
        if (!c.SunRise.HasValue) {
          // No sunrise this date
          c.sunCondition = CelestialStatus.NoRise;

        } else if (!c.SunSet.HasValue) {
          // No sunset this date
          c.sunCondition = CelestialStatus.NoSet;
        }
      }
      //Additional Times
      c.additionalSolarTimes = new AdditionalSolarTimes();
      //Dusk and Dawn
      //Civil
      evDate = Get_Event_Time(lw, phi, -6, actualDate);
      c.AdditionalSolarTimes.CivilDawn = evDate[0];
      c.AdditionalSolarTimes.CivilDusk = evDate[1];


      //Nautical
      evDate = Get_Event_Time(lw, phi, -12, actualDate);
      c.AdditionalSolarTimes.NauticalDawn = evDate[0];
      c.AdditionalSolarTimes.NauticalDusk = evDate[1];

      //Astronomical
      evDate = Get_Event_Time(lw, phi, -18, actualDate);

      c.AdditionalSolarTimes.AstronomicalDawn = evDate[0];
      c.AdditionalSolarTimes.AstronomicalDusk = evDate[1];

      //BottomDisc
      evDate = Get_Event_Time(lw, phi, -.2998, actualDate);
      c.AdditionalSolarTimes.SunriseBottomDisc = evDate[0];
      c.AdditionalSolarTimes.SunsetBottomDisc = evDate[1];

      CalculateSolarEclipse(date, lat, longi, c);

    }
    /// <summary>
    /// Gets time of event based on specified degree below horizon
    /// </summary>
    /// <param name="lw">Observer Longitude in radians</param>
    /// <param name="phi">Observer Latitude in radians</param>
    /// <param name="h">Angle in Degrees</param>
    /// <param name="date">Date of Event</param>
    /// <returns>DateTime?[]{rise, set}</returns>
    private static DateTime?[] Get_Event_Time(Double lw, Double phi, Double h, DateTime date) {
      //Create arrays. Index 0 = Day -1, 1 = Day, 2 = Day + 1;
      //These will be used to find exact day event occurs for comparison
      DateTime?[] sets = new DateTime?[] { null, null, null, null, null };
      DateTime?[] rises = new DateTime?[] { null, null, null, null, null };

      //Iterate starting with day -1;
      for (Int32 x = 0; x < 5; x++) {
        Double d = JulianConversions.GetJulian(date.AddDays(x - 2)) - j2000 + .5; //LESS PRECISE JULIAN NEEDED

        Double n = JulianCycle(d, lw);
        Double ds = ApproxTransit(0, lw, n);

        Double M = SolarMeanAnomaly(ds);
        Double L = EclipticLongitude(M);

        Double dec = Declination(L, 0);

        Double Jnoon = SolarTransitJ(ds, M, L);

        Double Jset;
        Double Jrise;


        //DateTime? solarNoon = JulianConversions.GetDate_FromJulian(Jnoon);
        //DateTime? nadir = JulianConversions.GetDate_FromJulian(Jnoon - 0.5);

        //Rise Set
        Jset = GetTime(h * rad, lw, phi, dec, n, M, L);
        Jrise = Jnoon - (Jset - Jnoon);

        DateTime? rise = JulianConversions.GetDate_FromJulian(Jrise);
        DateTime? set = JulianConversions.GetDate_FromJulian(Jset);

        rises[x] = rise;
        sets[x] = set;
      }

      //Compare and send
      DateTime? tRise = null;
      for (Int32 x = 0; x < 5; x++) {
        if (rises[x].HasValue) {
          if (rises[x].Value.Day == date.Day) {
            tRise = rises[x];
            break;
          }
        }
      }
      DateTime? tSet = null;
      for (Int32 x = 0; x < 5; x++) {
        if (sets[x].HasValue) {
          if (sets[x].Value.Day == date.Day) {
            tSet = sets[x];
            break;
          }
        }
      }
      return new DateTime?[] { tRise, tSet };
    }

    public static void CalculateZodiacSign(DateTime date, Celestial c) {
      //Aquarius (January 20 to February 18)
      //Pisces (February 19 to March 20)
      //Aries (March 21-April 19)
      //Taurus (April 20-May 20)
      //Gemini (May 21-June 20)
      //Cancer (June 21-July 22)
      //Leo (July 23-August 22)
      //Virgo (August 23-September 22)
      //Libra (September 23-October 22)
      //Scorpio (October 23-November 21)
      //Sagittarius (November 22-December 21)
      //Capricorn (December 22-January 19)           
      if (date >= new DateTime(date.Year, 1, 1) && date <= new DateTime(date.Year, 1, 19, 23, 59, 59)) {
        c.AstrologicalSigns.ZodiacSign = "Capricorn";
        return;
      }
      if (date >= new DateTime(date.Year, 1, 20) && date <= new DateTime(date.Year, 2, 18, 23, 59, 59)) {
        c.AstrologicalSigns.ZodiacSign = "Aquarius";
        return;
      }
      if (date >= new DateTime(date.Year, 2, 19) && date <= new DateTime(date.Year, 3, 20, 23, 59, 59)) {
        c.AstrologicalSigns.ZodiacSign = "Pisces";
        return;
      }
      if (date >= new DateTime(date.Year, 3, 21) && date <= new DateTime(date.Year, 4, 19, 23, 59, 59)) {
        c.AstrologicalSigns.ZodiacSign = "Aries";
        return;
      }
      if (date >= new DateTime(date.Year, 4, 20) && date <= new DateTime(date.Year, 5, 20, 23, 59, 59)) {
        c.AstrologicalSigns.ZodiacSign = "Taurus";
        return;
      }
      if (date >= new DateTime(date.Year, 5, 21) && date <= new DateTime(date.Year, 6, 20, 23, 59, 59)) {
        c.AstrologicalSigns.ZodiacSign = "Gemini";
        return;
      }
      if (date >= new DateTime(date.Year, 6, 21) && date <= new DateTime(date.Year, 7, 22, 23, 59, 59)) {
        c.AstrologicalSigns.ZodiacSign = "Cancer";
        return;
      }
      if (date >= new DateTime(date.Year, 7, 23) && date <= new DateTime(date.Year, 8, 22, 23, 59, 59)) {
        c.AstrologicalSigns.ZodiacSign = "Leo";
        return;
      }
      if (date >= new DateTime(date.Year, 8, 23) && date <= new DateTime(date.Year, 9, 22, 23, 59, 59)) {
        c.AstrologicalSigns.ZodiacSign = "Virgo";
        return;
      }
      if (date >= new DateTime(date.Year, 9, 23) && date <= new DateTime(date.Year, 10, 22, 23, 59, 59)) {
        c.AstrologicalSigns.ZodiacSign = "Libra";
        return;
      }
      if (date >= new DateTime(date.Year, 9, 23) && date <= new DateTime(date.Year, 11, 21, 23, 59, 59)) {
        c.AstrologicalSigns.ZodiacSign = "Scorpio";
        return;
      }
      if (date >= new DateTime(date.Year, 11, 21) && date <= new DateTime(date.Year, 12, 21, 23, 59, 59)) {
        c.AstrologicalSigns.ZodiacSign = "Sagittarius";
        return;
      }
      if (date >= new DateTime(date.Year, 12, 22) && date <= new DateTime(date.Year, 12, 31, 23, 59, 59)) {
        c.AstrologicalSigns.ZodiacSign = "Capricorn";
        return;
      }
    }
    public static void CalculateSolarEclipse(DateTime date, Double lat, Double longi, Celestial c) {
      //Convert to Radian
      Double latR = lat * Math.PI / 180;
      Double longR = longi * Math.PI / 180;
      List<List<String>> se = SolarEclipseCalc.CalculateSolarEclipse(date, latR, longR);
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
        c.SolarEclipse.LastEclipse = new SolarEclipseDetails(se[lastE]);
      }
      if (nextE >= 0) {
        c.SolarEclipse.NextEclipse = new SolarEclipseDetails(se[nextE]);
      }
    }

    #region Private Suntime Members
    private static readonly Double dayMS = 1000 * 60 * 60 * 24, j1970 = 2440588, j2000 = 2451545;
    private static readonly Double rad = Math.PI / 180;

    /*private static Double LocalSiderealTimeForTimeZone(Double lon, Double jd, Double z)
    {
  Double s = 24110.5 + 8640184.812999999 * jd / 36525 + 86636.6 * z + 86400 * lon;
        s = s / 86400;
        s = s - Math.Truncate(s);
  Double lst = s * 360 *rad;

        return lst;
    }*/
    private static Double SideRealTime(Double d, Double lw) {
      Double s = rad * (280.16 + 360.9856235 * d) - lw;
      return s;
    }
    private static Double SolarTransitJ(Double ds, Double M, Double L) => j2000 + ds + 0.0053 * Math.Sin(M) - 0.0069 * Math.Sin(2 * L);

    //CH15 
    //Formula 15.1
    //Returns Approximate Time
    private static Double HourAngle(Double h, Double phi, Double d) {
      //NUMBER RETURNING > and < 1 NaN;
      Double d1 = Math.Sin(h) - Math.Sin(phi) * Math.Sin(d);
      Double d2 = Math.Cos(phi) * Math.Cos(d);
      Double d3 = d1 / d2;

      return Math.Acos(d3);
    }
    private static Double ApproxTransit(Double Ht, Double lw, Double n) => 0.0009 + (Ht + lw) / (2 * Math.PI) + n;

    private static Double JulianCycle(Double d, Double lw) => Math.Round(d - 0.0009 - lw / (2 * Math.PI));

    //Returns Time of specified event based on suns angle
    private static Double GetTime(Double h, Double lw, Double phi, Double dec, Double n, Double M, Double L) {
      Double approxTime = HourAngle(h, phi, dec);    //Ch15 Formula 15.1  

      Double a = ApproxTransit(approxTime, lw, n);
      Double st = SolarTransitJ(a, M, L);

      return st;
    }
    private static Double Declination(Double l, Double b) {
      Double e = Math.PI / 180 * 23.4392911; // obliquity of the Earth

      return Math.Asin(Math.Sin(b) * Math.Cos(e) + Math.Cos(b) * Math.Sin(e) * Math.Sin(l));
    }

    private static void CalculateSunAngle(DateTime date, Double longi, Double lat, Celestial c) {
      TimeSpan ts = date - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
      Double dms = ts.TotalMilliseconds / dayMS - .5 + j1970 - j2000;

      Double lw = rad * -longi;
      Double phi = rad * lat;
      //Double e = rad * 23.4397;

      Double[] sc = SunCoords(dms);

      Double H = SideRealTime(dms, lw) - sc[1];

      c.sunAzimuth = Math.Atan2(Math.Sin(H), Math.Cos(H) * Math.Sin(phi) - Math.Tan(sc[0]) * Math.Cos(phi)) * 180 / Math.PI + 180;
      c.sunAltitude = Math.Asin(Math.Sin(phi) * Math.Sin(sc[0]) + Math.Cos(phi) * Math.Cos(sc[0]) * Math.Cos(H)) * 180 / Math.PI;
    }

    private static Double SolarMeanAnomaly(Double d) => rad * (357.5291 + 0.98560028 * d);

    private static Double EclipticLongitude(Double m) {
      Double c = rad * (1.9148 * Math.Sin(m) + 0.02 * Math.Sin(2 * m) + 0.0003 * Math.Sin(3 * m)); // equation of center
      Double p = rad * 102.9372; // perihelion of the Earth

      return m + c + p + Math.PI;
    }
    private static Double[] SunCoords(Double d) {

      Double m = SolarMeanAnomaly(d);
      Double l = EclipticLongitude(m);
      Double[] sc = new Double[2];
      Double b = 0;
      Double e = rad * 23.4397; // obliquity of the Earth
      sc[0] = Math.Asin(Math.Sin(b) * Math.Cos(e) + Math.Cos(b) * Math.Sin(e) * Math.Sin(l)); //declination
      sc[1] = Math.Atan2(Math.Sin(l) * Math.Cos(e) - Math.Tan(b) * Math.Sin(e), Math.Cos(l)); //rightAscension     
      return sc;
    }
    #endregion

  }
}
