using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace CoordinateSharp {
  /// <summary>
  /// Type of format a Coordinate parsed from. 
  /// </summary>
  [Serializable]
  public enum Parse_Format_Type {
    /// <summary>
    /// Coordinate was not initialized from a parser method.
    /// </summary>
    None,
    /// <summary>
    /// Signed Degree
    /// DD.dddd
    /// </summary>
    Signed_Degree,
    /// <summary>
    /// Decimal Degree
    /// P DD.dddd
    /// </summary>
    Decimal_Degree,
    /// <summary>
    /// Degree Decimal Minute
    /// P DD MM.sss
    /// </summary>
    Degree_Decimal_Minute,
    /// <summary>
    /// Degree Minute Second
    /// P DD MM SS.sss
    /// </summary>
    Degree_Minute_Second,
    /// <summary>
    /// Universal Transverse Mercator
    /// </summary>
    UTM,
    /// <summary>
    /// Military Grid Reference System
    /// </summary>
    MGRS,
    /// <summary>
    /// Spherical Cartesian
    /// </summary>
    Cartesian_Spherical,
    /// <summary>
    /// Earth Centered Earth Fixed
    /// </summary>
    Cartesian_ECEF
  }

  internal class FormatFinder {
    //Add main to Coordinate and tunnel to Format class. Add private methods to format.
    //WHEN PARSING NO EXCPETIONS FOR OUT OF RANGE ARGS WILL BE THROWN
    public static Boolean TryParse(String coordString, CartesianType ct, out Coordinate c) {
      //Turn of eagerload for efficiency
      EagerLoad eg = new EagerLoad {
        Cartesian = false,
        Celestial = false,
        UTM_MGRS = false
      };

      _ = new Coordinate(eg);
      String s = coordString;
      s = s.Trim(); //Trim all spaces before and after string
      //Try Signed Degree
      if (TrySignedDegree(s, out Double[] d)) {
        try {
          c = new Coordinate(d[0], d[1], eg) {
            Parse_Format = Parse_Format_Type.Signed_Degree
          };
          return true;
        } catch {//Parser failed try next method 
        }
      }

      //Try Decimal Degree
      if (TryDecimalDegree(s, out d)) {
        try {
          c = new Coordinate(d[0], d[1], eg) {
            Parse_Format = Parse_Format_Type.Decimal_Degree
          };
          return true;
        } catch {//Parser failed try next method 
        }
      }
      //Try DDM
      if (TryDegreeDecimalMinute(s, out d)) {
        try {
          //0 Lat Degree
          //1 Lat Minute
          //2 Lat Direction (0 = N, 1 = S)
          //3 Long Degree
          //4 Long Minute
          //5 Long Direction (0 = E, 1 = W)
          CoordinatesPosition latP = CoordinatesPosition.N;
          CoordinatesPosition lngP = CoordinatesPosition.E;
          if (d[2] != 0) { latP = CoordinatesPosition.S; }
          if (d[5] != 0) { lngP = CoordinatesPosition.W; }
          CoordinatePart lat = new CoordinatePart((Int32)d[0], d[1], latP);
          CoordinatePart lng = new CoordinatePart((Int32)d[3], d[4], lngP);
          c = new Coordinate(eg) {
            Latitude = lat,
            Longitude = lng,
            Parse_Format = Parse_Format_Type.Degree_Decimal_Minute
          };
          return true;
        } catch {//Parser failed try next method 
        }
      }
      //Try DMS
      if (TryDegreeMinuteSecond(s, out d)) {
        try {
          //0 Lat Degree
          //1 Lat Minute
          //2 Lat Second
          //3 Lat Direction (0 = N, 1 = S)
          //4 Long Degree
          //5 Long Minute
          //6 Long Second
          //7 Long Direction (0 = E, 1 = W)
          CoordinatesPosition latP = CoordinatesPosition.N;
          CoordinatesPosition lngP = CoordinatesPosition.E;
          if (d[3] != 0) { latP = CoordinatesPosition.S; }
          if (d[7] != 0) { lngP = CoordinatesPosition.W; }

          CoordinatePart lat = new CoordinatePart((Int32)d[0], (Int32)d[1], d[2], latP);
          CoordinatePart lng = new CoordinatePart((Int32)d[4], (Int32)d[5], d[6], lngP);
          c = new Coordinate(eg) {
            Latitude = lat,
            Longitude = lng,
            Parse_Format = Parse_Format_Type.Degree_Minute_Second
          };
          return true;
        } catch {//Parser failed try next method 
        }
      }

      //Try MGRS
      if (TryMGRS(s, out String[] um)) {
        try {
          Double zone = Convert.ToDouble(um[0]);
          Double easting = Convert.ToDouble(um[3]);
          Double northing = Convert.ToDouble(um[4]);
          MilitaryGridReferenceSystem mgrs = new MilitaryGridReferenceSystem(um[1], (Int32)zone, um[2], easting, northing);
          c = MilitaryGridReferenceSystem.MGRStoLatLong(mgrs);
          c.Parse_Format = Parse_Format_Type.MGRS;
          return true;
        } catch {//Parser failed try next method 
        }
      }
      //Try UTM
      if (TryUTM(s, out um)) {
        try {
          Double zone = Convert.ToDouble(um[0]);
          Double easting = Convert.ToDouble(um[2]);
          Double northing = Convert.ToDouble(um[3]);
          UniversalTransverseMercator utm = new UniversalTransverseMercator(um[1], (Int32)zone, easting, northing);
          c = UniversalTransverseMercator.ConvertUTMtoLatLong(utm);
          c.Parse_Format = Parse_Format_Type.UTM;
          return true;
        } catch {//Parser failed try next method 
        }
      }
      //Try Cartesian
      if (TryCartesian(s.ToUpper().Replace("KM", "").Replace("X", "").Replace("Y", "").Replace("Z", ""), out d)) {
        if (ct == CartesianType.Cartesian) {
          try {
            Cartesian cart = new Cartesian(d[0], d[1], d[2]);
            c = Cartesian.CartesianToLatLong(cart);
            c.Parse_Format = Parse_Format_Type.Cartesian_Spherical;
            return true;
          } catch {//Parser failed try next method 
          }
        }
        if (ct == CartesianType.ECEF) {
          try {
            ECEF ecef = new ECEF(d[0], d[1], d[2]);
            c = ECEF.ECEFToLatLong(ecef);
            c.Parse_Format = Parse_Format_Type.Cartesian_ECEF;
            return true;
          } catch {//Parser failed try next method 
          }
        }
      }

      c = null;
      return false;
    }
    private static Boolean TrySignedDegree(String s, out Double[] d) {
      d = null;
      if (Regex.Matches(s, @"[a-zA-Z]").Count != 0) { return false; } //Should contain no letters

      String[] sA = SpecialSplit(s, false);
      Double lat;
      Double lng;

      Double degLat;
      Double minLat; //Minutes & MinSeconds
      Double secLat;

      Int32 signLat = 1;

      Double degLng;
      Double minLng; //Minutes & MinSeconds
      Double secLng;

      Int32 signLng = 1;

      switch (sA.Count()) {
        case 2:
          if (!Double.TryParse(sA[0], out lat)) { return false; }
          if (!Double.TryParse(sA[1], out lng)) { return false; }
          d = new Double[] { lat, lng };
          return true;
        case 4:
          if (!Double.TryParse(sA[0], out degLat)) { return false; }
          if (!Double.TryParse(sA[1], out minLat)) { return false; }
          if (!Double.TryParse(sA[2], out degLng)) { return false; }
          if (!Double.TryParse(sA[3], out minLng)) { return false; }

          if (degLat < 0) { signLat = -1; }
          if (degLng < 0) { signLng = -1; }
          if (minLat >= 60 || minLat < 0) { return false; } //Handle in parser as degree will be incorrect.
          if (minLng >= 60 || minLng < 0) { return false; } //Handle in parser as degree will be incorrect.
          lat = (Math.Abs(degLat) + minLat / 60.0) * signLat;
          lng = (Math.Abs(degLng) + minLng / 60.0) * signLng;
          d = new Double[] { lat, lng };
          return true;
        case 6:
          if (!Double.TryParse(sA[0], out degLat)) { return false; }
          if (!Double.TryParse(sA[1], out minLat)) { return false; }
          if (!Double.TryParse(sA[2], out secLat)) { return false; }
          if (!Double.TryParse(sA[3], out degLng)) { return false; }
          if (!Double.TryParse(sA[4], out minLng)) { return false; }
          if (!Double.TryParse(sA[5], out secLng)) { return false; }
          if (degLat < 0) { signLat = -1; }
          if (degLng < 0) { signLng = -1; }
          if (minLat >= 60 || minLat < 0) { return false; } //Handle in parser as degree will be incorrect.
          if (minLng >= 60 || minLng < 0) { return false; } //Handle in parser as degree will be incorrect.
          if (secLat >= 60 || secLat < 0) { return false; } //Handle in parser as degree will be incorrect.
          if (secLng >= 60 || secLng < 0) { return false; } //Handle in parser as degree will be incorrect.
          lat = (Math.Abs(degLat) + minLat / 60.0 + secLat / 3600) * signLat;
          lng = (Math.Abs(degLng) + minLng / 60.0 + secLng / 3600) * signLng;
          d = new Double[] { lat, lng };
          return true;
        default:
          return false;
      }
    }
    private static Boolean TryDecimalDegree(String s, out Double[] d) {
      d = null;
      if (Regex.Matches(s, @"[a-zA-Z]").Count != 2) { return false; } //Should only contain 1 letter.

      String[] sA = SpecialSplit(s, true);
      if (sA.Count() == 2 || sA.Count() == 4) {

        Double latR = 1; //Sets negative if South
        Double lngR = 1; //Sets negative if West

        //Contact get brin directional indicator together with string
        if (sA.Count() == 4) {
          sA[0] += sA[1];
          sA[1] = sA[2] + sA[3];
        }

        //Find Directions
        if (!sA[0].Contains("N") && !sA[0].Contains("n")) {
          if (!sA[0].Contains("S") && !sA[0].Contains("s")) {
            return false;//No Direction Found
          }
          latR = -1;
        }
        if (!sA[1].Contains("E") && !sA[1].Contains("e")) {
          if (!sA[1].Contains("W") && !sA[1].Contains("w")) {
            return false;//No Direction Found
          }
          lngR = -1;
        }

        sA[0] = Regex.Replace(sA[0], "[^0-9.]", "");
        sA[1] = Regex.Replace(sA[1], "[^0-9.]", "");

        if (!Double.TryParse(sA[0], out Double lat)) { return false; }
        if (!Double.TryParse(sA[1], out Double lng)) { return false; }
        lat *= latR;
        lng *= lngR;
        d = new Double[] { lat, lng };
        return true;
      }

      return false;
    }
    private static Boolean TryDegreeDecimalMinute(String s, out Double[] d) {
      d = null;
      if (Regex.Matches(s, @"[a-zA-Z]").Count != 2) { return false; } //Should only contain 1 letter.

      String[] sA = SpecialSplit(s, true);
      if (sA.Count() == 4 || sA.Count() == 6) {

        Double latR = 0; //Sets 1 if South
        Double lngR = 0; //Sets 1 if West

        //Contact get in order to combine directional indicator together with string
        //Should reduce 6 items to 4
        if (sA.Count() == 6) {
          if (Char.IsLetter(sA[0][0])) { sA[0] += sA[1]; sA[1] = sA[2]; } else if (Char.IsLetter(sA[1][0])) { sA[0] += sA[1]; sA[1] = sA[2]; } else if (Char.IsLetter(sA[2][0])) { sA[0] += sA[2]; } else { return false; }

          if (Char.IsLetter(sA[3][0])) { sA[3] += sA[4]; sA[4] = sA[5]; } else if (Char.IsLetter(sA[4][0])) { sA[3] += sA[4]; sA[4] = sA[5]; } else if (Char.IsLetter(sA[5][0])) { sA[3] += sA[5]; } else { return false; }

          //Shift values for below logic
          sA[2] = sA[3];
          sA[3] = sA[4];
        }

        //Find Directions
        if (!sA[0].Contains("N") && !sA[0].Contains("n") && !sA[1].Contains("N") && !sA[1].Contains("n")) {
          if (!sA[0].Contains("S") && !sA[0].Contains("s") && !sA[1].Contains("S") && !sA[1].Contains("s")) {
            return false;//No Direction Found
          }
          latR = 1;
        }
        if (!sA[2].Contains("E") && !sA[2].Contains("e") && !sA[3].Contains("E") && !sA[3].Contains("e")) {
          if (!sA[2].Contains("W") && !sA[2].Contains("w") && !sA[3].Contains("W") && !sA[3].Contains("w")) {
            return false;//No Direction Found
          }
          lngR = 1;
        }

        sA[0] = Regex.Replace(sA[0], "[^0-9.]", "");
        sA[1] = Regex.Replace(sA[1], "[^0-9.]", "");
        sA[2] = Regex.Replace(sA[2], "[^0-9.]", "");
        sA[3] = Regex.Replace(sA[3], "[^0-9.]", "");

        if (!Double.TryParse(sA[0], out Double latD)) { return false; }
        if (!Double.TryParse(sA[1], out Double latMS)) { return false; }
        if (!Double.TryParse(sA[2], out Double lngD)) { return false; }
        if (!Double.TryParse(sA[3], out Double lngMS)) { return false; }

        d = new Double[] { latD, latMS, latR, lngD, lngMS, lngR };
        return true;
      }
      return false;
    }
    private static Boolean TryDegreeMinuteSecond(String s, out Double[] d) {
      d = null;
      if (Regex.Matches(s, @"[a-zA-Z]").Count != 2) { return false; } //Should only contain 1 letter.

      String[] sA = SpecialSplit(s, true);
      if (sA.Count() == 6 || sA.Count() == 8) {

        Double latR = 0; //Sets 1 if South
        Double lngR = 0; //Sets 1 if West

        //Contact get in order to combine directional indicator together with string
        //Should reduce 8 items to 6
        if (sA.Count() == 8) {
          if (Char.IsLetter(sA[0][0])) { sA[0] += sA[1]; sA[1] = sA[2]; sA[2] = sA[3]; } else if (Char.IsLetter(sA[1][0])) { sA[0] += sA[1]; sA[1] = sA[2]; sA[2] = sA[3]; } else if (Char.IsLetter(sA[3][0])) { sA[0] += sA[3]; } else { return false; }

          if (Char.IsLetter(sA[4][0])) { sA[4] += sA[5]; sA[5] = sA[6]; sA[6] = sA[7]; } else if (Char.IsLetter(sA[5][0])) { sA[4] += sA[5]; sA[5] = sA[6]; sA[6] = sA[7]; } else if (Char.IsLetter(sA[7][0])) { sA[4] += sA[7]; } else { return false; }

          //Shift values for below logic
          sA[3] = sA[4];
          sA[4] = sA[5];
          sA[5] = sA[6];
        }

        //Find Directions
        if (!sA[0].Contains("N") && !sA[0].Contains("n") && !sA[2].Contains("N") && !sA[2].Contains("n")) {
          if (!sA[0].Contains("S") && !sA[0].Contains("s") && !sA[2].Contains("S") && !sA[2].Contains("s")) {
            return false;//No Direction Found
          }
          latR = 1;
        }
        if (!sA[3].Contains("E") && !sA[3].Contains("e") && !sA[5].Contains("E") && !sA[5].Contains("e")) {
          if (!sA[3].Contains("W") && !sA[3].Contains("w") && !sA[5].Contains("W") && !sA[5].Contains("w")) {
            return false;//No Direction Found
          }
          lngR = 1;
        }
        sA[0] = Regex.Replace(sA[0], "[^0-9.]", "");
        sA[1] = Regex.Replace(sA[1], "[^0-9.]", "");
        sA[2] = Regex.Replace(sA[2], "[^0-9.]", "");
        sA[3] = Regex.Replace(sA[3], "[^0-9.]", "");
        sA[4] = Regex.Replace(sA[4], "[^0-9.]", "");
        sA[5] = Regex.Replace(sA[5], "[^0-9.]", "");

        if (!Double.TryParse(sA[0], out Double latD)) { return false; }
        if (!Double.TryParse(sA[1], out Double latM)) { return false; }
        if (!Double.TryParse(sA[2], out Double latS)) { return false; }
        if (!Double.TryParse(sA[3], out Double lngD)) { return false; }
        if (!Double.TryParse(sA[4], out Double lngM)) { return false; }
        if (!Double.TryParse(sA[5], out Double lngS)) { return false; }

        d = new Double[] { latD, latM, latS, latR, lngD, lngM, lngS, lngR };
        return true;
      }
      return false;
    }
    private static Boolean TryUTM(String s, out String[] utm) {
      utm = null;
      String[] sA = SpecialSplit(s, false);
      if (sA.Count() == 3 || sA.Count() == 4) {
        String zoneL;

        if (sA.Count() == 4) {

          if (Char.IsLetter(sA[0][0])) { sA[0] += sA[1]; sA[1] = sA[2]; sA[2] = sA[3]; } else if (Char.IsLetter(sA[1][0])) { sA[0] += sA[1]; sA[1] = sA[2]; sA[2] = sA[3]; } else { return false; }
        }
        zoneL = new String(sA[0].Where(Char.IsLetter).ToArray());
        if (zoneL == String.Empty) { return false; }
        sA[0] = Regex.Replace(sA[0], "[^0-9.]", "");

        if (!Double.TryParse(sA[0], out Double zone)) { return false; }
        if (!Double.TryParse(sA[1], out Double easting)) { return false; }
        if (!Double.TryParse(sA[2], out Double northing)) { return false; }

        utm = new String[] { zone.ToString(), zoneL, easting.ToString(), northing.ToString() };
        return true;
      }
      return false;
    }
    private static Boolean TryMGRS(String s, out String[] mgrs) {
      mgrs = null;
      String[] sA = SpecialSplit(s, false);
      if (sA.Count() == 4 || sA.Count() == 5) {
        String zoneL;
        String diagraph;

        if (sA.Count() == 5) {
          if (Char.IsLetter(sA[0][0])) { sA[0] += sA[1]; sA[1] = sA[2]; sA[2] = sA[3]; } else if (Char.IsLetter(sA[1][0])) { sA[0] += sA[1]; sA[1] = sA[2]; sA[2] = sA[3]; } else { return false; }
        }
        zoneL = new String(sA[0].Where(Char.IsLetter).ToArray());
        if (zoneL == String.Empty) { return false; }
        sA[0] = Regex.Replace(sA[0], "[^0-9.]", "");
        diagraph = sA[1];
        if (!Double.TryParse(sA[0], out Double zone)) { return false; }
        if (!Double.TryParse(sA[2], out Double easting)) { return false; }
        if (!Double.TryParse(sA[3], out Double northing)) { return false; }

        mgrs = new String[] { zone.ToString(), zoneL, diagraph, easting.ToString(), northing.ToString() };
        return true;
      }
      return false;
    }
    private static Boolean TryCartesian(String s, out Double[] d) {
      d = null;
      String[] sA = SpecialSplit(s, false);

      if (sA.Count() == 3) {
        if (!Double.TryParse(sA[0], out Double x)) { return false; }
        if (!Double.TryParse(sA[1], out Double y)) { return false; }
        if (!Double.TryParse(sA[2], out Double z)) { return false; }
        d = new Double[] { x, y, z };
        return true;
      }
      return false;
    }

    //KEEP DASHES FOR SIGNED AND CARTESIAN AS THEY ARE USED FOR NEGATVE VALUES
    private static String[] SpecialSplit(String s, Boolean removeDashes) {
      s = s.Replace("°", " ");
      s = s.Replace("º", " ");
      s = s.Replace("'", " ");
      s = s.Replace("\"", " ");
      s = s.Replace(",", " ");
      s = s.Replace("mE", " ");
      s = s.Replace("mN", " ");
      if (removeDashes) {
        s = s.Replace("-", " ");
      }
      return s.Split(new Char[0], StringSplitOptions.RemoveEmptyEntries);
    }
  }
  internal class FormatFinder_CoordPart {
    //Add main to Coordinate and tunnel to Format class. Add private methods to format.
    //WHEN PARSING NO EXCPETIONS FOR OUT OF RANGE ARGS WILL BE THROWN
    public static Boolean TryParse(String coordString, out CoordinatePart cp) {
      //Turn of eagerload for efficiency
      EagerLoad eg = new EagerLoad();
      Int32 type = 0; //0 = unspecifed, 1 = lat, 2 = long;
      eg.Cartesian = false;
      eg.Celestial = false;
      eg.UTM_MGRS = false;
      cp = null;
      Coordinate c = new Coordinate(eg);
      String s = coordString;
      s = s.Trim(); //Trim all spaces before and after string

      if (s[0] == ',') {
        type = 2;
        s = s.Replace(",", "");
        s = s.Trim();
      }
      if (s[0] == '*') {
        type = 1;
        s = s.Replace("*", "");
        s = s.Trim();
      }

      if (TrySignedDegree(s, type, out Double[] d)) {
        try {
          switch (type) {
            case 0:
              //Attempt Lat first (default for signed)
              try {
                cp = new CoordinatePart(d[0], CoordinateType.Lat);
                c.Parse_Format = Parse_Format_Type.Signed_Degree;
                return true;
              } catch {
                cp = new CoordinatePart(d[0], CoordinateType.Long);
                c.Parse_Format = Parse_Format_Type.Signed_Degree;
                return true;
              }
            case 1:
              //Attempt Lat
              cp = new CoordinatePart(d[0], CoordinateType.Lat);
              c.Parse_Format = Parse_Format_Type.Signed_Degree;
              return true;
            case 2:
              //Attempt long
              cp = new CoordinatePart(d[0], CoordinateType.Long);
              c.Parse_Format = Parse_Format_Type.Signed_Degree;
              return true;
          }
        } catch {
          //silent fail
        }
      }
      //SIGNED DEGREE FAILED, REMOVE DASHES FOR OTHER FORMATS
      s = s.Replace("-", " ");

      //All other formats should contain 1 letter.
      if (Regex.Matches(s, @"[a-zA-Z]").Count != 1) { return false; } //Should only contain 1 letter.
                                                                      //Get Coord Direction
      Int32 direction = Find_Position(s);

      if (direction == -1) {
        return false; //No direction found
      }
      //If Coordinate type int specified, look for mismatch
      if (type == 1 && (direction == 1 || direction == 3)) {
        return false; //mismatch
      }
      if (type == 2 && (direction == 0 || direction == 2)) {
        return false; //mismatch
      }
      CoordinateType t = direction == 0 || direction == 2 ? CoordinateType.Lat : CoordinateType.Long;
      s = Regex.Replace(s, "[^0-9. ]", ""); //Remove directional character
      s = s.Trim(); //Trim all spaces before and after string

      //Try Decimal Degree with Direction
      if (TryDecimalDegree(s, direction, out d)) {
        try {
          cp = new CoordinatePart(d[0], t);
          c.Parse_Format = Parse_Format_Type.Decimal_Degree;
          return true;
        } catch {//Parser failed try next method 
        }
      }
      //Try DDM
      if (TryDegreeDecimalMinute(s, out d)) {
        try {
          //0  Degree
          //1  Minute
          //2  Direction (0 = N, 1 = E, 2 = S, 3 = W)                          
          cp = new CoordinatePart((Int32)d[0], d[1], (CoordinatesPosition)direction);
          c.Parse_Format = Parse_Format_Type.Degree_Decimal_Minute;
          return true;
        } catch {
          //Parser failed try next method 
        }
      }
      //Try DMS
      if (TryDegreeMinuteSecond(s, out d)) {
        try {
          //0 Degree
          //1 Minute
          //2 Second
          //3 Direction (0 = N, 1 = E, 2 = S, 3 = W)                                     
          cp = new CoordinatePart((Int32)d[0], (Int32)d[1], d[2], (CoordinatesPosition)direction);
          c.Parse_Format = Parse_Format_Type.Degree_Minute_Second;
          return true;
        } catch {//Parser failed try next method 
        }
      }

      return false;
    }

    private static Boolean TrySignedDegree(String s, Int32 _, out Double[] d) {
      d = null;
      if (Regex.Matches(s, @"[a-zA-Z]").Count != 0) { return false; } //Should contain no letters

      String[] sA = SpecialSplit(s, false);
      Double deg;
      Double min; //Minutes & MinSeconds
      Double sec;

      Int32 sign = 1;
      switch (sA.Count()) {
        case 1:
          if (!Double.TryParse(sA[0], out deg)) { return false; }
          d = new Double[] { deg };
          return true;
        case 2:
          if (!Double.TryParse(sA[0], out deg)) { return false; }
          if (!Double.TryParse(sA[1], out min)) { return false; }

          if (deg < 0) { sign = -1; }
          if (min >= 60 || min < 0) { return false; } //Handle in parser as degree will be incorrect.
          d = new Double[] { (Math.Abs(deg) + min / 60.0) * sign };
          return true;
        case 3:
          if (!Double.TryParse(sA[0], out deg)) { return false; }
          if (!Double.TryParse(sA[1], out min)) { return false; }
          if (!Double.TryParse(sA[2], out sec)) { return false; }
          if (min >= 60 || min < 0) { return false; } //Handle in parser as degree will be incorrect.
          if (sec >= 60 || sec < 0) { return false; } //Handle in parser as degree will be incorrect.

          if (deg < 0) { sign = -1; }
          d = new Double[] { (Math.Abs(deg) + min / 60.0 + sec / 3600.0) * sign };
          return true;
        default:
          return false;
      }
    }
    private static Boolean TryDecimalDegree(String s, Int32 direction, out Double[] d) {
      d = null;
      Int32 sign = 1;
      //S or W
      if (direction == 2 || direction == 3) {
        sign = -1;
      }

      String[] sA = SpecialSplit(s, true);

      if (sA.Count() == 1) {
        if (!Double.TryParse(s, out Double coord)) { return false; }

        coord *= sign;
        d = new Double[] { coord };
        return true;
      }

      return false;
    }
    private static Boolean TryDegreeDecimalMinute(String s, out Double[] d) {
      d = null;



      String[] sA = SpecialSplit(s, true);
      if (sA.Count() == 2) {
        if (!Double.TryParse(sA[0], out Double deg)) { return false; }
        if (!Double.TryParse(sA[1], out Double minSec)) { return false; }

        d = new Double[] { deg, minSec };
        return true;
      }
      return false;
    }
    private static Boolean TryDegreeMinuteSecond(String s, out Double[] d) {
      d = null;



      String[] sA = SpecialSplit(s, true);
      if (sA.Count() == 3) {

        if (!Double.TryParse(sA[0], out Double deg)) { return false; }
        if (!Double.TryParse(sA[1], out Double min)) { return false; }
        if (!Double.TryParse(sA[2], out Double sec)) { return false; }

        d = new Double[] { deg, min, sec };
        return true;
      }
      return false;
    }

    private static Int32 Find_Position(String s) {
      //N=0
      //E=1
      //S=2
      //W=3
      //NOPOS = -1
      //Find Directions

      Int32 part = -1;
      if (s.Contains("N") || s.Contains("n")) {
        part = 0;
      }
      if (s.Contains("E") || s.Contains("e")) {
        part = 1;
      }
      if (s.Contains("S") || s.Contains("s")) {
        part = 2;

      }
      if (s.Contains("W") || s.Contains("w")) {
        part = 3;
      }
      return part;
    }

    //KEEP DASHES FOR SIGNED AND CARTESIAN AS THEY ARE USED FOR NEGATVE VALUES
    private static String[] SpecialSplit(String s, Boolean removeDashes) {
      s = s.Replace("°", " ");
      s = s.Replace("º", " ");
      s = s.Replace("'", " ");
      s = s.Replace("\"", " ");
      s = s.Replace(",", " ");
      s = s.Replace("mE", " ");
      s = s.Replace("mN", " ");
      if (removeDashes) {
        s = s.Replace("-", " ");
      }
      return s.Split(new Char[0], StringSplitOptions.RemoveEmptyEntries);
    }
  }
}
