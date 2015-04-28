namespace KaptureLibrary.ShapeAndMeasure
{
    /// <summary>
    /// (<see cref="Int32"/>) range.
    /// </summary>
    [System.Serializable]
    public class RangeInt
    {
        public int Low { get; set; }
        public int High { get; set; }
        public int Centre
        {
            get
            {
                return (High + Low) / 2;
            }
        }

        public RangeInt(int low, int high)
        {
            this.Low = low;
            this.High = high;
        }
        /// <summary>
        /// Test if <paramref name="value"/> is within the range.
        /// </summary>
        /// <param name="value">Test value.</param>
        /// <returns>If the <paramref name="value"/> is within the range.</returns>
        public bool IsInRange(int value)
        {
            return ((value >= this.Low) && (value <= this.High));
        }
        public override string ToString()
        {
            return "[" + this.Low + ", " + this.High + "]";
        }
    }
    /// <summary>
    /// (<see cref="Single"/>) range.
    /// </summary>
    public class RangeFloat
    {
        public float Low { get; set; }
        public float High { get; set; }
        public float Centre
        {
            get
            {
                return (High + Low) / 2;
            }
        }

        public RangeFloat(float low, float high)
        {
            this.Low = low;
            this.High = high;
        }
        /// <summary>
        /// Test if <paramref name="value"/> is within the range.
        /// </summary>
        /// <param name="value">Test value.</param>
        /// <returns>If the <paramref name="value"/> is within the range.</returns>
        public bool IsInRange(float value)
        {
            return ((value >= this.Low) && (value <= this.High));
        }
        public override string ToString()
        {
            return "[" + this.Low + ", " + this.High + "]";
        }
    }
}