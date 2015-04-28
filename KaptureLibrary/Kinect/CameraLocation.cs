using KaptureLibrary.Points;

namespace KaptureLibrary.Kinect
{
    /// <summary>
    /// Structure containing location and elevation information for the <see cref="KinectSensor"/>.
    /// </summary>
    [System.Serializable]
    public struct CameraLocation
    {
        /// <summary>
        /// Location of <see cref="KinectSensor"/>.
        /// </summary>
        public Point3D Origin;
        /// <summary>
        /// Elevation angle of <see cref="KinectSensor"/>.
        /// </summary>
        public double ElevationAngle;

        /// <summary>
        /// Initialise structure with values.
        /// </summary>
        /// <param name="origin">Location of <see cref="KinectSensor"/>.</param>
        /// <param name="elevation">Current camera elevation. Usually acquired from active <see cref="KinectSensor"/> object.</param>
        public CameraLocation(Point3D origin, double elevation)
        {
            this.Origin = origin;
            this.ElevationAngle = elevation;
        }

        #region ToString Override
        public override string ToString()
        {
            return "Camera @ " + this.Origin.ToString() + " Tilt: " + this.ElevationAngle * 180 / System.Math.PI + " degrees";
        }
        #endregion
    }
}