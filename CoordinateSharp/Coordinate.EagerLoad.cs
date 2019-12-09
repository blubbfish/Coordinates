using System;

namespace CoordinateSharp {
  /// <summary>
  /// Turn on/off eager loading of certain properties.
  /// </summary>
  [Serializable]
  public class EagerLoad {
    /// <summary>
    /// Create an EagerLoad object
    /// </summary>
    public EagerLoad() {
      this.Celestial = true;
      this.UTM_MGRS = true;
      this.Cartesian = true;
      this.ECEF = true;
    }

    /// <summary>
    /// Create an EagerLoad object with all options on or off
    /// </summary>
    /// <param name="isOn">Turns EagerLoad on or off</param>
    public EagerLoad(Boolean isOn) {
      this.Celestial = isOn;
      this.UTM_MGRS = isOn;
      this.Cartesian = isOn;
      this.ECEF = isOn;
    }

    /// <summary>
    /// Create an EagerLoad object with only the specified flag options turned on.
    /// </summary>
    /// <param name="et">EagerLoadType</param>
    public EagerLoad(EagerLoadType et) {
      this.Cartesian = false;
      this.Celestial = false;
      this.UTM_MGRS = false;
      this.ECEF = false;

      if (et.HasFlag(EagerLoadType.Cartesian)) {
        this.Cartesian = true;
      }
      if (et.HasFlag(EagerLoadType.Celestial)) {
        this.Celestial = true;
      }
      if (et.HasFlag(EagerLoadType.UTM_MGRS)) {
        this.UTM_MGRS = true;
      }
      if (et.HasFlag(EagerLoadType.ECEF)) {
        this.ECEF = true;
      }
    }

    /// <summary>
    /// Creates an EagerLoad object. Only the specified flags will be set to EagerLoad.
    /// </summary>
    /// <param name="et">EagerLoadType</param>
    /// <returns>EagerLoad</returns>
    public static EagerLoad Create(EagerLoadType et) {
      EagerLoad el = new EagerLoad(et);
      return el;
    }

    /// <summary>
    /// Eager load celestial information.
    /// </summary>
    public Boolean Celestial { get; set; }
    /// <summary>
    /// Eager load UTM and MGRS information
    /// </summary>
    public Boolean UTM_MGRS { get; set; }
    /// <summary>
    /// Eager load Cartesian information
    /// </summary>
    public Boolean Cartesian { get; set; }
    /// <summary>
    /// Eager load ECEF information
    /// </summary>
    public Boolean ECEF { get; set; }
  }
  /// <summary>
  /// EagerLoad Enumerator
  /// </summary>
  [Serializable]
  [Flags]
  public enum EagerLoadType {
    /// <summary>
    /// UTM and MGRS
    /// </summary>
    UTM_MGRS = 1,
    /// <summary>
    /// Celestial
    /// </summary>
    Celestial = 2,
    /// <summary>
    /// Cartesian
    /// </summary>
    Cartesian = 4,
    /// <summary>
    /// ECEF
    /// </summary>
    ECEF = 8

  }
}
