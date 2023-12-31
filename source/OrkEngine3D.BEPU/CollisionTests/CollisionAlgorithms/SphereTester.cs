﻿using System;
using BEPUphysics.CollisionShapes.ConvexShapes;
 
using BEPUphysics.Settings;
using BEPUutilities;

namespace BEPUphysics.CollisionTests.CollisionAlgorithms
{
    ///<summary>
    /// Helper class to test spheres against each other.
    ///</summary>
    public static class SphereTester
    {
        /// <summary>
        /// Computes contact data for two spheres.
        /// </summary>
        /// <param name="a">First sphere.</param>
        /// <param name="b">Second sphere.</param>
        /// <param name="positionA">Position of the first sphere.</param>
        /// <param name="positionB">Position of the second sphere.</param>
        /// <param name="contact">Contact data between the spheres, if any.</param>
        /// <returns>Whether or not the spheres are touching.</returns>
        public static bool AreSpheresColliding(SphereShape a, SphereShape b, ref OrkEngine3D.Mathematics.Vector3 positionA, ref OrkEngine3D.Mathematics.Vector3 positionB, out ContactData contact)
        {
            contact = new ContactData();

            float radiusSum = a.collisionMargin + b.collisionMargin;
            OrkEngine3D.Mathematics.Vector3 centerDifference;
            Vector3Ex.Subtract(ref positionB, ref positionA, out centerDifference);
            float centerDistance = centerDifference.LengthSquared();

            if (centerDistance < (radiusSum + CollisionDetectionSettings.maximumContactDistance) * (radiusSum + CollisionDetectionSettings.maximumContactDistance))
            {
                //In collision!

                if (radiusSum > Toolbox.Epsilon) //This would be weird, but it is still possible to cause a NaN.
                    Vector3Ex.Multiply(ref centerDifference, a.collisionMargin / (radiusSum), out  contact.Position);
                else contact.Position = new OrkEngine3D.Mathematics.Vector3();
                Vector3Ex.Add(ref contact.Position, ref positionA, out contact.Position);

                centerDistance = (float)Math.Sqrt(centerDistance);
                if (centerDistance > Toolbox.BigEpsilon)
                {
                    Vector3Ex.Divide(ref centerDifference, centerDistance, out contact.Normal);
                }
                else
                {
                    contact.Normal = Toolbox.UpVector;
                }
                contact.PenetrationDepth = radiusSum - centerDistance;

                return true;

            }
            return false;
        }
    }
}
