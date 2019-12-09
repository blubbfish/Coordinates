using System;
using System.ComponentModel;

namespace CoordinateSharp {
  /// <summary>
  /// Cartesian (X, Y, Z) Coordinate
  /// </summary>
  [Serializable]
  public class Cartesian : INotifyPropertyChanged {
    /// <summary>
    /// Create a Cartesian Object
    /// </summary>
    /// <param name="c"></param>
    public Cartesian(Coordinate c) {
      //formulas:
      this.x = Math.Cos(c.Latitude.ToRadians()) * Math.Cos(c.Longitude.ToRadians());
      this.y = Math.Cos(c.Latitude.ToRadians()) * Math.Sin(c.Longitude.ToRadians());
      this.z = Math.Sin(c.Latitude.ToRadians());
    }
    /// <summary>
    /// Create a Cartesian Object
    /// </summary>
    /// <param name="xc">X</param>
    /// <param name="yc">Y</param>
    /// <param name="zc">Z</param>
    public Cartesian(Double xc, Double yc, Double zc) {
      //formulas:
      this.x = xc;
      this.y = yc;
      this.z = zc;
    }
    /// <summary>
    /// Updates Cartesian Values
    /// </summary>
    /// <param name="c"></param>
    public void ToCartesian(Coordinate c) {
      this.x = Math.Cos(c.Latitude.ToRadians()) * Math.Cos(c.Longitude.ToRadians());
      this.y = Math.Cos(c.Latitude.ToRadians()) * Math.Sin(c.Longitude.ToRadians());
      this.z = Math.Sin(c.Latitude.ToRadians());
    }
    private Double x;
    private Double y;
    private Double z;

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
    /// Returns a Lat Long Coordinate object based on the provided Cartesian Coordinate
    /// </summary>
    /// <param name="x">X</param>
    /// <param name="y">Y</param>
    /// <param name="z">Z</param>
    /// <returns></returns>
    public static Coordinate CartesianToLatLong(Double x, Double y, Double z) {
      Double lon = Math.Atan2(y, x);
      Double hyp = Math.Sqrt(x * x + y * y);
      Double lat = Math.Atan2(z, hyp);

      Double Lat = lat * (180 / Math.PI);
      Double Lon = lon * (180 / Math.PI);
      return new Coordinate(Lat, Lon);
    }
    /// <summary>
    /// Returns a Lat Long Coordinate object based on the provided Cartesian Coordinate
    /// </summary>
    /// <param name="cart">Cartesian Coordinate</param>
    /// <returns></returns>
    public static Coordinate CartesianToLatLong(Cartesian cart) {
      Double x = cart.X;
      Double y = cart.Y;
      Double z = cart.Z;

      Double lon = Math.Atan2(y, x);
      Double hyp = Math.Sqrt(x * x + y * y);
      Double lat = Math.Atan2(z, hyp);

      Double Lat = lat * (180 / Math.PI);
      Double Lon = lon * (180 / Math.PI);
      return new Coordinate(Lat, Lon);
    }
    /// <summary>
    /// Cartesian Default String Format
    /// </summary>
    /// <returns>Cartesian Formatted Coordinate String</returns>
    /// <returns>Values rounded to the 8th place</returns>
    public override String ToString() => Math.Round(this.x, 8).ToString() + " " + Math.Round(this.y, 8).ToString() + " " + Math.Round(this.z, 8).ToString();
    /// <summary>
    /// Property changed event
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;
    /// <summary>
    /// Notify property changed
    /// </summary>
    /// <param name="propName">Property name</param>
    public void NotifyPropertyChanged(String propName) {
      if (this.PropertyChanged != null) {
        PropertyChanged(this, new PropertyChangedEventArgs(propName));
      }
    }
  }
}
