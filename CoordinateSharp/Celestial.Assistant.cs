using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CoordinateSharp {
  internal partial class MeeusTables {

    /// <summary>
    /// Returns Moon Periodic Value Er
    /// </summary>
    /// <param name="D">Moon's mean elongation</param>
    /// <param name="M">Sun's mean anomaly</param>
    /// <param name="N">Moon's mean anomaly</param>
    /// <param name="F">Moon's argument of latitude</param>
    /// <param name="T">Dynamic time</param>
    /// <returns>Er</returns>
    public static Double Moon_Periodic_Er(Double D, Double M, Double N, Double F, Double T) {
      //Table 47A contains 60 lines to sum
      Double[] values = new Double[] { D, M, N, F };
      Double sum = 0;
      for (Int32 x = 0; x < 60; x++) {
        sum += Get_Table47A_Values(values, x, T, false);
      }

      return sum;
    }
    /// <summary>
    /// Returns Moon Periodic Value El
    /// </summary>
    /// <param name="L">Moon's mean longitude</param>
    /// <param name="D">Moon's mean elongation</param>
    /// <param name="M">Sun's mean anomaly</param>
    /// <param name="N">Moon's mean anomaly</param>
    /// <param name="F">Moon's argument of latitude</param>
    /// <param name="T">Dynamic time</param>
    /// <returns>El</returns>
    public static Double Moon_Periodic_El(Double L, Double D, Double M, Double N, Double F, Double T) {
      //Table 47A contains 60 lines to sum
      Double[] values = new Double[] { D, M, N, F };
      Double sum = 0;
      for (Int32 x = 0; x < 60; x++) {
        sum += Get_Table47A_Values(values, x, T, true);
      }

      //Planetary adjustments
      Double A1 = 119.75 + 131.849 * T;
      Double A2 = 53.09 + 479264.290 * T;

      //Normalize 0-360 degree number
      A1 %= 360;
      if (A1 < 0) { A1 += 360; }
      A2 %= 360;
      if (A2 < 0) { A2 += 360; }

      //Convert DMF to radians
      A1 = A1 * Math.PI / 180;
      A2 = A2 * Math.PI / 180;

      //L TO RADIANS
      L %= 360;
      if (L < 0) { L += 360; }

      //Convert DMF to radians
      L = L * Math.PI / 180;

      sum += 3958 * Math.Sin(A1);
      sum += 1962 * Math.Sin(L - F);
      sum += 318 * Math.Sin(A2);

      return sum;
    }
    /// <summary>
    /// Returns Moon Periodic Value Eb
    /// </summary>
    /// <param name="L">Moon's mean longitude</param>
    /// <param name="D">Moon's mean elongation</param>
    /// <param name="M">Sun's mean anomaly</param>
    /// <param name="N">Moon's mean anomaly</param>
    /// <param name="F">Moon's argument of latitude</param>
    /// <param name="T">Dynamic time</param>
    /// <returns>Eb</returns>
    public static Double Moon_Periodic_Eb(Double L, Double D, Double M, Double N, Double F, Double T) {
      //Table 47B contains 60 lines to sum
      Double[] values = new Double[] { D, M, N, F };
      Double sum = 0;
      for (Int32 x = 0; x < 60; x++) {
        sum += Get_Table47B_Values(values, x, T);
      }

      //Planetary adjustments     
      Double A1 = 119.75 + 131.849 * T;
      Double A3 = 313.45 + 481266.484 * T;

      //Normalize 0-360 degree number   
      A1 %= 360;
      if (A1 < 0) { A1 += 360; }
      A3 %= 360;
      if (A3 < 0) { A3 += 360; }

      //Convert DMF to radians
      A1 = A1 * Math.PI / 180;
      A3 = A3 * Math.PI / 180;

      //L TO RADIANS
      L %= 360;
      if (L < 0) { L += 360; }

      //Convert DMF to radians
      L = L * Math.PI / 180;

      sum += -2235 * Math.Sin(L);
      sum += 382 * Math.Sin(A3);
      sum += 175 * Math.Sin(A1 - F);
      sum += 175 * Math.Sin(A1 + F);
      sum += 127 * Math.Sin(L - M);
      sum += -115 * Math.Sin(L + M);

      return sum;
    }
    //Ch 50
    /// <summary>
    /// Sum of Apogee Terms from Jean Meeus Astronomical Algorithms Table 50.A
    /// </summary>
    /// <param name="D">Moom's mean elongation at time JDE</param>
    /// <param name="M">Sun's mean anomaly</param>
    /// <param name="F">Moon's arguement f latitude</param>
    /// <param name="T">Time in Julian centuries since epoch 2000</param>
    /// <returns>double</returns>
    public static Double ApogeeTermsA(Double D, Double M, Double F, Double T) {
      Double sum;

      sum = Math.Sin(2 * D) * 0.4392;
      sum += Math.Sin(4 * D) * 0.0684;
      sum += Math.Sin(M) * .0456 - 0.00011 * T;
      sum += Math.Sin(2 * D - M) * .0426 - 0.00011 * T;
      sum += Math.Sin(2 * F) * .0212;
      sum += Math.Sin(D) * -0.0189;
      sum += Math.Sin(6 * D) * .0144;
      sum += Math.Sin(4 * D - M) * .0113;
      sum += Math.Sin(2 * D + 2 * F) * .0047;
      sum += Math.Sin(D + M) * .0036;
      sum += Math.Sin(8 * D) * .0035;
      sum += Math.Sin(6 * D - M) * .0034;
      sum += Math.Sin(2 * D - 2 * F) * -0.0034;
      sum += Math.Sin(2 * D - 2 * M) * .0022;
      sum += Math.Sin(3 * D) * -.0017;
      sum += Math.Sin(4 * D + 2 * F) * 0.0013;

      sum += Math.Sin(8 * D - M) * .0011;
      sum += Math.Sin(4 * D - 2 * M) * .0010;
      sum += Math.Sin(10 * D) * .0009;
      sum += Math.Sin(3 * D + M) * .0007;
      sum += Math.Sin(2 * M) * .0006;
      sum += Math.Sin(2 * D + M) * .0005;
      sum += Math.Sin(2 * D + 2 * M) * .0005;
      sum += Math.Sin(6 * D + 2 * F) * .0004;
      sum += Math.Sin(6 * D - 2 * M) * .0004;
      sum += Math.Sin(10 * D - M) * .0004;
      sum += Math.Sin(5 * D) * -0.0004;
      sum += Math.Sin(4 * D - 2 * F) * -0.0004;
      sum += Math.Sin(2 * F + M) * .0003;
      sum += Math.Sin(12 * D) * .0003;
      sum += Math.Sin(2 * D + 2 * F - M) * 0.0003;
      sum += Math.Sin(D - M) * -0.0003;
      return sum;
    }
    /// <summary>
    /// Sum of Perigee Terms from Jean Meeus Astronomical Algorithms Table 50.A
    /// </summary>
    /// <param name="D">Moom's mean elongation at time JDE</param>
    /// <param name="M">Sun's mean anomaly</param>
    /// <param name="F">Moon's arguement f latitude</param>
    /// <param name="T">Time in Julian centuries since epoch 2000</param>
    /// <returns>double</returns>
    public static Double PerigeeTermsA(Double D, Double M, Double F, Double T) {
      Double sum;

      sum = Math.Sin(2 * D) * -1.6769;
      sum += Math.Sin(4 * D) * .4589;
      sum += Math.Sin(6 * D) * -.1856;
      sum += Math.Sin(8 * D) * .0883;
      sum += Math.Sin(2 * D - M) * -.0773 + .00019 * T;
      sum += Math.Sin(M) * .0502 - .00013 * T;
      sum += Math.Sin(10 * D) * -.0460;
      sum += Math.Sin(4 * D - M) * .0422 - .00011 * T;
      sum += Math.Sin(6 * D - M) * -.0256;
      sum += Math.Sin(12 * D) * .0253;
      sum += Math.Sin(D) * .0237;
      sum += Math.Sin(8 * D - M) * .0162;
      sum += Math.Sin(14 * D) * -.0145;
      sum += Math.Sin(2 * F) * .0129;
      sum += Math.Sin(3 * D) * -.0112;
      sum += Math.Sin(10 * D - M) * -.0104;
      sum += Math.Sin(16 * D) * .0086;
      sum += Math.Sin(12 * D - M) * .0069;
      sum += Math.Sin(5 * D) * .0066;
      sum += Math.Sin(2 * D + 2 * F) * -.0053;
      sum += Math.Sin(18 * D) * -.0052;
      sum += Math.Sin(14 * D - M) * -.0046;
      sum += Math.Sin(7 * D) * -.0041;
      sum += Math.Sin(2 * D + M) * .0040;
      sum += Math.Sin(20 * D) * .0032;
      sum += Math.Sin(D + M) * -.0032;
      sum += Math.Sin(16 * D - M) * .0031;
      sum += Math.Sin(4 * D + M) * -.0029;
      sum += Math.Sin(9 * D) * .0027;
      sum += Math.Sin(4 * D + 2 * F) * .0027;

      sum += Math.Sin(2 * D - 2 * M) * -.0027;
      sum += Math.Sin(4 * D - 2 * M) * .0024;
      sum += Math.Sin(6 * D - 2 * M) * -.0021;
      sum += Math.Sin(22 * D) * -.0021;
      sum += Math.Sin(18 * D - M) * -.0021;
      sum += Math.Sin(6 * D + M) * .0019;
      sum += Math.Sin(11 * D) * -.0018;
      sum += Math.Sin(8 * D + M) * -.0014;
      sum += Math.Sin(4 * D - 2 * F) * -.0014;
      sum += Math.Sin(6 * D + 2 * F) * -.0014;
      sum += Math.Sin(3 * D + M) * .0014;
      sum += Math.Sin(5 * D + M) * -.0014;
      sum += Math.Sin(13 * D) * .0013;
      sum += Math.Sin(20 * D - M) * .0013;
      sum += Math.Sin(3 * D + 2 * M) * .0011;
      sum += Math.Sin(4 * D + 2 * F - 2 * M) * -.0011;
      sum += Math.Sin(D + 2 * M) * -.0010;
      sum += Math.Sin(22 * D - M) * -.0009;
      sum += Math.Sin(4 * F) * -.0008;
      sum += Math.Sin(6 * D - 2 * F) * .0008;
      sum += Math.Sin(2 * D - 2 * F + M) * .0008;
      sum += Math.Sin(2 * M) * .0007;
      sum += Math.Sin(2 * F - M) * .0007;
      sum += Math.Sin(2 * D + 4 * F) * .0007;
      sum += Math.Sin(2 * F - 2 * M) * -.0006;
      sum += Math.Sin(2 * D - 2 * F + 2 * M) * -.0006;
      sum += Math.Sin(24 * D) * .0006;
      sum += Math.Sin(4 * D - 4 * F) * .0005;
      sum += Math.Sin(2 * D + 2 * M) * .0005;
      sum += Math.Sin(D - M) * -.0004;

      return sum;
    }
    /// <summary>
    /// Sum of Apogee Terms from Jean Meeus Astronomical Algorithms Table 50.B
    /// </summary>
    /// <param name="D">Moom's mean elongation at time JDE</param>
    /// <param name="M">Sun's mean anomaly</param>
    /// <param name="F">Moon's arguement f latitude</param>
    /// <param name="T">Time in Julian centuries since epoch 2000</param>
    /// <returns>double</returns>
    public static Double ApogeeTermsB(Double D, Double M, Double F, Double T) {
      Double sum = 3245.251;

      sum += Math.Cos(2 * D) * -9.147;
      sum += Math.Cos(D) * -.841;
      sum += Math.Cos(2 * F) * .697;
      sum += Math.Cos(M) * -0.656 + .0016 * T;
      sum += Math.Cos(4 * D) * .355;
      sum += Math.Cos(2 * D - M) * .159;
      sum += Math.Cos(D + M) * .127;
      sum += Math.Cos(4 * D - M) * .065;

      sum += Math.Cos(6 * D) * .052;
      sum += Math.Cos(2 * D + M) * .043;
      sum += Math.Cos(2 * D + 2 * F) * .031;
      sum += Math.Cos(2 * D - 2 * F) * -.023;
      sum += Math.Cos(2 * D - 2 * M) * .022;
      sum += Math.Cos(2 * D + 2 * M) * .019;
      sum += Math.Cos(2 * M) * -.016;
      sum += Math.Cos(6 * D - M) * .014;
      sum += Math.Cos(8 * D) * .010;

      return sum;
    }
    /// <summary>
    /// Sum of Perigee Terms from Jean Meeus Astronomical Algorithms Table 50.B
    /// </summary>
    /// <param name="D">Moom's mean elongation at time JDE</param>
    /// <param name="M">Sun's mean anomaly</param>
    /// <param name="F">Moon's arguement f latitude</param>
    /// <param name="T">Time in Julian centuries since epoch 2000</param>
    /// <returns>double</returns>
    public static Double PerigeeTermsB(Double D, Double M, Double F, Double T) {
      //Sum of Perigee Terms from Jean Meeus Astronomical Algorithms Table 50.B          
      Double sum = 3629.215;

      sum += Math.Cos(2 * D) * 63.224;
      sum += Math.Cos(4 * D) * -6.990;
      sum += Math.Cos(2 * D - M) * 2.834 - .0071 * T;
      sum += Math.Cos(6 * D) * 1.927;
      sum += Math.Cos(D) * -1.263;
      sum += Math.Cos(8 * D) * -.702;
      sum += Math.Cos(M) * .696 - .0017 * T;
      sum += Math.Cos(2 * F) * -.690;
      sum += Math.Cos(4 * D - M) * -.629 + .0016 * T;
      sum += Math.Cos(2 * D - 2 * F) * -.392;
      sum += Math.Cos(10 * D) * .297;
      sum += Math.Cos(6 * D - M) * .260;
      sum += Math.Cos(3 * D) * .201;
      sum += Math.Cos(2 * D + M) * -.161;
      sum += Math.Cos(D + M) * .157;
      sum += Math.Cos(12 * D) * -.138;
      sum += Math.Cos(8 * D - M) * -.127;
      sum += Math.Cos(2 * D + 2 * F) * .104;
      sum += Math.Cos(2 * D - 2 * M) * .104;
      sum += Math.Cos(5 * D) * -.079;
      sum += Math.Cos(14 * D) * .068;

      sum += Math.Cos(10 * D - M) * .067;
      sum += Math.Cos(4 * D + M) * .054;
      sum += Math.Cos(12 * D - M) * -.038;
      sum += Math.Cos(4 * D - 2 * M) * -.038;
      sum += Math.Cos(7 * D) * .037;
      sum += Math.Cos(4 * D + 2 * F) * -.037;
      sum += Math.Cos(16 * D) * -.035;
      sum += Math.Cos(3 * D + M) * -.030;
      sum += Math.Cos(D - M) * .029;
      sum += Math.Cos(6 * D + M) * -.025;
      sum += Math.Cos(2 * M) * .023;
      sum += Math.Cos(14 * D - M) * .023;
      sum += Math.Cos(2 * D + 2 * M) * -.023;
      sum += Math.Cos(6 * D - 2 * M) * .022;
      sum += Math.Cos(2 * D - 2 * F - M) * -.021;
      sum += Math.Cos(9 * D) * -.020;
      sum += Math.Cos(18 * D) * .019;
      sum += Math.Cos(6 * D + 2 * F) * .017;
      sum += Math.Cos(2 * F - M) * .014;
      sum += Math.Cos(16 * D - M) * -.014;
      sum += Math.Cos(4 * D - 2 * F) * .013;
      sum += Math.Cos(8 * D + M) * .012;
      sum += Math.Cos(11 * D) * .011;
      sum += Math.Cos(5 * D + M) * .010;
      sum += Math.Cos(20 * D) * -.010;

      return sum;
    }

  }
  internal class MeeusFormulas {
    public static Double Get_Sidereal_Time(Double JD) {
      //Ch. 12
      //T = Dynamic Time
      //Oo = mean sidereal time at Greenwich at 0h UT
      Double T = (JD - 2451545) / 36525;
      Double Oo = 280.46061837 + 360.98564736629 * (JD - 2451545) +
                .000387933 * Math.Pow(T, 2) - Math.Pow(T, 3) / 38710000;
      return Oo;
    }
  }
  /// <summary>
  /// Used to display a celestial condition for a specified date.
  /// </summary>
  [Serializable]
  public enum CelestialStatus {
    /// <summary>
    /// Celestial body rises and sets on the set day.
    /// </summary>
    RiseAndSet,
    /// <summary>
    /// Celestial body is down all day
    /// </summary>
    DownAllDay,
    /// <summary>
    /// Celestial body is up all day
    /// </summary>
    UpAllDay,
    /// <summary>
    /// Celestial body rises, but does not set on the set day
    /// </summary>
    NoRise,
    /// <summary>
    /// Celestial body sets, but does not rise on the set day
    /// </summary>
    NoSet
  }
  /// <summary>
  ///  moon perigee or apogee indicator
  /// </summary>
  internal enum MoonDistanceType {
    /// <summary>
    /// Moon's perigee
    /// </summary>
    Perigee,
    /// <summary>
    /// Moon's apogee
    /// </summary>
    Apogee
  }
  /// <summary>
  /// Moon Illumination Information
  /// </summary>
  [Serializable]
  public class MoonIllum {

    /// <summary>
    /// Moon's fraction
    /// </summary>
    public Double Fraction { get; internal set; }
    /// <summary>
    /// Moon's Angle
    /// </summary>
    public Double Angle { get; internal set; }
    /// <summary>
    /// Moon's phase
    /// </summary>
    public Double Phase { get; internal set; }
    /// <summary>
    /// Moon's phase name for the specified day
    /// </summary>
    public String PhaseName { get; internal set; }

  }
  /// <summary>
  /// Stores Perigee or Apogee values
  /// </summary>
  [Serializable]
  public class PerigeeApogee {

    /// <summary>
    /// Initializes a Perigee or Apogee object
    /// </summary>
    /// <param name="d">Date of Event</param>
    /// <param name="p">Horizontal Parallax</param>
    /// <param name="dist">Distance</param>
    public PerigeeApogee(DateTime d, Double p, Distance dist) {
      this.Date = d;
      this.HorizontalParallax = p;
      this.Distance = dist;
    }

    /// <summary>
    /// Date of event.
    /// </summary>
    public DateTime Date { get; }
    /// <summary>
    /// Horizontal Parallax.
    /// </summary>
    public Double HorizontalParallax { get; }
    /// <summary>
    /// Moon's distance at event.
    /// </summary>
    public Distance Distance { get; }

    internal void Convert_To_Local_Time(Double offset) {
      FieldInfo[] fields = typeof(PerigeeApogee).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
      foreach (FieldInfo field in fields) {
        if (field.FieldType == typeof(DateTime)) {
          DateTime d = (DateTime)field.GetValue(this);
          if (d > new DateTime()) {
            d = d.AddHours(offset);
            field.SetValue(this, d);
          }
        }
      }
    }

  }
  /// <summary>
  /// Julian date conversions
  /// </summary>
  public class JulianConversions {
    //1.1.3.1
    private static readonly Double J1970 = 2440588, J2000 = 2451545;

    /// <summary>
    /// Returns JD.
    /// Meeus Ch 7.
    /// </summary>
    /// <param name="d">DateTime</param>
    /// <returns>JDE</returns>
    public static Double GetJulian(DateTime d) {
      Double y = d.Year;
      Double m = d.Month;
      Double dy = d.Day + d.TimeOfDay.TotalHours / 24;

      //If month is Jan or Feb add 12 to month and reduce year by 1.
      if (m <= 2) { m += 12; y -= 1; }

      Double A = (Int32)(d.Year / 100.0);
      Double B = 0;

      //Gregorian Start Date
      if (d >= new DateTime(1582, 10, 15)) {
        B = 2 - A + (Int32)(A / 4.0);
      }
      Double JD = (Int32)(365.25 * (y + 4716)) + (Int32)(30.6001 * (m + 1)) + dy + B - 1524.5;
      return JD;
    }
    /// <summary>
    /// Returns JD from epoch 2000.
    /// Meeus Ch 7.
    /// </summary>
    /// <param name="d">DateTime</param>
    /// <returns>JDE</returns>
    public static Double GetJulian_Epoch2000(DateTime d) => GetJulian(d) - J2000;
    /// <summary>
    /// Returns JD from epoch 1970.
    /// Meeus Ch 7.
    /// </summary>
    /// <param name="d">DateTime</param>
    /// <returns>JDE</returns>
    public static Double GetJulian_Epoch1970(DateTime d) => GetJulian(d) - J1970;

    /// <summary>
    /// Returns date from Julian
    /// Meeus ch. 7
    /// </summary>
    /// <param name="j">Julian</param>
    /// <returns>DateTime</returns>
    public static DateTime? GetDate_FromJulian(Double j) {
      if (Double.IsNaN(j)) { return null; } //No Event Occured

      j += .5;
      Double Z = Math.Floor(j);
      Double F = j - Z;
      Double A = Z;
      if (Z >= 2299161) {
        Double a = (Int32)((Z - 1867216.25) / 36524.25);
        A = Z + 1 + a - (Int32)(a / 4.0);
      }
      Double B = A + 1524;
      Double C = (Int32)((B - 122.1) / 365.25);
      Double D = (Int32)(365.25 * C);
      Double E = (Int32)((B - D) / 30.6001);

      Double day = B - D - (Int32)(30.6001 * E) + F;

      //Month is E-1 if month is < 14 or E-13 if month is 14 or 15
      Double month = E - 1;
      if (E > 13) { month -= 12; }

      //year is C-4716 if month>2 and C-4715 if month is 1 or 2
      Double year = C - 4715;
      if (month > 2) {
        year -= 1;
      }

      Double hours = day - Math.Floor(day);
      hours *= 24;
      Double minutes = hours - Math.Floor(hours);
      minutes *= 60;
      Double seconds = minutes - Math.Floor(minutes);
      seconds *= 60;

      day = Math.Floor(day);
      hours = Math.Floor(hours);
      minutes = Math.Floor(minutes);

      DateTime? date = new DateTime?(new DateTime((Int32)year, (Int32)month, (Int32)day, (Int32)hours, (Int32)minutes, (Int32)seconds));
      return date;
    }
    /// <summary>
    /// Returns date from Julian based on epoch 2000
    /// Meeus ch. 7
    /// </summary>
    /// <param name="j">Julian</param>
    /// <returns>DateTime</returns>
    public static DateTime? GetDate_FromJulian_Epoch2000(Double j) => GetDate_FromJulian(j + J2000);
    /// <summary>
    /// Returns date from Julian based on epoch 1970
    /// Meeus ch. 7
    /// </summary>
    /// <param name="j">Julian</param>
    /// <returns>DateTime</returns>
    public static DateTime? GetDate_FromJulian_Epoch1970(Double j) => GetDate_FromJulian(j + J1970);
  }
  /// <summary>
  /// Contains last and next perigee
  /// </summary>
  [Serializable]
  public class Perigee {

    /// <summary>
    /// Initializes an Perigee object.
    /// </summary>
    /// <param name="last"></param>
    /// <param name="next"></param>
    public Perigee(PerigeeApogee last, PerigeeApogee next) {
      this.LastPerigee = last;
      this.NextPerigee = next;
    }

    /// <summary>
    /// Last perigee
    /// </summary>
    public PerigeeApogee LastPerigee { get; }
    /// <summary>
    /// Next perigee
    /// </summary>
    public PerigeeApogee NextPerigee { get; }

    internal void ConvertTo_Local_Time(Double offset) {
      this.LastPerigee.Convert_To_Local_Time(offset);
      this.NextPerigee.Convert_To_Local_Time(offset);
    }

  }
  /// <summary>
  /// Contains last and next apogee
  /// </summary>
  [Serializable]
  public class Apogee {

    /// <summary>
    /// Initializes an Apogee object.
    /// </summary>
    /// <param name="last"></param>
    /// <param name="next"></param>
    public Apogee(PerigeeApogee last, PerigeeApogee next) {
      this.LastApogee = last;
      this.NextApogee = next;
    }

    /// <summary>
    /// Last apogee
    /// </summary>
    public PerigeeApogee LastApogee { get; }
    /// <summary>
    /// Next apogee
    /// </summary>
    public PerigeeApogee NextApogee { get; }

    internal void ConvertTo_Local_Time(Double offset) {
      this.LastApogee.Convert_To_Local_Time(offset);
      this.NextApogee.Convert_To_Local_Time(offset);
    }
  }
  /// <summary>
  /// Astrological Signs
  /// </summary>
  [Serializable]
  public class AstrologicalSigns {
    /// <summary>
    /// Astrological Zodiac Sign
    /// </summary>
    public String MoonName { get; internal set; }
    /// <summary>
    /// Astrological Moon Sign
    /// </summary>
    public String MoonSign { get; internal set; }
    /// <summary>
    /// Astrological Zodiac Sign
    /// </summary>
    public String ZodiacSign { get; internal set; }
  }
  /// <summary>
  /// Additional Solar Time Information
  /// </summary>
  [Serializable]
  public class AdditionalSolarTimes {
    /// <summary>
    /// Create an AdditionalSolarTimes object.
    /// </summary>
    public AdditionalSolarTimes() {
      //Set dates to avoid null errors. If year return 1900 event did not occur.
      this.CivilDawn = new DateTime();
      this.CivilDusk = new DateTime();
      this.NauticalDawn = new DateTime();
      this.NauticalDusk = new DateTime();

    }
    /// <summary>
    /// Returns Civil Dawn Time
    /// </summary>
    public DateTime? CivilDawn { get; internal set; }
    /// <summary>
    /// Returns Civil Dusk Time
    /// </summary>
    public DateTime? CivilDusk { get; internal set; }
    /// <summary>
    /// Returns Nautical Dawn Time
    /// </summary>
    public DateTime? NauticalDawn { get; internal set; }
    /// <summary>
    /// Returns Nautical Dusk Time
    /// </summary>
    public DateTime? NauticalDusk { get; internal set; }
    /// <summary>
    /// Returns Astronomical Dawn Time
    /// </summary>
    public DateTime? AstronomicalDawn { get; internal set; }
    /// <summary>
    /// Returns Astronomical Dusk Time
    /// </summary>
    public DateTime? AstronomicalDusk { get; internal set; }

    /// <summary>
    /// Returns the time when the bottom of the solar disc touches the horizon after sunrise
    /// </summary>
    public DateTime? SunriseBottomDisc { get; internal set; }
    /// <summary>
    /// Returns the time when the bottom of the solar disc touches the horizon before sunset
    /// </summary>
    public DateTime? SunsetBottomDisc { get; internal set; }

    internal void Convert_To_Local_Time(Double offset) {
      FieldInfo[] fields = typeof(AdditionalSolarTimes).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
      foreach (FieldInfo field in fields) {
        if (field.FieldType == typeof(DateTime?)) {
          DateTime? d = (DateTime?)field.GetValue(this);
          if (d.HasValue) {
            if (d > new DateTime()) {
              d = d.Value.AddHours(offset);
              field.SetValue(this, d);
            }
          }
        }
      }
    }
  }
  /// <summary>
  /// Class containing solar eclipse information
  /// </summary>
  [Serializable]
  public class SolarEclipse {
    /// <summary>
    /// Initialize a SolarEclipse object
    /// </summary>
    public SolarEclipse() {
      this.LastEclipse = new SolarEclipseDetails();
      this.NextEclipse = new SolarEclipseDetails();
    }
    /// <summary>
    /// Details about the previous solar eclipse
    /// </summary>
    public SolarEclipseDetails LastEclipse { get; internal set; }
    /// <summary>
    /// Details about the next solar eclipse
    /// </summary>
    public SolarEclipseDetails NextEclipse { get; internal set; }

    internal void ConvertTo_LocalTime(Double offset) {
      this.LastEclipse.Convert_To_Local_Time(offset);
      this.NextEclipse.Convert_To_Local_Time(offset);
    }
  }
  /// <summary>
  /// Class containing lunar eclipse information
  /// </summary>
  [Serializable]
  public class LunarEclipse {
    /// <summary>
    /// Initialize a LunarEclipse object
    /// </summary>
    public LunarEclipse() {
      this.LastEclipse = new LunarEclipseDetails();
      this.NextEclipse = new LunarEclipseDetails();
    }
    /// <summary>
    /// Details about the previous lunar eclipse
    /// </summary>
    public LunarEclipseDetails LastEclipse { get; internal set; }
    /// <summary>
    /// Details about the next lunar eclipse
    /// </summary>
    public LunarEclipseDetails NextEclipse { get; internal set; }

    internal void ConvertTo_LocalTime(Double offset) {
      this.LastEclipse.Convert_To_Local_Time(offset);
      this.NextEclipse.Convert_To_Local_Time(offset);
    }
  }
  /// <summary>
  /// Class containing specific solar eclipse information
  /// </summary>
  [Serializable]
  public class SolarEclipseDetails {

    /// <summary>
    /// Initialize a SolarEclipseDetails object
    /// </summary>
    /// <param name="values">Solar Eclipse String Values</param>
    public SolarEclipseDetails(List<String> values) {
      //Eclipse has value
      this.HasEclipseData = true;
      //Set Eclipse Date
      this.Date = Convert.ToDateTime(values[0]);

      switch (values[1]) {
        case "P":
          this.Type = SolarEclipseType.Partial;
          break;
        case "A":
          this.Type = SolarEclipseType.Annular;
          break;
        case "T":
          this.Type = SolarEclipseType.Total;
          break;
        default:
          break;
      }
      //Eclipse start
      if (TimeSpan.TryParse(values[2], out TimeSpan ts)) {
        this.PartialEclispeBegin = this.Date.Add(ts);
      }
      //A or T start
      if (TimeSpan.TryParse(values[4], out ts)) {
        this.AorTEclipseBegin = this.Date.Add(ts);
      }
      //Maximum Eclipse
      if (TimeSpan.TryParse(values[5], out ts)) {
        this.MaximumEclipse = this.Date.Add(ts);
      }
      //A or T ends
      if (TimeSpan.TryParse(values[8], out ts)) {
        this.AorTEclipseEnd = this.Date.Add(ts);
      }
      //Eclipse end
      if (TimeSpan.TryParse(values[9], out ts)) {
        this.PartialEclispeEnd = this.Date.Add(ts);
      }
      //A or T Duration
      if (values[13] != "-") {
        String s = values[13].Replace("m", ":").Replace("s", "");
        String[] ns = s.Split(':');
        Int32 secs = 0;

        _ = Int32.TryParse(ns[0], out Int32 mins);
        if (ns.Count() > 0) {
          _ = Int32.TryParse(ns[1], out secs);
        }

        TimeSpan time = new TimeSpan(0, mins, secs);

        this.AorTDuration = time;
      } else {
        this.AorTDuration = new TimeSpan();
      }
      this.Adjust_Dates();//Adjust dates if required (needed when eclipse crosses into next day).
    }
    /// <summary>
    /// Initialize an empty SolarEclipseDetails object
    /// </summary>
    public SolarEclipseDetails() => this.HasEclipseData = false;
    /// <summary>
    /// JS Eclipse Calc formulas didn't account for Z time calculation.
    /// Iterate through and adjust Z dates where eclipse is passed midnight.
    /// </summary>
    private void Adjust_Dates() {
      //Load array in reverse event order
      DateTime[] dateArray = new DateTime[] { this.PartialEclispeBegin, this.AorTEclipseBegin, this.MaximumEclipse, this.AorTEclipseEnd, this.PartialEclispeEnd };
      DateTime baseTime = this.PartialEclispeEnd;
      Boolean multiDay = false; //used to detrmine if eclipse crossed into next Z day

      for (Int32 x = 4; x >= 0; x--) {
        DateTime d = dateArray[x];
        //Check if date exist
        if (d > new DateTime()) {

          //Adjust if time is less than then baseTime.
          if (d > baseTime) {
            switch (x) {
              case 3:
                this.AorTEclipseEnd = this.AorTEclipseEnd.AddDays(-1);
                break;
              case 2:
                this.MaximumEclipse = this.MaximumEclipse.AddDays(-1);
                break;
              case 1:
                this.AorTEclipseBegin = this.AorTEclipseBegin.AddDays(-1);
                break;
              case 0:
                this.PartialEclispeBegin = this.PartialEclispeBegin.AddDays(-1);
                break;
              default:
                break;
            }

            multiDay = true;//Set true to change base date value.
          }
        }
      }
      if (multiDay) {
        this.Date = this.Date.AddDays(-1); //Shave day off base date if multiday.
      }
    }
    /// <summary>
    /// Determine if the SolarEclipseDetails object has been populated
    /// </summary>
    public Boolean HasEclipseData { get; }
    /// <summary>
    /// Date of solar eclipse
    /// </summary>
    public DateTime Date { get; private set; }
    /// <summary>
    /// Solar eclipse type
    /// </summary>
    public SolarEclipseType Type { get; }
    /// <summary>
    /// DateTime when the partial eclipse begins
    /// </summary>
    public DateTime PartialEclispeBegin { get; private set; }
    /// <summary>
    /// DateTime when an Annular or Total eclipse begins (if applicable)
    /// </summary>
    public DateTime AorTEclipseBegin { get; private set; }
    /// <summary>
    /// DateTime when eclipse is at Maximum
    /// </summary>
    public DateTime MaximumEclipse { get; private set; }

    /// <summary>
    /// DateTime when the Annular or Total eclipse ends (if applicable)
    /// </summary>
    public DateTime AorTEclipseEnd { get; private set; }
    /// <summary>
    /// DateTime when the partial elipse ends
    /// </summary>
    public DateTime PartialEclispeEnd { get; }
    /// <summary>
    /// Duration of Annular or Total eclipse (if applicable)
    /// </summary>
    public TimeSpan AorTDuration { get; }
    /// <summary>
    /// Solat eclipse default string
    /// </summary>
    /// <returns>Solar eclipse base date string</returns>
    public override String ToString() => this.Date.ToString("dd-MMM-yyyy");

    internal void Convert_To_Local_Time(Double offset) {
      FieldInfo[] fields = typeof(SolarEclipseDetails).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
      foreach (FieldInfo field in fields) {
        if (field.FieldType == typeof(DateTime)) {
          DateTime d = (DateTime)field.GetValue(this);
          if (d > new DateTime()) {
            d = d.AddHours(offset);
            field.SetValue(this, d);
          }
        }
      }

      this.Date = this.PartialEclispeBegin.Date;
    }
  }
  /// <summary>
  /// Class containing specific lunar eclipse information
  /// </summary>
  [Serializable]
  public class LunarEclipseDetails {

    /// <summary>
    /// Initialize a LunarEclipseDetails object
    /// </summary>
    /// <param name="values">Lunar Eclipse String Values</param>
    public LunarEclipseDetails(List<String> values) {
      //Eclipse has value
      this.HasEclipseData = true;
      //Set Eclipse Date
      this.Date = Convert.ToDateTime(values[0]);
      switch (values[1]) {
        case "T":
          this.Type = LunarEclipseType.Total;
          break;
        case "P":
          this.Type = LunarEclipseType.Partial;
          break;
        case "N":
          this.Type = LunarEclipseType.Penumbral;
          break;
        default:
          break;
      }
      //Penumbral Eclipse start
      if (TimeSpan.TryParse(values[4], out TimeSpan ts)) {
        this.PenumbralEclipseBegin = this.Date.Add(ts);
      }
      //PartialEclipse start
      if (TimeSpan.TryParse(values[6], out ts)) {
        this.PartialEclispeBegin = this.Date.Add(ts);
      }
      //Total start
      if (TimeSpan.TryParse(values[8], out ts)) {
        this.TotalEclipseBegin = this.Date.Add(ts);
      }
      //Mid Eclipse
      if (TimeSpan.TryParse(values[10], out ts)) {
        this.MidEclipse = this.Date.Add(ts);
      }
      //Total ends
      if (TimeSpan.TryParse(values[12], out ts)) {
        this.TotalEclipseEnd = this.Date.Add(ts);
      }
      //Partial Eclipse end
      if (TimeSpan.TryParse(values[14], out ts)) {
        this.PartialEclispeEnd = this.Date.Add(ts);
      }
      //Penumbral Eclipse end
      if (TimeSpan.TryParse(values[16], out ts)) {
        this.PenumbralEclispeEnd = this.Date.Add(ts);
      }
      this.Adjust_Dates();
    }
    /// <summary>
    /// Initialize an empty LunarEclipseDetails object
    /// </summary>
    public LunarEclipseDetails() => this.HasEclipseData = false;
    /// <summary>
    /// JS Eclipse Calc formulas didn't account for Z time calculation.
    /// Iterate through and adjust Z dates where eclipse is passed midnight.
    /// </summary>
    private void Adjust_Dates() {
      //Load array in squential order.
      DateTime[] dateArray = new DateTime[] { this.PenumbralEclipseBegin, this.PartialEclispeBegin, this.TotalEclipseBegin, this.MidEclipse, this.TotalEclipseEnd, this.PartialEclispeEnd, this.PenumbralEclispeEnd };
      Boolean multiDay = false; //used to detrmine if eclipse crossed into next Z day
      DateTime baseTime = this.PenumbralEclipseBegin;
      for (Int32 x = 0; x < dateArray.Count(); x++) {
        DateTime d = dateArray[x];
        //Check if date exist
        if (d > new DateTime()) {
          if (d < baseTime) {
            multiDay = true;
          }
        }
        baseTime = dateArray[x];
        if (multiDay == true) {
          switch (x) {
            case 1:
              this.PartialEclispeBegin = this.PartialEclispeBegin.AddDays(1);
              break;
            case 2:
              this.TotalEclipseBegin = this.TotalEclipseBegin.AddDays(1);
              break;
            case 3:
              this.MidEclipse = this.MidEclipse.AddDays(1);
              break;
            case 4:
              this.TotalEclipseEnd = this.TotalEclipseEnd.AddDays(1);
              break;
            case 5:
              this.PartialEclispeEnd = this.PartialEclispeEnd.AddDays(1);
              break;
            case 6:
              this.PenumbralEclispeEnd = this.PenumbralEclispeEnd.AddDays(1);
              break;
            default:
              break;
          }
        }
      }
    }

    /// <summary>
    /// Determine if the LunarEclipseDetails object has been populated
    /// </summary>
    public Boolean HasEclipseData { get; }
    /// <summary>
    /// Date of lunar eclipse
    /// </summary>
    public DateTime Date { get; private set; }
    /// <summary>
    /// Lunar eclipse type
    /// </summary>
    public LunarEclipseType Type { get; }
    /// <summary>
    /// DateTime when the penumbral eclipse begins
    /// </summary>
    public DateTime PenumbralEclipseBegin { get; }
    /// <summary>
    /// DateTime when the partial eclipse begins (if applicable)
    /// </summary>
    public DateTime PartialEclispeBegin { get; private set; }
    /// <summary>
    /// DateTime when Total eclipse begins (if applicable)
    /// </summary>
    public DateTime TotalEclipseBegin { get; private set; }
    /// <summary>
    /// DateTime when eclipse is at Mid
    /// </summary>
    public DateTime MidEclipse { get; private set; }
    /// <summary>
    /// DateTime when Total eclipse ends (if applicable)
    /// </summary>
    public DateTime TotalEclipseEnd { get; private set; }
    /// <summary>
    /// DateTime when the partial elipse ends (if applicable)
    /// </summary>
    public DateTime PartialEclispeEnd { get; private set; }
    /// <summary>
    /// DateTime when the penumbral elipse ends
    /// </summary>
    public DateTime PenumbralEclispeEnd { get; private set; }
    /// <summary>
    /// Lunar eclipse default string
    /// </summary>
    /// <returns>Lunar eclipse base date string</returns>
    public override String ToString() => this.Date.ToString("dd-MMM-yyyy");

    internal void Convert_To_Local_Time(Double offset) {
      FieldInfo[] fields = typeof(LunarEclipseDetails).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
      foreach (FieldInfo field in fields) {
        if (field.FieldType == typeof(DateTime)) {
          DateTime d = (DateTime)field.GetValue(this);
          if (d > new DateTime()) {
            d = d.AddHours(offset);
            field.SetValue(this, d);
          }
        }
      }
      this.Date = this.PenumbralEclipseBegin.Date;

    }

  }
  internal class MoonTimes {
    public DateTime Set { get; internal set; }
    public DateTime Rise { get; internal set; }
    public CelestialStatus Status { get; internal set; }
  }
  internal class MoonPosition {
    public Double Azimuth { get; internal set; }
    public Double Altitude { get; internal set; }
    public Distance Distance { get; internal set; }
    public Double ParallacticAngle { get; internal set; }
    public Double ParallaxCorection { get; internal set; }
  }
  internal class CelCoords {
    public Double Ra { get; internal set; }
    public Double Dec { get; internal set; }
    public Double Dist { get; internal set; }
  }

  /// <summary>
  /// Solar eclipse type
  /// </summary>
  [Serializable]
  public enum SolarEclipseType {
    /// <summary>
    /// Partial Eclipse
    /// </summary>
    Partial,
    /// <summary>
    /// Annular Eclipse
    /// </summary>
    Annular,
    /// <summary>
    /// Total Eclipse...of the heart...
    /// </summary>
    Total
  }
  /// <summary>
  /// Lunar eclipse type
  /// </summary>
  [Serializable]
  public enum LunarEclipseType {
    /// <summary>
    /// Penumbral Eclipse
    /// </summary>
    Penumbral,
    /// <summary>
    /// Partial Eclipse
    /// </summary>
    Partial,
    /// <summary>
    /// Total Eclipse...of the heart...
    /// </summary>
    Total
  }
}
