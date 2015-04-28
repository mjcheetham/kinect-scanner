
namespace KaptureLibrary.Points
{
    /// <summary>
    /// Represents a 2D point in the integer (<see cref="Int32"/>) field.
    /// </summary>
    [System.Serializable]
    public class Point2D : IPoint
    {
        public int X { get; set; }
        public int Y { get; set; }

        #region Constructors
        public Point2D(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
        #endregion

        #region ToString Override
        public override string ToString()
        {
            return "{ " + X.ToString() + ", " + Y.ToString() + " }";
        }
        #endregion

        #region IPoint
        public string GetComponentsAsString()
        {
            return X.ToString() + " " + Y.ToString();
        }
        #endregion
    }
}
