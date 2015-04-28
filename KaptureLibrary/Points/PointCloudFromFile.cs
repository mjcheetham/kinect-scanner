using System;
using System.Collections.Generic;
using System.IO;

namespace KaptureLibrary.Points
{
    public partial class PointCloud
    {
        /// <summary>
        /// Create a point cloud from a saved PLY file.
        /// </summary>
        /// <param name="path">Existing PLY file location.</param>
        /// <returns>Point cloud of specified file.</returns>
        public static ICollection<Point3D> CreateFromFile(string path)
        {
            var ply = new StreamReader(path);
            var cloud = new List<Point3D>();

            int vertices;
            bool IsRgb = ReadHeader(ply, out vertices);
            for (int v = 0; v < vertices; v++)
            {
                var vertexString = ply.ReadLine();
                var vertexProperties = vertexString.Split(' ');

                Point3D p;

                float x = float.Parse(vertexProperties[0]);
                float y = float.Parse(vertexProperties[1]);
                float z = float.Parse(vertexProperties[2]);
                if (IsRgb)
                {
                    byte r = byte.Parse(vertexProperties[3]);
                    byte g = byte.Parse(vertexProperties[4]);
                    byte b = byte.Parse(vertexProperties[5]);
                    p = new Point3D(x, y, z, r, g, b);
                }
                else
                {
                    p = new Point3D(x, y, z);
                }

                cloud.Add(p);
            }

            return cloud;
        }

        private static bool ReadHeader(StreamReader ply, out int vertexCount)
        {
            if (ply.ReadLine() != "ply")
                throw new IOException();

            if (!ply.ReadLine().Contains("format ascii"))
                throw new IOException();

            if (!ply.ReadLine().Contains("Kapture"))
                System.Console.WriteLine("WARNING: This file was not made by a version of Kapture.");

            string line = ply.ReadLine();
            if (!line.Contains("element vertex"))
                throw new IOException();

            string numVerticesString = line.Split(' ')[2];

            // move to end of header
            bool rgb = false;
            do
            {
                line = ply.ReadLine();
                if (line == ("property uchar red")) rgb = true;
            } while (line != "end_header");

            vertexCount = Int32.Parse(numVerticesString);
            return rgb;
        }
    }
}
