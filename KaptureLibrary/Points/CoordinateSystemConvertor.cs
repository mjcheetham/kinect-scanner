using System;

namespace KaptureLibrary.Points
{
    public static class CoordinateSystemConvertor
    {
        #region Spherical Components
        public static float Azimuthal(Point3D pointXYZ)
        {
            return (float)Math.Atan2(pointXYZ.Z, pointXYZ.X); // increasing from +ve X-axis
        }

        public static float Polar(Point3D pointXYZ)
        {
            return (float)Math.Atan2(Math.Sqrt((pointXYZ.X * pointXYZ.X) + (pointXYZ.Z * pointXYZ.Z)), pointXYZ.Y); // increasing from +ve Y-axis
        }

        public static float Radius(Point3D pointXYZ)
        {
            return (float)Math.Sqrt((pointXYZ.X * pointXYZ.X) + (pointXYZ.Y * pointXYZ.Y) + (pointXYZ.Z * pointXYZ.Z));
        }
        #endregion

        #region Cartesian Components
        public static float XComponent(Point3DSpherical pointSpherical)
        {
            return pointSpherical.Radius * (float)Math.Sin(pointSpherical.Polar) * (float)Math.Cos(pointSpherical.Azimuthal);
        }

        public static float YComponent(Point3DSpherical pointSpherical)
        {
            return pointSpherical.Radius * (float)Math.Cos(pointSpherical.Polar);
        }

        public static float ZComponent(Point3DSpherical pointSpherical)
        {
            return pointSpherical.Radius * (float)Math.Sin(pointSpherical.Polar) * (float)Math.Sin(pointSpherical.Azimuthal);
        }
        #endregion

        #region Entire System Point Conversion
        public static Point3DSpherical ToSpherical(Point3D pointXYZ)
        {
            var r = Radius(pointXYZ);
            var polar = Polar(pointXYZ);
            var az = Azimuthal(pointXYZ);
            return new Point3DSpherical(r, polar, az);
        }

        public static Point3D ToCartesian(Point3DSpherical pointSpherical)
        {
            var x = XComponent(pointSpherical);
            var y = YComponent(pointSpherical);
            var z = ZComponent(pointSpherical);
            return new Point3D(x, y, z);
        }
        #endregion
    }
}
