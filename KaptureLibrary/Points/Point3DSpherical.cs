using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KaptureLibrary.Points
{
    public class Point3DSpherical : Point3D
    {
        /// <summary>
        /// R; Length.
        /// </summary>
        public float Radius { get { return base.X; } set { base.X = value; } }
        /// <summary>
        /// Theta; Inclination; Angle down from Y (vertical) axis.
        /// </summary>
        public float Polar { get { return base.Y; } set { base.Y = value; } }
        /// <summary>
        /// Phi; Angle centred around Y (vertical) axis; In XZ plane.
        /// </summary>
        public float Azimuthal { get { return base.Z; } set { base.Z = value; } }

        public Point3DSpherical(float radius, float polar, float azimuthal) : base(radius, polar, azimuthal) { }
    }
}
