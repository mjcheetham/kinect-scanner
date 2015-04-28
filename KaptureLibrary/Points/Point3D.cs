using KaptureLibrary.ShapeAndMeasure;
using MathNet.Numerics.LinearAlgebra.Single;
using System;

namespace KaptureLibrary.Points
{
    /// <summary>
    /// Represents a 3D point in the single precision floating-point (<see cref="Single"/>) field.
    /// </summary>
    [System.Serializable]
    public class Point3D : IPoint, ITransformable3D, ICloneable
    {
        private DenseVector v;

        public virtual float X { get { return v[0]; } set { v[0] = value; } }
        public virtual float Y { get { return v[1]; } set { v[1] = value; } }
        public virtual float Z { get { return v[2]; } set { v[2] = value; } }

        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public float Confidence { get; set; }

        #region Constructors
        public Point3D(float x, float y, float z)
        {
            this.v = new DenseVector(new float[] { x, y, z });
        }
        public Point3D(float x, float y, float z, byte r, byte g, byte b)
            : this(x, y, z)
        {
            this.R = r; this.G = g; this.B = b;
        }
        #endregion

        #region ToString Override
        public override string ToString()
        {
            return "{ " + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() +
                " } : [ " + R.ToString() + ", " + G.ToString() + ", " + B.ToString() + " ]";
        }
        #endregion

        #region IPoint Members
        public string GetComponentsAsString()
        {
            return X.ToString() + " " + Y.ToString() + " " + Z.ToString() + " " +
                R.ToString() + " " + G.ToString() + " " + B.ToString();
        }
        #endregion

        #region Vector Casts
        public static implicit operator DenseVector(Point3D p)
        {
            return p.v;
        }
        public static implicit operator Point3D(DenseVector v)
        {
            return new Point3D(v[0], v[1], v[2]);
        }
        #endregion

        #region ITransformable3D Members
        public void ApplyAffineTransformation(Matrix T)
        {
            var u = new DenseVector(new float[] { this.X, this.Y, this.Z, 1 });
            var v = T * u;
            this.X = v[0]; this.Y = v[1]; this.Z = v[2];
        }
        public void RotateAboutAxis(Axis axis, double angle)
        {
            double xp = this.X, yp = this.Y, zp = this.Z;
            double cosT = System.Math.Cos(angle);
            double sinT = System.Math.Sin(angle);
            switch (axis)
            {
                case Axis.X:
                    {
                        yp = (this.Y * cosT - this.Z * sinT);
                        zp = (this.Y * sinT + this.Z * cosT);
                        break;
                    }
                case Axis.Y:
                    {
                        xp = (this.X * cosT - this.Z * sinT);
                        zp = (this.X * sinT + this.Z * cosT);
                        break;
                    }
                case Axis.Z:
                    {
                        xp = (this.X * cosT - this.Y * sinT);
                        yp = (this.X * sinT + this.Y * cosT);
                        break;
                    }
                default:
                    throw new System.ArgumentException("Must specify an axis to rotate about.");
            }
            this.X = (float)xp; this.Y = (float)yp; this.Z = (float)zp;
        }
        public void Translate(float x, float y, float z)
        {
            this.X += x;
            this.Y += y;
            this.Z += z;
        }
        public void Scale(float x, float y, float z)
        {
            this.X *= x;
            this.Y *= y;
            this.Z *= z;
        }
        #endregion

        #region ICloneable Memebers
        /// <summary>
        /// Performs a deep clone of the Point3D structure.
        /// </summary>
        /// <returns>Deep Clone</returns>
        public object Clone()
        {
            return new Point3D(this.X, this.Y, this.Z, this.R, this.G, this.B);
        }
        #endregion
    }
}
