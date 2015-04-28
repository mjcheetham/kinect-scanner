using KaptureLibrary.Kinect;
using KaptureLibrary.Points;
using KaptureLibrary.ShapeAndMeasure;
using KaptureLibrary.Tracking;
using System;
using System.Collections.Generic;
using System.IO;

namespace KaptureLibrary.Processing.PointConstruction
{
    public class PointCloudBuilder
    {
        #region Private Fields
        private Calibration calibration;
        private ICollection<byte> mappingParams;
        private Volume clippingVolume;
        #endregion

        private StreamWriter markerLog;

        #region Constructors
        public PointCloudBuilder(Calibration calibration, ICollection<byte> mappingParameters)
        {
            this.calibration = calibration;
            this.mappingParams = mappingParameters;
            // CONFIG
            this.clippingVolume = new Cylinder(new Point3D(0, 0.01f, 0), 0.3f, 0.10f);

            if (DebugSettings.Default.DebugMode)
            {
                markerLog = new StreamWriter(DebugSettings.Default.LogRoot + @"\markerPosition.csv");
                markerLog.AutoFlush = true;
                markerLog.WriteLine("f,t,x,y,z,confidence");
            }
        }
        #endregion

        internal bool ConstructCloud(Frame frame, Dictionary<TrackingMarker,Point2D> markers2D,
            out PointCloud cloud, out KeyValuePair<TrackingMarker,Point3D> rotationMarker)
        {
            var camera = this.calibration.Camera;

            // create new offline mapper from passed kinect params.
            var threadMapper = new Mapper(this.mappingParams);

            #region Convert 2D points to 3D markers
            var trackingPoints3D = threadMapper.Convert2DTrackingPointsTo3DTrackingPoints(
                markers2D, frame.DepthFormat, frame.Depth, frame.ColourFormat);

            // Transform 3D markers to object basis
            foreach (var pair in trackingPoints3D)
            {
                pair.Value.RotateAboutAxis(Axis.X, -camera.ElevationAngle); // to null
                pair.Value.Translate(camera.Origin.X, camera.Origin.Y, camera.Origin.Z); // to obj

                // Set confidence values
                float R = calibration.TurntableRadius; // turntable radius
                // radius in XZ-plane (ignoring Y component)
                float radiusXZ = (float)Math.Sqrt(Math.Pow(pair.Value.X, 2) + Math.Pow(pair.Value.Z, 2));
                float delta = Math.Abs(radiusXZ - R); // calculate the distance from real circumference

                // C(delta) = C_max - C_max/delta_max * delta
                pair.Value.Confidence = 1 - (delta / 0.08f);
            }
            #endregion

            #region Get the marker with the best confidence
            KeyValuePair<TrackingMarker, Point3D> bestPoint = new KeyValuePair<TrackingMarker,Point3D>();
            bool firstEntry = true;
            foreach (var p in trackingPoints3D)
            {
                var candidatePoint = p;

                // do we trust this point? +ve confidence => trust
                if (candidatePoint.Value.Confidence == 0) continue;

                // is this the first time round? if so assume first it's the best!
                if (firstEntry) { firstEntry = false; bestPoint = candidatePoint; }

                // set best to be the most reliable of current and best-so-far
                if (candidatePoint.Value.Confidence > bestPoint.Value.Confidence) bestPoint = candidatePoint;
            }
            rotationMarker = bestPoint;
            if (bestPoint.Value == null) // we still didn't find a good cube! we generate no cloud!
            {
                cloud = new PointCloud();
                return false; 
            }
            // set best cube as output rotation marker
            
            #endregion

            if (DebugSettings.Default.DebugMode)
            {
                markerLog.WriteLine("{0},{1},{2},{3},{4},{5}",
                    frame.FrameNumber, frame.Timestamp, bestPoint.Value.X, bestPoint.Value.Y, bestPoint.Value.Z, bestPoint.Value.Confidence);
            }

            #region Create object point cloud
            // get raw scene
            var rawPoints = threadMapper.GeneratePointCloud(frame.DepthFormat, frame.Depth, frame.ColourFormat, frame.Colour);
            var clippedCloud = new PointCloud();
            foreach (var p in rawPoints)
            {
                // rotate
                p.RotateAboutAxis(Axis.X, -camera.ElevationAngle); // to null
                p.Translate(camera.Origin.X, camera.Origin.Y, camera.Origin.Z); // to obj
                // clip?
                if (this.clippingVolume.IsInside(p))
                {
                    // include the point
                    clippedCloud.Points.Add(p);
                }
                // else drop point
            }
            cloud = clippedCloud;

            if (DebugSettings.Default.DebugMode)
            {
                KaptureLibrary.IO.PointCloudWriter.WritePLY(DebugSettings.Default.LogRoot +
                @"\geom\cloud" + frame.FrameNumber + ".ply", clippedCloud);
            }
            #endregion

            return true;
        }
    }
}
