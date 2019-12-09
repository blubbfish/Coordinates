/*
CoordinateSharp is a .NET standard library that is intended to ease geographic coordinate 
format conversions and location based celestial calculations.
https://github.com/Tronald/CoordinateSharp

Many celestial formulas in this library are based on Jean Meeus's 
Astronomical Algorithms (2nd Edition). Comments that reference only a chapter
are refering to this work.

MIT License

(c) 2017, Justin Gielski

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.


THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE. 
*/

using System;
using System.ComponentModel;

namespace CoordinateSharp {
  /// <summary>
  /// Observable class for handling all location based information.
  /// This is the main class for CoordinateSharp.
  /// </summary>
  /// <remarks>
  /// All information should be pulled from this class to include celestial information
  /// </remarks>
  [Serializable]
  public class Coordinate : INotifyPropertyChanged {
    /// <summary>
    /// Creates an empty Coordinate.
    /// </summary>
    /// <remarks>
    /// Values will need to be provided to latitude/longitude CoordinateParts manually
    /// </remarks>
    public Coordinate() {
      this.FormatOptions = new CoordinateFormatOptions();
      this.geoDate = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
      this.latitude = new CoordinatePart(CoordinateType.Lat);
      this.longitude = new CoordinatePart(CoordinateType.Long);
      this.latitude.parent = this;
      this.longitude.parent = this;
      this.CelestialInfo = new Celestial();
      this.UTM = new UniversalTransverseMercator(this.latitude.ToDouble(), this.longitude.ToDouble(), this);
      this.MGRS = new MilitaryGridReferenceSystem(this.UTM);
      this.Cartesian = new Cartesian(this);
      this.ecef = new ECEF(this);

      this.EagerLoadSettings = new EagerLoad();

      this.equatorial_radius = 6378137.0;
      this.inverse_flattening = 298.257223563;
    }
    /// <summary>
    /// Creates an empty Coordinate with custom datum.
    /// </summary>
    /// <remarks>
    /// Values will need to be provided to latitude/longitude CoordinateParts manually
    /// </remarks>
    internal Coordinate(Double equatorialRadius, Double inverseFlattening, Boolean _) {
      this.FormatOptions = new CoordinateFormatOptions();
      this.geoDate = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
      this.latitude = new CoordinatePart(CoordinateType.Lat);
      this.longitude = new CoordinatePart(CoordinateType.Long);
      this.latitude.parent = this;
      this.longitude.parent = this;
      this.CelestialInfo = new Celestial();
      this.UTM = new UniversalTransverseMercator(this.latitude.ToDouble(), this.longitude.ToDouble(), this, equatorialRadius, inverseFlattening);
      this.MGRS = new MilitaryGridReferenceSystem(this.UTM);
      this.Cartesian = new Cartesian(this);
      this.ecef = new ECEF(this);

      this.EagerLoadSettings = new EagerLoad();
      this.Set_Datum(equatorialRadius, inverseFlattening);
    }
    /// <summary>
    /// Creates a populated Coordinate based on decimal (signed degrees) formated latitude and longitude.
    /// </summary>
    /// <param name="lat">latitude</param>
    /// <param name="longi">longitude</param>
    /// <remarks>
    /// Geodate will default to 1/1/1900 GMT until provided
    /// </remarks>
    public Coordinate(Double lat, Double longi) {
      this.FormatOptions = new CoordinateFormatOptions();
      this.geoDate = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
      this.latitude = new CoordinatePart(lat, CoordinateType.Lat);
      this.longitude = new CoordinatePart(longi, CoordinateType.Long);
      this.latitude.parent = this;
      this.longitude.parent = this;
      this.CelestialInfo = new Celestial(lat, longi, this.geoDate);
      this.UTM = new UniversalTransverseMercator(lat, longi, this);
      this.MGRS = new MilitaryGridReferenceSystem(this.UTM);
      this.Cartesian = new Cartesian(this);
      this.ecef = new ECEF(this);
      this.EagerLoadSettings = new EagerLoad();

      this.equatorial_radius = 6378137.0;
      this.inverse_flattening = 298.257223563;
    }
    /// <summary>
    /// Creates a populated Coordinate object with an assigned GeoDate.
    /// </summary>
    /// <param name="lat">latitude</param>
    /// <param name="longi">longitude</param>
    /// <param name="date">DateTime (UTC)</param>
    public Coordinate(Double lat, Double longi, DateTime date) {
      this.FormatOptions = new CoordinateFormatOptions();
      this.latitude = new CoordinatePart(lat, CoordinateType.Lat);
      this.longitude = new CoordinatePart(longi, CoordinateType.Long);
      this.latitude.parent = this;
      this.longitude.parent = this;
      this.CelestialInfo = new Celestial(lat, longi, date);
      this.geoDate = date;
      this.UTM = new UniversalTransverseMercator(lat, longi, this);
      this.MGRS = new MilitaryGridReferenceSystem(this.UTM);
      this.Cartesian = new Cartesian(this);
      this.ecef = new ECEF(this);
      this.EagerLoadSettings = new EagerLoad();

      this.equatorial_radius = 6378137.0;
      this.inverse_flattening = 298.257223563;
    }

    /// <summary>
    /// Creates an empty Coordinates object with specificied eager loading options.
    /// </summary>
    /// <remarks>
    /// Values will need to be provided to latitude/longitude manually
    /// </remarks>
    /// <param name="eagerLoad">Eager loading options</param>
    public Coordinate(EagerLoad eagerLoad) {
      this.FormatOptions = new CoordinateFormatOptions();
      this.geoDate = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
      this.latitude = new CoordinatePart(CoordinateType.Lat);
      this.longitude = new CoordinatePart(CoordinateType.Long);
      this.latitude.parent = this;
      this.longitude.parent = this;

      if (eagerLoad.Cartesian) {
        this.Cartesian = new Cartesian(this);
      }
      if (eagerLoad.Celestial) {
        this.CelestialInfo = new Celestial();
      }
      if (eagerLoad.UTM_MGRS) {
        this.UTM = new UniversalTransverseMercator(this.latitude.ToDouble(), this.longitude.ToDouble(), this);
        this.MGRS = new MilitaryGridReferenceSystem(this.UTM);
      }
      if (eagerLoad.ECEF) {
        this.ecef = new ECEF(this);
      }
      this.EagerLoadSettings = eagerLoad;

      this.equatorial_radius = 6378137.0;
      this.inverse_flattening = 298.257223563;
    }
    /// <summary>
    /// Creates a populated Coordinate object with specified eager loading options.
    /// </summary>
    /// <remarks>
    /// Geodate will default to 1/1/1900 GMT until provided
    /// </remarks>
    /// <param name="lat">latitude</param>
    /// <param name="longi">longitude</param>
    /// <param name="eagerLoad">Eager loading options</param>
    public Coordinate(Double lat, Double longi, EagerLoad eagerLoad) {
      this.FormatOptions = new CoordinateFormatOptions();
      this.geoDate = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
      this.latitude = new CoordinatePart(lat, CoordinateType.Lat);
      this.longitude = new CoordinatePart(longi, CoordinateType.Long);
      this.latitude.parent = this;
      this.longitude.parent = this;

      if (eagerLoad.Celestial) {
        this.CelestialInfo = new Celestial(lat, longi, this.geoDate);
      }
      if (eagerLoad.UTM_MGRS) {
        this.UTM = new UniversalTransverseMercator(lat, longi, this);
        this.MGRS = new MilitaryGridReferenceSystem(this.UTM);
      }
      if (eagerLoad.Cartesian) {
        this.Cartesian = new Cartesian(this);
      }
      if (eagerLoad.ECEF) {
        this.ecef = new ECEF(this);
      }

      this.EagerLoadSettings = eagerLoad;

      this.equatorial_radius = 6378137.0;
      this.inverse_flattening = 298.257223563;
    }
    /// <summary>
    /// Creates a populated Coordinate object with specified eager load options and an assigned GeoDate.
    /// </summary>
    /// <param name="lat">Decimal format latitude</param>
    /// <param name="longi">Decimal format longitude</param>
    /// <param name="date">DateTime you wish to use for celestial calculation</param>
    /// <param name="eagerLoad">Eager loading options</param>
    public Coordinate(Double lat, Double longi, DateTime date, EagerLoad eagerLoad) {
      this.FormatOptions = new CoordinateFormatOptions();
      this.latitude = new CoordinatePart(lat, CoordinateType.Lat);
      this.longitude = new CoordinatePart(longi, CoordinateType.Long);
      this.latitude.parent = this;
      this.longitude.parent = this;
      this.geoDate = date;
      if (eagerLoad.Celestial) {
        this.CelestialInfo = new Celestial(lat, longi, date);
      }

      if (eagerLoad.UTM_MGRS) {
        this.UTM = new UniversalTransverseMercator(lat, longi, this);
        this.MGRS = new MilitaryGridReferenceSystem(this.UTM);
      }
      if (eagerLoad.Cartesian) {
        this.Cartesian = new Cartesian(this);
      }
      if (eagerLoad.ECEF) {
        this.ecef = new ECEF(this);
      }
      this.EagerLoadSettings = eagerLoad;

      this.equatorial_radius = 6378137.0;
      this.inverse_flattening = 298.257223563;
    }

    private CoordinatePart latitude;
    private CoordinatePart longitude;
    private ECEF ecef;
    private DateTime geoDate;
    internal Double equatorial_radius;
    internal Double inverse_flattening;

    /// <summary>
    /// Latitudinal Coordinate Part
    /// </summary>
    public CoordinatePart Latitude {
      get => this.latitude;
      set {
        if (this.latitude != value) {
          if (value.Position == CoordinatesPosition.E || value.Position == CoordinatesPosition.W) { throw new ArgumentException("Invalid Position", "Latitudinal positions cannot be set to East or West."); }
          this.latitude = value;
          this.latitude.parent = this;
          if (this.EagerLoadSettings.Celestial) {
            this.CelestialInfo.CalculateCelestialTime(this.Latitude.DecimalDegree, this.Longitude.DecimalDegree, this.geoDate);
          }
          if (this.longitude != null) {

            if (this.EagerLoadSettings.UTM_MGRS) {
              this.UTM = new UniversalTransverseMercator(this.latitude.ToDouble(), this.longitude.ToDouble(), this, this.UTM.equatorial_radius, this.UTM.inverse_flattening);
              this.MGRS = new MilitaryGridReferenceSystem(this.UTM);
            }
            if (this.EagerLoadSettings.Cartesian) {
              this.Cartesian = new Cartesian(this);
            }
            if (this.EagerLoadSettings.ECEF) {
              this.ecef = new ECEF(this);
            }
          }

        }
      }
    }
    /// <summary>
    /// Longitudinal Coordinate Part
    /// </summary>
    public CoordinatePart Longitude {
      get => this.longitude;
      set {
        if (this.longitude != value) {
          if (value.Position == CoordinatesPosition.N || value.Position == CoordinatesPosition.S) { throw new ArgumentException("Invalid Position", "Longitudinal positions cannot be set to North or South."); }
          this.longitude = value;
          this.longitude.parent = this;
          if (this.EagerLoadSettings.Celestial) {
            this.CelestialInfo.CalculateCelestialTime(this.Latitude.DecimalDegree, this.Longitude.DecimalDegree, this.geoDate);
          }
          if (this.latitude != null) {
            if (this.EagerLoadSettings.UTM_MGRS) {
              this.UTM = new UniversalTransverseMercator(this.latitude.ToDouble(), this.longitude.ToDouble(), this, this.UTM.equatorial_radius, this.UTM.inverse_flattening);
              this.MGRS = new MilitaryGridReferenceSystem(this.UTM);
            }
            if (this.EagerLoadSettings.Cartesian) {
              this.Cartesian = new Cartesian(this);
            }
            if (this.EagerLoadSettings.ECEF) {
              this.ecef = new ECEF(this);
            }
          }

        }
      }
    }
    /// <summary>
    /// Date used to calculate celestial information
    /// </summary>
    /// <remarks>
    /// Assumes all times are in UTC
    /// </remarks>
    public DateTime GeoDate {
      get => this.geoDate;
      set {
        if (this.geoDate != value) {
          this.geoDate = value;
          if (this.EagerLoadSettings.Celestial) {
            this.CelestialInfo.CalculateCelestialTime(this.Latitude.DecimalDegree, this.Longitude.DecimalDegree, this.geoDate);
            this.NotifyPropertyChanged("CelestialInfo");
          }

          this.NotifyPropertyChanged("GeoDate");
        }
      }
    }
    /// <summary>
    /// Universal Transverse Mercator Values
    /// </summary>
    public UniversalTransverseMercator UTM { get; private set; }
    /// <summary>
    /// Military Grid Reference System (NATO UTM)
    /// </summary>
    public MilitaryGridReferenceSystem MGRS { get; private set; }
    /// <summary>
    /// Cartesian (Based on Spherical Earth)
    /// </summary>
    public Cartesian Cartesian { get; private set; }
    /// <summary>
    /// Earth Centered Earth Fixed Coordinate. 
    /// Uses Ellipsoidal height with no geoid model included.
    /// 0 = Mean Sea Level based on the provided Datum.
    /// </summary>
    public ECEF ECEF {
      get => this.ecef;

      //Required due to GeoDetic Height
      internal set {
        if (this.ecef != value) {
          this.ecef = value;
          this.NotifyPropertyChanged("ECEF");
        }
      }
    }

    //PARSER INDICATOR
    private Parse_Format_Type parse_Format = Parse_Format_Type.None;
    /// <summary>
    /// Used to determine what format the coordinate was parsed from.
    /// Will equal "None" if Coordinate was not initialzed via a TryParse() method.
    /// </summary>
    public Parse_Format_Type Parse_Format {
      get => this.parse_Format;
      internal set {
        if (this.parse_Format != value) {
          this.parse_Format = value;
          this.NotifyPropertyChanged("Parse_Format");
        }
      }
    }

    /// <summary>
    /// Celestial information based on the objects location and geographic UTC date.
    /// </summary>
    public Celestial CelestialInfo { get; private set; }

    /// <summary>
    /// Initialize celestial information (required if eager loading is turned off).
    /// </summary>
    public void LoadCelestialInfo() => this.CelestialInfo = Celestial.LoadCelestial(this);
    /// <summary>
    /// Initialize UTM and MGRS information (required if eager loading is turned off).
    /// </summary>
    public void LoadUTM_MGRS_Info() {
      this.UTM = new UniversalTransverseMercator(this.latitude.ToDouble(), this.longitude.ToDouble(), this);
      this.MGRS = new MilitaryGridReferenceSystem(this.UTM);
    }
    /// <summary>
    /// Initialize cartesian information (required if eager loading is turned off).
    /// </summary>
    public void LoadCartesianInfo() => this.Cartesian = new Cartesian(this);
    /// <summary>
    /// Initialize ECEF information (required if eager loading is turned off).
    /// </summary>
    public void LoadECEFInfo() => this.ecef = new ECEF(this);

    /// <summary>
    /// Coordinate string formatting options.
    /// </summary>
    public CoordinateFormatOptions FormatOptions { get; set; }
    /// <summary>
    /// Eager loading settings.
    /// </summary>
    public EagerLoad EagerLoadSettings { get; set; }

    /// <summary>
    /// Bindable formatted coordinate string.
    /// </summary>
    /// <remarks>Bind to this property when MVVM patterns used</remarks>
    public String Display => this.Latitude.Display + " " + this.Longitude.Display;
    /// <summary>
    /// Overridden Coordinate ToString() method.
    /// </summary>
    /// <returns>string (formatted).</returns>
    public override String ToString() {
      String latString = this.latitude.ToString();
      String longSting = this.longitude.ToString();
      return latString + " " + longSting;
    }

    /// <summary>
    /// Overridden Coordinate ToString() method that accepts formatting. 
    /// Refer to documentation for coordinate format options.
    /// </summary>
    /// <param name="options">CoordinateFormatOptions</param>
    /// <returns>Custom formatted coordinate</returns>
    public String ToString(CoordinateFormatOptions options) {
      String latString = this.latitude.ToString(options);
      String longSting = this.longitude.ToString(options);
      return latString + " " + longSting;
    }

    /// <summary>
    /// Set a custom datum for coordinate conversions and distance calculation.
    /// Objects must be loaded prior to setting if EagerLoading is turned off or else the items Datum won't be set.
    /// Use overload if EagerLoading options are used.
    /// </summary>
    /// <param name="radius">Equatorial Radius</param>
    /// <param name="flat">Inverse Flattening</param>
    public void Set_Datum(Double radius, Double flat) {
      //WGS84
      //RADIUS 6378137.0;
      //FLATTENING 298.257223563;
      if (this.UTM != null) {
        this.UTM.inverse_flattening = flat;
        this.UTM.ToUTM(this.Latitude.ToDouble(), this.Longitude.ToDouble(), this.UTM);
        this.MGRS = new MilitaryGridReferenceSystem(this.UTM);
        this.NotifyPropertyChanged("UTM");
        this.NotifyPropertyChanged("MGRS");
      }
      if (this.ecef != null) {
        this.ecef.equatorial_radius = radius;
        this.ecef.inverse_flattening = flat;
        this.ecef.ToECEF(this);
        this.NotifyPropertyChanged("ECEF");
      }
      this.equatorial_radius = radius;
      this.inverse_flattening = flat;
    }

    /// <summary>
    /// Set a custom datum for coordinate conversions and distance calculation for specified coordinate formats only.
    /// Objects must be loaded prior to setting if EagerLoading is turned off.
    /// </summary>
    /// <param name="radius">Equatorial Radius</param>
    /// <param name="flat">Inverse Flattening</param>
    /// <param name="cd">Coordinate_Datum</param>
    public void Set_Datum(Double radius, Double flat, Coordinate_Datum cd) {
      //WGS84
      //RADIUS 6378137.0;
      //FLATTENING 298.257223563;

      if (cd.HasFlag(Coordinate_Datum.UTM_MGRS)) {
        if (this.UTM == null || this.MGRS == null) { throw new NullReferenceException("UTM/MGRS objects must be loaded prior to changing the datum."); }
        this.UTM.inverse_flattening = flat;
        this.UTM.ToUTM(this.Latitude.ToDouble(), this.Longitude.ToDouble(), this.UTM);
        this.MGRS = new MilitaryGridReferenceSystem(this.UTM);
        this.NotifyPropertyChanged("UTM");
        this.NotifyPropertyChanged("MGRS");

      }
      if (cd.HasFlag(Coordinate_Datum.ECEF)) {
        if (this.ECEF == null) { throw new NullReferenceException("ECEF objects must be loaded prior to changing the datum."); }
        this.ecef.equatorial_radius = radius;
        this.ecef.inverse_flattening = flat;
        this.ecef.ToECEF(this);
        this.NotifyPropertyChanged("ECEF");

      }
      if (cd.HasFlag(Coordinate_Datum.LAT_LONG)) {
        this.equatorial_radius = radius;
        this.inverse_flattening = flat;
      }
    }


    /// <summary>
    /// Returns a Distance object based on the current and specified coordinate (Haversine / Spherical Earth).
    /// </summary>
    /// <param name="c2">Coordinate</param>
    /// <returns>Distance</returns>
    public Distance Get_Distance_From_Coordinate(Coordinate c2) => new Distance(this, c2);
    /// <summary>
    /// Returns a Distance object based on the current and specified coordinate and specified earth shape.
    /// </summary>
    /// <param name="c2">Coordinate</param>
    /// <param name="shape">Earth shape</param>
    /// <returns>Distance</returns>
    public Distance Get_Distance_From_Coordinate(Coordinate c2, Shape shape) => new Distance(this, c2, shape);

    /// <summary>
    /// Move coordinate based on provided bearing and distance (in meters).
    /// </summary>
    /// <param name="distance">Distance in meters</param>
    /// <param name="bearing">Bearing</param>
    /// <param name="shape">Shape of earth</param>
    /// <example>
    /// The following example moves a coordinate 10km in the direction of 
    /// the specified bearing using ellipsoidal earth calculations.
    /// <code>
    /// //N 25º 0' 0" E 25º 0' 0"
    /// Coordinate c = Coordinate(25,25);
    /// 
    /// double meters = 10000;
    /// double bearing = 25;
    /// 
    /// //Move coordinate the specified meters
    /// //and direction using ellipsoidal calculations
    /// c.Move(meters, bearing, Shape.Ellipsoid);
    /// 
    /// //New Coordinate - N 25º 4' 54.517" E 24º 57' 29.189"
    /// </code>
    /// </example>
    public void Move(Double distance, Double bearing, Shape shape) {
      //Convert to Radians for formula
      Double lat1 = this.latitude.ToRadians();
      Double lon1 = this.longitude.ToRadians();
      Double crs12 = bearing * Math.PI / 180; //Convert bearing to radians

      Double[] ellipse = new Double[] { this.equatorial_radius, this.inverse_flattening };

      if (shape == Shape.Sphere) {
        Double[] cd = Distance_Assistant.Direct(lat1, lon1, crs12, distance);
        Double lat2 = cd[0] * (180 / Math.PI);
        Double lon2 = cd[1] * (180 / Math.PI);
        //ADJUST CORD
        this.Latitude.DecimalDegree = lat2;
        this.Longitude.DecimalDegree = -lon2;//v2.1.1.1
      } else {
        Double[] cde = Distance_Assistant.Direct_Ell(lat1, -lon1, crs12, distance, ellipse);  // ellipse uses East negative
                                                                                              //Convert back from radians 
        Double lat2 = cde[0] * (180 / Math.PI);
        Double lon2 = cde[1] * (180 / Math.PI); //v2.1.1.1          
                                                //ADJUST CORD
        this.Latitude.DecimalDegree = lat2;
        this.Longitude.DecimalDegree = lon2;
      }
    }
    /// <summary>
    /// Move a coordinate a specified distance (in meters) towards a target coordinate.
    /// </summary>
    /// <param name="target">Target coordinate</param>
    /// <param name="distance">Distance toward target in meters</param>
    /// <param name="shape">Shape of earth</param>
    /// <example>
    /// The following example moves a coordinate 10km towards a target coordinate using
    /// ellipsoidal earth calculations.
    /// <code>
    /// //N 25º 0' 0" E 25º 0' 0"
    /// Coordinate coord = Coordinate(25,25);
    /// 
    /// //Target Coordinate
    /// Coordinate target = new Coordinate(26.5, 23.2);
    /// 
    /// double meters = 10000;
    /// 
    /// //Move coordinate the specified meters
    /// //towards target using ellipsoidal calculations
    /// coord.Move(target, meters, Shape.Ellipsoid);
    /// 
    /// //New Coordinate - N 24º 56' 21.526" E 25º 4' 23.944"
    /// </code>
    /// </example>
    public void Move(Coordinate target, Double distance, Shape shape) {
      Distance d = new Distance(this, target, shape);
      //Convert to Radians for formula
      Double lat1 = this.latitude.ToRadians();
      Double lon1 = this.longitude.ToRadians();
      Double crs12 = d.Bearing * Math.PI / 180; //Convert bearing to radians

      Double[] ellipse = new Double[] { this.equatorial_radius, this.inverse_flattening };

      if (shape == Shape.Sphere) {
        Double[] cd = Distance_Assistant.Direct(lat1, lon1, crs12, distance);
        Double lat2 = cd[0] * (180 / Math.PI);
        Double lon2 = cd[1] * (180 / Math.PI);

        //ADJUST CORD
        this.Latitude.DecimalDegree = lat2;
        this.Longitude.DecimalDegree = -lon2; //v2.1.1.1 update
      } else {
        Double[] cde = Distance_Assistant.Direct_Ell(lat1, -lon1, crs12, distance, ellipse);  // ellipse uses East negative
                                                                                              //Convert back from radians 
        Double lat2 = cde[0] * (180 / Math.PI);
        Double lon2 = cde[1] * (180 / Math.PI); // v2.1.1.1           
                                                //ADJUST CORD
        this.Latitude.DecimalDegree = lat2;
        this.Longitude.DecimalDegree = lon2;
      }
    }
    /// <summary>
    /// Move coordinate based on provided bearing and distance (in meters).
    /// </summary>
    /// <param name="distance">Distance</param>
    /// <param name="bearing">Bearing</param>
    /// <param name="shape">Shape of earth</param>
    /// <example>
    /// The following example moves a coordinate 10km in the direction of 
    /// the specified bearing using ellipsoidal earth calculations.
    /// <code>
    /// //N 25º 0' 0" E 25º 0' 0"
    /// Coordinate c = Coordinate(25,25);
    /// 
    /// Distance distance = new Distance(10, DistanceType.Kilometers);
    /// double bearing = 25;
    /// 
    /// //Move coordinate the specified distance
    /// //and direction using ellipsoidal calculations
    /// c.Move(distance, bearing, Shape.Ellipsoid);
    /// 
    /// //New Coordinate - N 25º 4' 54.517" E 24º 57' 29.189"
    /// </code>
    /// </example>
    public void Move(Distance distance, Double bearing, Shape shape) {
      //Convert to Radians for formula
      Double lat1 = this.latitude.ToRadians();
      Double lon1 = this.longitude.ToRadians();
      Double crs12 = bearing * Math.PI / 180; //Convert bearing to radians

      Double[] ellipse = new Double[] { this.equatorial_radius, this.inverse_flattening };

      if (shape == Shape.Sphere) {
        Double[] cd = Distance_Assistant.Direct(lat1, lon1, crs12, distance.Meters);
        Double lat2 = cd[0] * (180 / Math.PI);
        Double lon2 = cd[1] * (180 / Math.PI);

        //ADJUST CORD
        this.Latitude.DecimalDegree = lat2;
        this.Longitude.DecimalDegree = -lon2; //v2.1.1.1
      } else {
        Double[] cde = Distance_Assistant.Direct_Ell(lat1, -lon1, crs12, distance.Meters, ellipse);  // ellipse uses East negative
                                                                                                     //Convert back from radians 
        Double lat2 = cde[0] * (180 / Math.PI);
        Double lon2 = cde[1] * (180 / Math.PI); //v2.1.1.1         
                                                //ADJUST CORD
        this.Latitude.DecimalDegree = lat2;
        this.Longitude.DecimalDegree = lon2;
      }
    }
    /// <summary>
    /// Move a coordinate a specified distance towards a target coordinate.
    /// </summary>
    /// <param name="target">Target coordinate</param>
    /// <param name="distance">Distance toward target</param>
    /// <param name="shape">Shape of earth</param>
    /// <example>
    /// The following example moves a coordinate 10km towards a target coordinate using
    /// ellipsoidal earth calculations.
    /// <code>
    /// //N 25º 0' 0" E 25º 0' 0"
    /// Coordinate coord = Coordinate(25,25);
    /// 
    /// //Target Coordinate
    /// Coordinate target = new Coordinate(26.5, 23.2);
    /// 
    /// Distance distance = new Distance(10, DistanceType.Kilometers);
    /// 
    /// //Move coordinate the specified distance
    /// //towards target using ellipsoidal calculations
    /// coord.Move(target, distance, Shape.Ellipsoid);
    /// 
    /// //New Coordinate - N 24º 56' 21.526" E 25º 4' 23.944"
    /// </code>
    /// </example>
    public void Move(Coordinate target, Distance distance, Shape shape) {
      Distance d = new Distance(this, target, shape);
      //Convert to Radians for formula
      Double lat1 = this.latitude.ToRadians();
      Double lon1 = this.longitude.ToRadians();
      Double crs12 = d.Bearing * Math.PI / 180; //Convert bearing to radians

      Double[] ellipse = new Double[] { this.equatorial_radius, this.inverse_flattening };

      if (shape == Shape.Sphere) {
        Double[] cd = Distance_Assistant.Direct(lat1, lon1, crs12, distance.Meters);
        Double lat2 = cd[0] * (180 / Math.PI);
        Double lon2 = cd[1] * (180 / Math.PI);

        //ADJUST CORD
        this.Latitude.DecimalDegree = lat2;
        this.Longitude.DecimalDegree = -lon2; //v2.1.1.1 update
      } else {
        Double[] cde = Distance_Assistant.Direct_Ell(lat1, -lon1, crs12, distance.Meters, ellipse);
        //Convert back from radians 
        Double lat2 = cde[0] * (180 / Math.PI);
        Double lon2 = cde[1] * (180 / Math.PI); //v2.1.1.1         
                                                //ADJUST CORD
        this.Latitude.DecimalDegree = lat2;
        this.Longitude.DecimalDegree = lon2;
      }
    }

    /// <summary>
    /// Attempts to parse a string into a Coordinate.
    /// </summary>
    /// <param name="s">Coordinate string</param>
    /// <param name="c">Coordinate</param>
    /// <returns>boolean</returns>
    /// <example>
    /// <code>
    /// Coordinate c;
    /// if(Coordinate.TryParse("N 32.891º W 64.872º",out c))
    /// {
    ///     Console.WriteLine(c); //N 32º 53' 28.212" W 64º 52' 20.914"
    /// }
    /// </code>
    /// </example>
    public static Boolean TryParse(String s, out Coordinate c) {
      if (FormatFinder.TryParse(s, CartesianType.Cartesian, out c)) {
        Parse_Format_Type pft = c.Parse_Format;
        c = new Coordinate(c.Latitude.ToDouble(), c.Longitude.ToDouble()) {
          parse_Format = pft
        }; //Reset with EagerLoad back on.

        return true;
      }
      return false;
    }
    /// <summary>
    /// Attempts to parse a string into a Coordinate with specified DateTime
    /// </summary>
    /// <param name="s">Coordinate string</param>
    /// <param name="geoDate">GeoDate</param>
    /// <param name="c">Coordinate</param>
    /// <returns>boolean</returns>
    /// <example>
    /// <code>
    /// Coordinate c;
    /// if(Coordinate.TryParse("N 32.891º W 64.872º", new DateTime(2018,7,7), out c))
    /// {
    ///     Console.WriteLine(c); //N 32º 53' 28.212" W 64º 52' 20.914"
    /// }
    /// </code>
    /// </example>
    public static Boolean TryParse(String s, DateTime geoDate, out Coordinate c) {
      if (FormatFinder.TryParse(s, CartesianType.Cartesian, out c)) {
        Parse_Format_Type pft = c.Parse_Format;
        c = new Coordinate(c.Latitude.ToDouble(), c.Longitude.ToDouble(), geoDate) {
          parse_Format = pft
        }; //Reset with EagerLoad back on.

        return true;
      }
      return false;
    }
    /// <summary>
    /// Attempts to parse a string into a Coordinate.
    /// </summary>
    /// <param name="s">Coordinate string</param>
    /// <param name="c">Coordinate</param>
    /// <param name="ct">Cartesian Type</param>
    /// <returns>boolean</returns>
    /// <example>
    /// <code>
    /// Coordinate c;
    /// if(Coordinate.TryParse("N 32.891º W 64.872º", CartesianType.Cartesian, out c))
    /// {
    ///     Console.WriteLine(c); //N 32º 53' 28.212" W 64º 52' 20.914"
    /// }
    /// </code>
    /// </example>
    public static Boolean TryParse(String s, CartesianType ct, out Coordinate c) {
      if (FormatFinder.TryParse(s, ct, out c)) {
        Parse_Format_Type pft = c.Parse_Format;
        if (ct == CartesianType.ECEF) {
          Distance h = c.ecef.GeoDetic_Height;
          c = new Coordinate(c.Latitude.ToDouble(), c.Longitude.ToDouble()); //Reset with EagerLoad back on.
          c.ecef.Set_GeoDetic_Height(c, h);
        } else {
          c = new Coordinate(c.Latitude.ToDouble(), c.Longitude.ToDouble()); //Reset with EagerLoad back on.
        }
        c.parse_Format = pft;

        return true;
      }
      return false;
    }
    /// <summary>
    /// Attempts to parse a string into a Coordinate with specified DateTime
    /// </summary>
    /// <param name="s">Coordinate string</param>
    /// <param name="geoDate">GeoDate</param>
    /// <param name="c">Coordinate</param>
    /// <param name="ct">Cartesian Type</param>
    /// <returns>boolean</returns>
    /// <example>
    /// <code>
    /// Coordinate c;
    /// if(Coordinate.TryParse("N 32.891º W 64.872º", new DateTime(2018,7,7), CartesianType.Cartesian, out c))
    /// {
    ///     Console.WriteLine(c); //N 32º 53' 28.212" W 64º 52' 20.914"
    /// }
    /// </code>
    /// </example>
    public static Boolean TryParse(String s, DateTime geoDate, CartesianType ct, out Coordinate c) {
      if (FormatFinder.TryParse(s, ct, out c)) {
        Parse_Format_Type pft = c.Parse_Format;
        if (ct == CartesianType.ECEF) {
          Distance h = c.ecef.GeoDetic_Height;
          c = new Coordinate(c.Latitude.ToDouble(), c.Longitude.ToDouble(), geoDate); //Reset with EagerLoad back on.
          c.ecef.Set_GeoDetic_Height(c, h);
        } else {
          c = new Coordinate(c.Latitude.ToDouble(), c.Longitude.ToDouble(), geoDate); //Reset with EagerLoad back on.
        }
        c.parse_Format = pft;

        return true;
      }
      return false;
    }

    /// <summary>
    /// Property changed event
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;
    /// <summary>
    /// Notify property changed
    /// </summary>
    /// <param name="propName">Property name</param>
    public void NotifyPropertyChanged(String propName) {
      switch (propName) {
        case "CelestialInfo":
          if (!this.EagerLoadSettings.Celestial || this.CelestialInfo == null) { return; } //Prevent Null Exceptions and calls while eagerloading is off
          this.CelestialInfo.CalculateCelestialTime(this.latitude.DecimalDegree, this.longitude.DecimalDegree, this.geoDate);
          break;
        case "UTM":
          if (!this.EagerLoadSettings.UTM_MGRS || this.UTM == null) { return; }
          this.UTM.ToUTM(this.latitude.ToDouble(), this.longitude.ToDouble(), this.UTM);
          break;
        case "utm":
          //Adjust case and notify of change. 
          //Use to notify without calling ToUTM()
          propName = "UTM";
          break;
        case "MGRS":
          if (!this.EagerLoadSettings.UTM_MGRS || this.MGRS == null) { return; }
          this.MGRS.ToMGRS(this.UTM);
          break;
        case "Cartesian":
          if (!this.EagerLoadSettings.Cartesian || this.Cartesian == null) { return; }
          this.Cartesian.ToCartesian(this);
          break;
        case "ECEF":
          if (!this.EagerLoadSettings.ECEF) { return; }
          this.ECEF.ToECEF(this);
          break;
        default:
          break;
      }
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
  }
  /// <summary>
  /// Observable class for handling latitudinal and longitudinal coordinate parts.
  /// </summary>
  /// <remarks>
  /// Objects can be passed to Coordinate object Latitude and Longitude properties.
  /// </remarks>
  [Serializable]
  public class CoordinatePart : INotifyPropertyChanged {
    //Defaults:
    //Format: Degrees Minutes Seconds
    //Rounding: Dependent upon selected format
    //Leading Zeros: False
    //Trailing Zeros: False
    //Display Symbols: True (All Symbols display)
    //Display Hyphens: False
    //Position Display: First                               

    private Double decimalDegree;
    private Double decimalMinute;
    private Int32 degrees;
    private Int32 minutes;
    private Double seconds;
    private CoordinatesPosition position;
    private readonly CoordinateType type;

    internal Coordinate parent;
    /// <summary>
    /// Used to determine and notify the CoordinatePart parent Coordinate object.
    /// </summary>
    public Coordinate Parent => this.parent;

    /// <summary>
    /// Observable decimal format coordinate.
    /// </summary>
    public Double DecimalDegree {
      get => this.decimalDegree;
      set {
        //If changing, notify the needed property changes
        if (this.decimalDegree != value) {
          //Validate the value
          if (this.type == CoordinateType.Lat) {
            if (value > 90) {
              throw new ArgumentOutOfRangeException("Degrees out of range", "Latitude degrees cannot be greater than 90");
            }
            if (value < -90) {
              throw new ArgumentOutOfRangeException("Degrees out of range", "Latitude degrees cannot be less than -90");
            }

          }
          if (this.type == CoordinateType.Long) {
            if (value > 180) {
              throw new ArgumentOutOfRangeException("Degrees out of range", "Longitude degrees cannot be greater than 180");
            }
            if (value < -180) {
              throw new ArgumentOutOfRangeException("Degrees out of range", "Longitude degrees cannot be less than -180");
            }

          }
          this.decimalDegree = value;

          //Update Position
          if ((this.position == CoordinatesPosition.N || this.position == CoordinatesPosition.E) && this.decimalDegree < 0) {
            this.position = this.type == CoordinateType.Lat ? CoordinatesPosition.S : CoordinatesPosition.W;

          }
          if ((this.position == CoordinatesPosition.W || this.position == CoordinatesPosition.S) && this.decimalDegree >= 0) {
            this.position = this.type == CoordinateType.Lat ? CoordinatesPosition.N : CoordinatesPosition.E;

          }
          //Update the Degree & Decimal Minute
          Double degABS = Math.Abs(this.decimalDegree); //Make decimalDegree positive for calculations
          Double degFloor = Math.Truncate(degABS); //Truncate the number leftto extract the degree
          Decimal f = Convert.ToDecimal(degFloor); //Convert to degree to decimal to keep precision during calculations
          Decimal ddm = Convert.ToDecimal(degABS) - f; //Extract decimalMinute value from decimalDegree
          ddm *= 60; //Multiply by 60 to get readable decimalMinute

          Double dm = Convert.ToDouble(ddm); //Convert decimalMinutes back to double for storage
          Int32 df = Convert.ToInt32(degFloor); //Convert degrees to int for storage

          if (this.degrees != df) {
            this.degrees = df;

          }
          if (this.decimalMinute != dm) {
            this.decimalMinute = dm;

          }
          //Update Minutes Seconds              
          Double dmFloor = Math.Floor(dm); //Get number left of decimal to grab minute value
          Int32 mF = Convert.ToInt32(dmFloor); //Convert minute to int for storage
          f = Convert.ToDecimal(dmFloor); //Create a second minute value and store as decimal for precise calculation

          Decimal s = ddm - f; //Get seconds from minutes
          s *= 60; //Multiply by 60 to get readable seconds
          Double secs = Convert.ToDouble(s); //Convert back to double for storage

          if (this.minutes != mF) {
            this.minutes = mF;

          }
          if (this.seconds != secs) {
            this.seconds = secs;
          }
          this.NotifyProperties(PropertyTypes.DecimalDegree);
        }
      }
    }
    /// <summary>
    /// Observable decimal format minute.
    /// </summary>
    public Double DecimalMinute {
      get => this.decimalMinute;
      set {
        if (this.decimalMinute != value) {
          if (value < 0) { value *= -1; }//Adjust accidental negative input
                                         //Validate values     

          Decimal dm = Math.Abs(Convert.ToDecimal(value)) / 60;
          Double decMin = Convert.ToDouble(dm);
          if (this.type == CoordinateType.Lat) {

            if (this.degrees + decMin > 90) { throw new ArgumentOutOfRangeException("Degrees out of range", "Latitudinal degrees cannot be greater than 90"); }
          } else {
            if (this.degrees + decMin > 180) { throw new ArgumentOutOfRangeException("Degrees out of range", "Longitudinal degrees cannot be greater than 180"); }
          }
          if (value >= 60) { throw new ArgumentOutOfRangeException("Minutes out of range", "Coordinate Minutes cannot be greater than or equal to 60"); }
          if (value < 0) { throw new ArgumentOutOfRangeException("Minutes out of range", "Coordinate Minutes cannot be less than 0"); }


          this.decimalMinute = value;


          Decimal decValue = Convert.ToDecimal(value); //Convert value to decimal for precision during calculation
          Decimal dmFloor = Math.Floor(decValue); //Extract minutes
          Decimal secs = decValue - dmFloor; //Extract seconds
          secs *= 60; //Convert seconds to human readable format

          Decimal newDM = decValue / 60; //divide decimalMinute by 60 to get storage value
          Decimal newDD = this.degrees + newDM;//Add new decimal value to the floor degree value to get new decimalDegree;
          if (this.decimalDegree < 0) { newDD *= -1; } //Restore negative if needed

          this.decimalDegree = Convert.ToDouble(newDD);  //Convert back to double for storage                      


          this.minutes = Convert.ToInt32(dmFloor); //Convert minutes to int for storage

          this.seconds = Convert.ToDouble(secs); //Convert seconds to double for storage 
          this.NotifyProperties(PropertyTypes.DecimalMinute);
        }
      }

    }
    /// <summary>
    /// Observable coordinate degree.
    /// </summary>
    public Int32 Degrees {
      get => this.degrees;
      set {
        //Validate Value
        if (this.degrees != value) {

          if (value < 0) { value *= -1; }//Adjust accidental negative input

          if (this.type == CoordinateType.Lat) {
            if (value + this.decimalMinute / 100.0 > 90) {
              throw new ArgumentOutOfRangeException("Degrees", "Latitude degrees cannot be greater than 90");
            }
          }
          if (this.type == CoordinateType.Long) {
            if (value + this.decimalMinute / 100.0 > 180) {
              throw new ArgumentOutOfRangeException("Degrees", "Longitude degrees cannot be greater than 180");
            }

          }

          Decimal f = Convert.ToDecimal(this.degrees);

          this.degrees = value;

          Double degABS = Math.Abs(this.decimalDegree); //Make decimalDegree positive for calculations
          Decimal dDec = Convert.ToDecimal(degABS); //Convert to Decimal for precision during calculations              
                                                    //Convert degrees to decimal to keep precision        
          Decimal dm = dDec - f; //Extract minutes                                      
          Decimal newDD = this.degrees + dm; //Add minutes to new degree for decimalDegree

          if (this.decimalDegree < 0) { newDD *= -1; } //Set negative as required

          this.decimalDegree = Convert.ToDouble(newDD); // Convert decimalDegree to double for storage
          this.NotifyProperties(PropertyTypes.Degree);
        }
      }
    }
    /// <summary>
    /// Observable coordinate minute.
    /// </summary>
    public Int32 Minutes {
      get => this.minutes;
      set {
        if (this.minutes != value) {
          if (value < 0) { value *= -1; }//Adjust accidental negative input
                                         //Validate the minutes
          Decimal vMin = Convert.ToDecimal(value);
          if (this.type == CoordinateType.Lat) {
            if (this.degrees + vMin / 60 > 90) { throw new ArgumentOutOfRangeException("Degrees out of range", "Latitudinal degrees cannot be greater than 90"); }
          } else {
            if (this.degrees + vMin / 60 > 180) { throw new ArgumentOutOfRangeException("Degrees out of range", "Longitudinal degrees cannot be greater than 180"); }
          }
          if (value >= 60) {
            throw new ArgumentOutOfRangeException("Minutes out of range", "Minutes cannot be greater than or equal to 60");
          }
          if (value < 0) {
            throw new ArgumentOutOfRangeException("Minutes out of range", "Minutes cannot be less than 0");
          }
          Decimal minFloor = Convert.ToDecimal(this.minutes);//Convert decimal to minutes for calculation
          Decimal f = Convert.ToDecimal(this.degrees); //Convert to degree to keep precision during calculation 

          this.minutes = value;


          Double degABS = Math.Abs(this.decimalDegree); //Make decimalDegree positive
          Decimal dDec = Convert.ToDecimal(degABS); //Convert to decimalDegree for precision during calucation                        

          Decimal dm = dDec - f; //Extract minutes
          dm *= 60; //Make minutes human readable

          Decimal secs = dm - minFloor;//Extract Seconds

          Decimal newDM = this.minutes + secs;//Add seconds to minutes for decimalMinute
          Double decMin = Convert.ToDouble(newDM); //Convert decimalMinute to double for storage
          this.decimalMinute = decMin; //Round to correct precision


          newDM /= 60; //Convert decimalMinute to storage format
          Decimal newDeg = f + newDM; //Add value to degree for decimalDegree
          if (this.decimalDegree < 0) { newDeg *= -1; }// Set to negative as required.
          this.decimalDegree = Convert.ToDouble(newDeg);//Convert to double and roun to correct precision for storage
          this.NotifyProperties(PropertyTypes.Minute);
        }
      }
    }
    /// <summary>
    /// Observable coordinate second.
    /// </summary>
    public Double Seconds {
      get => this.seconds;
      set {
        if (value < 0) { value *= -1; }//Adjust accidental negative input
        if (this.seconds != value) {
          //Validate Seconds
          Decimal vSec = Convert.ToDecimal(value);
          vSec /= 60;

          Decimal vMin = Convert.ToDecimal(this.minutes);
          vMin += vSec;
          vMin /= 60;

          if (this.type == CoordinateType.Lat) {
            if (this.degrees + vMin > 90) { throw new ArgumentOutOfRangeException("Degrees out of range", "Latitudinal degrees cannot be greater than 90"); }
          } else {
            if (this.degrees + vMin > 180) { throw new ArgumentOutOfRangeException("Degrees out of range", "Longitudinal degrees cannot be greater than 180"); }
          }
          if (value >= 60) {
            throw new ArgumentOutOfRangeException("Seconds out of range", "Seconds cannot be greater than or equal to 60");
          }
          if (value < 0) {
            throw new ArgumentOutOfRangeException("Seconds out of range", "Seconds cannot be less than 0");
          }
          this.seconds = value;


          //Double degABS = Math.Abs(this.decimalDegree); //Make decimalDegree positive
          //Double degFloor = Math.Truncate(degABS); //Truncate the number left of the decimal
          //Decimal f = Convert.ToDecimal(degFloor); //Convert to decimal to keep precision

          Decimal secs = Convert.ToDecimal(this.seconds); //Convert seconds to decimal for calculations
          secs /= 60; //Convert to storage format
          Decimal dm = this.minutes + secs;//Add seconds to minutes for decimalMinute
          Double minFD = Convert.ToDouble(dm); //Convert decimalMinute for storage
          this.decimalMinute = minFD;//Round to proper precision

          Decimal nm = Convert.ToDecimal(this.decimalMinute) / 60;//Convert decimalMinute to decimal and divide by 60 to get storage format decimalMinute
          Double newDeg = this.degrees + Convert.ToDouble(nm);//Convert to double and add to degree for storage decimalDegree
          if (this.decimalDegree < 0) { newDeg *= -1; }//Make negative as needed
          this.decimalDegree = newDeg;//Update decimalDegree and round to proper precision    
          this.NotifyProperties(PropertyTypes.Second);
        }
      }
    }
    /// <summary>
    /// Formate coordinate part string.
    /// </summary>
    public String Display => this.parent != null ? this.ToString(this.parent.FormatOptions) : this.ToString();
    /// <summary>
    /// Observable coordinate position.
    /// </summary>
    public CoordinatesPosition Position {
      get => this.position;
      set {
        if (this.position != value) {
          if (this.type == CoordinateType.Long && (value == CoordinatesPosition.N || value == CoordinatesPosition.S)) {
            throw new InvalidOperationException("You cannot change a Longitudinal type coordinate into a Latitudinal");
          }
          if (this.type == CoordinateType.Lat && (value == CoordinatesPosition.E || value == CoordinatesPosition.W)) {
            throw new InvalidOperationException("You cannot change a Latitudinal type coordinate into a Longitudinal");
          }
          this.decimalDegree *= -1; // Change the position
          this.position = value;
          this.NotifyProperties(PropertyTypes.Position);
        }
      }
    }

    /// <summary>
    /// Creates an empty CoordinatePart.
    /// </summary>
    /// <param name="t">CoordinateType</param>
    /// <param name="c">Parent Coordinate object</param>
    [Obsolete("Method is deprecated. You no longer need to pass a Coordinate object through the constructor.")]
    public CoordinatePart(CoordinateType t, Coordinate c) {
      this.parent = c;
      this.type = t;
      this.decimalDegree = 0;
      this.degrees = 0;
      this.minutes = 0;
      this.seconds = 0;
      this.position = this.type == CoordinateType.Lat ? CoordinatesPosition.N : CoordinatesPosition.E;
    }
    /// <summary>
    /// Creates a populated CoordinatePart from a decimal format part.
    /// </summary>
    /// <param name="value">Coordinate decimal value</param>
    /// <param name="t">Coordinate type</param>
    /// <param name="c">Parent Coordinate object</param>
    [Obsolete("Method is deprecated. You no longer need to pass a Coordinate object through the constructor.")]
    public CoordinatePart(Double value, CoordinateType t, Coordinate c) {
      this.parent = c;
      this.type = t;

      if (this.type == CoordinateType.Long) {
        if (value > 180) { throw new ArgumentOutOfRangeException("Degrees out of range", "Longitudinal coordinate decimal cannot be greater than 180."); }
        if (value < -180) { throw new ArgumentOutOfRangeException("Degrees out of range", "Longitudinal coordinate decimal cannot be less than 180."); }
        this.position = value < 0 ? CoordinatesPosition.W : CoordinatesPosition.E;
      } else {
        if (value > 90) { throw new ArgumentOutOfRangeException("Degrees out of range", "Latitudinal coordinate decimal cannot be greater than 90."); }
        if (value < -90) { throw new ArgumentOutOfRangeException("Degrees out of range", "Latitudinal coordinate decimal cannot be less than 90."); }
        this.position = value < 0 ? CoordinatesPosition.S : CoordinatesPosition.N;
      }
      Decimal dd = Convert.ToDecimal(value);
      dd = Math.Abs(dd);
      Decimal ddFloor = Math.Floor(dd);//DEGREE
      Decimal dm = dd - ddFloor;
      dm *= 60; //DECIMAL MINUTE
      Decimal dmFloor = Math.Floor(dm); //MINUTES
      Decimal sec = dm - dmFloor;
      sec *= 60;//SECONDS


      this.decimalDegree = value;
      this.degrees = Convert.ToInt32(ddFloor);
      this.minutes = Convert.ToInt32(dmFloor);
      this.decimalMinute = Convert.ToDouble(dm);
      this.seconds = Convert.ToDouble(sec);
    }
    /// <summary>
    /// Creates a populated CoordinatePart object from a Degrees Minutes Seconds part.
    /// </summary>
    /// <param name="deg">Degrees</param>
    /// <param name="min">Minutes</param>
    /// <param name="sec">Seconds</param>
    /// <param name="pos">Coordinate Part Position</param>
    /// <param name="c">Parent Coordinate</param>
    [Obsolete("Method is deprecated. You no longer need to pass a Coordinate object through the constructor.")]
    public CoordinatePart(Int32 deg, Int32 min, Double sec, CoordinatesPosition pos, Coordinate c) {
      this.parent = c;
      this.type = pos == CoordinatesPosition.N || pos == CoordinatesPosition.S ? CoordinateType.Lat : CoordinateType.Long;

      if (deg < 0) { throw new ArgumentOutOfRangeException("Degrees out of range", "Degrees cannot be less than 0."); }
      if (min < 0) { throw new ArgumentOutOfRangeException("Minutes out of range", "Minutes cannot be less than 0."); }
      if (sec < 0) { throw new ArgumentOutOfRangeException("Seconds out of range", "Seconds cannot be less than 0."); }
      if (min >= 60) { throw new ArgumentOutOfRangeException("Minutes out of range", "Minutes cannot be greater than or equal to 60."); }
      if (sec >= 60) { throw new ArgumentOutOfRangeException("Seconds out of range", "Seconds cannot be greater than or equal to 60."); }
      this.degrees = deg;
      this.minutes = min;
      this.seconds = sec;
      this.position = pos;

      Decimal secD = Convert.ToDecimal(sec);
      secD /= 60; //Decimal Seconds
      Decimal minD = Convert.ToDecimal(min);
      minD += secD; //Decimal Minutes

      if (this.type == CoordinateType.Long) {
        if (deg + minD / 60 > 180) { throw new ArgumentOutOfRangeException("Degrees out of range", "Longitudinal Degrees cannot be greater than 180."); }
      } else {
        if (deg + minD / 60 > 90) { throw new ArgumentOutOfRangeException("Degrees out of range", "Latitudinal Degrees cannot be greater than 90."); }
      }
      this.decimalMinute = Convert.ToDouble(minD);
      Decimal dd = Convert.ToDecimal(deg) + minD / 60;


      if (pos == CoordinatesPosition.S || pos == CoordinatesPosition.W) {
        dd *= -1;
      }
      this.decimalDegree = Convert.ToDouble(dd);
    }
    /// <summary>
    /// Creates a populated CoordinatePart from a Degrees Minutes Seconds part.
    /// </summary>
    /// <param name="deg">Degrees</param>
    /// <param name="minSec">Decimal Minutes</param> 
    /// <param name="pos">Coordinate Part Position</param>
    /// <param name="c">Parent Coordinate object</param>
    [Obsolete("Method is deprecated. You no longer need to pass a Coordinate object through the constructor.")]
    public CoordinatePart(Int32 deg, Double minSec, CoordinatesPosition pos, Coordinate c) {
      this.parent = c;

      this.type = pos == CoordinatesPosition.N || pos == CoordinatesPosition.S ? CoordinateType.Lat : CoordinateType.Long;

      if (deg < 0) { throw new ArgumentOutOfRangeException("Degree out of range", "Degree cannot be less than 0."); }
      if (minSec < 0) { throw new ArgumentOutOfRangeException("Minutes out of range", "Minutes cannot be less than 0."); }

      if (minSec >= 60) { throw new ArgumentOutOfRangeException("Minutes out of range", "Minutes cannot be greater than or equal to 60."); }

      if (this.type == CoordinateType.Lat) {
        if (deg + minSec / 60 > 90) { throw new ArgumentOutOfRangeException("Degree out of range", "Latitudinal degrees cannot be greater than 90."); }
      } else {
        if (deg + minSec / 60 > 180) { throw new ArgumentOutOfRangeException("Degree out of range", "Longitudinal degrees cannot be greater than 180."); }
      }
      this.degrees = deg;
      this.decimalMinute = minSec;
      this.position = pos;

      Decimal minD = Convert.ToDecimal(minSec);
      Decimal minFloor = Math.Floor(minD);
      this.minutes = Convert.ToInt32(minFloor);
      Decimal sec = minD - minFloor;
      sec *= 60;
      Decimal secD = Convert.ToDecimal(sec);
      this.seconds = Convert.ToDouble(secD);
      Decimal dd = deg + minD / 60;

      if (pos == CoordinatesPosition.S || pos == CoordinatesPosition.W) {
        dd *= -1;
      }
      this.decimalDegree = Convert.ToDouble(dd);
    }

    /// <summary>
    /// Creates an empty CoordinatePart.
    /// </summary>
    /// <param name="t">CoordinateType</param>
    public CoordinatePart(CoordinateType t) {
      this.type = t;
      this.decimalDegree = 0;
      this.degrees = 0;
      this.minutes = 0;
      this.seconds = 0;
      this.position = this.type == CoordinateType.Lat ? CoordinatesPosition.N : CoordinatesPosition.E;
    }
    /// <summary>
    /// Creates a populated CoordinatePart from a decimal format part.
    /// </summary>
    /// <param name="value">Coordinate decimal value</param>
    /// <param name="t">Coordinate type</param>
    public CoordinatePart(Double value, CoordinateType t) {
      this.type = t;

      if (this.type == CoordinateType.Long) {
        if (value > 180) { throw new ArgumentOutOfRangeException("Degrees out of range", "Longitudinal coordinate decimal cannot be greater than 180."); }
        if (value < -180) { throw new ArgumentOutOfRangeException("Degrees out of range", "Longitudinal coordinate decimal cannot be less than 180."); }
        this.position = value < 0 ? CoordinatesPosition.W : CoordinatesPosition.E;
      } else {
        if (value > 90) { throw new ArgumentOutOfRangeException("Degrees out of range", "Latitudinal coordinate decimal cannot be greater than 90."); }
        if (value < -90) { throw new ArgumentOutOfRangeException("Degrees out of range", "Latitudinal coordinate decimal cannot be less than 90."); }
        this.position = value < 0 ? CoordinatesPosition.S : CoordinatesPosition.N;
      }
      Decimal dd = Convert.ToDecimal(value);
      dd = Math.Abs(dd);
      Decimal ddFloor = Math.Floor(dd);//DEGREE
      Decimal dm = dd - ddFloor;
      dm *= 60; //DECIMAL MINUTE
      Decimal dmFloor = Math.Floor(dm); //MINUTES
      Decimal sec = dm - dmFloor;
      sec *= 60;//SECONDS


      this.decimalDegree = value;
      this.degrees = Convert.ToInt32(ddFloor);
      this.minutes = Convert.ToInt32(dmFloor);
      this.decimalMinute = Convert.ToDouble(dm);
      this.seconds = Convert.ToDouble(sec);
    }
    /// <summary>
    /// Creates a populated CoordinatePart object from a Degrees Minutes Seconds part.
    /// </summary>
    /// <param name="deg">Degrees</param>
    /// <param name="min">Minutes</param>
    /// <param name="sec">Seconds</param>
    /// <param name="pos">Coordinate Part Position</param>
    public CoordinatePart(Int32 deg, Int32 min, Double sec, CoordinatesPosition pos) {
      this.type = pos == CoordinatesPosition.N || pos == CoordinatesPosition.S ? CoordinateType.Lat : CoordinateType.Long;

      if (deg < 0) { throw new ArgumentOutOfRangeException("Degrees out of range", "Degrees cannot be less than 0."); }
      if (min < 0) { throw new ArgumentOutOfRangeException("Minutes out of range", "Minutes cannot be less than 0."); }
      if (sec < 0) { throw new ArgumentOutOfRangeException("Seconds out of range", "Seconds cannot be less than 0."); }
      if (min >= 60) { throw new ArgumentOutOfRangeException("Minutes out of range", "Minutes cannot be greater than or equal to 60."); }
      if (sec >= 60) { throw new ArgumentOutOfRangeException("Seconds out of range", "Seconds cannot be greater than or equal to 60."); }
      this.degrees = deg;
      this.minutes = min;
      this.seconds = sec;
      this.position = pos;

      Decimal secD = Convert.ToDecimal(sec);
      secD /= 60; //Decimal Seconds
      Decimal minD = Convert.ToDecimal(min);
      minD += secD; //Decimal Minutes

      if (this.type == CoordinateType.Long) {
        if (deg + minD / 60 > 180) { throw new ArgumentOutOfRangeException("Degrees out of range", "Longitudinal Degrees cannot be greater than 180."); }
      } else {
        if (deg + minD / 60 > 90) { throw new ArgumentOutOfRangeException("Degrees out of range", "Latitudinal Degrees cannot be greater than 90."); }
      }
      this.decimalMinute = Convert.ToDouble(minD);
      Decimal dd = Convert.ToDecimal(deg) + minD / 60;


      if (pos == CoordinatesPosition.S || pos == CoordinatesPosition.W) {
        dd *= -1;
      }
      this.decimalDegree = Convert.ToDouble(dd);
    }
    /// <summary>
    /// Creates a populated CoordinatePart from a Degrees Minutes Seconds part.
    /// </summary>
    /// <param name="deg">Degrees</param>
    /// <param name="minSec">Decimal Minutes</param> 
    /// <param name="pos">Coordinate Part Position</param>
    public CoordinatePart(Int32 deg, Double minSec, CoordinatesPosition pos) {
      this.type = pos == CoordinatesPosition.N || pos == CoordinatesPosition.S ? CoordinateType.Lat : CoordinateType.Long;

      if (deg < 0) { throw new ArgumentOutOfRangeException("Degree out of range", "Degree cannot be less than 0."); }
      if (minSec < 0) { throw new ArgumentOutOfRangeException("Minutes out of range", "Minutes cannot be less than 0."); }

      if (minSec >= 60) { throw new ArgumentOutOfRangeException("Minutes out of range", "Minutes cannot be greater than or equal to 60."); }

      if (this.type == CoordinateType.Lat) {
        if (deg + minSec / 60 > 90) { throw new ArgumentOutOfRangeException("Degree out of range", "Latitudinal degrees cannot be greater than 90."); }
      } else {
        if (deg + minSec / 60 > 180) { throw new ArgumentOutOfRangeException("Degree out of range", "Longitudinal degrees cannot be greater than 180."); }
      }
      this.degrees = deg;
      this.decimalMinute = minSec;
      this.position = pos;

      Decimal minD = Convert.ToDecimal(minSec);
      Decimal minFloor = Math.Floor(minD);
      this.minutes = Convert.ToInt32(minFloor);
      Decimal sec = minD - minFloor;
      sec *= 60;
      Decimal secD = Convert.ToDecimal(sec);
      this.seconds = Convert.ToDouble(secD);
      Decimal dd = deg + minD / 60;

      if (pos == CoordinatesPosition.S || pos == CoordinatesPosition.W) {
        dd *= -1;
      }
      this.decimalDegree = Convert.ToDouble(dd);
    }

    /// <summary>
    /// Signed degrees (decimal) format coordinate.
    /// </summary>
    /// <returns>double</returns>
    public Double ToDouble() => this.decimalDegree;

    /// <summary>
    /// Overridden Coordinate ToString() method
    /// </summary>
    /// <returns>Dstring</returns>
    public override String ToString() => this.parent == null ? this.FormatString(new CoordinateFormatOptions()) : this.FormatString(this.Parent.FormatOptions);

    /// <summary>
    /// Formatted CoordinatePart string.
    /// </summary>
    /// <param name="options">CoordinateFormatOptions</param>
    /// <returns>string (formatted)</returns>
    public String ToString(CoordinateFormatOptions options) => this.FormatString(options);
    /// <summary>
    /// String formatting logic
    /// </summary>
    /// <param name="options">CoordinateFormatOptions</param>
    /// <returns>Formatted coordinate part string</returns>
    private String FormatString(CoordinateFormatOptions options) {
      #region Assign Formatting Rules
      ToStringType type = options.Format switch
      {
        CoordinateFormatType.Degree_Minutes_Seconds => ToStringType.Degree_Minute_Second,
        CoordinateFormatType.Degree_Decimal_Minutes => ToStringType.Degree_Decimal_Minute,
        CoordinateFormatType.Decimal_Degree => ToStringType.Decimal_Degree,
        CoordinateFormatType.Decimal => ToStringType.Decimal,
        _ => ToStringType.Degree_Minute_Second,
      };
      Int32? rounding = options.Round;
      Boolean lead = options.Display_Leading_Zeros;
      Boolean trail = options.Display_Trailing_Zeros;
      Boolean symbols = options.Display_Symbols;
      Boolean degreeSymbol = options.Display_Degree_Symbol;
      Boolean minuteSymbol = options.Display_Minute_Symbol;
      Boolean secondsSymbol = options.Display_Seconds_Symbol;
      Boolean hyphen = options.Display_Hyphens;
      Boolean positionFirst = options.Position_First;
      #endregion

      switch (type) {
        case ToStringType.Decimal_Degree:
          if (rounding == null) { rounding = 6; }
          return this.ToDecimalDegreeString(rounding.Value, lead, trail, symbols, degreeSymbol, positionFirst, hyphen);
        case ToStringType.Degree_Decimal_Minute:
          if (rounding == null) { rounding = 3; }
          return this.ToDegreeDecimalMinuteString(rounding.Value, lead, trail, symbols, degreeSymbol, minuteSymbol, hyphen, positionFirst);
        case ToStringType.Degree_Minute_Second:
          if (rounding == null) { rounding = 3; }
          return this.ToDegreeMinuteSecondString(rounding.Value, lead, trail, symbols, degreeSymbol, minuteSymbol, secondsSymbol, hyphen, positionFirst);
        case ToStringType.Decimal:
          if (rounding == null) { rounding = 9; }
          Double dub = this.ToDouble();
          dub = Math.Round(dub, rounding.Value);
          String lt = this.Leading_Trailing_Format(lead, trail, rounding.Value, this.Position);
          return String.Format(lt, dub);
      }

      return String.Empty;
    }
    //DMS Coordinate Format
    private String ToDegreeMinuteSecondString(Int32 rounding, Boolean lead, Boolean trail, Boolean symbols, Boolean degreeSymbol, Boolean minuteSymbol, Boolean secondSymbol, Boolean hyphen, Boolean positionFirst) {

      String leadString = this.Leading_Trailing_Format(lead, false, rounding, this.Position);
      String d = String.Format(leadString, this.Degrees); // Degree String
      String minute = lead ? String.Format("{0:00}", this.Minutes) : this.Minutes.ToString();
      String leadTrail = this.Leading_Trailing_Format(lead, trail, rounding);

      Double sc = Math.Round(this.Seconds, rounding);
      String second = String.Format(leadTrail, sc);
      String hs = " ";
      String ds = "";
      String ms = "";
      String ss = "";
      if (symbols) {
        if (degreeSymbol) { ds = "º"; }
        if (minuteSymbol) { ms = "'"; }
        if (secondSymbol) { ss = "\""; }
      }
      if (hyphen) { hs = "-"; }

      return positionFirst
        ? this.Position.ToString() + hs + d + ds + hs + minute + ms + hs + second + ss
        : d + ds + hs + minute + ms + hs + second + ss + hs + this.Position.ToString();
    }
    //DDM Coordinate Format
    private String ToDegreeDecimalMinuteString(Int32 rounding, Boolean lead, Boolean trail, Boolean symbols, Boolean degreeSymbol, Boolean minuteSymbol, Boolean hyphen, Boolean positionFirst) {
      String leadString = "{0:0";
      if (lead) {
        if (this.Position == CoordinatesPosition.E || this.Position == CoordinatesPosition.W) {
          leadString += "00";
        } else {
          leadString += "0";
        }
      }
      leadString += "}";
      String d = String.Format(leadString, this.Degrees); // Degree String

      String leadTrail = "{0:0";
      if (lead) {
        leadTrail += "0";
      }
      leadTrail += ".";
      if (trail) {
        for (Int32 i = 0; i < rounding; i++) {
          leadTrail += "0";
        }
      } else {
        leadTrail += "#########";
      }
      leadTrail += "}";

      Double ns = this.Seconds / 60;
      Double c = Math.Round(this.Minutes + ns, rounding);
      if (c == 60 && this.Degrees + 1 < 91) { c = 0; d = String.Format(leadString, this.Degrees + 1); }//Adjust for rounded maxed out Seconds. will Convert 42 60.0 to 43
      String ms = String.Format(leadTrail, c);
      String hs = " ";
      String ds = "";
      String ss = "";
      if (symbols) {
        if (degreeSymbol) { ds = "º"; }
        if (minuteSymbol) { ss = "'"; }
      }
      if (hyphen) { hs = "-"; }

      return positionFirst ? this.Position.ToString() + hs + d + ds + hs + ms + ss : d + ds + hs + ms + ss + hs + this.Position.ToString();

    }
    ////DD Coordinate Format
    private String ToDecimalDegreeString(Int32 rounding, Boolean lead, Boolean trail, Boolean symbols, Boolean degreeSymbol, Boolean positionFirst, Boolean hyphen) {
      String degreeS = "";
      String hyph = " ";
      if (degreeSymbol) { degreeS = "º"; }
      if (!symbols) { degreeS = ""; }
      if (hyphen) { hyph = "-"; }

      String leadTrail = "{0:0";
      if (lead) {
        if (this.Position == CoordinatesPosition.E || this.Position == CoordinatesPosition.W) {
          leadTrail += "00";
        } else {
          leadTrail += "0";
        }
      }
      leadTrail += ".";
      if (trail) {
        for (Int32 i = 0; i < rounding; i++) {
          leadTrail += "0";
        }
      } else {
        leadTrail += "#########";
      }
      leadTrail += "}";

      Double result = this.Degrees + Convert.ToDouble(this.Minutes) / 60 + Convert.ToDouble(this.Seconds) / 3600;
      result = Math.Round(result, rounding);
      String d = String.Format(leadTrail, Math.Abs(result));
      return positionFirst ? this.Position.ToString() + hyph + d + degreeS : d + degreeS + hyph + this.Position.ToString();

    }

    private String Leading_Trailing_Format(Boolean isLead, Boolean isTrail, Int32 rounding, CoordinatesPosition? p = null) {
      String leadString = "{0:0";
      if (isLead) {
        if (p != null) {
          if (p.Value == CoordinatesPosition.W || p.Value == CoordinatesPosition.E) {
            leadString += "00";
          }
        } else {
          leadString += "0";
        }
      }

      leadString += ".";
      if (isTrail) {
        for (Int32 i = 0; i < rounding; i++) {
          leadString += "0";
        }
      } else {
        leadString += "#########";
      }

      leadString += "}";
      return leadString;

    }

    //private String FormatError(String argument, String rule) => "'" + argument + "' is not a valid argument for string format rule: " + rule + ".";

    private enum ToStringType {
      Decimal_Degree, Degree_Decimal_Minute, Degree_Minute_Second, Decimal
    }
    /// <summary>
    /// Notify the correct properties and parent properties.
    /// </summary>
    /// <param name="p">Property Type</param>
    private void NotifyProperties(PropertyTypes p) {
      switch (p) {
        case PropertyTypes.DecimalDegree:
          this.NotifyPropertyChanged("DecimalDegree");
          this.NotifyPropertyChanged("DecimalMinute");
          this.NotifyPropertyChanged("Degrees");
          this.NotifyPropertyChanged("Minutes");
          this.NotifyPropertyChanged("Seconds");
          this.NotifyPropertyChanged("Position");
          break;
        case PropertyTypes.DecimalMinute:
          this.NotifyPropertyChanged("DecimalDegree");
          this.NotifyPropertyChanged("DecimalMinute");
          this.NotifyPropertyChanged("Minutes");
          this.NotifyPropertyChanged("Seconds");
          break;
        case PropertyTypes.Degree:
          this.NotifyPropertyChanged("DecimalDegree");
          this.NotifyPropertyChanged("Degree");
          break;
        case PropertyTypes.Minute:
          this.NotifyPropertyChanged("DecimalDegree");
          this.NotifyPropertyChanged("DecimalMinute");
          this.NotifyPropertyChanged("Minutes");
          break;
        case PropertyTypes.Position:
          this.NotifyPropertyChanged("DecimalDegree");
          this.NotifyPropertyChanged("Position");
          break;
        case PropertyTypes.Second:
          this.NotifyPropertyChanged("DecimalDegree");
          this.NotifyPropertyChanged("DecimalMinute");
          this.NotifyPropertyChanged("Seconds");
          break;
        default:
          this.NotifyPropertyChanged("DecimalDegree");
          this.NotifyPropertyChanged("DecimalMinute");
          this.NotifyPropertyChanged("Degrees");
          this.NotifyPropertyChanged("Minutes");
          this.NotifyPropertyChanged("Seconds");
          this.NotifyPropertyChanged("Position");
          break;
      }
      this.NotifyPropertyChanged("Display");

      if (this.Parent != null) {
        this.Parent.NotifyPropertyChanged("Display");
        this.Parent.NotifyPropertyChanged("CelestialInfo");
        this.Parent.NotifyPropertyChanged("UTM");
        this.Parent.NotifyPropertyChanged("MGRS");
        this.Parent.NotifyPropertyChanged("Cartesian");
        this.Parent.NotifyPropertyChanged("ECEF");
      }

    }

    /// <summary>
    /// Property changed event.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;
    /// <summary>
    /// Notify property changed
    /// </summary>
    /// <param name="propName">Property name</param>
    public void NotifyPropertyChanged(String propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

    /// <summary>
    /// Used for notifying the correct properties.
    /// </summary>
    private enum PropertyTypes {
      DecimalDegree, DecimalMinute, Position, Degree, Minute, Second, FormatChange
    }

    /// <summary>
    /// Returns CoordinatePart in radians
    /// </summary>
    /// <returns></returns>
    public Double ToRadians() => this.decimalDegree * Math.PI / 180;
    /// <summary>
    /// Attempts to parse a string into a CoordinatePart.
    /// </summary>
    /// <param name="s">CoordinatePart string</param>
    /// <param name="cp">CoordinatePart</param>
    /// <returns>boolean</returns>
    /// <example>
    /// <code>
    /// CoordinatePart cp;
    /// if(CoordinatePart.TryParse("N 32.891º", out cp))
    /// {
    ///     Console.WriteLine(cp); //N 32º 53' 28.212"
    /// }
    /// </code>
    /// </example>
    public static Boolean TryParse(String s, out CoordinatePart cp) => FormatFinder_CoordPart.TryParse(s, out cp) ? true : false;
    /// <summary>
    /// Attempts to parse a string into a CoordinatePart. 
    /// </summary>
    /// <param name="s">CoordinatePart string</param>
    /// <param name="t">CoordinateType</param>
    /// <param name="cp">CoordinatePart</param>
    /// <returns>boolean</returns>
    /// <example>
    /// <code>
    /// CoordinatePart cp;
    /// if(CoordinatePart.TryParse("-32.891º", CoordinateType.Long, out cp))
    /// {
    ///     Console.WriteLine(cp); //W 32º 53' 27.6"
    /// }
    /// </code>
    /// </example>
    public static Boolean TryParse(String s, CoordinateType t, out CoordinatePart cp) {
      //Comma at beginning parses to long
      //Asterik forces lat
      s = t == CoordinateType.Long ? "," + s : "*" + s;
      return FormatFinder_CoordPart.TryParse(s, out cp) ? true : false;
    }

  }
}