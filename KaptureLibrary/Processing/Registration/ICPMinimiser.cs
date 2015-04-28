using KaptureLibrary.Points;
using KaptureLibrary.Trees.KdTree;
using MathNet.Numerics.LinearAlgebra.Single;
using System;
using System.Collections.Generic;
using System.IO;

namespace KaptureLibrary.Processing.Registration
{
    internal class PointPair<PointT>
    {
        public PointT Scene { get; set; }
        public PointT Model { get; set; }
        public PointPair(PointT scene, PointT model) { this.Scene = scene; this.Model = model; }
    }

    public class ICPMinimiser
    {
        #region Private Fields
        private float minError;
        private int maxIter;
        private bool UseSampling;
        #endregion

        #region Constructors
        public ICPMinimiser(bool useSampling)
        {
            this.minError = Registration.Settings.Default.MinimumRegistrationError;
            this.maxIter = Registration.Settings.Default.MaximumRegistrationIterations;
            this.UseSampling = useSampling;
            if (DebugSettings.Default.DebugMode)
            {
                log = new StreamWriter(DebugSettings.Default.LogRoot + @"\icp.csv");
                log.WriteLine("Pair,Iterations,LastError");
                log.AutoFlush = true;
            }
        }
        #endregion

        private StreamWriter log;
        private int logN = 0;

        internal PointCloud Process(PointCloud cloud, PointCloud prevCloud, Matrix bestGuessTransformation)
        {
            if (cloud.Points.Count < 4) return cloud;

            var T_0 = (DenseMatrix)bestGuessTransformation;

            // sample point clouds
            PointCloud sampleScene;
            PointCloud model = prevCloud;
            if (this.UseSampling)
            {
                var samplingParams = new DistributedSamplingParameters(
                    (int)(cloud.Points.Count * Registration.Settings.Default.SamplingProportion),
                    Registration.Settings.Default.MinimumSamplingDistance);
                sampleScene = DistributedSampler.GetRandomSample(cloud, samplingParams);
            }
            else
            {
                sampleScene = cloud;
            }
            

            // run ICP on sampled data
            var T_opt = CalculateTransformation(sampleScene, model, T_0);

            // create deep clone of unoptimised cloud
            var optCloud = (PointCloud)cloud.Clone();

            // apply T_opt to entire cloud and return
            for (int i = 0; i < optCloud.Points.Count; i++) optCloud.Points[i].ApplyAffineTransformation(T_opt);

            // return optimised cloud
            return optCloud;
        }

        private DenseMatrix CalculateTransformation(PointCloud scene_in, PointCloud model_in, DenseMatrix T_0)
        {
            var T = new DenseMatrix[maxIter];
            T[0] = T_0;

            var startTime = DateTime.Now;

            // perform deep copy of input scene because we modify it directly in transform calculation
            var scene = (PointCloud)scene_in.Clone();
            // no need to clone the model since we don't modify it
            var model = model_in;

            // precompute KdTree of model (this doesn't change for each model-scene pair)
            var kdModel = KdTree.Construct(3, (List<Vector>)model);
            int k = 1;
            float err = 0f;
            for (; k < maxIter; k++)
            {
                // Step 1 - Apply previous transform T_k-1
                for (int i = 0; i < scene.Points.Count; i++) scene.Points[i].ApplyAffineTransformation(T[k - 1]);

                // Step 2 - Find correspondances
                var pairs = new PointPair<Vector>[scene.Points.Count];
                for (int i = 0; i < scene.Points.Count; i++)
                {
                    var nn = kdModel.FindNearestNeighbour(scene.Points[i]);
                    pairs[i] = new PointPair<Vector>(scene.Points[i], nn);
                }

                // Step 3 - Check convergence criterion
                err = 0;
                foreach (var pair in pairs)
                {
                    var v = pair.Scene - pair.Model;
                    // measure 'distance' or error between colour of these points
                    err += (float)Math.Sqrt(v.DotProduct(v));
                }
                err /= scene.Points.Count; // average error
                if (err <= this.minError) break; // transform T_k-1 was best! terminate.
                if (k + 1 == maxIter) break; // pointless compute ahead? else, must compute better transform!...

                // Step 4 - Calculate composite matrix H
                var H = new DenseMatrix(3);
                for (int i = 0; i < pairs.Length; i++)
                {
                    H += (DenseMatrix)DenseVector.OuterProduct(pairs[i].Scene, pairs[i].Model);
                }

                // Step 5 - Decompose H-matrix using SVD; calculate 3x3 rotation matrix R
                var svd = H.Svd(true);
                // Check if special reflection case (det(V) < 0)
                var VT = svd.VT();
                if (VT.Transpose().Determinant() < 0) VT.SetRow(2, -VT.Row(2)); // negate row 3
                DenseMatrix R = (DenseMatrix)(svd.U() * VT);

                // Step 6 - Calculate complete affine transformation matrix, T           
                // --> R_ext   = 4x4 transformation matrix using R rotation
                var R_ext = DenseMatrix.Identity(4);
                R_ext.SetSubMatrix(0, 3, 0, 3, R.Transpose());

                // --> T       = C_model R_ext C_scene
                T[k] = R_ext; // new transform ready (from T[k-1] * Scene -> Model)
            }

            // reached optimal transformation sequence, return compound matrix T_opt = T_n * ... * T_1 * T_0
            var T_opt = DenseMatrix.Identity(4);
            foreach (var Tk in T)
            {
                if (Tk == null) break;
                T_opt = Tk * T_opt;
            }

            if (DebugSettings.Default.DebugMode) this.log.WriteLine("{0},{1},{2}", this.logN++, k, err);

            return T_opt;
        }

    }
}