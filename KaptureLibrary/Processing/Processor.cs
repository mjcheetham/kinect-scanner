using KaptureLibrary.IO;
using KaptureLibrary.Points;
using KaptureLibrary.Processing.DataFiltering;
using KaptureLibrary.Processing.ImageAnalysis;
using KaptureLibrary.Processing.PointConstruction;
using KaptureLibrary.Processing.Registration;
using KaptureLibrary.Tracking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Matrix = MathNet.Numerics.LinearAlgebra.Single.DenseMatrix;
using Vector = MathNet.Numerics.LinearAlgebra.Single.DenseVector;

namespace KaptureLibrary.Processing
{
    public static class Processor
    {
        #region Kalman Filter Modelling
        const float g = 9.80665f;
        const float stdDevProcessNoise = 0.20f;
        const float stdDevMeasurementNoise = 0.01f;
        const float mu = 0.003f;
        static Matrix FGenerator(float dt)
        {
            return Matrix.OfArray(new float[,] { { 1, dt }, { 0, 1 } });
        }

        static Matrix BGenerator(float dt)
        {
            return Matrix.OfArray(new float[,] { { 0.5f * dt * dt }, { dt } });
        }

        static Matrix QGenerator(float dt, float stdDevProcessNoise)
        {
            // B * B^T * stdDev_w ^ 2
            var dt2 = dt * dt;
            var dt3 = dt2 * dt;
            var BBT = Matrix.OfArray(new float[,] { { 0.25f * dt2 * dt2, 0.5f * dt3 },
                                                    {        0.5f * dt3,        dt2 }});
            return BBT * stdDevProcessNoise * stdDevProcessNoise;
        }

        static Matrix RGenerator(float stdDevMeasurementNoise)
        {
            return Matrix.OfArray(new float[,] { { stdDevMeasurementNoise * stdDevMeasurementNoise } });
        }
        #endregion

        #region Asynchronous Processing
        public static async Task<PointCloud> GenerateObject(Recording recording, IProgress<int> progress, CancellationToken ct)
        {
            // Quick access image formats
            var dFormat = recording.DepthFormat;
            var cFormat = recording.ColourFormat;

            #region Check for calibration data
            if (!recording.IsCalibratedRecording) { throw new IOException("Pre-calibrated recording required!"); }
            var calibration = recording.Calibration;
            #endregion

            #region Kalman Filter setup
            var kSetup = new KalmanSetup()
            {
                funcF = new Func<float, Matrix>(FGenerator),
                funcB = new Func<float, Matrix>(BGenerator),
                H = Matrix.OfArray(new float[,] { { 1, 0 } }),
                X0 = new Vector(new float[] { 0, 0 }),
                P0 = Matrix.OfArray(new float[,] { { 10, 0 }, { 0, 10 } }),
                funcQ = new Func<float, float, Matrix>(QGenerator),
                funcR = new Func<float, Matrix>(RGenerator),
                StdDevMeasurement = stdDevMeasurementNoise,
                StdDevProcess = stdDevProcessNoise
            };
            float R = calibration.TurntableRadius;
            float acc = -4 * mu * g / (3 * R);
            var accelerationVector = new Vector(new float[] { acc });
            #endregion

            #region Initialise pipeline stages
            var cv = new HueThresholder(calibration.Markers, calibration.MarkerSearchSpace);
            var geom = new PointCloudBuilder(calibration, recording.MappingParameters);
            var kf = new KalmanFilter(kSetup);
            var opt = new ICPMinimiser(true);
            #endregion

            // start new pipeline thread
            return await Task.Run<PointCloud>(() =>
                {
                    var returnCloud = new PointCloud();
                    PointCloud prevCloud = null;
                    float lastTime = 0;
                    double theta = 0;
                    Point3D lastx = new Point3D(0, 0, 0);

                    int advanceUnit = Processing.Settings.Default.ProcessEveryFrame ? 1 : 10;

                    #region KF Debug Logging
                    StreamWriter filterLog = null;
                    if (DebugSettings.Default.DebugMode)
                    {
                        filterLog = new StreamWriter(DebugSettings.Default.LogRoot + @"\filter.csv");
                        filterLog.AutoFlush = true;
                        filterLog.WriteLine("t,dt,dx,dtheta,theta,theta_KF,omega_KF");
                    }
                    #endregion

                    for (int i = 0; i < recording.NumberOfFrames; i += advanceUnit)
                    {
                        ct.ThrowIfCancellationRequested();
                        progress.Report(i);

                        #region Fetch
                        // Fetch frame from disk
                        float timestamp = recording.Timestamps[i];
                        var colour = recording.FetchColourFrame(i);
                        var depth = recording.FetchDepthFrame(i);
                        Frame f = new Frame(i, timestamp, depth, dFormat, colour, cFormat);
                        #endregion

                        #region Analyse (CV)
                        var markers2D = cv.IdentifyMarkers(f);
                        if (markers2D.Count == 0)
                        {
                            i -= (advanceUnit - 1);
                            continue;
                        }
                        #endregion

                        #region Build Geometry (GEOM)
                        PointCloud cloud;
                        KeyValuePair<TrackingMarker, Point3D> rotationMarker;
                        var geomSuccess = geom.ConstructCloud(f, markers2D, out cloud, out rotationMarker);
                        if (!geomSuccess)
                        {
                            i -= (advanceUnit - 1);
                            continue;
                        }
                        #endregion

                        #region Filter
                        // reposition marker based on offset
                        var polarX = CoordinateSystemConvertor.ToSpherical(rotationMarker.Value);
                        polarX.Azimuthal += (float)rotationMarker.Key.RotationalOffset;
                        var x = CoordinateSystemConvertor.ToCartesian(polarX);

                        // rotation approximation
                        var dx = ((Vector)x - lastx).Norm(3);
                        var dtheta = dx / calibration.TurntableRadius;

                        // update cumulative angle (with estimated angle)
                        theta += dtheta;

                        // calculate time delta
                        float dt = f.Timestamp - lastTime;

                        if (DebugSettings.Default.DebugMode)
                            filterLog.Write("{0},{1},{2},{3},{4},", f.Timestamp, dt, dx, dtheta, theta);

                        // calculate state vector
                        var stateVector = new Vector(new float[] { (float)theta });

                        // pass through kalman filter
                        var filteredStateVector = kf.Update(dt, stateVector, accelerationVector);

                        // extract angle from state vector
                        var filteredTheta = filteredStateVector[0];
                        var filteredOmega = filteredStateVector[1];

                        if (DebugSettings.Default.DebugMode)
                            filterLog.WriteLine("{0},{1}", filteredTheta, filteredOmega);

                        // update last time, last position and set filtered cumulative theta
                        theta = filteredTheta;
                        lastx = x;
                        lastTime = f.Timestamp;

                        #endregion

                        #region Optimise (OPT)
                        PointCloud optCloud = null;
                        if (prevCloud == null)
                        {
                            optCloud = cloud; // first frame; this is requires no optimising transformation
                        }
                        else if (prevCloud.Points.Count == 0)
                        {
                            optCloud = cloud; // no points in previous cloud (model); cannot align the current cloud to model
                        }
                        else
                        {
                            // Build best guess transformation matrix
                            float sinT = (float)Math.Sin(filteredTheta);
                            float cosT = (float)Math.Cos(filteredTheta);
                            var bestGuessTransformation = Matrix.Identity(4);
                            bestGuessTransformation[0, 0] = cosT;
                            bestGuessTransformation[0, 2] = -sinT;
                            bestGuessTransformation[2, 0] = sinT;
                            bestGuessTransformation[2, 2] = cosT;
                            // returned object is result of aligning cloud with prevCloud
                            optCloud = opt.Process(cloud, prevCloud, bestGuessTransformation);
                        }
                        prevCloud = cloud;
                        #endregion

                        #region Retire
                        returnCloud.Points.AddRange(optCloud.Points);
                        #endregion
                    }

                    if (DebugSettings.Default.DebugMode)
                        filterLog.Close();
                    return returnCloud;
                });
        }
        #endregion
    }
}
