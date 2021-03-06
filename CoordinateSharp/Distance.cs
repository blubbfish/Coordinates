using System;
using System.Diagnostics;

namespace CoordinateSharp {
  /// <summary>
  /// Contains distance values between two coordinates.
  /// </summary>
  [Serializable]
  public class Distance {

    /// <summary>
    /// Initializes a distance object using Haversine (Spherical Earth).
    /// </summary>
    /// <param name="c1">Coordinate 1</param>
    /// <param name="c2">Coordinate 2</param>
    public Distance(Coordinate c1, Coordinate c2) => this.Haversine(c1, c2);
    /// <summary>
    /// Initializes a distance object using Haversine (Spherical Earth) or Vincenty (Elliptical Earth).
    /// </summary>
    /// <param name="c1">Coordinate 1</param>
    /// <param name="c2">Coordinate 2</param>
    /// <param name="shape">Shape of earth</param>
    public Distance(Coordinate c1, Coordinate c2, Shape shape) {
      if (shape == Shape.Sphere) {
        this.Haversine(c1, c2);
      } else {
        this.Vincenty(c1, c2);
      }
    }
    /// <summary>
    /// Initializes distance object based on distance in KM
    /// </summary>
    /// <param name="km">Kilometers</param>
    public Distance(Double km) {
      this.Kilometers = km;
      this.Meters = km * 1000;
      this.Feet = this.Meters * 3.28084;
      this.Miles = this.Meters * 0.000621371;
      this.NauticalMiles = this.Meters * 0.0005399565;
      this.Bearing = 0;//None specified
    }
    /// <summary>
    /// Initializaes distance object based on specified distance and measurement type
    /// </summary>
    /// <param name="distance">Distance</param>
    /// <param name="type">Measurement type</param>

    public Distance(Double distance, DistanceType type) {
      this.Bearing = 0;
      switch (type) {
        case DistanceType.Feet:
          this.Feet = distance;
          this.Meters = this.Feet * 0.3048;
          this.Kilometers = this.Meters / 1000;
          this.Miles = this.Meters * 0.000621371;
          this.NauticalMiles = this.Meters * 0.0005399565;
          break;
        case DistanceType.Kilometers:
          this.Kilometers = distance;
          this.Meters = this.Kilometers * 1000;
          this.Feet = this.Meters * 3.28084;
          this.Miles = this.Meters * 0.000621371;
          this.NauticalMiles = this.Meters * 0.0005399565;
          break;
        case DistanceType.Meters:
          this.Meters = distance;
          this.Kilometers = this.Meters / 1000;
          this.Feet = this.Meters * 3.28084;
          this.Miles = this.Meters * 0.000621371;
          this.NauticalMiles = this.Meters * 0.0005399565;
          break;
        case DistanceType.Miles:
          this.Miles = distance;
          this.Meters = this.Miles * 1609.344;
          this.Feet = this.Meters * 3.28084;
          this.Kilometers = this.Meters / 1000;
          this.NauticalMiles = this.Meters * 0.0005399565;
          break;
        case DistanceType.NauticalMiles:
          this.NauticalMiles = distance;
          this.Meters = this.NauticalMiles * 1852.001;
          this.Feet = this.Meters * 3.28084;
          this.Kilometers = this.Meters / 1000;
          this.Miles = this.Meters * 0.000621371;
          break;
        default:
          this.Kilometers = distance;
          this.Meters = distance * 1000;
          this.Feet = this.Meters * 3.28084;
          this.Miles = this.Meters * 0.000621371;
          this.NauticalMiles = this.Meters * 0.0005399565;
          break;
      }
    }
    private void Vincenty(Coordinate coord1, Coordinate coord2) {
      Double lat1, lat2, lon1, lon2;
      Double d, crs12;


      lat1 = coord1.Latitude.ToRadians();
      lat2 = coord2.Latitude.ToRadians();
      lon1 = coord1.Longitude.ToRadians() * -1; //REVERSE FOR CALC 2.1.1.1
      lon2 = coord2.Longitude.ToRadians() * -1; //REVERSE FOR CALC 2.1.1.1

      //Ensure datums match between coords
      if (coord1.equatorial_radius != coord2.equatorial_radius || coord1.inverse_flattening != coord2.inverse_flattening) {
        throw new InvalidOperationException("The datum set does not match between Coordinate objects.");
      }
      Double[] ellipse = new Double[] { coord1.equatorial_radius, coord1.inverse_flattening };


      // elliptic code
      Double[] cde = Distance_Assistant.Dist_Ell(lat1, -lon1, lat2, -lon2, ellipse);  // ellipse uses East negative
      crs12 = cde[1] * (180 / Math.PI); //Bearing
      _ = cde[2] * (180 / Math.PI); //Reverse Bearing
      d = cde[0]; //Distance

      this.Bearing = crs12;
      //reverseBearing = crs21;
      this.Meters = d;
      this.Kilometers = d / 1000;
      this.Feet = d * 3.28084;
      this.Miles = d * 0.000621371;
      this.NauticalMiles = d * 0.0005399565;

    }

    private void Haversine(Coordinate coord1, Coordinate coord2) {
      ////RADIANS
      Double lat1 = coord1.Latitude.ToRadians();
      Double long1 = coord1.Longitude.ToRadians();
      Double lat2 = coord2.Latitude.ToRadians();
      Double long2 = coord2.Longitude.ToRadians();

      //Distance Calcs
      Double R = 6371000; //6378137.0;//6371e3; //meters
      Double latRad = coord2.Latitude.ToRadians() - coord1.Latitude.ToRadians();
      Double longRad = coord2.Longitude.ToRadians() - coord1.Longitude.ToRadians();

      Double a = Math.Sin(latRad / 2.0) * Math.Sin(latRad / 2.0) +
          Math.Cos(lat1) * Math.Cos(lat2) * Math.Sin(longRad / 2.0) * Math.Sin(longRad / 2.0);
      Double cl = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
      Double dist = R * cl;

      //Get bearing         
      Double dLong = long2 - long1;
      Double y = Math.Sin(dLong) * Math.Cos(lat2);
      Double x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLong);
      Double brng = Math.Atan2(y, x) * (180 / Math.PI); //Convert bearing back to degrees.

      //if (brng < 0) { brng -= 180; brng = Math.Abs(brng); }
      brng = (brng + 360) % 360; //v2.1.1.1 NORMALIZE HEADING

      this.Kilometers = dist / 1000;
      this.Meters = dist;
      this.Feet = dist * 3.28084;
      this.Miles = dist * 0.000621371;
      this.NauticalMiles = dist * 0.0005399565;
      this.Bearing = brng;
    }
    /// <summary>
    /// Distance in Kilometers
    /// </summary>
    public Double Kilometers { get; private set; }
    /// <summary>
    /// Distance in Statute Miles
    /// </summary>
    public Double Miles { get; private set; }
    /// <summary>
    /// Distance in Nautical Miles
    /// </summary>
    public Double NauticalMiles { get; private set; }
    /// <summary>
    /// Distance in Meters
    /// </summary>
    public Double Meters { get; private set; }
    /// <summary>
    /// Distance in Feet
    /// </summary>
    public Double Feet { get; private set; }
    /// <summary>
    /// Initial Bearing from Coordinate 1 to Coordinate 2
    /// </summary>
    public Double Bearing { get; private set; }
  }
  /// <summary>
  /// Distance measurement type
  /// </summary>
  public enum DistanceType {
    /// <summary>
    /// Distance in Meters
    /// </summary>
    Meters,
    /// <summary>
    /// Distance in Kilometers
    /// </summary>
    Kilometers,
    /// <summary>
    /// Distance in Feet
    /// </summary>
    Feet,
    /// <summary>
    /// Distance in Statute Miles
    /// </summary>
    Miles,
    /// <summary>
    /// Distance in Nautical Miles
    /// </summary>
    NauticalMiles
  }

  [Serializable]
  internal class Distance_Assistant {
    /// <summary>
    /// Returns new geodetic coordinate in radians
    /// </summary>
    /// <param name="glat1">Latitude in Radians</param>
    /// <param name="glon1">Longitude in Radians</param>
    /// <param name="faz">Bearing</param>
    /// <param name="s">Distance</param>
    /// <param name="ellipse">Earth Ellipse Values</param>
    /// <returns>double[]</returns>
    public static Double[] Direct_Ell(Double glat1, Double glon1, Double faz, Double s, Double[] ellipse) {
      glon1 *= -1; //REVERSE LONG FOR CALC 2.1.1.1
      Double EPS = 0.00000000005;//Used to determine if starting at pole.
      Double r, tu, sf, cf, b, cu, su, sa, c2a, x, c, d, y, sy = 0, cy = 0, cz = 0, e = 0;
      Double glat2, glon2, f;

      //Determine if near pole
      if (Math.Abs(Math.Cos(glat1)) < EPS && !(Math.Abs(Math.Sin(faz)) < EPS)) {
        Debug.WriteLine("Warning: Location is at earth's pole. Only N-S courses are meaningful at this location.");
      }


      Double a = ellipse[0];//Equitorial Radius
      f = 1 / ellipse[1];//Flattening
      r = 1 - f;
      tu = r * Math.Tan(glat1);
      sf = Math.Sin(faz);
      cf = Math.Cos(faz);
      b = cf == 0 ? 0.0 : 2.0 * Math.Atan2(tu, cf);
      cu = 1.0 / Math.Sqrt(1 + tu * tu);
      su = tu * cu;
      sa = cu * sf;
      c2a = 1 - sa * sa;
      x = 1.0 + Math.Sqrt(1.0 + c2a * (1.0 / (r * r) - 1.0));
      x = (x - 2.0) / x;
      c = 1.0 - x;
      c = (x * x / 4.0 + 1.0) / c;
      d = (0.375 * x * x - 1.0) * x;
      tu = s / (r * a * c);
      y = tu;
      c = y + 1;
      while (Math.Abs(y - c) > EPS) {
        sy = Math.Sin(y);
        cy = Math.Cos(y);
        cz = Math.Cos(b + y);
        e = 2.0 * cz * cz - 1.0;
        c = y;
        x = e * cy;
        y = e + e - 1.0;
        y = (((sy * sy * 4.0 - 3.0) * y * cz * d / 6.0 + x) *
                d / 4.0 - cz) * sy * d + tu;
      }

      b = cu * cy * cf - su * sy;
      c = r * Math.Sqrt(sa * sa + b * b);
      d = su * cy + cu * sy * cf;

      glat2 = ModM.ModLat(Math.Atan2(d, c));
      c = cu * cy - su * sy * cf;
      x = Math.Atan2(sy * sf, c);
      c = ((-3.0 * c2a + 4.0) * f + 4.0) * c2a * f / 16.0;
      d = ((e * cy * c + cz) * sy * c + y) * sa;
      glon2 = ModM.ModLon(glon1 + x - (1.0 - c) * d * f);  //Adjust for IDL
                                                           //baz = ModM.ModCrs(Math.Atan2(sa, b) + Math.PI);
      return new Double[] { glat2, glon2 };
    }
    /// <summary>
    /// Returns new geodetic coordinate in radians
    /// </summary>
    /// <param name="lat1">Latitude in radians</param>
    /// <param name="lon1">Longitude in radians</param>
    /// <param name="crs12">Bearing</param>
    /// <param name="d12">Distance</param>
    /// <returns>double[]</returns>
    public static Double[] Direct(Double lat1, Double lon1, Double crs12, Double d12) {
      lon1 *= -1; //REVERSE LONG FOR CALC 2.1.1.1
      Double EPS = 0.00000000005;//Used to determine if near pole.
      Double dlon, lat, lon;
      d12 *= 0.0005399565; //convert meter to nm
      d12 /= 180 * 60 / Math.PI;//Convert to Radian
                                //Determine if near pole
      if (Math.Abs(Math.Cos(lat1)) < EPS && !(Math.Abs(Math.Sin(crs12)) < EPS)) {
        Debug.WriteLine("Warning: Location is at earth's pole. Only N-S courses are meaningful at this location.");
      }

      lat = Math.Asin(Math.Sin(lat1) * Math.Cos(d12) +
                    Math.Cos(lat1) * Math.Sin(d12) * Math.Cos(crs12));
      if (Math.Abs(Math.Cos(lat)) < EPS) {
        lon = 0.0; //endpoint a pole
      } else {
        dlon = Math.Atan2(Math.Sin(crs12) * Math.Sin(d12) * Math.Cos(lat1),
                      Math.Cos(d12) - Math.Sin(lat1) * Math.Sin(lat));
        lon = ModM.Mod(lon1 - dlon + Math.PI, 2 * Math.PI) - Math.PI;
      }

      return new Double[] { lat, lon };
    }
    public static Double[] Dist_Ell(Double glat1, Double glon1, Double glat2, Double glon2, Double[] ellipse) {
      Double a = ellipse[0]; //Equitorial Radius
      Double f = 1 / ellipse[1]; //Flattening

      Double r, tu1, tu2, cu1, su1, cu2, s1, b1, f1;
      Double sx = 0, cx = 0, sy = 0, cy = 0, y = 0, c2a = 0, cz = 0, e = 0;
      Double EPS = 0.00000000005;
      Double faz, baz, s;
      Double iter = 1;
      Double MAXITER = 100;
      if (glat1 + glat2 == 0.0 && Math.Abs(glon1 - glon2) == Math.PI) {
        Debug.WriteLine("Warning: Course and distance between antipodal points is undefined");
        glat1 += 0.00001; // allow algorithm to complete
      }
      if (glat1 == glat2 && (glon1 == glon2 || Math.Abs(Math.Abs(glon1 - glon2) - 2 * Math.PI) < EPS)) {
        Debug.WriteLine("Warning: Points 1 and 2 are identical- course undefined");
        //D
        //crs12
        //crs21
        return new Double[] { 0, 0, Math.PI };
      }
      r = 1 - f;
      tu1 = r * Math.Tan(glat1);
      tu2 = r * Math.Tan(glat2);
      cu1 = 1.0 / Math.Sqrt(1.0 + tu1 * tu1);
      su1 = cu1 * tu1;
      cu2 = 1.0 / Math.Sqrt(1.0 + tu2 * tu2);
      s1 = cu1 * cu2;
      b1 = s1 * tu2;
      f1 = b1 * tu1;
      Double x = glon2 - glon1;
      Double d = x + 1;
      Double c;
      while (Math.Abs(d - x) > EPS && iter < MAXITER) {
        iter += 1;
        sx = Math.Sin(x);
        cx = Math.Cos(x);
        tu1 = cu2 * sx;
        tu2 = b1 - su1 * cu2 * cx;
        sy = Math.Sqrt(tu1 * tu1 + tu2 * tu2);
        cy = s1 * cx + f1;
        y = Math.Atan2(sy, cy);
        Double sa = s1 * sx / sy;
        c2a = 1 - sa * sa;
        cz = f1 + f1;
        if (c2a > 0.0) {
          cz = cy - cz / c2a;
        }
        e = cz * cz * 2.0 - 1.0;
        c = ((-3.0 * c2a + 4.0) * f + 4.0) * c2a * f / 16.0;
        d = x;
        x = ((e * cy * c + cz) * sy * c + y) * sa;
        x = (1.0 - c) * x * f + glon2 - glon1;
      }
      faz = ModM.ModCrs(Math.Atan2(tu1, tu2));
      baz = ModM.ModCrs(Math.Atan2(cu1 * sx, b1 * cx - su1 * cu2) + Math.PI);
      x = Math.Sqrt((1 / (r * r) - 1) * c2a + 1);
      x += 1;
      x = (x - 2.0) / x;
      c = 1.0 - x;
      c = (x * x / 4.0 + 1.0) / c;
      d = (0.375 * x * x - 1.0) * x;
      x = e * cy;
      s = ((((sy * sy * 4.0 - 3.0) * (1.0 - e - e) * cz * d / 6.0 - x) * d / 4.0 + cz) * sy * d + y) * c * a * r;

      if (Math.Abs(iter - MAXITER) < EPS) {
        Debug.WriteLine("Warning: Distance algorithm did not converge");
      }

      return new Double[] { s, faz, baz };
    }
  }
}
