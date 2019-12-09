using System;
using System.ComponentModel;

namespace CoordinateSharp {
  /// <summary>
  /// Earth Centered - Earth Fixed (X,Y,Z) Coordinate 
  /// </summary>
  [Serializable]
  public class ECEF : INotifyPropertyChanged {
    /// <summary>
    /// Create an ECEF Object
    /// </summary>
    /// <param name="c">Coordinate</param>
    public ECEF(Coordinate c) {
      this.equatorial_radius = 6378137.0;
      this.inverse_flattening = 298.257223563;
      this.WGS84();
      this.geodetic_height = new Distance(0);
      Double[] ecef = this.LatLong_To_ECEF(c.Latitude.DecimalDegree, c.Longitude.DecimalDegree, this.geodetic_height.Kilometers);
      this.x = ecef[0];
      this.y = ecef[1];
      this.z = ecef[2];
    }
    /// <summary>
    /// Create an ECEF Object
    /// </summary>
    /// <param name="c">Coordinate</param>
    /// <param name="height">Coordinate</param>
    public ECEF(Coordinate c, Distance height) {
      this.equatorial_radius = 6378137.0;
      this.inverse_flattening = 298.257223563;
      this.WGS84();
      this.geodetic_height = height;
      Double[] ecef = this.LatLong_To_ECEF(c.Latitude.DecimalDegree, c.Longitude.DecimalDegree, this.geodetic_height.Kilometers);
      this.x = ecef[0];
      this.y = ecef[1];
      this.z = ecef[2];
    }
    /// <summary>
    /// Create an ECEF Object
    /// </summary>
    /// <param name="xc">X</param>
    /// <param name="yc">Y</param>
    /// <param name="zc">Z</param>
    public ECEF(Double xc, Double yc, Double zc) {
      this.equatorial_radius = 6378137.0;
      this.inverse_flattening = 298.257223563;
      this.WGS84();
      this.geodetic_height = new Distance(0);
      this.x = xc;
      this.y = yc;
      this.z = zc;
    }
    /// <summary>
    /// Updates ECEF Values
    /// </summary>
    /// <param name="c">Coordinate</param>
    public void ToECEF(Coordinate c) {
      this.equatorial_radius = 6378137.0;
      this.inverse_flattening = 298.257223563;
      this.WGS84();
      Double[] ecef = this.LatLong_To_ECEF(c.Latitude.DecimalDegree, c.Longitude.DecimalDegree, this.geodetic_height.Kilometers);
      this.x = ecef[0];
      this.y = ecef[1];
      this.z = ecef[2];
    }

    //Globals for calucations
    private Double EARTH_A;
    private Double EARTH_B;
    //private Double EARTH_F;
    private Double EARTH_Ecc;
    private Double EARTH_Esq;

    //ECEF Values
    private Double x;
    private Double y;
    private Double z;
    private Distance geodetic_height;

    //Datum
    internal Double equatorial_radius;
    internal Double inverse_flattening;

    /// <summary>
    /// Datum Equatorial Radius / Semi Major Axis
    /// </summary>
    public Double Equatorial_Radius => this.equatorial_radius;

    /// <summary>
    /// Datum Flattening
    /// </summary>
    public Double Inverse_Flattening => this.inverse_flattening;

    /// <summary>
    /// X Coordinate
    /// </summary>
    public Double X {
      get => this.x;
      set {
        if (this.x != value) {
          this.x = value;
          this.NotifyPropertyChanged("X");
        }
      }
    }
    /// <summary>
    /// y Coordinate
    /// </summary>
    public Double Y {
      get => this.y;
      set {
        if (this.y != value) {
          this.y = value;
          this.NotifyPropertyChanged("Y");
        }
      }
    }
    /// <summary>
    /// Z Coordinate
    /// </summary>
    public Double Z {
      get => this.z;
      set {
        if (this.z != value) {
          this.z = value;
          this.NotifyPropertyChanged("Z");
        }
      }
    }

    /// <summary>
    /// GeoDetic Height from Mean Sea Level.
    /// Used for converting Lat Long / ECEF.
    /// Default value is 0. Adjust as needed.
    /// </summary>
    public Distance GeoDetic_Height {
      get => this.geodetic_height;
      internal set {
        if (this.geodetic_height != value) {
          this.geodetic_height = value;
          this.NotifyPropertyChanged("Height");

        }
      }
    }

    /// <summary>
    /// Sets GeoDetic height for ECEF conversion.
    /// Recalculate ECEF Coordinate
    /// </summary>
    /// <param name="c">Coordinate</param>
    /// <param name="dist">Height</param>
    public void Set_GeoDetic_Height(Coordinate c, Distance dist) {
      this.geodetic_height = dist;
      Double[] values = this.LatLong_To_ECEF(c.Latitude.DecimalDegree, c.Longitude.DecimalDegree, dist.Kilometers);
      this.x = values[0];
      this.y = values[1];
      this.z = values[2];

    }

    /// <summary>
    /// Returns a Geodetic Coordinate object based on the provided ECEF Coordinate
    /// </summary>
    /// <param name="x">X</param>
    /// <param name="y">Y</param>
    /// <param name="z">Z</param>
    /// <returns>Coordinate</returns>
    public static Coordinate ECEFToLatLong(Double x, Double y, Double z) {
      ECEF ecef = new ECEF(x, y, z);
      Double[] values = ecef.ECEF_To_LatLong(x, y, z);
      ecef.geodetic_height = new Distance(values[2]);

      Coordinate c = new Coordinate(values[0], values[1]) {
        ECEF = ecef
      };
      return c;
    }
    /// <summary>
    /// Returns a Geodetic Coordinate object based on the provided ECEF Coordinate
    /// </summary>
    /// <param name="ecef">ECEF Coordinate</param>
    /// <returns>Coordinate</returns>
    public static Coordinate ECEFToLatLong(ECEF ecef) {
      Double[] values = ecef.ECEF_To_LatLong(ecef.X, ecef.Y, ecef.Z);

      Coordinate c = new Coordinate(values[0], values[1]);
      //Distance height = new Distance(values[2]);

      ecef.geodetic_height = new Distance(values[2]);
      c.ECEF = ecef;

      return c;
    }
    /// <summary>
    /// ECEF Default String Format
    /// </summary>
    /// <returns>ECEF Formatted Coordinate String</returns>
    /// <returns>Values rounded to the 3rd place</returns>
    public override String ToString() => Math.Round(this.x, 3).ToString() + " km, " + Math.Round(this.y, 3).ToString() + " km, " + Math.Round(this.z, 3).ToString() + " km";

    /// <summary>
    /// Property changed event
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;
    /// <summary>
    /// Notify property changed
    /// </summary>
    /// <param name="propName">Property name</param>
    public void NotifyPropertyChanged(String propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

    //CONVERSION LOGIC      
    /// <summary>
    /// Initialize EARTH global variables based on the Datum
    /// </summary>
    private void WGS84() {
      Double wgs84a = this.equatorial_radius / 1000;
      Double wgs84f = 1.0 / this.inverse_flattening;
      Double wgs84b = wgs84a * (1.0 - wgs84f);

      this.EarthCon(wgs84a, wgs84b);
    }

    /// <summary>
    /// Sets Earth Constants as Globals
    /// </summary>
    /// <param name="a">a</param>
    /// <param name="b">b</param>
    private void EarthCon(Double a, Double b) {
      //Double f = 1 - b / a;
      Double eccsq = 1 - b * b / (a * a);
      Double ecc = Math.Sqrt(eccsq);

      this.EARTH_A = a;
      this.EARTH_B = b;
      //this.EARTH_F = f;
      this.EARTH_Ecc = ecc;
      this.EARTH_Esq = eccsq;
    }

    /// <summary>
    /// Compute the radii at the geodetic latitude (degrees)
    /// </summary>
    /// <param name="lat">Latitude in degres</param>
    /// <returns>double[]</returns>
    private Double[] Radcur(Double lat) {
      Double[] rrnrm = new Double[3];

      Double dtr = Math.PI / 180.0;

      Double a = this.EARTH_A;
      Double b = this.EARTH_B;

      Double asq = a * a;
      Double bsq = b * b;
      Double eccsq = 1 - bsq / asq;
      //Double ecc = Math.Sqrt(eccsq);

      Double clat = Math.Cos(dtr * lat);
      Double slat = Math.Sin(dtr * lat);

      Double dsq = 1.0 - eccsq * slat * slat;
      Double d = Math.Sqrt(dsq);

      Double rn = a / d;
      Double rm = rn * (1.0 - eccsq) / dsq;

      Double rho = rn * clat;
      Double z = (1.0 - eccsq) * rn * slat;
      Double rsq = rho * rho + z * z;
      Double r = Math.Sqrt(rsq);

      rrnrm[0] = r;
      rrnrm[1] = rn;
      rrnrm[2] = rm;

      return rrnrm;

    }

    /// <summary>
    /// Physical radius of the Earth
    /// </summary>
    /// <param name="lat">Latidude in degrees</param>
    /// <returns>double</returns>
    private Double Rearth(Double lat) {
      Double[] rrnrm;
      rrnrm = this.Radcur(lat);
      Double r = rrnrm[0];

      return r;
    }

    /// <summary>
    /// Converts geocentric latitude to geodetic latitude
    /// </summary>
    /// <param name="flatgc">Geocentric latitude</param>
    /// <param name="altkm">Altitude in KM</param>
    /// <returns>double</returns>
    private Double Gc2gd(Double flatgc, Double altkm) {
      Double dtr = Math.PI / 180.0;
      Double rtd = 1 / dtr;

      Double ecc = this.EARTH_Ecc;
      Double esq = ecc * ecc;

      //approximation by stages
      //1st use gc-lat as if is gd, then correct alt dependence

      Double altnow = altkm;

      Double[] rrnrm = this.Radcur(flatgc);
      Double rn = rrnrm[1];

      Double ratio = 1 - esq * rn / (rn + altnow);

      Double tlat = Math.Tan(dtr * flatgc) / ratio;
      Double flatgd = rtd * Math.Atan(tlat);

      //now use this approximation for gd-lat to get rn etc.

      rrnrm = this.Radcur(flatgd);
      rn = rrnrm[1];

      ratio = 1 - esq * rn / (rn + altnow);
      tlat = Math.Tan(dtr * flatgc) / ratio;
      flatgd = rtd * Math.Atan(tlat);

      return flatgd;
    }

    /// <summary>
    /// Converts geodetic latitude to geocentric latitude
    /// </summary>
    /// <param name="flatgd">Geodetic latitude tp geocentric latitide</param>
    /// <param name="altkm">Altitude in KM</param>
    /// <returns>double</returns>
    /*private Double gd2gc(Double flatgd, Double altkm) {
      Double dtr = Math.PI / 180.0;
      Double rtd = 1 / dtr;

      Double ecc = this.EARTH_Ecc;
      Double esq = ecc * ecc;

      Double altnow = altkm;

      Double[] rrnrm = this.Radcur(flatgd);
      Double rn = rrnrm[1];

      Double ratio = 1 - esq * rn / (rn + altnow);

      Double tlat = Math.Tan(dtr * flatgd) * ratio;
      Double flatgc = rtd * Math.Atan(tlat);

      return flatgc;
    }*/

    /// <summary>
    /// Converts lat / long to east, north, up vectors
    /// </summary>
    /// <param name="flat">Latitude</param>
    /// <param name="flon">Longitude</param>
    /// <returns>Array[] of double[]</returns>
    /*private Array[] llenu(Double flat, Double flon) {
      Double clat, slat, clon, slon;
      Double[] ee = new Double[3];
      Double[] en = new Double[3];
      Double[] eu = new Double[3];

      Array[] enu = new Array[3];

      Double dtr = Math.PI / 180.0;

      clat = Math.Cos(dtr * flat);
      slat = Math.Sin(dtr * flat);
      clon = Math.Cos(dtr * flon);
      slon = Math.Sin(dtr * flon);

      ee[0] = -slon;
      ee[1] = clon;
      ee[2] = 0.0;

      en[0] = -clon * slat;
      en[1] = -slon * slat;
      en[2] = clat;

      eu[0] = clon * clat;
      eu[1] = slon * clat;
      eu[2] = slat;

      enu[0] = ee;
      enu[1] = en;
      enu[2] = eu;

      return enu;
    }*/

    /// <summary>
    /// Gets ECEF vector in KM
    /// </summary>
    /// <param name="lat">Latitude</param>
    /// <param name="longi">Longitude</param>
    /// <param name="altkm">Altitude in KM</param>
    /// <returns>double[]</returns>
    private Double[] LatLong_To_ECEF(Double lat, Double longi, Double altkm) {
      Double dtr = Math.PI / 180.0;

      Double clat = Math.Cos(dtr * lat);
      Double slat = Math.Sin(dtr * lat);
      Double clon = Math.Cos(dtr * longi);
      Double slon = Math.Sin(dtr * longi);

      Double[] rrnrm = this.Radcur(lat);
      Double rn = rrnrm[1];
      //Double re = rrnrm[0];

      Double ecc = this.EARTH_Ecc;
      Double esq = ecc * ecc;

      Double x = (rn + altkm) * clat * clon;
      Double y = (rn + altkm) * clat * slon;
      Double z = ((1 - esq) * rn + altkm) * slat;

      Double[] xvec = new Double[3];

      xvec[0] = x;
      xvec[1] = y;
      xvec[2] = z;

      return xvec;
    }

    /// <summary>
    /// Converts ECEF X, Y, Z to GeoDetic Lat / Long and Height in KM
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    private Double[] ECEF_To_LatLong(Double x, Double y, Double z) {
      Double dtr = Math.PI / 180.0;

      //_ = new Double[3];
      Double[] llhvec = new Double[3];
      Double slat, tangd, flatn, dlat, clat;
      Double flat;
      Double altkm;

      Double esq = this.EARTH_Esq;

      Double rp = Math.Sqrt(x * x + y * y + z * z);

      Double flatgc = Math.Asin(z / rp) / dtr;
      Double flon;
      Double testval = Math.Abs(x) + Math.Abs(y);
      flon = testval < 1.0e-10 ? 0.0 : Math.Atan2(y, x) / dtr;
      if (flon < 0.0) { flon += 360.0; }

      Double p = Math.Sqrt(x * x + y * y);

      //Pole special case

      if (p < 1.0e-10) {
        flat = 90.0;
        if (z < 0.0) { flat = -90.0; }

        altkm = rp - this.Rearth(flat);
        llhvec[0] = flat;
        llhvec[1] = flon;
        llhvec[2] = altkm;

        return llhvec;
      }

      //first iteration, use flatgc to get altitude 
      //and alt needed to convert gc to gd lat.

      Double rnow = this.Rearth(flatgc);
      altkm = rp - rnow;
      flat = this.Gc2gd(flatgc, altkm);

      Double[] rrnrm = this.Radcur(flat);
      Double rn = rrnrm[1];

      for (Int32 kount = 0; kount < 5; kount++) {
        slat = Math.Sin(dtr * flat);
        tangd = (z + rn * esq * slat) / p;
        flatn = Math.Atan(tangd) / dtr;

        dlat = flatn - flat;
        flat = flatn;
        clat = Math.Cos(dtr * flat);

        rrnrm = this.Radcur(flat);
        rn = rrnrm[1];

        altkm = p / clat - rn;

        if (Math.Abs(dlat) < 1.0e-12) { break; }

      }
      //CONVERTER WORKS IN E LAT ONLY, IF E LAT > 180 LAT IS WEST SO IT MUCST BE CONVERTED TO Decimal

      if (flon > 180) { flon -= 360; }
      llhvec[0] = flat;
      llhvec[1] = flon;
      llhvec[2] = altkm;

      return llhvec;
    }


  }
}
