using MathNet.Numerics.LinearAlgebra.Single;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaptureLibrary.Points
{
    public partial class PointCloud : ICloneable
    {
        public List<Point3D> Points { get; set; }
        public PointCloud()
        {
            this.Points = new List<Point3D>();
        }
        public PointCloud(ICollection<Point3D> points)
        {
            this.Points = new List<Point3D>(points);
        }

        #region Vector Collection Casting
        public static implicit operator List<Vector>(PointCloud cloud)
        {
            return cloud.Points.ConvertAll((p) => { return (Vector)p; });
        }
        #endregion

        #region ICloneable Memebers
        public object Clone()
        {
            var ps = new Point3D[this.Points.Count];
            this.Points.CopyTo(ps, 0);
            return new PointCloud(ps);
        }
        #endregion
    }
}
