using KaptureLibrary.Diagnostics;
using KaptureLibrary.Points;
using KaptureLibrary.ShapeAndMeasure;
using KaptureLibrary.Tracking;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace KaptureLibrary.Kinect
{
    public class SensorManager
    {
        private KinectSensor sensor;
        private ColorImageFormat desiredColourFormat;
        private DepthImageFormat desiredDepthFormat;
        public bool Connected { get; private set; }
        public bool LiveStreamsEnabled { get; private set; }

        #region Constructors
        public SensorManager(DepthFormat dFormat, ColourFormat cFormat)
        {
            this.desiredDepthFormat = FormatConvertor.ConvertToKinect(dFormat);
            this.desiredColourFormat = FormatConvertor.ConvertToKinect(cFormat);
            this.frameReadyEventsRegistered = false;
        }
        #endregion

        #region Connect/Disconnect
        /// <summary>
        /// Attempt to connect to the first <see cref="KinectSensor"/> and initialise the streams.
        /// </summary>
        /// <returns>If the connection was successful or not.</returns>
        public bool Connect()
        {
            // Initialise the sensor
            if (KinectSensor.KinectSensors.Count == 0)
            {
                // No Kinect connected.
                return false;
            }
            foreach (KinectSensor s in KinectSensor.KinectSensors)
            {
                // Find a connected Kinect!
                if (s.Status == KinectStatus.Connected)
                {
                    this.sensor = s;
                }
            }

            if (this.sensor == null) return false;

            // set colour and depth streams enabled
            this.sensor.ColorStream.Enable(this.desiredColourFormat);
            this.sensor.DepthStream.Enable(this.desiredDepthFormat);
            this.sensor.Start();

            this.EnableLiveStreams();
            this.Connected = true;
            return true;
        }

        /// <summary>
        /// Stop the sensor and remove internal references to the <see cref="KinectSensor"/> object
        /// </summary>
        public void Disconnect()
        {
            if (this.sensor != null)
            {
                this.DisableLiveStreams();
                this.sensor.Stop();
                Thread.Sleep(100); // allow kinect to stop!
                this.sensor = null;
                this.Connected = false;
            }
        }

        #endregion

        #region Capture to Disk
        public async Task CaptureToDiskAsync(string mappingParamsPath, string timestampPath, string dPath, string cPath,
            int durationms, IProgress<long> progress)
        {
            UnregisterFrameReadyEvents();

            int maxWaitms = 100 / 3;

            var paramsTask = Task.Run(() =>
            {
                var readOnlyParams = this.sensor.CoordinateMapper.ColorToDepthRelationalParameters;
                var mappingParams = new byte[readOnlyParams.Count];
                readOnlyParams.CopyTo(mappingParams, 0);
                var mapFile = new FileStream(mappingParamsPath, FileMode.Create);
                mapFile.Write(mappingParams, 0, mappingParams.Length);
                mapFile.Close();
            });

            var rootTimer = new PerformanceTimer();

            var captureTask = Task.Run(() =>
                {
                    var depthFile = new BinaryWriter(new FileStream(dPath, FileMode.Create));
                    var dPxls = new short[this.sensor.DepthStream.FramePixelDataLength];
                    var colourFile = new BinaryWriter(new FileStream(cPath, FileMode.Create));
                    var cPxls = new byte[this.sensor.ColorStream.FramePixelDataLength];

                    var timestampFile = new BinaryWriter(new FileStream(timestampPath, FileMode.Create));
                    long t0 = 0;
                    long tEnd = 1;

                    rootTimer.Start();
                    for (long t = 0; t < tEnd; )
                    {
                        var c = this.sensor.ColorStream.OpenNextFrame(maxWaitms);
                        var d = this.sensor.DepthStream.OpenNextFrame(maxWaitms);
                        if (c == null || d == null) { continue; } // missed a frame!
                        t = c.Timestamp;

                        // first entry? get t0 offset
                        if (tEnd == 1) { t0 = t; tEnd = durationms + t; }
                        timestampFile.Write((t - t0) * 0.001f);
                        progress.Report(t - t0);

                        c.CopyPixelDataTo(cPxls);
                        c.Dispose();
                        d.CopyPixelDataTo(dPxls);
                        d.Dispose();

                        colourFile.Write(cPxls);
                        foreach (var s in dPxls)
                            depthFile.Write(s);

                        if (this.LiveColourFrameReady != null)
                            LiveColourFrameReady(this, cPxls);
                        if (this.LiveDepthFrameReady != null)
                            LiveDepthFrameReady(this, dPxls);
                    }
                    rootTimer.Stop();

                    timestampFile.Close();
                    depthFile.Close();
                    colourFile.Close();
                });

            await Task.WhenAll(captureTask, paramsTask);

            Trace.WriteLine("Total running time: " + rootTimer.Duration + " seconds.");

            RegisterFrameReadyEvents();
        }
        #endregion

        #region Live Stream Control
        private void EnableLiveStreams()
        {
            this.LiveStreamsEnabled = true;
            RegisterFrameReadyEvents();
        }
        private void DisableLiveStreams()
        {
            this.LiveStreamsEnabled = false;
            UnregisterFrameReadyEvents();
        }
        #endregion

        #region Kinect Event Registration
        private bool frameReadyEventsRegistered;
        private void RegisterFrameReadyEvents()
        {
            if (this.frameReadyEventsRegistered) return;
            frameReadyEventsRegistered = true;
            this.sensor.ColorFrameReady += sensor_ColorFrameReady;
            this.sensor.DepthFrameReady += sensor_DepthFrameReady;
        }
        private void UnregisterFrameReadyEvents()
        {
            if (!this.frameReadyEventsRegistered) return;
            frameReadyEventsRegistered = false;
            this.sensor.ColorFrameReady -= sensor_ColorFrameReady;
            this.sensor.DepthFrameReady -= sensor_DepthFrameReady;
        }
        #endregion

        #region Kinect FrameReady Event Handlers
        private void sensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            if (this.sensor == null) return;
            using (DepthImageFrame frame = e.OpenDepthImageFrame())
            {
                if (frame == null) return;
                var shorts = new short[this.sensor.DepthStream.FramePixelDataLength];
                frame.CopyPixelDataTo(shorts);
                if (this.LiveDepthFrameReady != null)
                    LiveDepthFrameReady(this, shorts);

            }
        }
        private void sensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            if (this.sensor == null) return;
            using (ColorImageFrame frame = e.OpenColorImageFrame())
            {
                if (frame == null) return;
                var pixels = new byte[this.sensor.ColorStream.FramePixelDataLength];
                frame.CopyPixelDataTo(pixels);
                if (this.LiveColourFrameReady != null)
                    LiveColourFrameReady(this, pixels);
            }
        }
        #endregion

        #region Events
        public delegate void DepthReadyEventHandler(object sender, short[] frame);
        public delegate void ColourReadyEventHandler(object sender, byte[] frame);
        public event DepthReadyEventHandler LiveDepthFrameReady;
        public event ColourReadyEventHandler LiveColourFrameReady;
        #endregion

        #region Calibration
        public async Task SaveCalibrationToDiskAsync(string path, AreaInt markerSearchSpace,
            ICollection<TrackingMarker> markers)
        {
            UnregisterFrameReadyEvents();
            await Task.Run(() =>
                {
                    var c = PerformCalibration(markerSearchSpace, markers);
                    IFormatter formatter = new BinaryFormatter();
                    var f = new FileStream(path, FileMode.Create);
                    formatter.Serialize(f, c);
                });
            RegisterFrameReadyEvents();
        }
        /// <summary>
        /// Perform calibration of the scene using user input and saves parameters to a file.
        /// </summary>
        /// <param name="path">Calibration parameters file to save.</param>
        /// <param name="markerSearchSpace">Space to search for tracking cubes within the colour image.</param>
        /// <param name="markers">Collection of active tracking cubes.</param>
        private Calibration PerformCalibration(AreaInt markerSearchSpace, ICollection<TrackingMarker> markers)
        {
            var calibration = new Calibration();

            // set search space
            calibration.MarkerSearchSpace = markerSearchSpace;

            // Capture 1 frame
            DepthImagePixel[] depthCalFrame = new DepthImagePixel[FormatConvertor.PixelDataLength(this.desiredDepthFormat)];
            
            using (var f = this.sensor.DepthStream.OpenNextFrame(100))
                f.CopyDepthImagePixelDataTo(depthCalFrame);

            // Convert colour frame to depth frame
            DepthImagePoint[] dips = new DepthImagePoint[FormatConvertor.PixelDataLength(this.desiredDepthFormat)];
            this.sensor.CoordinateMapper.MapColorFrameToDepthFrame(this.desiredColourFormat,
                this.desiredDepthFormat, depthCalFrame, dips);

            // Convert colour box centre, L & R to depth point         
            // Get points at edge of 2D search area (for use in 3D TT search clipping)
            var cPixelWidth = FormatConvertor.PixelWidth(desiredColourFormat);
            int colourCentreIndex = markerSearchSpace.Centre.X + markerSearchSpace.Centre.Y * cPixelWidth;
            int leftColourIndex = markerSearchSpace.RangeX.Low + markerSearchSpace.RangeY.High * cPixelWidth;
            int rightColourIndex = markerSearchSpace.RangeX.High + markerSearchSpace.RangeY.High * cPixelWidth;
            var centreDepthPoint = dips[colourCentreIndex];
            var leftDepthPoint = dips[leftColourIndex];
            var rightDepthPoint = dips[rightColourIndex];

            // get 3d point in camera basis of centre, left & right of user-drawn box
            var centre3DRaw = this.sensor.CoordinateMapper.MapDepthPointToSkeletonPoint(
                this.desiredDepthFormat, centreDepthPoint);
            var left3DRaw = this.sensor.CoordinateMapper.MapDepthPointToSkeletonPoint(
                this.desiredDepthFormat, leftDepthPoint);
            var right3DRaw = this.sensor.CoordinateMapper.MapDepthPointToSkeletonPoint(
                this.desiredDepthFormat, rightDepthPoint);

            // get y-value of turntable
            var tilt = this.sensor.ElevationAngle * Math.PI / 180;
            var finalCameraY = (float)(centre3DRaw.Y * Math.Cos(-tilt) - centre3DRaw.Z * Math.Sin(-tilt));

            // get minX and maxX to search-clip in
            var minXClip = left3DRaw.X;
            var maxXClip = right3DRaw.X;

            // get point cloud of captured scene
            var calSceneRaw = new SkeletonPoint[FormatConvertor.PixelDataLength(this.desiredDepthFormat)];
            this.sensor.CoordinateMapper.MapDepthFrameToSkeletonFrame(this.desiredDepthFormat, depthCalFrame, calSceneRaw);
            PointCloud calScene = new PointCloud();
            foreach (var p in calSceneRaw)
            {
                var myP = new Point3D(p.X, p.Y, p.Z); // vector-style point
                myP.RotateAboutAxis(Axis.X, -tilt); // rotate to null

                if (myP.X < minXClip) continue; // too far left
                if (myP.X > maxXClip) continue; // too far right

                if (myP.Y + 0.01f < finalCameraY) continue; // too low down
                if (myP.Y > finalCameraY + Kinect.CalibrationSettings.Default.MaxY) continue; // too high up

                if (myP.Z < Kinect.CalibrationSettings.Default.MinZ) continue; // too close
                if (myP.Z > Kinect.CalibrationSettings.Default.MaxZ) continue; // too far back

                calScene.Points.Add(myP); // got here, so point is ok
            }

            // calculate centre point from max-min ranges in X,Z
            float maxX = calScene.Points[0].X, maxZ = calScene.Points[0].Z;
            float minX = calScene.Points[0].X, minZ = calScene.Points[0].Z;
            for (int i = 1; i < calScene.Points.Count; i++)
            {
                if (calScene.Points[i].X > maxX) maxX = calScene.Points[i].X;
                if (calScene.Points[i].Z > maxZ) maxZ = calScene.Points[i].Z;

                if (calScene.Points[i].X < minX) minX = calScene.Points[i].X;
                if (calScene.Points[i].Z < minZ) minZ = calScene.Points[i].Z;
            }
            var finalCameraX = (minX + maxX) / 2;
            var finalCameraZ = (minZ + maxZ) / 2;

            // set turntable radius (from average of X and Z ranges)
            var radius = (float)((maxX - minX) + (maxZ - minZ)) / 4;
            calibration.TurntableRadius = radius;

            // set final camera location
            calibration.Camera = new CameraLocation(
                new Point3D(-finalCameraX, -finalCameraY, -finalCameraZ),
                tilt);

            // use chosen samples
            calibration.Markers = markers;

            if (DebugSettings.Default.DebugMode)
            {
                // quick calibration tt cloud output
                KaptureLibrary.IO.PointCloudWriter.WritePLY(DebugSettings.Default.LogRoot + @"\calibration.ply", calScene);

                // log output
                StreamWriter log = new StreamWriter(DebugSettings.Default.LogRoot + @"\calibration.log");
                log.WriteLine("CAMERA = " + calibration.Camera.Origin.ToString());
                log.WriteLine("TILT = " + calibration.Camera.ElevationAngle.ToString());
                log.WriteLine("SEARCHSPACE2D = " + calibration.MarkerSearchSpace.ToString());
                log.WriteLine("TTRADIUS = " + calibration.TurntableRadius);
                log.WriteLine("MARKERS := ");
                foreach (var cube in calibration.Markers)
                {
                    log.Write(" (*) " + cube.Hue.ToString() + " HUE:" + cube.Hue.ToString());
                    log.Write(" +- " + cube.HueTolerance.ToString());
                    log.WriteLine(" ROTOFFSET: " + cube.RotationalOffset.ToString());
                }
                log.Close();
            }

            return calibration;
        }
        #endregion

    }
}
