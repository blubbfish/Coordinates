using System;
using System.Collections.Generic;

namespace CoordinateSharp {
  /// <summary>
  /// The main class for handling location based celestial information.
  /// </summary>
  /// <remarks>
  /// This class can calculate various pieces of solar and lunar data, based on location and date
  /// </remarks>
  [Serializable]
  public class Celestial {

    //When a rise or a set does not occur, the DateTime will return null

    /// <summary>
    /// Creates an empty Celestial.
    /// </summary>
    public Celestial() {
      this.astrologicalSigns = new AstrologicalSigns();
      this.lunarEclipse = new LunarEclipse();
      this.solarEclipse = new SolarEclipse();
      this.CalculateCelestialTime(0, 0, new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    private Celestial(Boolean hasCalcs) {

      this.astrologicalSigns = new AstrologicalSigns();
      this.lunarEclipse = new LunarEclipse();
      this.solarEclipse = new SolarEclipse();
      if (hasCalcs) { this.CalculateCelestialTime(0, 0, new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)); }
    }

    /// <summary>
    /// Creates a Celestial based on a location and specified date
    /// </summary>
    /// <param name="lat">latitude</param>
    /// <param name="longi">longitude</param>
    /// <param name="geoDate">DateTime (UTC)</param>
    public Celestial(Double lat, Double longi, DateTime geoDate) {
      DateTime d = new DateTime(geoDate.Year, geoDate.Month, geoDate.Day, geoDate.Hour, geoDate.Minute, geoDate.Second, DateTimeKind.Utc);
      this.astrologicalSigns = new AstrologicalSigns();
      this.lunarEclipse = new LunarEclipse();
      this.solarEclipse = new SolarEclipse();
      this.CalculateCelestialTime(lat, longi, d);
    }

    /// <summary>
    /// Creates a Celestial based on a location and date in the provided Coordinate.
    /// </summary>
    /// <param name="c">Coordinate</param>
    /// <returns>Celestial</returns>
    public static Celestial LoadCelestial(Coordinate c) {
      //DateTime geoDate = c.GeoDate;

      //DateTime d = new DateTime(geoDate.Year, geoDate.Month, geoDate.Day, geoDate.Hour, geoDate.Minute, geoDate.Second, DateTimeKind.Utc);
      Celestial cel = new Celestial(c.Latitude.ToDouble(), c.Longitude.ToDouble(), c.GeoDate);

      return cel;
    }

    /// <summary>
    /// Converts Celestial values to local times.
    /// </summary>
    /// <param name="c">Coordinate</param>
    /// <param name="offset">UTC offset</param>
    /// <returns></returns>
    public static Celestial Celestial_LocalTime(Coordinate c, Double offset) {
      if (offset < -12 || offset > 12) { throw new ArgumentOutOfRangeException("Time offsets cannot be greater 12 or less than -12."); }
      //Probably need to offset initial UTC date so user can op in local
      //Determine best way to do this.
      DateTime d = c.GeoDate.AddHours(offset);

      //Get 3 objects for comparison
      Celestial cel = new Celestial(c.Latitude.ToDouble(), c.Longitude.ToDouble(), c.GeoDate);
      Celestial celPre = new Celestial(c.Latitude.ToDouble(), c.Longitude.ToDouble(), c.GeoDate.AddDays(-1));
      Celestial celPost = new Celestial(c.Latitude.ToDouble(), c.Longitude.ToDouble(), c.GeoDate.AddDays(1));

      //Slip objects for comparison. Compare with slipped date. 
      celPre.Local_Convert(c, offset);
      cel.Local_Convert(c, offset);
      celPost.Local_Convert(c, offset);

      //Get SunSet
      Int32 i = Determine_Slipped_Event_Index(cel.SunSet, celPre.SunSet, celPost.SunSet, d);
      cel.sunSet = Get_Correct_Slipped_Date(cel.SunSet, celPre.SunSet, celPost.SunSet, i);
      cel.AdditionalSolarTimes.CivilDusk = Get_Correct_Slipped_Date(cel.AdditionalSolarTimes.CivilDusk,
          celPre.AdditionalSolarTimes.CivilDusk, celPost.AdditionalSolarTimes.CivilDusk, i);
      cel.AdditionalSolarTimes.NauticalDusk = Get_Correct_Slipped_Date(cel.AdditionalSolarTimes.NauticalDusk,
         celPre.AdditionalSolarTimes.NauticalDusk, celPost.AdditionalSolarTimes.NauticalDusk, i);
      //Get SunRise
      i = Determine_Slipped_Event_Index(cel.SunRise, celPre.SunRise, celPost.SunRise, d);
      cel.sunRise = Get_Correct_Slipped_Date(cel.SunRise, celPre.SunRise, celPost.SunRise, i);
      cel.AdditionalSolarTimes.CivilDawn = Get_Correct_Slipped_Date(cel.AdditionalSolarTimes.CivilDawn,
          celPre.AdditionalSolarTimes.CivilDawn, celPost.AdditionalSolarTimes.CivilDawn, i);
      cel.AdditionalSolarTimes.NauticalDawn = Get_Correct_Slipped_Date(cel.AdditionalSolarTimes.NauticalDawn,
         celPre.AdditionalSolarTimes.NauticalDawn, celPost.AdditionalSolarTimes.NauticalDawn, i);

      //MoonRise
      i = Determine_Slipped_Event_Index(cel.MoonRise, celPre.MoonRise, celPost.MoonRise, d);
      cel.moonRise = Get_Correct_Slipped_Date(cel.MoonRise, celPre.MoonRise, celPost.MoonRise, i);

      //MoonSet
      i = Determine_Slipped_Event_Index(cel.MoonSet, celPre.MoonSet, celPost.MoonSet, d);
      cel.moonSet = Get_Correct_Slipped_Date(cel.MoonSet, celPre.MoonSet, celPost.MoonSet, i);

      //Local Conditions
      CelestialStatus[] cels = new CelestialStatus[]
      {
                celPre.MoonCondition,cel.MoonCondition,celPost.MoonCondition
      };
      cel.moonCondition = Celestial.GetStatus(cel.MoonRise, cel.MoonSet, cels);
      cels = new CelestialStatus[]
      {
                celPre.SunCondition, cel.SunCondition, celPost.SunCondition
      };
      cel.sunCondition = Celestial.GetStatus(cel.SunRise, cel.SunSet, cels);

      //Load IsUp values based on local time with populated Celestial
      Celestial.Calculate_Celestial_IsUp_Booleans(d, cel);

      return cel;
    }

    private static CelestialStatus GetStatus(DateTime? rise, DateTime? set, CelestialStatus[] cels) {
      if (set.HasValue && rise.HasValue) { return CelestialStatus.RiseAndSet; }
      if (set.HasValue && !rise.HasValue) { return CelestialStatus.NoRise; }
      if (!set.HasValue && rise.HasValue) { return CelestialStatus.NoSet; }
      for (Int32 x = 0; x < 3; x++) {
        if (cels[x] == CelestialStatus.DownAllDay || cels[x] == CelestialStatus.UpAllDay) {
          return cels[x];
        }
      }
      return cels[1];
    }

    /// <summary>
    /// In place time slip
    /// </summary>
    /// <param name="c">Coordinate</param>
    /// <param name="offset">hour offset</param>
    private void Local_Convert(Coordinate c, Double offset) {
      //Find new lunar set rise times
      if (this.MoonSet.HasValue) { this.moonSet = this.moonSet.Value.AddHours(offset); }
      if (this.MoonRise.HasValue) { this.moonRise = this.moonRise.Value.AddHours(offset); }
      //Perigee
      this.Perigee.ConvertTo_Local_Time(offset);
      //Apogee
      this.Apogee.ConvertTo_Local_Time(offset);
      //Eclipse
      this.LunarEclipse.ConvertTo_LocalTime(offset);

      ////Solar
      if (this.sunSet.HasValue) { this.sunSet = this.sunSet.Value.AddHours(offset); }
      if (this.SunRise.HasValue) { this.sunRise = this.SunRise.Value.AddHours(offset); }
      this.AdditionalSolarTimes.Convert_To_Local_Time(offset);

      //Eclipse
      this.SolarEclipse.ConvertTo_LocalTime(offset);
      SunCalc.CalculateZodiacSign(c.GeoDate.AddHours(offset), this);
      MoonCalc.GetMoonSign(c.GeoDate.AddHours(offset), this);

    }

    /*private PerigeeApogee Get_Correct_Slipped_Date(PerigeeApogee actual, PerigeeApogee pre, PerigeeApogee post, Int32 i)
    {
        switch (i)
        {
            case 0:
                return pre;
            case 1:
                return actual;
            case 2:
                return post;
            default:
                return actual;
        }
    }*/
    private static DateTime? Get_Correct_Slipped_Date(DateTime? actual, DateTime? pre, DateTime? post, Int32 i) => i switch
    {
      0 => pre,
      1 => actual,
      2 => post,
      _ => null,
    };
    private static Int32 Determine_Slipped_Event_Index(DateTime? actual, DateTime? pre, DateTime? post, DateTime d) {

      if (actual.HasValue) {
        if (actual.Value.Day != d.Day) {
          if (pre.HasValue) {
            if (pre.Value.Day == d.Day) { return 0; }
          }
          if (post.HasValue) {
            if (post.Value.Day == d.Day) { return 2; }
          }
          return 3;
        }
      } else {
        if (pre.HasValue) {
          if (pre.Value.Day == d.Day) { return 0; }
        }
        if (post.HasValue) {
          if (post.Value.Day == d.Day) { return 2; }
        }
      }
      return 1;
    }

    internal DateTime? sunSet;
    internal DateTime? sunRise;
    internal DateTime? moonSet;
    internal DateTime? moonRise;

    internal Double sunAltitude;
    internal Double sunAzimuth;
    internal Double moonAltitude;
    internal Double moonAzimuth;

    internal Distance moonDistance;

    internal CelestialStatus sunCondition;
    internal CelestialStatus moonCondition;

    internal Boolean isSunUp;
    internal Boolean isMoonUp;

    internal MoonIllum moonIllum;
    internal Perigee perigee;
    internal Apogee apogee;
    internal AdditionalSolarTimes additionalSolarTimes;
    internal AstrologicalSigns astrologicalSigns;
    internal SolarEclipse solarEclipse;
    internal LunarEclipse lunarEclipse;

    /// <summary>
    /// Sunset time.
    /// </summary>
    public DateTime? SunSet => this.sunSet;
    /// <summary>
    /// Sunrise time.
    /// </summary>
    public DateTime? SunRise => this.sunRise;
    /// <summary>
    /// Moonset time.
    /// </summary>
    public DateTime? MoonSet => this.moonSet;
    /// <summary>
    /// Moonrise time.
    /// </summary>
    public DateTime? MoonRise => this.moonRise;

    /// <summary>
    /// Sun altitude in degrees (E of N).
    /// </summary>
    public Double SunAltitude => this.sunAltitude;
    /// <summary>
    /// Sun azimuth in degrees (E of N).
    /// </summary>
    public Double SunAzimuth => this.sunAzimuth;
    /// <summary>
    /// Moon altitude in degrees (corrected for parallax and refraction).
    /// </summary>
    public Double MoonAltitude => this.moonAltitude;
    /// <summary>
    /// Moon azimuth in degrees (E of N).
    /// </summary>
    public Double MoonAzimuth => this.moonAzimuth;

    /// <summary>
    /// Estimated moon distance from the earth.
    /// </summary>
    public Distance MoonDistance => this.moonDistance;
    /// <summary>
    /// Sun's Condition based on the provided date.
    /// </summary>
    public CelestialStatus SunCondition => this.sunCondition;
    /// <summary>
    /// Moon's condition based on the provided date.
    /// </summary>
    public CelestialStatus MoonCondition => this.moonCondition;

    /// <summary>
    /// Determine if the sun is currently up, based on sunset and sunrise time at the provided location and date.
    /// </summary>
    public Boolean IsSunUp => this.isSunUp;

    /// <summary>
    /// Determine if the moon is currently up, based on moonset and moonrise time at the provided location and date.
    /// </summary>
    public Boolean IsMoonUp => this.isMoonUp;

    /// <summary>
    /// Moon ilumination details based on the provided date.
    /// </summary>
    /// <remarks>
    /// Contains phase, phase name, fraction and angle
    /// </remarks>
    public MoonIllum MoonIllum => this.moonIllum;
    /// <summary>
    /// Moons perigee details based on the provided date.
    /// </summary>
    public Perigee Perigee => this.perigee;
    /// <summary>
    /// Moons apogee details based on the provided date.
    /// </summary>
    public Apogee Apogee => this.apogee;

    /// <summary>
    /// Additional solar event times based on the provided date and location.
    /// </summary>
    /// <remarks>Contains civil and nautical dawn and dusk times.</remarks>
    public AdditionalSolarTimes AdditionalSolarTimes => this.additionalSolarTimes;
    /// <summary>
    /// Astrological signs based on the provided date.
    /// </summary>
    /// <remarks>
    /// Contains zodiac, moon sign and moon name during full moon events
    /// </remarks>
    public AstrologicalSigns AstrologicalSigns => this.astrologicalSigns;

    /// <summary>
    /// Returns a SolarEclipse.
    /// </summary>
    public SolarEclipse SolarEclipse => this.solarEclipse;
    /// <summary>
    /// Returns a LunarEclipse.
    /// </summary>
    public LunarEclipse LunarEclipse => this.lunarEclipse;

    /// <summary>
    /// Calculates all celestial data. Coordinates will notify as changes occur
    /// </summary>
    /// <param name="lat">Decimal format latitude</param>
    /// <param name="longi">Decimal format longitude</param>
    /// <param name="date">Geographic DateTime</param>
    internal void CalculateCelestialTime(Double lat, Double longi, DateTime date) {
      date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, DateTimeKind.Utc);
      SunCalc.CalculateSunTime(lat, longi, date, this);
      MoonCalc.GetMoonTimes(date, lat, longi, this);
      MoonCalc.GetMoonDistance(date, this);

      SunCalc.CalculateZodiacSign(date, this);
      MoonCalc.GetMoonSign(date, this);

      MoonCalc.GetMoonIllumination(date, this, lat, longi);


      this.perigee = MoonCalc.GetPerigeeEvents(date);
      this.apogee = MoonCalc.GetApogeeEvents(date);

      Calculate_Celestial_IsUp_Booleans(date, this);

    }
    /// <summary>
    /// Calculate celestial data based on lat/long and date.
    /// </summary>
    /// <param name="lat">Decimal format latitude</param>
    /// <param name="longi">Decimal format longitude</param>
    /// <param name="date">Geographic DateTime</param>
    /// <returns>Fully populated Celestial object</returns>
    public static Celestial CalculateCelestialTimes(Double lat, Double longi, DateTime date) {
      date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, DateTimeKind.Utc);

      Celestial c = new Celestial(false);

      SunCalc.CalculateSunTime(lat, longi, date, c);
      MoonCalc.GetMoonTimes(date, lat, longi, c);
      MoonCalc.GetMoonDistance(date, c);
      SunCalc.CalculateZodiacSign(date, c);
      MoonCalc.GetMoonSign(date, c);
      MoonCalc.GetMoonIllumination(date, c, lat, longi);

      c.perigee = MoonCalc.GetPerigeeEvents(date);
      c.apogee = MoonCalc.GetApogeeEvents(date);

      Calculate_Celestial_IsUp_Booleans(date, c);

      return c;
    }

    /// <summary>
    /// Calculate sun data based on lat/long and date.
    /// </summary>
    /// <param name="lat">latitude</param>
    /// <param name="longi">longitude</param>
    /// <param name="date">DateTime</param>
    /// <returns>Celestial (Partially Populated)</returns>
    public static Celestial CalculateSunData(Double lat, Double longi, DateTime date) {
      date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, DateTimeKind.Utc);

      Celestial c = new Celestial(false);
      SunCalc.CalculateSunTime(lat, longi, date, c);
      SunCalc.CalculateZodiacSign(date, c);

      return c;
    }
    /// <summary>
    /// Calculate moon data based on lat/long and date.
    /// </summary>
    /// <param name="lat">latitude</param>
    /// <param name="longi">longitude</param>
    /// <param name="date">DateTime</param>
    /// <returns>Celestial (Partially Populated)</returns>
    public static Celestial CalculateMoonData(Double lat, Double longi, DateTime date) {
      date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, DateTimeKind.Utc);

      Celestial c = new Celestial(false);

      MoonCalc.GetMoonTimes(date, lat, longi, c);
      MoonCalc.GetMoonDistance(date, c);
      MoonCalc.GetMoonSign(date, c);
      MoonCalc.GetMoonIllumination(date, c, lat, longi);

      c.perigee = MoonCalc.GetPerigeeEvents(date);
      c.apogee = MoonCalc.GetApogeeEvents(date);

      return c;
    }

    /// <summary>
    /// Returns a List containing solar eclipse data for the century.
    /// Century return is based on the date passed.
    /// </summary>
    /// <param name="lat">latitude</param>
    /// <param name="longi">longitude</param>
    /// <param name="date">DateTime</param>
    /// <returns></returns>
    public static List<SolarEclipseDetails> Get_Solar_Eclipse_Table(Double lat, Double longi, DateTime date) {
      //Convert to Radians
      Double latR = lat * Math.PI / 180;
      Double longR = longi * Math.PI / 180;
      //Get solar data based on date
      Double[] events = Eclipse.SolarData.SolarDateData_100Year(date);
      //Return list of solar data.
      return SolarEclipseCalc.CalculateSolarEclipse(date, latR, longR, events);
    }
    /// <summary>
    /// Returns a List containing solar eclipse data for the century.
    /// Century return is based on the date passed.
    /// </summary>
    /// <param name="lat">latitude</param>
    /// <param name="longi">longitude</param>
    /// <param name="date">DateTime</param>
    /// <returns></returns>
    public static List<LunarEclipseDetails> Get_Lunar_Eclipse_Table(Double lat, Double longi, DateTime date) {
      //Convert to Radians
      Double latR = lat * Math.PI / 180;
      Double longR = longi * Math.PI / 180;
      //Get solar data based on date
      Double[] events = Eclipse.LunarData.LunarDateData_100Year(date);
      //Return list of solar data.
      return LunarEclipseCalc.CalculateLunarEclipse(date, latR, longR, events);
    }

    /// <summary>
    /// Set bool SunIsUp and MoonIsUp values
    /// </summary>
    /// <param name="date">Coordinate GeoDate</param>
    /// <param name="cel">Celestial Object</param>
    private static void Calculate_Celestial_IsUp_Booleans(DateTime date, Celestial cel) {
      //SUN
      switch (cel.SunCondition) {
        case CelestialStatus.DownAllDay:
          cel.isSunUp = false;
          break;
        case CelestialStatus.UpAllDay:
          cel.isSunUp = true;
          break;
        case CelestialStatus.NoRise:
          cel.isSunUp = date < cel.SunSet;
          break;
        case CelestialStatus.NoSet:
          cel.isSunUp = date > cel.SunRise;
          break;
        case CelestialStatus.RiseAndSet:
          cel.isSunUp = cel.SunRise < cel.SunSet ? date > cel.SunRise && date < cel.SunSet : date > cel.SunRise || date < cel.SunSet;
          break;
        default:
          //Should never be reached. If reached, previous calculations failed somewhere.
          break;
      }

      //MOON
      switch (cel.MoonCondition) {
        case CelestialStatus.DownAllDay:
          cel.isMoonUp = false;
          break;
        case CelestialStatus.UpAllDay:
          cel.isMoonUp = true;
          break;
        case CelestialStatus.NoRise:
          cel.isMoonUp = date < cel.MoonSet;
          break;
        case CelestialStatus.NoSet:
          cel.isMoonUp = date > cel.MoonRise;
          break;
        case CelestialStatus.RiseAndSet:
          cel.isMoonUp = cel.MoonRise < cel.MoonSet ? date > cel.MoonRise && date < cel.MoonSet : date > cel.MoonRise || date < cel.MoonSet;
          break;
        default:
          //Should never be reached. If reached, previous calculations failed somewhere.
          break;
      }
    }

    /// <summary>
    /// Returns Apogee object containing last and next apogee based on the specified date.
    /// </summary>
    /// <param name="d">DateTime</param>
    /// <returns>Apogee</returns>
    public static Apogee GetApogees(DateTime d) => MoonCalc.GetApogeeEvents(d);
    /// <summary>
    /// Returns Perigee object containing last and next perigee based on the specified date.
    /// </summary>
    /// <param name="d">DateTime</param>
    /// <returns>Perigee</returns>
    public static Perigee GetPerigees(DateTime d) => MoonCalc.GetPerigeeEvents(d);
  }

}
