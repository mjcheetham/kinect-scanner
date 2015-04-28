using KaptureLibrary.Points;
using System.Collections.Generic;
using DMatrix = MathNet.Numerics.LinearAlgebra.Single.DenseMatrix;
using DVector = MathNet.Numerics.LinearAlgebra.Single.DenseVector;
using Vector = MathNet.Numerics.LinearAlgebra.Single.Vector;

namespace KaptureLibrary.ShapeAndMeasure
{
    /// <summary>
    /// Represents a mathematical plane.
    /// </summary>
    public class Plane
    {
        #region Properties
        /// <summary>
        /// Normal vector to the plane.
        /// </summary>
        public DVector Normal { get; set; }
        /// <summary>
        /// A point within the plane.
        /// </summary>
        public DVector PlanarPoint { get; set; }
        #endregion

        #region Constructors
        public Plane(DVector normal, DVector pointInThePlane)
        {
            this.Normal = (DVector)normal.Normalize(3);
            this.PlanarPoint = pointInThePlane;
        }

        public Plane(DVector pointA, DVector pointB, DVector pointC)
        {
            var u = pointC - pointA;
            var v = pointC - pointB;

            this.Normal = (DVector)CrossProduct(u, v).Normalize(3);
            this.PlanarPoint = pointA;
        }
        #endregion

        #region Overrrides
        public override string ToString()
        {
            return "Plane: p = " + this.PlanarPoint.ToString() + ", n = " + this.Normal.ToString();
        }
        #endregion

        #region Math Helpers
        private Vector CrossProduct(Vector u, Vector v)
        {
            var iArr = new float[,] { { u[1], u[2] }, { v[1], v[2] } };
            var jArr = new float[,] { { u[2], u[0] }, { v[2], v[0] } };
            var kArr = new float[,] { { u[0], u[1] }, { v[0], v[1] } };

            var i = DMatrix.OfArray(iArr);
            var j = DMatrix.OfArray(jArr);
            var k = DMatrix.OfArray(kArr);

            var nArr = new float[] { i.Determinant(), j.Determinant(), k.Determinant() };

            return new DVector(nArr);
        }
        #endregion

        #region Plane/Point Comparisions
        private float ComparePointToPlane(Point3D p)
        {
            return this.Normal.DotProduct(p - this.PlanarPoint);
        }
        /// <summary>
        /// Selects the points from a collection which are within the plane.
        /// </summary>
        /// <param name="points">Collection of points to test.</param>
        /// <returns>Those from the <paramref name="points"/> collection which are within the plane.</returns>
        /// <remarks>Within the limit of the current macheps.</remarks>
        public ICollection<Point3D> PointsInPlane(ICollection<Point3D> points)
        {
            var selected = new List<Point3D>();
            foreach (var p in points)
            {
                if (MathNet.Numerics.Precision.AlmostEqual(ComparePointToPlane(p), 0f)) selected.Add(p);
            }
            return selected;
        }
        /// <summary>
        /// Selects the points from a collection which are in front of the plane.
        /// </summary>
        /// <param name="points">Collection of points to test.</param>
        /// <returns>Those from the <paramref name="points"/> collection which are in front of the plane.</returns>
        /// <remarks>'In front' refers to 'in the direction of the plane normal'.</remarks>
        public ICollection<Point3D> PointsInFrontOfPlane(ICollection<Point3D> points)
        {
            var selected = new List<Point3D>();
            foreach (var p in points)
            {
                if (this.ComparePointToPlane(p) > 0) selected.Add(p);
            }
            return selected;
        }
        /// <summary>
        /// Selects the points from a collection which are behind the plane.
        /// </summary>
        /// <param name="points">Collection of points to test.</param>
        /// <returns>Those from the <paramref name="points"/> collection which are behind the plane.</returns>
        /// <remarks>'Behind' refers to 'in the opposite direction of the plane normal'.</remarks>
        public ICollection<Point3D> PointsBehindPlane(ICollection<Point3D> points)
        {
            var selected = new List<Point3D>();
            foreach (var p in points)
            {
                if (this.ComparePointToPlane(p) < 0) selected.Add(p);
            }
            return selected;
        }
        #endregion
    }
}