using System;
using System.Collections.Generic;
using System.Linq;

namespace CoordinateSharp {



  /// <summary>
  /// Used for UTM/MGRS Conversions
  /// </summary>
  [Serializable]
  internal class LatZones {
    public static List<String> longZongLetters = new List<String>(new String[]{"C", "D", "E", "F", "G", "H", "J", "K", "L", "M", "N", "P", "Q", "R", "S", "T",
        "U", "V", "W", "X"});
  }
  /// <summary>
  /// Used for handling diagraph determination
  /// </summary>
  [Serializable]
  internal class Digraphs {
    private readonly List<Digraph> digraph1;
    private readonly List<Digraph> digraph2;

    private readonly String[] digraph1Array = { "A", "B", "C", "D", "E", "F", "G", "H", "J", "K", "L", "M", "N", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

    private readonly String[] digraph2Array = { "V", "A", "B", "C", "D", "E", "F", "G", "H", "J", "K", "L", "M", "N", "P", "Q", "R", "S", "T", "U", "V" };

    public Digraphs() {
      this.digraph1 = new List<Digraph>();
      this.digraph2 = new List<Digraph>();

      this.digraph1.Add(new Digraph() { Zone = 1, Letter = "A" });
      this.digraph1.Add(new Digraph() { Zone = 2, Letter = "B" });
      this.digraph1.Add(new Digraph() { Zone = 3, Letter = "C" });
      this.digraph1.Add(new Digraph() { Zone = 4, Letter = "D" });
      this.digraph1.Add(new Digraph() { Zone = 5, Letter = "E" });
      this.digraph1.Add(new Digraph() { Zone = 6, Letter = "F" });
      this.digraph1.Add(new Digraph() { Zone = 7, Letter = "G" });
      this.digraph1.Add(new Digraph() { Zone = 8, Letter = "H" });
      this.digraph1.Add(new Digraph() { Zone = 9, Letter = "J" });
      this.digraph1.Add(new Digraph() { Zone = 10, Letter = "K" });
      this.digraph1.Add(new Digraph() { Zone = 11, Letter = "L" });
      this.digraph1.Add(new Digraph() { Zone = 12, Letter = "M" });
      this.digraph1.Add(new Digraph() { Zone = 13, Letter = "N" });
      this.digraph1.Add(new Digraph() { Zone = 14, Letter = "P" });
      this.digraph1.Add(new Digraph() { Zone = 15, Letter = "Q" });
      this.digraph1.Add(new Digraph() { Zone = 16, Letter = "R" });
      this.digraph1.Add(new Digraph() { Zone = 17, Letter = "S" });
      this.digraph1.Add(new Digraph() { Zone = 18, Letter = "T" });
      this.digraph1.Add(new Digraph() { Zone = 19, Letter = "U" });
      this.digraph1.Add(new Digraph() { Zone = 20, Letter = "V" });
      this.digraph1.Add(new Digraph() { Zone = 21, Letter = "W" });
      this.digraph1.Add(new Digraph() { Zone = 22, Letter = "X" });
      this.digraph1.Add(new Digraph() { Zone = 23, Letter = "Y" });
      this.digraph1.Add(new Digraph() { Zone = 24, Letter = "Z" });
      this.digraph1.Add(new Digraph() { Zone = 1, Letter = "A" });

      this.digraph2.Add(new Digraph() { Zone = 0, Letter = "V" });
      this.digraph2.Add(new Digraph() { Zone = 1, Letter = "A" });
      this.digraph2.Add(new Digraph() { Zone = 2, Letter = "B" });
      this.digraph2.Add(new Digraph() { Zone = 3, Letter = "C" });
      this.digraph2.Add(new Digraph() { Zone = 4, Letter = "D" });
      this.digraph2.Add(new Digraph() { Zone = 5, Letter = "E" });
      this.digraph2.Add(new Digraph() { Zone = 6, Letter = "F" });
      this.digraph2.Add(new Digraph() { Zone = 7, Letter = "G" });
      this.digraph2.Add(new Digraph() { Zone = 8, Letter = "H" });
      this.digraph2.Add(new Digraph() { Zone = 9, Letter = "J" });
      this.digraph2.Add(new Digraph() { Zone = 10, Letter = "K" });
      this.digraph2.Add(new Digraph() { Zone = 11, Letter = "L" });
      this.digraph2.Add(new Digraph() { Zone = 12, Letter = "M" });
      this.digraph2.Add(new Digraph() { Zone = 13, Letter = "N" });
      this.digraph2.Add(new Digraph() { Zone = 14, Letter = "P" });
      this.digraph2.Add(new Digraph() { Zone = 15, Letter = "Q" });
      this.digraph2.Add(new Digraph() { Zone = 16, Letter = "R" });
      this.digraph2.Add(new Digraph() { Zone = 17, Letter = "S" });
      this.digraph2.Add(new Digraph() { Zone = 18, Letter = "T" });
      this.digraph2.Add(new Digraph() { Zone = 19, Letter = "U" });
      this.digraph2.Add(new Digraph() { Zone = 20, Letter = "V" });
    }

    internal Int32 GetDigraph1Index(String letter) {
      for (Int32 i = 0; i < this.digraph1Array.Length; i++) {
        if (this.digraph1Array[i].Equals(letter)) {
          return i + 1;
        }
      }

      return -1;
    }

    internal Int32 GetDigraph2Index(String letter) {
      for (Int32 i = 0; i < this.digraph2Array.Length; i++) {
        if (this.digraph2Array[i].Equals(letter)) {
          return i;
        }
      }

      return -1;
    }

    internal String GetDigraph1(Int32 longZone, Double easting) {
      Int32 a1 = longZone;
      Double a2 = 8 * ((a1 - 1) % 3) + 1;

      Double a3 = easting;
      Double a4 = a2 + (Int32)(a3 / 100000) - 1;
      return this.digraph1.Where(x => x.Zone == Math.Floor(a4)).FirstOrDefault().Letter;
    }

    internal String GetDigraph2(Int32 longZone, Double northing) {
      Int32 a1 = longZone;
      Double a2 = 1 + 5 * ((a1 - 1) % 2);
      Double a3 = northing;
      Double a4 = a2 + (Int32)(a3 / 100000);
      a4 = (a2 + (Int32)(a3 / 100000.0)) % 20;
      a4 = Math.Floor(a4);
      if (a4 < 0) {
        a4 += 19;
      }
      return this.digraph2.Where(x => x.Zone == Math.Floor(a4)).FirstOrDefault().Letter;
    }

  }
  /// <summary>
  /// Diagraph model
  /// </summary>
  [Serializable]
  internal class Digraph {
    public Int32 Zone { get; set; }
    public String Letter { get; set; }
  }
  /// <summary>
  /// Used for setting whether a coordinate part is latitudinal or longitudinal.
  /// </summary>
  [Serializable]
  public enum CoordinateType {
    /// <summary>
    /// Latitude
    /// </summary>
    Lat,
    /// <summary>
    /// Longitude
    /// </summary>
    Long
  }
  /// <summary>
  /// Used to set a coordinate part position.
  /// </summary>
  [Serializable]
  public enum CoordinatesPosition : Int32 {
    /// <summary>
    /// North
    /// </summary>
    N,
    /// <summary>
    /// East
    /// </summary>
    E,
    /// <summary>
    /// South
    /// </summary>
    S,
    /// <summary>
    /// West
    /// </summary>
    W
  }


  /// <summary>
  /// Coordinate type datum specification
  /// </summary>
  [Serializable]
  [Flags]
  public enum Coordinate_Datum {
    /// <summary>
    /// Lat Long GeoDetic
    /// </summary>
    LAT_LONG = 1,
    /// <summary>
    /// UTM and MGRS
    /// </summary>
    UTM_MGRS = 2,
    /// <summary>
    /// ECEF
    /// </summary>
    ECEF = 4,
  }

  /// <summary>
  /// Cartesian Coordinate Type
  /// </summary>
  public enum CartesianType {
    /// <summary>
    /// Spherical Cartesian
    /// </summary>
    Cartesian,
    /// <summary>
    /// Earth Centered Earth Fixed
    /// </summary>
    ECEF,
  }
  /// <summary>
  /// Used for easy read math functions
  /// </summary>
  [Serializable]
  internal static class ModM {
    public static Double Mod(Double x, Double y) => x - y * Math.Floor(x / y);

    public static Double ModLon(Double x) => Mod(x + Math.PI, 2 * Math.PI) - Math.PI;

    public static Double ModCrs(Double x) => Mod(x, 2 * Math.PI);

    public static Double ModLat(Double x) => Mod(x + Math.PI / 2, 2 * Math.PI) - Math.PI / 2;
  }
  /// <summary>
  /// Earth Shape for Calculations.
  /// </summary>

  [Serializable]
  public enum Shape {
    /// <summary>
    /// Calculate as sphere (less accurate, more efficient).
    /// </summary>
    Sphere,
    /// <summary>
    /// Calculate as ellipsoid (more accurate, less efficient).
    /// </summary>
    Ellipsoid
  }


}
