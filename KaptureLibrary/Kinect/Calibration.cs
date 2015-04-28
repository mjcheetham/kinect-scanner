using KaptureLibrary.ShapeAndMeasure;
using KaptureLibrary.Tracking;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace KaptureLibrary.Kinect
{
    /// <summary>
    /// Scene calibration information.
    /// </summary>
    [System.Serializable]
    public struct Calibration
    {
        #region Properties
        /// <summary>
        /// Location of the <see cref="KinectSensor"/>.
        /// </summary>
        public CameraLocation Camera { get; set; }

        /// <summary>
        /// Set of <see cref="TrackingMarker"/>s to use for tracking.
        /// </summary>
        public ICollection<TrackingMarker> Markers { get; set; }

        /// <summary>
        /// Space to search for tracking markers within colour image.
        /// </summary>
        public AreaInt MarkerSearchSpace { get; set; }

        /// <summary>
        /// Estimated radius of the turntable.
        /// </summary>
        public float TurntableRadius { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create calibration information from depth and colour frame.
        /// </summary>
        /// <param name="camera">Camera location information.</param>
        /// <param name="markers">Collection of tracking markers in use.</param>
        /// <param name="markerSearchSpace">Tracking marker search space.</param>
        /// <param name="turntableRadius">Estimated radius of the turntable.</param>
        public Calibration(CameraLocation camera, ICollection<TrackingMarker> markers,
            AreaInt markerSearchSpace, float turntableRadius) : this()
        {
            this.Camera = camera;
            this.Markers = markers;
            this.MarkerSearchSpace = markerSearchSpace;
            this.TurntableRadius = turntableRadius;
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Create calibration information from a serialised file.
        /// </summary>
        /// <param name="path">Serialised calibration information.</param>
        /// <returns>Calibration information contained within the file.</returns>
        public static Calibration CreateFromFile(string path)
        {
            IFormatter formatter = new BinaryFormatter();
            var f = new FileStream(path, FileMode.Open);
            object c = formatter.Deserialize(f);

            return (Calibration)c;
        }
        #endregion
    }
}
