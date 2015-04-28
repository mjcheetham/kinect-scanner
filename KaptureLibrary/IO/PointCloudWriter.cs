using KaptureLibrary.Points;
using System;
using System.Collections.Generic;
using System.IO;

namespace KaptureLibrary.IO
{
    public static class PointCloudWriter
    {
        /// <summary>
        /// Save a full RGB <see cref="PointCloud"/>s as a PLY file to the specified path.
        /// </summary>
        /// <param name="outputPath">Location to save the PLY file.</param>
        /// <param name="cloud">Point cloud to save.</param>
        public static void WritePLY(string outputPath, PointCloud cloud)
        {
            if (cloud == null) throw new ArgumentNullException("cloud");
            WritePLY(outputPath, cloud.Points);
        }
        /// <summary>
        /// Save a full collection of <see cref="Point3D"/>s as a PLY file to the specified path.
        /// </summary>
        /// <param name="outputPath">Location to save the PLY file.</param>
        /// <param name="points">Collection of points to save.</param>
        public static void WritePLY(string outputPath, ICollection<Point3D> points)
        {
            var ply = new StreamWriter(outputPath);
            WriteHeader(ply, points.Count, true);
            foreach (Point3D p in points)
            {
                ply.WriteLine(p.GetComponentsAsString());
            }
            WriteFooter(ply);
            ply.Close();
        }

        private static void WriteFooter(StreamWriter ply)
        {
            ply.WriteLine("0 0 0 0");
        }

        private static void WriteHeader(StreamWriter ply, int numVertices, bool rgb)
        {
            ply.WriteLine("ply\nformat ascii 1.0\ncomment Made by Kapture 3");
            ply.Write("element vertex ");
            ply.WriteLine(numVertices);
            ply.WriteLine("property float x\nproperty float y\nproperty float z");
            if (rgb) ply.WriteLine("property uchar red\nproperty uchar green\nproperty uchar blue");
            ply.WriteLine("element face 0");
            ply.WriteLine("property list uchar int vertex_index\nend_header");
        }
    }
}
