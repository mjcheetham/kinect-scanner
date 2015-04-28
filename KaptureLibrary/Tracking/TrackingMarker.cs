
namespace KaptureLibrary.Tracking
{
    public enum MarkerName { Red, Green, Blue, Purple }

    /// <summary>
    /// Represents a tracking marker placed around the turntable.
    /// </summary>
    [System.Serializable]
    public struct TrackingMarker
    {
        /// <summary>
        /// The assigned name of the marker.
        /// </summary>
        public MarkerName Name { get; private set; }

        /// <summary>
        /// Central hue value of the marker's colour.
        /// </summary>
        public int Hue { get; set; }

        /// <summary>
        /// Tolerance of the hue value.
        /// </summary>
        public int HueTolerance { get; set; }

        /// <summary>
        /// Relative fixed rotation of this tracking marker in radians.
        /// </summary>
        public double RotationalOffset { get; set; }

        #region Constructors
        public TrackingMarker(MarkerName name, int hue, int tolerance, double offset)
            : this()
        {
            this.Name = name;
            this.Hue = hue;
            this.HueTolerance = tolerance;
            this.RotationalOffset = offset;
        }
        #endregion

        #region ToString Override
        public override string ToString()
        {
            return this.Name.ToString() + " = " + this.Hue + " +- " + this.HueTolerance +
                " deg @ " + this.RotationalOffset.ToString();
        }
        #endregion
    }
}
