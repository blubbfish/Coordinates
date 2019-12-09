using System;
using System.Collections.Generic;

namespace CoordinateSharp {
  /// <summary>
  /// Geo Fence class. It helps to check if points/coordinates are inside a polygon, 
  /// Next to a polyline, and counting...
  /// </summary>
  public class GeoFence {
    #region Fields
    private readonly List<Point> _points = new List<Point>();
    #endregion

    /// <summary>
    /// Prepare GeoFence with a list of points
    /// </summary>
    /// <param name="points">List of points</param>
    public GeoFence(List<Point> points) => this._points = points;

    /// <summary>
    /// Prepare Geofence with a list of coordinates
    /// </summary>
    /// <param name="coordinates">List of coordinates</param>
    public GeoFence(List<Coordinate> coordinates) {
      foreach (Coordinate c in coordinates) {
        this._points.Add(new Point { Latitude = c.Latitude.ToDouble(), Longitude = c.Longitude.ToDouble() });
      }
    }

    #region Utils
    private Coordinate ClosestPointOnSegment(Point a, Point b, Coordinate p) {
      Point d = new Point {
        Longitude = b.Longitude - a.Longitude,
        Latitude = b.Latitude - a.Latitude,
      };

      Double number = (p.Longitude.ToDouble() - a.Longitude) * d.Longitude + (p.Latitude.ToDouble() - a.Latitude) * d.Latitude;

      if (number <= 0.0) {
        return new Coordinate(a.Latitude, a.Longitude);
      }

      Double denom = d.Longitude * d.Longitude + d.Latitude * d.Latitude;

      return number >= denom ? new Coordinate(b.Latitude, b.Longitude) : new Coordinate(a.Latitude + number / denom * d.Latitude, a.Longitude + number / denom * d.Longitude);
    }
    #endregion

    /// <summary>
    /// The function will return true if the point x,y is inside the polygon, or
    /// false if it is not.  If the point is exactly on the edge of the polygon,
    /// then the function may return true or false.
    /// </summary>
    /// <param name="point">The point to test</param>
    /// <returns>bool</returns>
    public Boolean IsPointInPolygon(Coordinate point) {
      if (point == null) {
        return false;
      }

      Double latitude = point.Latitude.ToDouble();
      Double longitude = point.Longitude.ToDouble();
      Int32 sides = this._points.Count;
      Int32 j = sides - 1;
      Boolean pointStatus = false;
      for (Int32 i = 0; i < sides; i++) {
        if (this._points[i].Latitude < latitude && this._points[j].Latitude >= latitude || this._points[j].Latitude < latitude && this._points[i].Latitude >= latitude) {
          if (this._points[i].Longitude + (latitude - this._points[i].Latitude) / (this._points[j].Latitude - this._points[i].Latitude) * (this._points[j].Longitude - this._points[i].Longitude) < longitude) {
            pointStatus = !pointStatus;
          }
        }
        j = i;
      }
      return pointStatus;
    }

    /// <summary>
    /// The function will return true if the point x,y is next the given range of 
    /// the polyline, or false if it is not.
    /// </summary>
    /// <param name="point">The point to test</param>
    /// <param name="range">The range in meters</param>
    /// <returns>bool</returns>
    public Boolean IsPointInRangeOfLine(Coordinate point, Double range) {
      if (point == null) {
        return false;
      }

      for (Int32 i = 0; i < this._points.Count - 1; i++) {
        Coordinate c = this.ClosestPointOnSegment(this._points[i], this._points[i + 1], point);
        if (c.Get_Distance_From_Coordinate(point).Meters <= range) {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// The function will return true if the point x,y is next the given range of 
    /// the polyline, or false if it is not.
    /// </summary>
    /// <param name="point">The point to test</param>
    /// <param name="range">The range is a distance object</param>
    /// <returns>bool</returns>
    public Boolean IsPointInRangeOfLine(Coordinate point, Distance range) => point == null || range == null ? false : this.IsPointInRangeOfLine(point, range.Meters);

    /// <summary>
    /// This class is a help class to simplify GeoFence calculus
    /// </summary>
    public class Point {

      /// <summary>
      /// Initialize empty point
      /// </summary>
      public Point() {

      }
      /// <summary>
      /// Initialize point with defined Latitude and Longitude
      /// </summary>
      /// <param name="lat">Latitude (signed)</param>
      /// <param name="lng">Longitude (signed)</param>
      public Point(Double lat, Double lng) {
        this.Latitude = lat;
        this.Longitude = lng;
      }
      /// <summary>
      /// The longitude in degrees
      /// </summary>
      public Double Longitude;
      /// <summary>
      /// The latitude in degrees
      /// </summary>
      public Double Latitude;
    }
  }
}