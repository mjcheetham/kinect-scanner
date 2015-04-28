using KaptureLibrary.Points;
using KaptureLibrary.Trees.KdTree;
using System;
using System.Collections.Generic;
using Vector = MathNet.Numerics.LinearAlgebra.Single.Vector;

namespace KaptureLibrary.Processing.Registration
{
    public class SamplingException : Exception
    {
        public SamplingException(string message) : base(message) { }
    }

    public struct DistributedSamplingParameters
    {
        public int NumberSamplePoints { get; set; }
        public float MinimumAcceptableDistance { get; set; }
        public DistributedSamplingParameters(int numPoints, float minRange) : this()
        { this.NumberSamplePoints = numPoints; this.MinimumAcceptableDistance = minRange; }
    }

    public static class DistributedSampler
    {
        public static PointCloud GetRandomSample(PointCloud cloud, DistributedSamplingParameters samplingParams)
        {
            PointCloud sample = new PointCloud();
            var indices = SelectPointIndices(cloud, samplingParams.NumberSamplePoints, samplingParams.MinimumAcceptableDistance);
            foreach (var i in indices) sample.Points.Add((Point3D)cloud.Points[i].Clone());
            return sample;
        }

        private static List<int> SelectPointIndices(PointCloud cloud, int numPoints, float minRange)
        {
            List<int> indices = new List<int>();
            int maxIter = (int)(cloud.Points.Count * Registration.Settings.Default.MaxSamplingIterationProportion);

            Random rand = new Random();

            KdTreeNode kdRoot = new KdTreeNode(cloud.Points[rand.Next(0, cloud.Points.Count - 1)]);
            var kd = new KdTree(3, kdRoot);

            for (int i = 1; indices.Count < numPoints; i++)
            {
                if (!(i < maxIter)) throw new SamplingException("Please relax your sampling threshold parameters!");

                int randomIndex = rand.Next(0, cloud.Points.Count - 1);
                if (indices.Contains(randomIndex)) continue;

                Point3D p = cloud.Points[randomIndex];
                var nn = new List<Vector>(kd.FindInRange(p, minRange));

                if (nn.Count == 0)
                {
                    indices.Add(randomIndex);
                    kd.Add(p);
                }
            }
            return indices;
        }
    }
}
