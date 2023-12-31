﻿namespace BEPUutilities
{
    /// <summary>
    /// Provides XNA-like plane functionality.
    /// </summary>
    public struct Plane
    {
        /// <summary>
        /// Normal of the plane.
        /// </summary>
        public OrkEngine3D.Mathematics.Vector3 Normal;
        /// <summary>
        /// Negative distance to the plane from the origin along the normal.
        /// </summary>
        public float D;

        /// <summary>
        /// Constructs a new plane.
        /// </summary>
        /// <param name="normal">Normal of the plane.</param>
        /// <param name="d">Negative distance to the plane from the origin along the normal</param>
        public Plane(OrkEngine3D.Mathematics.Vector3 normal, float d)
        {
            this.Normal = normal;
            this.D = d;
        }

        /// <summary>
        /// Gets the dot product of the position offset from the plane along the plane's normal.
        /// </summary>
        /// <param name="v">Position to compute the dot product of.</param>
        /// <param name="dot">Dot product.</param>
        public void DotCoordinate(ref OrkEngine3D.Mathematics.Vector3 v, out float dot)
        {
            dot = Normal.X * v.X + Normal.Y * v.Y + Normal.Z * v.Z + D;
        }
    }
}
