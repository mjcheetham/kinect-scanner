using KaptureLibrary.Points;
using System;

namespace KaptureLibrary.ShapeAndMeasure
{
    public abstract class Volume
    {
        /// <summary>
        /// Test is a point is within the volume.
        /// </summary>
        /// <param name="point">Test point.</param>
        /// <returns>If the <paramref name="point"/> is within the volume.</returns>
        public abstract bool IsInside(Point3D point);
    }

    /// <summary>
    /// Represents a standard Cartesian axes aligned cuboid.
    /// </summary>
    public class Cuboid : Volume
    {
        public RangeFloat RangeX { get; private set; }
        public RangeFloat RangeY { get; private set; }
        public RangeFloat RangeZ { get; private set; }

        public Cuboid(RangeFloat x, RangeFloat y, RangeFloat z)
        {
            this.RangeX = x; this.RangeY = y; this.RangeZ = z;
        }

        public Cuboid(Point3D topLeft, Point3D bottomRight)
        {
            this.RangeX = new RangeFloat(topLeft.X, bottomRight.X);
            this.RangeY = new RangeFloat(topLeft.Y, bottomRight.Y);
            this.RangeZ = new RangeFloat(topLeft.Z, bottomRight.Z);
        }

        public override bool IsInside(Point3D point)
        {
            return (this.RangeX.IsInRange(point.X) && this.RangeY.IsInRange(point.Y) && this.RangeZ.IsInRange(point.Z));
        }
    }

    /// <summary>
    /// Represents a Z-axis aligned Cylinder.
    /// </summary>
    public class Cylinder : Volume
    {
        public float Bottom { get; set; }
        public float Top { get; set; }
        public float Radius { get; set; }
        public Point3D BaseCentre { get; set; }

        public Cylinder(Point3D baseCentre, float height, float radius)
        {
            this.BaseCentre = baseCentre;
            this.Radius = radius;
            this.Bottom = this.BaseCentre.Y;
            this.Top = this.Bottom + height;
        }

        public override bool IsInside(Point3D point)
        {
            var dxSqr = Math.Pow(point.X - this.BaseCentre.X, 2);
            var dzSqr = Math.Pow(point.Z - this.BaseCentre.Z, 2);
            var rSqr = Math.Pow(this.Radius, 2);
            bool withinRadius = (dxSqr + dzSqr) < rSqr;

            return (point.Y < this.Top && point.Y > this.Bottom && withinRadius);
        }
    }
}