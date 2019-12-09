using System;

namespace CoordinateSharp {
  /// <summary>
  /// Coordinate formatting options for a Coordinate object.
  /// </summary>
  [Serializable]
  public class CoordinateFormatOptions {
    /// <summary>
    /// Set default values with the constructor.
    /// </summary>
    public CoordinateFormatOptions() {
      this.Format = CoordinateFormatType.Degree_Minutes_Seconds;
      this.Round = 3;
      this.Display_Leading_Zeros = false;
      this.Display_Trailing_Zeros = false;
      this.Display_Symbols = true;
      this.Display_Degree_Symbol = true;
      this.Display_Minute_Symbol = true;
      this.Display_Seconds_Symbol = true;
      this.Display_Hyphens = false;
      this.Position_First = true;
    }
    /// <summary>
    /// Coordinate format type.
    /// </summary>
    public CoordinateFormatType Format { get; set; }
    /// <summary>
    /// Rounds Coordinates to the set value.
    /// </summary>
    public Int32 Round { get; set; }
    /// <summary>
    /// Displays leading zeros.
    /// </summary>
    public Boolean Display_Leading_Zeros { get; set; }
    /// <summary>
    /// Display trailing zeros.
    /// </summary>
    public Boolean Display_Trailing_Zeros { get; set; }
    /// <summary>
    /// Allow symbols to display.
    /// </summary>
    public Boolean Display_Symbols { get; set; }
    /// <summary>
    /// Display degree symbols.
    /// </summary>
    public Boolean Display_Degree_Symbol { get; set; }
    /// <summary>
    /// Display minute symbols.
    /// </summary>
    public Boolean Display_Minute_Symbol { get; set; }
    /// <summary>
    /// Display secons symbol.
    /// </summary>
    public Boolean Display_Seconds_Symbol { get; set; }
    /// <summary>
    /// Display hyphens between values.
    /// </summary>
    public Boolean Display_Hyphens { get; set; }
    /// <summary>
    /// Show coordinate position first.
    /// Will show last if set 'false'.
    /// </summary>
    public Boolean Position_First { get; set; }
  }
  /// <summary>
  /// Coordinate Format Types.
  /// </summary>
  [Serializable]
  public enum CoordinateFormatType {
    /// <summary>
    /// Decimal Degree Format
    /// </summary>
    /// <remarks>
    /// Example: N 40.456 W 75.456
    /// </remarks>
    Decimal_Degree,
    /// <summary>
    /// Decimal Degree Minutes Format
    /// </summary>
    /// <remarks>
    /// Example: N 40º 34.552' W 70º 45.408'
    /// </remarks>
    Degree_Decimal_Minutes,
    /// <summary>
    /// Decimal Degree Minutes Format
    /// </summary>
    /// <remarks>
    /// Example: N 40º 34" 36.552' W 70º 45" 24.408'
    /// </remarks>
    Degree_Minutes_Seconds,
    /// <summary>
    /// Decimal Format
    /// </summary>
    /// <remarks>
    /// Example: 40.57674 -70.46574
    /// </remarks>
    Decimal
  }
}
