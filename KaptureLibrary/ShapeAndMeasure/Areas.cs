using KaptureLibrary.Points;

namespace KaptureLibrary.ShapeAndMeasure
{
    /// <summary>
    /// Represents an area in the integer (<see cref="Int32"/>) field.
    /// </summary>
    [System.Serializable]
    public class AreaInt
    {
        public RangeInt RangeX { get; private set; }
        public RangeInt RangeY { get; private set; }
        public Point2D Centre
        {
            get
            {
                if (this._centre == null) this._centre = new Point2D(RangeX.Centre, RangeY.Centre);
                return _centre;
            }
        }
        private Point2D _centre;

        public AreaInt(RangeInt x, RangeInt y)
        {
            this.RangeX = x; this.RangeY = y;
        }

        public AreaInt(Point2D topLeft, Point2D bottomRight)
        {
            this.RangeX = new RangeInt(topLeft.X, bottomRight.X);
            this.RangeY = new RangeInt(topLeft.Y, bottomRight.Y);
        }

        /// <summary>
        /// Test is a point is within the area.
        /// </summary>
        /// <param name="point">Test point.</param>
        /// <returns>If the <paramref name="point"/> is within the area.</returns>
        public bool IsInside(Point2D point)
        {
            return (this.RangeX.IsInRange(point.X) && this.RangeY.IsInRange(point.Y));
        }

        public override string ToString()
        {
            return "(" + this.RangeX.Low.ToString() + ", " + this.RangeY.Low.ToString() + ") (" +
                this.RangeX.High.ToString() + ", " + this.RangeY.High.ToString() + ")";
        }


    }
}