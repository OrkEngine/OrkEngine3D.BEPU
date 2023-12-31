using System;
using System.Collections.Generic;
using BEPUphysics.CollisionTests;
using BEPUutilities.DataStructures;
using BEPUutilities.ResourceManagement;

namespace BEPUutilities
{
    //TODO: It would be nice to split and improve this monolith into individually superior, organized components.


    /// <summary>
    /// Helper class with many algorithms for intersection testing and 3D math.
    /// </summary>
    public static class Toolbox
    {
        /// <summary>
        /// Large tolerance value. Defaults to 1e-5f.
        /// </summary>
        public static float BigEpsilon = 1E-5f;

        /// <summary>
        /// Tolerance value. Defaults to 1e-7f.
        /// </summary>
        public static float Epsilon = 1E-7f;

        /// <summary>
        /// Represents an invalid OrkEngine3D.Mathematics.Vector3.
        /// </summary>
        public static readonly OrkEngine3D.Mathematics.Vector3 NoVector = new OrkEngine3D.Mathematics.Vector3(-float.MaxValue, -float.MaxValue, -float.MaxValue);

        /// <summary>
        /// Reference for a vector with dimensions (0,0,1).
        /// </summary>
        public static OrkEngine3D.Mathematics.Vector3 BackVector = Vector3Ex.Backward;

        /// <summary>
        /// Reference for a vector with dimensions (0,-1,0).
        /// </summary>
        public static OrkEngine3D.Mathematics.Vector3 DownVector = Vector3Ex.Down;

        /// <summary>
        /// Reference for a vector with dimensions (0,0,-1).
        /// </summary>
        public static OrkEngine3D.Mathematics.Vector3 ForwardVector = Vector3Ex.Forward;

        /// <summary>
        /// Refers to the identity quaternion.
        /// </summary>
        public static OrkEngine3D.Mathematics.Quaternion IdentityOrientation = OrkEngine3D.Mathematics.Quaternion.Identity;

        /// <summary>
        /// Reference for a vector with dimensions (-1,0,0).
        /// </summary>
        public static OrkEngine3D.Mathematics.Vector3 LeftVector = Vector3Ex.Left;

        /// <summary>
        /// Reference for a vector with dimensions (1,0,0).
        /// </summary>
        public static OrkEngine3D.Mathematics.Vector3 RightVector = Vector3Ex.Right;

        /// <summary>
        /// Reference for a vector with dimensions (0,1,0).
        /// </summary>
        public static OrkEngine3D.Mathematics.Vector3 UpVector = Vector3Ex.Up;

        /// <summary>
        /// OrkEngine3D.Mathematics.Matrix4x4 containing zeroes for every element.
        /// </summary>
        public static OrkEngine3D.Mathematics.Matrix4x4 ZeroMatrix = new OrkEngine3D.Mathematics.Matrix4x4(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        /// <summary>
        /// Reference for a vector with dimensions (0,0,0).
        /// </summary>
        public static OrkEngine3D.Mathematics.Vector3 ZeroVector = OrkEngine3D.Mathematics.Vector3.Zero;

        /// <summary>
        /// Refers to the rigid identity transformation.
        /// </summary>
        public static RigidTransform RigidIdentity = RigidTransform.Identity;

        #region Segment/Ray-Triangle Tests

        /// <summary>
        /// Determines the intersection between a ray and a triangle.
        /// </summary>
        /// <param name="ray">Ray to test.</param>
        /// <param name="maximumLength">Maximum length to travel in units of the direction's length.</param>
        /// <param name="a">First vertex of the triangle.</param>
        /// <param name="b">Second vertex of the triangle.</param>
        /// <param name="c">Third vertex of the triangle.</param>
        /// <param name="hitClockwise">True if the the triangle was hit on the clockwise face, false otherwise.</param>
        /// <param name="hit">Hit data of the ray, if any</param>
        /// <returns>Whether or not the ray and triangle intersect.</returns>
        public static bool FindRayTriangleIntersection(ref Ray ray, float maximumLength, ref OrkEngine3D.Mathematics.Vector3 a, ref OrkEngine3D.Mathematics.Vector3 b, ref OrkEngine3D.Mathematics.Vector3 c, out bool hitClockwise, out RayHit hit)
        {
            hitClockwise = false;
            hit = new RayHit();
            OrkEngine3D.Mathematics.Vector3 ab, ac;
            Vector3Ex.Subtract(ref b, ref a, out ab);
            Vector3Ex.Subtract(ref c, ref a, out ac);

            Vector3Ex.Cross(ref ab, ref ac, out hit.Normal);
            if (hit.Normal.LengthSquared() < Epsilon)
                return false; //Degenerate triangle!

            float d;
            Vector3Ex.Dot(ref ray.Direction, ref hit.Normal, out d);
            d = -d;

            hitClockwise = d >= 0;

            OrkEngine3D.Mathematics.Vector3 ap;
            Vector3Ex.Subtract(ref ray.Position, ref a, out ap);

            Vector3Ex.Dot(ref ap, ref hit.Normal, out hit.T);
            hit.T /= d;
            if (hit.T < 0 || hit.T > maximumLength)
                return false;//Hit is behind origin, or too far away.

            Vector3Ex.Multiply(ref ray.Direction, hit.T, out hit.Location);
            Vector3Ex.Add(ref ray.Position, ref hit.Location, out hit.Location);

            // Compute barycentric coordinates
            Vector3Ex.Subtract(ref hit.Location, ref a, out ap);
            float ABdotAB, ABdotAC, ABdotAP;
            float ACdotAC, ACdotAP;
            Vector3Ex.Dot(ref ab, ref ab, out ABdotAB);
            Vector3Ex.Dot(ref ab, ref ac, out ABdotAC);
            Vector3Ex.Dot(ref ab, ref ap, out ABdotAP);
            Vector3Ex.Dot(ref ac, ref ac, out ACdotAC);
            Vector3Ex.Dot(ref ac, ref ap, out ACdotAP);

            float denom = 1 / (ABdotAB * ACdotAC - ABdotAC * ABdotAC);
            float u = (ACdotAC * ABdotAP - ABdotAC * ACdotAP) * denom;
            float v = (ABdotAB * ACdotAP - ABdotAC * ABdotAP) * denom;

            return (u >= -Toolbox.BigEpsilon) && (v >= -Toolbox.BigEpsilon) && (u + v <= 1 + Toolbox.BigEpsilon);

        }

        /// <summary>
        /// Determines the intersection between a ray and a triangle.
        /// </summary>
        /// <param name="ray">Ray to test.</param>
        /// <param name="maximumLength">Maximum length to travel in units of the direction's length.</param>
        /// <param name="sidedness">Sidedness of the triangle to test.</param>
        /// <param name="a">First vertex of the triangle.</param>
        /// <param name="b">Second vertex of the triangle.</param>
        /// <param name="c">Third vertex of the triangle.</param>
        /// <param name="hit">Hit data of the ray, if any</param>
        /// <returns>Whether or not the ray and triangle intersect.</returns>
        public static bool FindRayTriangleIntersection(ref Ray ray, float maximumLength, TriangleSidedness sidedness, ref OrkEngine3D.Mathematics.Vector3 a, ref OrkEngine3D.Mathematics.Vector3 b, ref OrkEngine3D.Mathematics.Vector3 c, out RayHit hit)
        {
            hit = new RayHit();
            OrkEngine3D.Mathematics.Vector3 ab, ac;
            Vector3Ex.Subtract(ref b, ref a, out ab);
            Vector3Ex.Subtract(ref c, ref a, out ac);

            Vector3Ex.Cross(ref ab, ref ac, out hit.Normal);
            if (hit.Normal.LengthSquared() < Epsilon)
                return false; //Degenerate triangle!

            float d;
            Vector3Ex.Dot(ref ray.Direction, ref hit.Normal, out d);
            d = -d;
            switch (sidedness)
            {
                case TriangleSidedness.DoubleSided:
                    if (d <= 0) //Pointing the wrong way.  Flip the normal.
                    {
                        Vector3Ex.Negate(ref hit.Normal, out hit.Normal);
                        d = -d;
                    }
                    break;
                case TriangleSidedness.Clockwise:
                    if (d <= 0) //Pointing the wrong way.  Can't hit.
                        return false;

                    break;
                case TriangleSidedness.Counterclockwise:
                    if (d >= 0) //Pointing the wrong way.  Can't hit.
                        return false;

                    Vector3Ex.Negate(ref hit.Normal, out hit.Normal);
                    d = -d;
                    break;
            }

            OrkEngine3D.Mathematics.Vector3 ap;
            Vector3Ex.Subtract(ref ray.Position, ref a, out ap);

            Vector3Ex.Dot(ref ap, ref hit.Normal, out hit.T);
            hit.T /= d;
            if (hit.T < 0 || hit.T > maximumLength)
                return false;//Hit is behind origin, or too far away.

            Vector3Ex.Multiply(ref ray.Direction, hit.T, out hit.Location);
            Vector3Ex.Add(ref ray.Position, ref hit.Location, out hit.Location);

            // Compute barycentric coordinates
            Vector3Ex.Subtract(ref hit.Location, ref a, out ap);
            float ABdotAB, ABdotAC, ABdotAP;
            float ACdotAC, ACdotAP;
            Vector3Ex.Dot(ref ab, ref ab, out ABdotAB);
            Vector3Ex.Dot(ref ab, ref ac, out ABdotAC);
            Vector3Ex.Dot(ref ab, ref ap, out ABdotAP);
            Vector3Ex.Dot(ref ac, ref ac, out ACdotAC);
            Vector3Ex.Dot(ref ac, ref ap, out ACdotAP);

            float denom = 1 / (ABdotAB * ACdotAC - ABdotAC * ABdotAC);
            float u = (ACdotAC * ABdotAP - ABdotAC * ACdotAP) * denom;
            float v = (ABdotAB * ACdotAP - ABdotAC * ABdotAP) * denom;

            return (u >= -Toolbox.BigEpsilon) && (v >= -Toolbox.BigEpsilon) && (u + v <= 1 + Toolbox.BigEpsilon);

        }

        /// <summary>
        /// Finds the intersection between the given segment and the given plane defined by three points.
        /// </summary>
        /// <param name="a">First endpoint of segment.</param>
        /// <param name="b">Second endpoint of segment.</param>
        /// <param name="d">First vertex of a triangle which lies on the plane.</param>
        /// <param name="e">Second vertex of a triangle which lies on the plane.</param>
        /// <param name="f">Third vertex of a triangle which lies on the plane.</param>
        /// <param name="q">Intersection point.</param>
        /// <returns>Whether or not the segment intersects the plane.</returns>
        public static bool GetSegmentPlaneIntersection(OrkEngine3D.Mathematics.Vector3 a, OrkEngine3D.Mathematics.Vector3 b, OrkEngine3D.Mathematics.Vector3 d, OrkEngine3D.Mathematics.Vector3 e, OrkEngine3D.Mathematics.Vector3 f, out OrkEngine3D.Mathematics.Vector3 q)
        {
            Plane p;
            p.Normal = OrkEngine3D.Mathematics.Vector3.Cross(e - d, f - d);
            p.D = Vector3Ex.Dot(p.Normal, d);
            float t;
            return GetSegmentPlaneIntersection(a, b, p, out t, out q);
        }

        /// <summary>
        /// Finds the intersection between the given segment and the given plane.
        /// </summary>
        /// <param name="a">First endpoint of segment.</param>
        /// <param name="b">Second enpoint of segment.</param>
        /// <param name="p">Plane for comparison.</param>
        /// <param name="q">Intersection point.</param>
        /// <returns>Whether or not the segment intersects the plane.</returns>
        public static bool GetSegmentPlaneIntersection(OrkEngine3D.Mathematics.Vector3 a, OrkEngine3D.Mathematics.Vector3 b, Plane p, out OrkEngine3D.Mathematics.Vector3 q)
        {
            float t;
            return GetLinePlaneIntersection(ref a, ref b, ref p, out t, out q) && t >= 0 && t <= 1;
        }

        /// <summary>
        /// Finds the intersection between the given segment and the given plane.
        /// </summary>
        /// <param name="a">First endpoint of segment.</param>
        /// <param name="b">Second endpoint of segment.</param>
        /// <param name="p">Plane for comparison.</param>
        /// <param name="t">Interval along segment to intersection.</param>
        /// <param name="q">Intersection point.</param>
        /// <returns>Whether or not the segment intersects the plane.</returns>
        public static bool GetSegmentPlaneIntersection(OrkEngine3D.Mathematics.Vector3 a, OrkEngine3D.Mathematics.Vector3 b, Plane p, out float t, out OrkEngine3D.Mathematics.Vector3 q)
        {
            return GetLinePlaneIntersection(ref a, ref b, ref p, out t, out q) && t >= 0 && t <= 1;
        }

        /// <summary>
        /// Finds the intersection between the given line and the given plane.
        /// </summary>
        /// <param name="a">First endpoint of segment defining the line.</param>
        /// <param name="b">Second endpoint of segment defining the line.</param>
        /// <param name="p">Plane for comparison.</param>
        /// <param name="t">Interval along line to intersection (A + t * AB).</param>
        /// <param name="q">Intersection point.</param>
        /// <returns>Whether or not the line intersects the plane.  If false, the line is parallel to the plane's surface.</returns>
        public static bool GetLinePlaneIntersection(ref OrkEngine3D.Mathematics.Vector3 a, ref OrkEngine3D.Mathematics.Vector3 b, ref Plane p, out float t, out OrkEngine3D.Mathematics.Vector3 q)
        {
            OrkEngine3D.Mathematics.Vector3 ab;
            Vector3Ex.Subtract(ref b, ref a, out ab);
            float denominator;
            Vector3Ex.Dot(ref p.Normal, ref ab, out denominator);
            if (denominator < Epsilon && denominator > -Epsilon)
            {
                //Surface of plane and line are parallel (or very close to it).
                q = new OrkEngine3D.Mathematics.Vector3();
                t = float.MaxValue;
                return false;
            }
            float numerator;
            Vector3Ex.Dot(ref p.Normal, ref a, out numerator);
            t = (p.D - numerator) / denominator;
            //Compute the intersection position.
            Vector3Ex.Multiply(ref ab, t, out q);
            Vector3Ex.Add(ref a, ref q, out q);
            return true;
        }

        /// <summary>
        /// Finds the intersection between the given ray and the given plane.
        /// </summary>
        /// <param name="ray">Ray to test against the plane.</param>
        /// <param name="p">Plane for comparison.</param>
        /// <param name="t">Interval along line to intersection (A + t * AB).</param>
        /// <param name="q">Intersection point.</param>
        /// <returns>Whether or not the line intersects the plane.  If false, the line is parallel to the plane's surface.</returns>
        public static bool GetRayPlaneIntersection(ref Ray ray, ref Plane p, out float t, out OrkEngine3D.Mathematics.Vector3 q)
        {
            float denominator;
            Vector3Ex.Dot(ref p.Normal, ref ray.Direction, out denominator);
            if (denominator < Epsilon && denominator > -Epsilon)
            {
                //Surface of plane and line are parallel (or very close to it).
                q = new OrkEngine3D.Mathematics.Vector3();
                t = float.MaxValue;
                return false;
            }
            float numerator;
            Vector3Ex.Dot(ref p.Normal, ref ray.Position, out numerator);
            t = (p.D - numerator) / denominator;
            //Compute the intersection position.
            Vector3Ex.Multiply(ref ray.Direction, t, out q);
            Vector3Ex.Add(ref ray.Position, ref q, out q);
            return t >= 0;
        }

        #endregion

        #region Point-Triangle Tests

        /// <summary>
        /// Determines the closest point on a triangle given by points a, b, and c to point p.
        /// </summary>
        /// <param name="a">First vertex of triangle.</param>
        /// <param name="b">Second vertex of triangle.</param>
        /// <param name="c">Third vertex of triangle.</param>
        /// <param name="p">Point for comparison.</param>
        /// <param name="closestPoint">Closest point on tetrahedron to point.</param>
        /// <returns>Voronoi region containing the closest point.</returns>
        public static VoronoiRegion GetClosestPointOnTriangleToPoint(ref OrkEngine3D.Mathematics.Vector3 a, ref OrkEngine3D.Mathematics.Vector3 b, ref OrkEngine3D.Mathematics.Vector3 c, ref OrkEngine3D.Mathematics.Vector3 p, out OrkEngine3D.Mathematics.Vector3 closestPoint)
        {
            float v, w;
            OrkEngine3D.Mathematics.Vector3 ab;
            Vector3Ex.Subtract(ref b, ref a, out ab);
            OrkEngine3D.Mathematics.Vector3 ac;
            Vector3Ex.Subtract(ref c, ref a, out ac);
            //Vertex region A?
            OrkEngine3D.Mathematics.Vector3 ap;
            Vector3Ex.Subtract(ref p, ref a, out ap);
            float d1;
            Vector3Ex.Dot(ref ab, ref ap, out d1);
            float d2;
            Vector3Ex.Dot(ref ac, ref ap, out d2);
            if (d1 <= 0 && d2 < 0)
            {
                closestPoint = a;
                return VoronoiRegion.A;
            }
            //Vertex region B?
            OrkEngine3D.Mathematics.Vector3 bp;
            Vector3Ex.Subtract(ref p, ref b, out bp);
            float d3;
            Vector3Ex.Dot(ref ab, ref bp, out d3);
            float d4;
            Vector3Ex.Dot(ref ac, ref bp, out d4);
            if (d3 >= 0 && d4 <= d3)
            {
                closestPoint = b;
                return VoronoiRegion.B;
            }
            //Edge region AB?
            float vc = d1 * d4 - d3 * d2;
            if (vc <= 0 && d1 >= 0 && d3 <= 0)
            {
                v = d1 / (d1 - d3);
                Vector3Ex.Multiply(ref ab, v, out closestPoint);
                Vector3Ex.Add(ref closestPoint, ref a, out closestPoint);
                return VoronoiRegion.AB;
            }
            //Vertex region C?
            OrkEngine3D.Mathematics.Vector3 cp;
            Vector3Ex.Subtract(ref p, ref c, out cp);
            float d5;
            Vector3Ex.Dot(ref ab, ref cp, out d5);
            float d6;
            Vector3Ex.Dot(ref ac, ref cp, out d6);
            if (d6 >= 0 && d5 <= d6)
            {
                closestPoint = c;
                return VoronoiRegion.C;
            }
            //Edge region AC?
            float vb = d5 * d2 - d1 * d6;
            if (vb <= 0 && d2 >= 0 && d6 <= 0)
            {
                w = d2 / (d2 - d6);
                Vector3Ex.Multiply(ref ac, w, out closestPoint);
                Vector3Ex.Add(ref closestPoint, ref a, out closestPoint);
                return VoronoiRegion.AC;
            }
            //Edge region BC?
            float va = d3 * d6 - d5 * d4;
            if (va <= 0 && (d4 - d3) >= 0 && (d5 - d6) >= 0)
            {
                w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                Vector3Ex.Subtract(ref c, ref b, out closestPoint);
                Vector3Ex.Multiply(ref closestPoint, w, out closestPoint);
                Vector3Ex.Add(ref closestPoint, ref b, out closestPoint);
                return VoronoiRegion.BC;
            }
            //Inside triangle?
            float denom = 1 / (va + vb + vc);
            v = vb * denom;
            w = vc * denom;
            OrkEngine3D.Mathematics.Vector3 abv;
            Vector3Ex.Multiply(ref ab, v, out abv);
            OrkEngine3D.Mathematics.Vector3 acw;
            Vector3Ex.Multiply(ref ac, w, out acw);
            Vector3Ex.Add(ref a, ref abv, out closestPoint);
            Vector3Ex.Add(ref closestPoint, ref acw, out closestPoint);
            return VoronoiRegion.ABC;
        }

        /// <summary>
        /// Determines the closest point on a triangle given by points a, b, and c to point p and provides the subsimplex whose voronoi region contains the point.
        /// </summary>
        /// <param name="a">First vertex of triangle.</param>
        /// <param name="b">Second vertex of triangle.</param>
        /// <param name="c">Third vertex of triangle.</param>
        /// <param name="p">Point for comparison.</param>
        /// <param name="subsimplex">The source of the voronoi region which contains the point.</param>
        /// <param name="closestPoint">Closest point on tetrahedron to point.</param>
        [Obsolete("Used for simplex tests; consider using the PairSimplex and its variants instead for simplex-related testing.")]
        public static void GetClosestPointOnTriangleToPoint(ref OrkEngine3D.Mathematics.Vector3 a, ref OrkEngine3D.Mathematics.Vector3 b, ref OrkEngine3D.Mathematics.Vector3 c, ref OrkEngine3D.Mathematics.Vector3 p, RawList<OrkEngine3D.Mathematics.Vector3> subsimplex, out OrkEngine3D.Mathematics.Vector3 closestPoint)
        {
            subsimplex.Clear();
            float v, w;
            OrkEngine3D.Mathematics.Vector3 ab;
            Vector3Ex.Subtract(ref b, ref a, out ab);
            OrkEngine3D.Mathematics.Vector3 ac;
            Vector3Ex.Subtract(ref c, ref a, out ac);
            //Vertex region A?
            OrkEngine3D.Mathematics.Vector3 ap;
            Vector3Ex.Subtract(ref p, ref a, out ap);
            float d1;
            Vector3Ex.Dot(ref ab, ref ap, out d1);
            float d2;
            Vector3Ex.Dot(ref ac, ref ap, out d2);
            if (d1 <= 0 && d2 < 0)
            {
                subsimplex.Add(a);
                closestPoint = a;
                return;
            }
            //Vertex region B?
            OrkEngine3D.Mathematics.Vector3 bp;
            Vector3Ex.Subtract(ref p, ref b, out bp);
            float d3;
            Vector3Ex.Dot(ref ab, ref bp, out d3);
            float d4;
            Vector3Ex.Dot(ref ac, ref bp, out d4);
            if (d3 >= 0 && d4 <= d3)
            {
                subsimplex.Add(b);
                closestPoint = b;
                return;
            }
            //Edge region AB?
            float vc = d1 * d4 - d3 * d2;
            if (vc <= 0 && d1 >= 0 && d3 <= 0)
            {
                subsimplex.Add(a);
                subsimplex.Add(b);
                v = d1 / (d1 - d3);
                Vector3Ex.Multiply(ref ab, v, out closestPoint);
                Vector3Ex.Add(ref closestPoint, ref a, out closestPoint);
                return;
            }
            //Vertex region C?
            OrkEngine3D.Mathematics.Vector3 cp;
            Vector3Ex.Subtract(ref p, ref c, out cp);
            float d5;
            Vector3Ex.Dot(ref ab, ref cp, out d5);
            float d6;
            Vector3Ex.Dot(ref ac, ref cp, out d6);
            if (d6 >= 0 && d5 <= d6)
            {
                subsimplex.Add(c);
                closestPoint = c;
                return;
            }
            //Edge region AC?
            float vb = d5 * d2 - d1 * d6;
            if (vb <= 0 && d2 >= 0 && d6 <= 0)
            {
                subsimplex.Add(a);
                subsimplex.Add(c);
                w = d2 / (d2 - d6);
                Vector3Ex.Multiply(ref ac, w, out closestPoint);
                Vector3Ex.Add(ref closestPoint, ref a, out closestPoint);
                return;
            }
            //Edge region BC?
            float va = d3 * d6 - d5 * d4;
            if (va <= 0 && (d4 - d3) >= 0 && (d5 - d6) >= 0)
            {
                subsimplex.Add(b);
                subsimplex.Add(c);
                w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                Vector3Ex.Subtract(ref c, ref b, out closestPoint);
                Vector3Ex.Multiply(ref closestPoint, w, out closestPoint);
                Vector3Ex.Add(ref closestPoint, ref b, out closestPoint);
                return;
            }
            //Inside triangle?
            subsimplex.Add(a);
            subsimplex.Add(b);
            subsimplex.Add(c);
            float denom = 1 / (va + vb + vc);
            v = vb * denom;
            w = vc * denom;
            OrkEngine3D.Mathematics.Vector3 abv;
            Vector3Ex.Multiply(ref ab, v, out abv);
            OrkEngine3D.Mathematics.Vector3 acw;
            Vector3Ex.Multiply(ref ac, w, out acw);
            Vector3Ex.Add(ref a, ref abv, out closestPoint);
            Vector3Ex.Add(ref closestPoint, ref acw, out closestPoint);
        }

        /// <summary>
        /// Determines the closest point on a triangle given by points a, b, and c to point p and provides the subsimplex whose voronoi region contains the point.
        /// </summary>
        /// <param name="q">Simplex containing triangle for testing.</param>
        /// <param name="i">Index of first vertex of triangle.</param>
        /// <param name="j">Index of second vertex of triangle.</param>
        /// <param name="k">Index of third vertex of triangle.</param>
        /// <param name="p">Point for comparison.</param>
        /// <param name="subsimplex">The source of the voronoi region which contains the point, enumerated as a = 0, b = 1, c = 2.</param>
        /// <param name="baryCoords">Barycentric coordinates of the point on the triangle.</param>
        /// <param name="closestPoint">Closest point on tetrahedron to point.</param>
        [Obsolete("Used for simplex tests; consider using the PairSimplex and its variants instead for simplex-related testing.")]
        public static void GetClosestPointOnTriangleToPoint(RawList<OrkEngine3D.Mathematics.Vector3> q, int i, int j, int k, ref OrkEngine3D.Mathematics.Vector3 p, RawList<int> subsimplex, RawList<float> baryCoords, out OrkEngine3D.Mathematics.Vector3 closestPoint)
        {
            subsimplex.Clear();
            baryCoords.Clear();
            float v, w;
            OrkEngine3D.Mathematics.Vector3 a = q[i];
            OrkEngine3D.Mathematics.Vector3 b = q[j];
            OrkEngine3D.Mathematics.Vector3 c = q[k];
            OrkEngine3D.Mathematics.Vector3 ab;
            Vector3Ex.Subtract(ref b, ref a, out ab);
            OrkEngine3D.Mathematics.Vector3 ac;
            Vector3Ex.Subtract(ref c, ref a, out ac);
            //Vertex region A?
            OrkEngine3D.Mathematics.Vector3 ap;
            Vector3Ex.Subtract(ref p, ref a, out ap);
            float d1;
            Vector3Ex.Dot(ref ab, ref ap, out d1);
            float d2;
            Vector3Ex.Dot(ref ac, ref ap, out d2);
            if (d1 <= 0 && d2 < 0)
            {
                subsimplex.Add(i);
                baryCoords.Add(1);
                closestPoint = a;
                return; //barycentric coordinates (1,0,0)
            }
            //Vertex region B?
            OrkEngine3D.Mathematics.Vector3 bp;
            Vector3Ex.Subtract(ref p, ref b, out bp);
            float d3;
            Vector3Ex.Dot(ref ab, ref bp, out d3);
            float d4;
            Vector3Ex.Dot(ref ac, ref bp, out d4);
            if (d3 >= 0 && d4 <= d3)
            {
                subsimplex.Add(j);
                baryCoords.Add(1);
                closestPoint = b;
                return; //barycentric coordinates (0,1,0)
            }
            //Edge region AB?
            float vc = d1 * d4 - d3 * d2;
            if (vc <= 0 && d1 >= 0 && d3 <= 0)
            {
                subsimplex.Add(i);
                subsimplex.Add(j);
                v = d1 / (d1 - d3);
                baryCoords.Add(1 - v);
                baryCoords.Add(v);
                Vector3Ex.Multiply(ref ab, v, out closestPoint);
                Vector3Ex.Add(ref closestPoint, ref a, out closestPoint);
                return; //barycentric coordinates (1-v, v, 0)
            }
            //Vertex region C?
            OrkEngine3D.Mathematics.Vector3 cp;
            Vector3Ex.Subtract(ref p, ref c, out cp);
            float d5;
            Vector3Ex.Dot(ref ab, ref cp, out d5);
            float d6;
            Vector3Ex.Dot(ref ac, ref cp, out d6);
            if (d6 >= 0 && d5 <= d6)
            {
                subsimplex.Add(k);
                baryCoords.Add(1);
                closestPoint = c;
                return; //barycentric coordinates (0,0,1)
            }
            //Edge region AC?
            float vb = d5 * d2 - d1 * d6;
            if (vb <= 0 && d2 >= 0 && d6 <= 0)
            {
                subsimplex.Add(i);
                subsimplex.Add(k);
                w = d2 / (d2 - d6);
                baryCoords.Add(1 - w);
                baryCoords.Add(w);
                Vector3Ex.Multiply(ref ac, w, out closestPoint);
                Vector3Ex.Add(ref closestPoint, ref a, out closestPoint);
                return; //barycentric coordinates (1-w, 0, w)
            }
            //Edge region BC?
            float va = d3 * d6 - d5 * d4;
            if (va <= 0 && (d4 - d3) >= 0 && (d5 - d6) >= 0)
            {
                subsimplex.Add(j);
                subsimplex.Add(k);
                w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                baryCoords.Add(1 - w);
                baryCoords.Add(w);
                Vector3Ex.Subtract(ref c, ref b, out closestPoint);
                Vector3Ex.Multiply(ref closestPoint, w, out closestPoint);
                Vector3Ex.Add(ref closestPoint, ref b, out closestPoint);
                return; //barycentric coordinates (0, 1 - w ,w)
            }
            //Inside triangle?
            subsimplex.Add(i);
            subsimplex.Add(j);
            subsimplex.Add(k);
            float denom = 1 / (va + vb + vc);
            v = vb * denom;
            w = vc * denom;
            baryCoords.Add(1 - v - w);
            baryCoords.Add(v);
            baryCoords.Add(w);
            OrkEngine3D.Mathematics.Vector3 abv;
            Vector3Ex.Multiply(ref ab, v, out abv);
            OrkEngine3D.Mathematics.Vector3 acw;
            Vector3Ex.Multiply(ref ac, w, out acw);
            Vector3Ex.Add(ref a, ref abv, out closestPoint);
            Vector3Ex.Add(ref closestPoint, ref acw, out closestPoint);
            //return a + ab * v + ac * w; //barycentric coordinates (1 - v - w, v, w)
        }

        /// <summary>
        /// Determines if supplied point is within the triangle as defined by the provided vertices.
        /// </summary>
        /// <param name="vA">A vertex of the triangle.</param>
        /// <param name="vB">A vertex of the triangle.</param>
        /// <param name="vC">A vertex of the triangle.</param>
        /// <param name="p">The point for comparison against the triangle.</param>
        /// <returns>Whether or not the point is within the triangle.</returns>
        public static bool IsPointInsideTriangle(ref OrkEngine3D.Mathematics.Vector3 vA, ref OrkEngine3D.Mathematics.Vector3 vB, ref OrkEngine3D.Mathematics.Vector3 vC, ref OrkEngine3D.Mathematics.Vector3 p)
        {
            float u, v, w;
            GetBarycentricCoordinates(ref p, ref vA, ref vB, ref vC, out u, out v, out w);
            //Are the barycoords valid?
            return (u > -Epsilon) && (v > -Epsilon) && (w > -Epsilon);
        }

        /// <summary>
        /// Determines if supplied point is within the triangle as defined by the provided vertices.
        /// </summary>
        /// <param name="vA">A vertex of the triangle.</param>
        /// <param name="vB">A vertex of the triangle.</param>
        /// <param name="vC">A vertex of the triangle.</param>
        /// <param name="p">The point for comparison against the triangle.</param>
        /// <param name="margin">Extra area on the edges of the triangle to include.  Can be negative.</param>
        /// <returns>Whether or not the point is within the triangle.</returns>
        public static bool IsPointInsideTriangle(ref OrkEngine3D.Mathematics.Vector3 vA, ref OrkEngine3D.Mathematics.Vector3 vB, ref OrkEngine3D.Mathematics.Vector3 vC, ref OrkEngine3D.Mathematics.Vector3 p, float margin)
        {
            float u, v, w;
            GetBarycentricCoordinates(ref p, ref vA, ref vB, ref vC, out u, out v, out w);
            //Are the barycoords valid?
            return (u > -margin) && (v > -margin) && (w > -margin);
        }

        #endregion

        #region Point-Line Tests

        /// <summary>
        /// Determines the closest point on the provided segment ab to point p.
        /// </summary>
        /// <param name="a">First endpoint of segment.</param>
        /// <param name="b">Second endpoint of segment.</param>
        /// <param name="p">Point for comparison.</param>
        /// <param name="closestPoint">Closest point on the edge to p.</param>
        public static void GetClosestPointOnSegmentToPoint(ref OrkEngine3D.Mathematics.Vector3 a, ref OrkEngine3D.Mathematics.Vector3 b, ref OrkEngine3D.Mathematics.Vector3 p, out OrkEngine3D.Mathematics.Vector3 closestPoint)
        {
            OrkEngine3D.Mathematics.Vector3 ab;
            Vector3Ex.Subtract(ref b, ref a, out ab);
            OrkEngine3D.Mathematics.Vector3 ap;
            Vector3Ex.Subtract(ref p, ref a, out ap);
            float t;
            Vector3Ex.Dot(ref ap, ref ab, out t);
            if (t <= 0)
            {
                closestPoint = a;
            }
            else
            {
                float denom = ab.X * ab.X + ab.Y * ab.Y + ab.Z * ab.Z;
                if (t >= denom)
                {
                    closestPoint = b;
                }
                else
                {
                    t = t / denom;
                    OrkEngine3D.Mathematics.Vector3 tab;
                    Vector3Ex.Multiply(ref ab, t, out tab);
                    Vector3Ex.Add(ref a, ref tab, out closestPoint);
                }
            }
        }

        /// <summary>
        /// Determines the closest point on the provided segment ab to point p.
        /// </summary>
        /// <param name="a">First endpoint of segment.</param>
        /// <param name="b">Second endpoint of segment.</param>
        /// <param name="p">Point for comparison.</param>
        /// <param name="subsimplex">The source of the voronoi region which contains the point.</param>
        /// <param name="closestPoint">Closest point on the edge to p.</param>
        [Obsolete("Used for simplex tests; consider using the PairSimplex and its variants instead for simplex-related testing.")]
        public static void GetClosestPointOnSegmentToPoint(ref OrkEngine3D.Mathematics.Vector3 a, ref OrkEngine3D.Mathematics.Vector3 b, ref OrkEngine3D.Mathematics.Vector3 p, List<OrkEngine3D.Mathematics.Vector3> subsimplex, out OrkEngine3D.Mathematics.Vector3 closestPoint)
        {
            subsimplex.Clear();
            OrkEngine3D.Mathematics.Vector3 ab;
            Vector3Ex.Subtract(ref b, ref a, out ab);
            OrkEngine3D.Mathematics.Vector3 ap;
            Vector3Ex.Subtract(ref p, ref a, out ap);
            float t;
            Vector3Ex.Dot(ref ap, ref ab, out t);
            if (t <= 0)
            {
                //t = 0;//Don't need this for returning purposes.
                subsimplex.Add(a);
                closestPoint = a;
            }
            else
            {
                float denom = ab.X * ab.X + ab.Y * ab.Y + ab.Z * ab.Z;
                if (t >= denom)
                {
                    //t = 1;//Don't need this for returning purposes.
                    subsimplex.Add(b);
                    closestPoint = b;
                }
                else
                {
                    t = t / denom;
                    subsimplex.Add(a);
                    subsimplex.Add(b);
                    OrkEngine3D.Mathematics.Vector3 tab;
                    Vector3Ex.Multiply(ref ab, t, out tab);
                    Vector3Ex.Add(ref a, ref tab, out closestPoint);
                }
            }
        }

        /// <summary>
        /// Determines the closest point on the provided segment ab to point p.
        /// </summary>
        /// <param name="q">List of points in the containing simplex.</param>
        /// <param name="i">Index of first endpoint of segment.</param>
        /// <param name="j">Index of second endpoint of segment.</param>
        /// <param name="p">Point for comparison.</param>
        /// <param name="subsimplex">The source of the voronoi region which contains the point, enumerated as a = 0, b = 1.</param>
        /// <param name="baryCoords">Barycentric coordinates of the point.</param>
        /// <param name="closestPoint">Closest point on the edge to p.</param>
        [Obsolete("Used for simplex tests; consider using the PairSimplex and its variants instead for simplex-related testing.")]
        public static void GetClosestPointOnSegmentToPoint(List<OrkEngine3D.Mathematics.Vector3> q, int i, int j, ref OrkEngine3D.Mathematics.Vector3 p, List<int> subsimplex, List<float> baryCoords, out OrkEngine3D.Mathematics.Vector3 closestPoint)
        {
            OrkEngine3D.Mathematics.Vector3 a = q[i];
            OrkEngine3D.Mathematics.Vector3 b = q[j];
            subsimplex.Clear();
            baryCoords.Clear();
            OrkEngine3D.Mathematics.Vector3 ab;
            Vector3Ex.Subtract(ref b, ref a, out ab);
            OrkEngine3D.Mathematics.Vector3 ap;
            Vector3Ex.Subtract(ref p, ref a, out ap);
            float t;
            Vector3Ex.Dot(ref ap, ref ab, out t);
            if (t <= 0)
            {
                subsimplex.Add(i);
                baryCoords.Add(1);
                closestPoint = a;
            }
            else
            {
                float denom = ab.X * ab.X + ab.Y * ab.Y + ab.Z * ab.Z;
                if (t >= denom)
                {
                    subsimplex.Add(j);
                    baryCoords.Add(1);
                    closestPoint = b;
                }
                else
                {
                    t = t / denom;
                    subsimplex.Add(i);
                    subsimplex.Add(j);
                    baryCoords.Add(1 - t);
                    baryCoords.Add(t);
                    OrkEngine3D.Mathematics.Vector3 tab;
                    Vector3Ex.Multiply(ref ab, t, out tab);
                    Vector3Ex.Add(ref a, ref tab, out closestPoint);
                }
            }
        }


        /// <summary>
        /// Determines the shortest squared distance from the point to the line.
        /// </summary>
        /// <param name="p">Point to check against the line.</param>
        /// <param name="a">First point on the line.</param>
        /// <param name="b">Second point on the line.</param>
        /// <returns>Shortest squared distance from the point to the line.</returns>
        public static float GetSquaredDistanceFromPointToLine(ref OrkEngine3D.Mathematics.Vector3 p, ref OrkEngine3D.Mathematics.Vector3 a, ref OrkEngine3D.Mathematics.Vector3 b)
        {
            OrkEngine3D.Mathematics.Vector3 ap, ab;
            Vector3Ex.Subtract(ref p, ref a, out ap);
            Vector3Ex.Subtract(ref b, ref a, out ab);
            float e;
            Vector3Ex.Dot(ref ap, ref ab, out e);
            return ap.LengthSquared() - e * e / ab.LengthSquared();
        }

        #endregion

        #region Line-Line Tests

        /// <summary>
        /// Computes closest points c1 and c2 betwen segments p1q1 and p2q2.
        /// </summary>
        /// <param name="p1">First point of first segment.</param>
        /// <param name="q1">Second point of first segment.</param>
        /// <param name="p2">First point of second segment.</param>
        /// <param name="q2">Second point of second segment.</param>
        /// <param name="c1">Closest point on first segment.</param>
        /// <param name="c2">Closest point on second segment.</param>
        public static void GetClosestPointsBetweenSegments(OrkEngine3D.Mathematics.Vector3 p1, OrkEngine3D.Mathematics.Vector3 q1, OrkEngine3D.Mathematics.Vector3 p2, OrkEngine3D.Mathematics.Vector3 q2, out OrkEngine3D.Mathematics.Vector3 c1, out OrkEngine3D.Mathematics.Vector3 c2)
        {
            float s, t;
            GetClosestPointsBetweenSegments(ref p1, ref q1, ref p2, ref q2, out s, out t, out c1, out c2);
        }

        /// <summary>
        /// Computes closest points c1 and c2 betwen segments p1q1 and p2q2.
        /// </summary>
        /// <param name="p1">First point of first segment.</param>
        /// <param name="q1">Second point of first segment.</param>
        /// <param name="p2">First point of second segment.</param>
        /// <param name="q2">Second point of second segment.</param>
        /// <param name="s">Distance along the line to the point for first segment.</param>
        /// <param name="t">Distance along the line to the point for second segment.</param>
        /// <param name="c1">Closest point on first segment.</param>
        /// <param name="c2">Closest point on second segment.</param>
        public static void GetClosestPointsBetweenSegments(ref OrkEngine3D.Mathematics.Vector3 p1, ref OrkEngine3D.Mathematics.Vector3 q1, ref OrkEngine3D.Mathematics.Vector3 p2, ref OrkEngine3D.Mathematics.Vector3 q2,
                                                           out float s, out float t, out OrkEngine3D.Mathematics.Vector3 c1, out OrkEngine3D.Mathematics.Vector3 c2)
        {
            //Segment direction vectors
            OrkEngine3D.Mathematics.Vector3 d1;
            Vector3Ex.Subtract(ref q1, ref p1, out d1);
            OrkEngine3D.Mathematics.Vector3 d2;
            Vector3Ex.Subtract(ref q2, ref p2, out d2);
            OrkEngine3D.Mathematics.Vector3 r;
            Vector3Ex.Subtract(ref p1, ref p2, out r);
            //distance
            float a = d1.LengthSquared();
            float e = d2.LengthSquared();
            float f;
            Vector3Ex.Dot(ref d2, ref r, out f);

            if (a <= Epsilon && e <= Epsilon)
            {
                //These segments are more like points.
                s = t = 0.0f;
                c1 = p1;
                c2 = p2;
                return;
            }
            if (a <= Epsilon)
            {
                // First segment is basically a point.
                s = 0.0f;
                t = MathHelper.Clamp(f / e, 0.0f, 1.0f);
            }
            else
            {
                float c = Vector3Ex.Dot(d1, r);
                if (e <= Epsilon)
                {
                    // Second segment is basically a point.
                    t = 0.0f;
                    s = MathHelper.Clamp(-c / a, 0.0f, 1.0f);
                }
                else
                {
                    float b = Vector3Ex.Dot(d1, d2);
                    float denom = a * e - b * b;

                    // If segments not parallel, compute closest point on L1 to L2, and
                    // clamp to segment S1. Else pick some s (here .5f)
                    if (denom != 0.0f)
                        s = MathHelper.Clamp((b * f - c * e) / denom, 0.0f, 1.0f);
                    else //Parallel, just use .5f
                        s = .5f;


                    t = (b * s + f) / e;

                    if (t < 0)
                    {
                        //Closest point is before the segment.
                        t = 0;
                        s = MathHelper.Clamp(-c / a, 0, 1);
                    }
                    else if (t > 1)
                    {
                        //Closest point is after the segment.
                        t = 1;
                        s = MathHelper.Clamp((b - c) / a, 0, 1);
                    }
                }
            }

            Vector3Ex.Multiply(ref d1, s, out c1);
            Vector3Ex.Add(ref c1, ref p1, out c1);
            Vector3Ex.Multiply(ref d2, t, out c2);
            Vector3Ex.Add(ref c2, ref p2, out c2);
        }


        /// <summary>
        /// Computes closest points c1 and c2 betwen lines p1q1 and p2q2.
        /// </summary>
        /// <param name="p1">First point of first segment.</param>
        /// <param name="q1">Second point of first segment.</param>
        /// <param name="p2">First point of second segment.</param>
        /// <param name="q2">Second point of second segment.</param>
        /// <param name="s">Distance along the line to the point for first segment.</param>
        /// <param name="t">Distance along the line to the point for second segment.</param>
        /// <param name="c1">Closest point on first segment.</param>
        /// <param name="c2">Closest point on second segment.</param>
        public static void GetClosestPointsBetweenLines(ref OrkEngine3D.Mathematics.Vector3 p1, ref OrkEngine3D.Mathematics.Vector3 q1, ref OrkEngine3D.Mathematics.Vector3 p2, ref OrkEngine3D.Mathematics.Vector3 q2,
                                                           out float s, out float t, out OrkEngine3D.Mathematics.Vector3 c1, out OrkEngine3D.Mathematics.Vector3 c2)
        {
            //Segment direction vectors
            OrkEngine3D.Mathematics.Vector3 d1;
            Vector3Ex.Subtract(ref q1, ref p1, out d1);
            OrkEngine3D.Mathematics.Vector3 d2;
            Vector3Ex.Subtract(ref q2, ref p2, out d2);
            OrkEngine3D.Mathematics.Vector3 r;
            Vector3Ex.Subtract(ref p1, ref p2, out r);
            //distance
            float a = d1.LengthSquared();
            float e = d2.LengthSquared();
            float f;
            Vector3Ex.Dot(ref d2, ref r, out f);

            if (a <= Epsilon && e <= Epsilon)
            {
                //These segments are more like points.
                s = t = 0.0f;
                c1 = p1;
                c2 = p2;
                return;
            }
            if (a <= Epsilon)
            {
                // First segment is basically a point.
                s = 0.0f;
                t = MathHelper.Clamp(f / e, 0.0f, 1.0f);
            }
            else
            {
                float c = Vector3Ex.Dot(d1, r);
                if (e <= Epsilon)
                {
                    // Second segment is basically a point.
                    t = 0.0f;
                    s = MathHelper.Clamp(-c / a, 0.0f, 1.0f);
                }
                else
                {
                    float b = Vector3Ex.Dot(d1, d2);
                    float denom = a * e - b * b;

                    // If segments not parallel, compute closest point on L1 to L2, and
                    // clamp to segment S1. Else pick some s (here .5f)
                    if (denom != 0f)
                        s = (b * f - c * e) / denom;
                    else //Parallel, just use .5f
                        s = .5f;


                    t = (b * s + f) / e;
                }
            }

            Vector3Ex.Multiply(ref d1, s, out c1);
            Vector3Ex.Add(ref c1, ref p1, out c1);
            Vector3Ex.Multiply(ref d2, t, out c2);
            Vector3Ex.Add(ref c2, ref p2, out c2);
        }



        #endregion


        #region Point-Plane Tests

        /// <summary>
        /// Determines if vectors o and p are on opposite sides of the plane defined by a, b, and c.
        /// </summary>
        /// <param name="o">First point for comparison.</param>
        /// <param name="p">Second point for comparison.</param>
        /// <param name="a">First vertex of the plane.</param>
        /// <param name="b">Second vertex of plane.</param>
        /// <param name="c">Third vertex of plane.</param>
        /// <returns>Whether or not vectors o and p reside on opposite sides of the plane.</returns>
        public static bool ArePointsOnOppositeSidesOfPlane(ref OrkEngine3D.Mathematics.Vector3 o, ref OrkEngine3D.Mathematics.Vector3 p, ref OrkEngine3D.Mathematics.Vector3 a, ref OrkEngine3D.Mathematics.Vector3 b, ref OrkEngine3D.Mathematics.Vector3 c)
        {
            OrkEngine3D.Mathematics.Vector3 ab, ac, ap, ao;
            Vector3Ex.Subtract(ref b, ref a, out ab);
            Vector3Ex.Subtract(ref c, ref a, out ac);
            Vector3Ex.Subtract(ref p, ref a, out ap);
            Vector3Ex.Subtract(ref o, ref a, out ao);
            OrkEngine3D.Mathematics.Vector3 q;
            Vector3Ex.Cross(ref ab, ref ac, out q);
            float signp;
            Vector3Ex.Dot(ref ap, ref q, out signp);
            float signo;
            Vector3Ex.Dot(ref ao, ref q, out signo);
            if (signp * signo <= 0)
                return true;
            return false;
        }

        /// <summary>
        /// Determines the distance between a point and a plane..
        /// </summary>
        /// <param name="point">Point to project onto plane.</param>
        /// <param name="normal">Normal of the plane.</param>
        /// <param name="pointOnPlane">Point located on the plane.</param>
        /// <returns>Distance from the point to the plane.</returns>
        public static float GetDistancePointToPlane(ref OrkEngine3D.Mathematics.Vector3 point, ref OrkEngine3D.Mathematics.Vector3 normal, ref OrkEngine3D.Mathematics.Vector3 pointOnPlane)
        {
            OrkEngine3D.Mathematics.Vector3 offset;
            Vector3Ex.Subtract(ref point, ref pointOnPlane, out offset);
            float dot;
            Vector3Ex.Dot(ref normal, ref offset, out dot);
            return dot / normal.LengthSquared();
        }

        /// <summary>
        /// Determines the location of the point when projected onto the plane defined by the normal and a point on the plane.
        /// </summary>
        /// <param name="point">Point to project onto plane.</param>
        /// <param name="normal">Normal of the plane.</param>
        /// <param name="pointOnPlane">Point located on the plane.</param>
        /// <param name="projectedPoint">Projected location of point onto plane.</param>
        public static void GetPointProjectedOnPlane(ref OrkEngine3D.Mathematics.Vector3 point, ref OrkEngine3D.Mathematics.Vector3 normal, ref OrkEngine3D.Mathematics.Vector3 pointOnPlane, out OrkEngine3D.Mathematics.Vector3 projectedPoint)
        {
            float dot;
            Vector3Ex.Dot(ref normal, ref point, out dot);
            float dot2;
            Vector3Ex.Dot(ref pointOnPlane, ref normal, out dot2);
            float t = (dot - dot2) / normal.LengthSquared();
            OrkEngine3D.Mathematics.Vector3 multiply;
            Vector3Ex.Multiply(ref normal, t, out multiply);
            Vector3Ex.Subtract(ref point, ref multiply, out projectedPoint);
        }

        /// <summary>
        /// Determines if a point is within a set of planes defined by the edges of a triangle.
        /// </summary>
        /// <param name="point">Point for comparison.</param>
        /// <param name="planes">Edge planes.</param>
        /// <param name="centroid">A point known to be inside of the planes.</param>
        /// <returns>Whether or not the point is within the edge planes.</returns>
        public static bool IsPointWithinFaceExtrusion(OrkEngine3D.Mathematics.Vector3 point, List<Plane> planes, OrkEngine3D.Mathematics.Vector3 centroid)
        {
            foreach (Plane plane in planes)
            {
                float centroidPlaneDot;
                plane.DotCoordinate(ref centroid, out centroidPlaneDot);
                float pointPlaneDot;
                plane.DotCoordinate(ref point, out pointPlaneDot);
                if (!((centroidPlaneDot <= Epsilon && pointPlaneDot <= Epsilon) || (centroidPlaneDot >= -Epsilon && pointPlaneDot >= -Epsilon)))
                {
                    //Point's NOT the same side of the centroid, so it's 'outside.'
                    return false;
                }
            }
            return true;
        }


        #endregion

        #region Tetrahedron Tests
        //Note: These methods are unused in modern systems, but are kept around for verification.

        /// <summary>
        /// Determines the closest point on a tetrahedron to a provided point p.
        /// </summary>
        /// <param name="a">First vertex of the tetrahedron.</param>
        /// <param name="b">Second vertex of the tetrahedron.</param>
        /// <param name="c">Third vertex of the tetrahedron.</param>
        /// <param name="d">Fourth vertex of the tetrahedron.</param>
        /// <param name="p">Point for comparison.</param>
        /// <param name="closestPoint">Closest point on the tetrahedron to the point.</param>
        public static void GetClosestPointOnTetrahedronToPoint(ref OrkEngine3D.Mathematics.Vector3 a, ref OrkEngine3D.Mathematics.Vector3 b, ref OrkEngine3D.Mathematics.Vector3 c, ref OrkEngine3D.Mathematics.Vector3 d, ref OrkEngine3D.Mathematics.Vector3 p, out OrkEngine3D.Mathematics.Vector3 closestPoint)
        {
            // Start out assuming point inside all halfspaces, so closest to itself
            closestPoint = p;
            OrkEngine3D.Mathematics.Vector3 pq;
            OrkEngine3D.Mathematics.Vector3 q;
            float bestSqDist = float.MaxValue;
            // If point outside face abc then compute closest point on abc
            if (ArePointsOnOppositeSidesOfPlane(ref p, ref d, ref a, ref b, ref c))
            {
                GetClosestPointOnTriangleToPoint(ref a, ref b, ref c, ref p, out q);
                Vector3Ex.Subtract(ref q, ref p, out pq);
                float sqDist = pq.X * pq.X + pq.Y * pq.Y + pq.Z * pq.Z;
                // Update best closest point if (squared) distance is less than current best
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    closestPoint = q;
                }
            }
            // Repeat test for face acd
            if (ArePointsOnOppositeSidesOfPlane(ref p, ref b, ref a, ref c, ref d))
            {
                GetClosestPointOnTriangleToPoint(ref a, ref c, ref d, ref p, out q);
                Vector3Ex.Subtract(ref q, ref p, out pq);
                float sqDist = pq.X * pq.X + pq.Y * pq.Y + pq.Z * pq.Z;
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    closestPoint = q;
                }
            }
            // Repeat test for face adb
            if (ArePointsOnOppositeSidesOfPlane(ref p, ref c, ref a, ref d, ref b))
            {
                GetClosestPointOnTriangleToPoint(ref a, ref d, ref b, ref p, out q);
                Vector3Ex.Subtract(ref q, ref p, out pq);
                float sqDist = pq.X * pq.X + pq.Y * pq.Y + pq.Z * pq.Z;
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    closestPoint = q;
                }
            }
            // Repeat test for face bdc
            if (ArePointsOnOppositeSidesOfPlane(ref p, ref a, ref b, ref d, ref c))
            {
                GetClosestPointOnTriangleToPoint(ref b, ref d, ref c, ref p, out q);
                Vector3Ex.Subtract(ref q, ref p, out pq);
                float sqDist = pq.X * pq.X + pq.Y * pq.Y + pq.Z * pq.Z;
                if (sqDist < bestSqDist)
                {
                    closestPoint = q;
                }
            }
        }

        /// <summary>
        /// Determines the closest point on a tetrahedron to a provided point p.
        /// </summary>
        /// <param name="a">First vertex of the tetrahedron.</param>
        /// <param name="b">Second vertex of the tetrahedron.</param>
        /// <param name="c">Third vertex of the tetrahedron.</param>
        /// <param name="d">Fourth vertex of the tetrahedron.</param>
        /// <param name="p">Point for comparison.</param>
        /// <param name="subsimplex">The source of the voronoi region which contains the point.</param>
        /// <param name="closestPoint">Closest point on the tetrahedron to the point.</param>
        [Obsolete("This method was used for older GJK simplex tests.  If you need simplex tests, consider the PairSimplex class and its variants.")]
        public static void GetClosestPointOnTetrahedronToPoint(ref OrkEngine3D.Mathematics.Vector3 a, ref OrkEngine3D.Mathematics.Vector3 b, ref OrkEngine3D.Mathematics.Vector3 c, ref OrkEngine3D.Mathematics.Vector3 d, ref OrkEngine3D.Mathematics.Vector3 p, RawList<OrkEngine3D.Mathematics.Vector3> subsimplex, out OrkEngine3D.Mathematics.Vector3 closestPoint)
        {
            // Start out assuming point inside all halfspaces, so closest to itself
            subsimplex.Clear();
            subsimplex.Add(a); //Provides a baseline; if the object is not outside of any planes, then it's inside and the subsimplex is the tetrahedron itself.
            subsimplex.Add(b);
            subsimplex.Add(c);
            subsimplex.Add(d);
            closestPoint = p;
            OrkEngine3D.Mathematics.Vector3 pq;
            OrkEngine3D.Mathematics.Vector3 q;
            float bestSqDist = float.MaxValue;
            // If point outside face abc then compute closest point on abc
            if (ArePointsOnOppositeSidesOfPlane(ref p, ref d, ref a, ref b, ref c))
            {
                GetClosestPointOnTriangleToPoint(ref a, ref b, ref c, ref p, subsimplex, out q);
                Vector3Ex.Subtract(ref q, ref p, out pq);
                float sqDist = pq.X * pq.X + pq.Y * pq.Y + pq.Z * pq.Z;
                // Update best closest point if (squared) distance is less than current best
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    closestPoint = q;
                }
            }
            // Repeat test for face acd
            if (ArePointsOnOppositeSidesOfPlane(ref p, ref b, ref a, ref c, ref d))
            {
                GetClosestPointOnTriangleToPoint(ref a, ref c, ref d, ref p, subsimplex, out q);
                Vector3Ex.Subtract(ref q, ref p, out pq);
                float sqDist = pq.X * pq.X + pq.Y * pq.Y + pq.Z * pq.Z;
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    closestPoint = q;
                }
            }
            // Repeat test for face adb
            if (ArePointsOnOppositeSidesOfPlane(ref p, ref c, ref a, ref d, ref b))
            {
                GetClosestPointOnTriangleToPoint(ref a, ref d, ref b, ref p, subsimplex, out q);
                Vector3Ex.Subtract(ref q, ref p, out pq);
                float sqDist = pq.X * pq.X + pq.Y * pq.Y + pq.Z * pq.Z;
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    closestPoint = q;
                }
            }
            // Repeat test for face bdc
            if (ArePointsOnOppositeSidesOfPlane(ref p, ref a, ref b, ref d, ref c))
            {
                GetClosestPointOnTriangleToPoint(ref b, ref d, ref c, ref p, subsimplex, out q);
                Vector3Ex.Subtract(ref q, ref p, out pq);
                float sqDist = pq.X * pq.X + pq.Y * pq.Y + pq.Z * pq.Z;
                if (sqDist < bestSqDist)
                {
                    closestPoint = q;
                }
            }
        }

        /// <summary>
        /// Determines the closest point on a tetrahedron to a provided point p.
        /// </summary>
        /// <param name="tetrahedron">List of 4 points composing the tetrahedron.</param>
        /// <param name="p">Point for comparison.</param>
        /// <param name="subsimplex">The source of the voronoi region which contains the point, enumerated as a = 0, b = 1, c = 2, d = 3.</param>
        /// <param name="baryCoords">Barycentric coordinates of p on the tetrahedron.</param>
        /// <param name="closestPoint">Closest point on the tetrahedron to the point.</param>
        [Obsolete("This method was used for older GJK simplex tests.  If you need simplex tests, consider the PairSimplex class and its variants.")]
        public static void GetClosestPointOnTetrahedronToPoint(RawList<OrkEngine3D.Mathematics.Vector3> tetrahedron, ref OrkEngine3D.Mathematics.Vector3 p, RawList<int> subsimplex, RawList<float> baryCoords, out OrkEngine3D.Mathematics.Vector3 closestPoint)
        {
            var subsimplexCandidate = CommonResources.GetIntList();
            var baryCoordsCandidate = CommonResources.GetFloatList();
            OrkEngine3D.Mathematics.Vector3 a = tetrahedron[0];
            OrkEngine3D.Mathematics.Vector3 b = tetrahedron[1];
            OrkEngine3D.Mathematics.Vector3 c = tetrahedron[2];
            OrkEngine3D.Mathematics.Vector3 d = tetrahedron[3];
            closestPoint = p;
            OrkEngine3D.Mathematics.Vector3 pq;
            float bestSqDist = float.MaxValue;
            subsimplex.Clear();
            subsimplex.Add(0); //Provides a baseline; if the object is not outside of any planes, then it's inside and the subsimplex is the tetrahedron itself.
            subsimplex.Add(1);
            subsimplex.Add(2);
            subsimplex.Add(3);
            baryCoords.Clear();
            OrkEngine3D.Mathematics.Vector3 q;
            bool baryCoordsFound = false;

            // If point outside face abc then compute closest point on abc
            if (ArePointsOnOppositeSidesOfPlane(ref p, ref d, ref a, ref b, ref c))
            {
                GetClosestPointOnTriangleToPoint(tetrahedron, 0, 1, 2, ref p, subsimplexCandidate, baryCoordsCandidate, out q);
                Vector3Ex.Subtract(ref q, ref p, out pq);
                float sqDist = pq.LengthSquared();
                // Update best closest point if (squared) distance is less than current best
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    closestPoint = q;
                    subsimplex.Clear();
                    baryCoords.Clear();
                    for (int k = 0; k < subsimplexCandidate.Count; k++)
                    {
                        subsimplex.Add(subsimplexCandidate[k]);
                        baryCoords.Add(baryCoordsCandidate[k]);
                    }
                    //subsimplex.AddRange(subsimplexCandidate);
                    //baryCoords.AddRange(baryCoordsCandidate);
                    baryCoordsFound = true;
                }
            }
            // Repeat test for face acd
            if (ArePointsOnOppositeSidesOfPlane(ref p, ref b, ref a, ref c, ref d))
            {
                GetClosestPointOnTriangleToPoint(tetrahedron, 0, 2, 3, ref p, subsimplexCandidate, baryCoordsCandidate, out q);
                Vector3Ex.Subtract(ref q, ref p, out pq);
                float sqDist = pq.LengthSquared();
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    closestPoint = q;
                    subsimplex.Clear();
                    baryCoords.Clear();
                    for (int k = 0; k < subsimplexCandidate.Count; k++)
                    {
                        subsimplex.Add(subsimplexCandidate[k]);
                        baryCoords.Add(baryCoordsCandidate[k]);
                    }
                    //subsimplex.AddRange(subsimplexCandidate);
                    //baryCoords.AddRange(baryCoordsCandidate);
                    baryCoordsFound = true;
                }
            }
            // Repeat test for face adb
            if (ArePointsOnOppositeSidesOfPlane(ref p, ref c, ref a, ref d, ref b))
            {
                GetClosestPointOnTriangleToPoint(tetrahedron, 0, 3, 1, ref p, subsimplexCandidate, baryCoordsCandidate, out q);
                Vector3Ex.Subtract(ref q, ref p, out pq);
                float sqDist = pq.LengthSquared();
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    closestPoint = q;
                    subsimplex.Clear();
                    baryCoords.Clear();
                    for (int k = 0; k < subsimplexCandidate.Count; k++)
                    {
                        subsimplex.Add(subsimplexCandidate[k]);
                        baryCoords.Add(baryCoordsCandidate[k]);
                    }
                    //subsimplex.AddRange(subsimplexCandidate);
                    //baryCoords.AddRange(baryCoordsCandidate);
                    baryCoordsFound = true;
                }
            }
            // Repeat test for face bdc
            if (ArePointsOnOppositeSidesOfPlane(ref p, ref a, ref b, ref d, ref c))
            {
                GetClosestPointOnTriangleToPoint(tetrahedron, 1, 3, 2, ref p, subsimplexCandidate, baryCoordsCandidate, out q);
                Vector3Ex.Subtract(ref q, ref p, out pq);
                float sqDist = pq.LengthSquared();
                if (sqDist < bestSqDist)
                {
                    closestPoint = q;
                    subsimplex.Clear();
                    baryCoords.Clear();
                    for (int k = 0; k < subsimplexCandidate.Count; k++)
                    {
                        subsimplex.Add(subsimplexCandidate[k]);
                        baryCoords.Add(baryCoordsCandidate[k]);
                    }
                    //subsimplex.AddRange(subsimplexCandidate);
                    //baryCoords.AddRange(baryCoordsCandidate);
                    baryCoordsFound = true;
                }
            }
            if (!baryCoordsFound)
            {
                //subsimplex is the entire tetrahedron, can only occur when objects intersect!  Determinants of each of the tetrahedrons based on triangles composing the sides and the point itself.
                //This is basically computing the volume of parallelepipeds (triple scalar product).
                //Could be quicker just to do it directly.
                float abcd = (new OrkEngine3D.Mathematics.Matrix4x4(tetrahedron[0].X, tetrahedron[0].Y, tetrahedron[0].Z, 1,
                                         tetrahedron[1].X, tetrahedron[1].Y, tetrahedron[1].Z, 1,
                                         tetrahedron[2].X, tetrahedron[2].Y, tetrahedron[2].Z, 1,
                                         tetrahedron[3].X, tetrahedron[3].Y, tetrahedron[3].Z, 1)).GetDeterminant();
                float pbcd = (new OrkEngine3D.Mathematics.Matrix4x4(p.X, p.Y, p.Z, 1,
                                         tetrahedron[1].X, tetrahedron[1].Y, tetrahedron[1].Z, 1,
                                         tetrahedron[2].X, tetrahedron[2].Y, tetrahedron[2].Z, 1,
                                         tetrahedron[3].X, tetrahedron[3].Y, tetrahedron[3].Z, 1)).GetDeterminant();
                float apcd = (new OrkEngine3D.Mathematics.Matrix4x4(tetrahedron[0].X, tetrahedron[0].Y, tetrahedron[0].Z, 1,
                                         p.X, p.Y, p.Z, 1,
                                         tetrahedron[2].X, tetrahedron[2].Y, tetrahedron[2].Z, 1,
                                         tetrahedron[3].X, tetrahedron[3].Y, tetrahedron[3].Z, 1)).GetDeterminant();
                float abpd = (new OrkEngine3D.Mathematics.Matrix4x4(tetrahedron[0].X, tetrahedron[0].Y, tetrahedron[0].Z, 1,
                                         tetrahedron[1].X, tetrahedron[1].Y, tetrahedron[1].Z, 1,
                                         p.X, p.Y, p.Z, 1,
                                         tetrahedron[3].X, tetrahedron[3].Y, tetrahedron[3].Z, 1)).GetDeterminant();
                abcd = 1 / abcd;
                baryCoords.Add(pbcd * abcd); //u
                baryCoords.Add(apcd * abcd); //v
                baryCoords.Add(abpd * abcd); //w
                baryCoords.Add(1 - baryCoords[0] - baryCoords[1] - baryCoords[2]); //x = 1-u-v-w
            }
            CommonResources.GiveBack(subsimplexCandidate);
            CommonResources.GiveBack(baryCoordsCandidate);
        }

        #endregion





        #region Miscellaneous

        ///<summary>
        /// Tests a ray against a sphere.
        ///</summary>
        ///<param name="ray">Ray to test.</param>
        ///<param name="spherePosition">Position of the sphere.</param>
        ///<param name="radius">Radius of the sphere.</param>
        ///<param name="maximumLength">Maximum length of the ray in units of the ray direction's length.</param>
        ///<param name="hit">Hit data of the ray, if any.</param>
        ///<returns>Whether or not the ray hits the sphere.</returns>
        public static bool RayCastSphere(ref Ray ray, ref OrkEngine3D.Mathematics.Vector3 spherePosition, float radius, float maximumLength, out RayHit hit)
        {
            OrkEngine3D.Mathematics.Vector3 normalizedDirection;
            float length = ray.Direction.Length();
            Vector3Ex.Divide(ref ray.Direction, length, out normalizedDirection);
            maximumLength *= length;
            hit = new RayHit();
            OrkEngine3D.Mathematics.Vector3 m;
            Vector3Ex.Subtract(ref ray.Position, ref spherePosition, out m);
            float b = Vector3Ex.Dot(m, normalizedDirection);
            float c = m.LengthSquared() - radius * radius;

            if (c > 0 && b > 0)
                return false;
            float discriminant = b * b - c;
            if (discriminant < 0)
                return false;

            hit.T = -b - (float)Math.Sqrt(discriminant);
            if (hit.T < 0)
                hit.T = 0;
            if (hit.T > maximumLength)
                return false;
            hit.T /= length;
            Vector3Ex.Multiply(ref normalizedDirection, hit.T, out hit.Location);
            Vector3Ex.Add(ref hit.Location, ref ray.Position, out hit.Location);
            Vector3Ex.Subtract(ref hit.Location, ref spherePosition, out hit.Normal);
            hit.Normal.Normalize();
            return true;
        }


        /// <summary>
        /// Computes the velocity of a point as if it were attached to an object with the given center and velocity.
        /// </summary>
        /// <param name="point">Point to compute the velocity of.</param>
        /// <param name="center">Center of the object to which the point is attached.</param>
        /// <param name="linearVelocity">Linear velocity of the object.</param>
        /// <param name="angularVelocity">Angular velocity of the object.</param>
        /// <param name="velocity">Velocity of the point.</param>
        public static void GetVelocityOfPoint(ref OrkEngine3D.Mathematics.Vector3 point, ref OrkEngine3D.Mathematics.Vector3 center, ref OrkEngine3D.Mathematics.Vector3 linearVelocity, ref OrkEngine3D.Mathematics.Vector3 angularVelocity, out OrkEngine3D.Mathematics.Vector3 velocity)
        {
            OrkEngine3D.Mathematics.Vector3 offset;
            Vector3Ex.Subtract(ref point, ref center, out offset);
            Vector3Ex.Cross(ref angularVelocity, ref offset, out velocity);
            Vector3Ex.Add(ref velocity, ref linearVelocity, out velocity);
        }

        /// <summary>
        /// Computes the velocity of a point as if it were attached to an object with the given center and velocity.
        /// </summary>
        /// <param name="point">Point to compute the velocity of.</param>
        /// <param name="center">Center of the object to which the point is attached.</param>
        /// <param name="linearVelocity">Linear velocity of the object.</param>
        /// <param name="angularVelocity">Angular velocity of the object.</param>
        /// <returns>Velocity of the point.</returns>
        public static OrkEngine3D.Mathematics.Vector3 GetVelocityOfPoint(OrkEngine3D.Mathematics.Vector3 point, OrkEngine3D.Mathematics.Vector3 center, OrkEngine3D.Mathematics.Vector3 linearVelocity, OrkEngine3D.Mathematics.Vector3 angularVelocity)
        {
            OrkEngine3D.Mathematics.Vector3 toReturn;
            GetVelocityOfPoint(ref point, ref center, ref linearVelocity, ref angularVelocity, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Expands a bounding box by the given sweep.
        /// </summary>
        /// <param name="boundingBox">Bounding box to expand.</param>
        /// <param name="sweep">Sweep to expand the bounding box with.</param>
        public static void ExpandBoundingBox(ref BoundingBox boundingBox, ref OrkEngine3D.Mathematics.Vector3 sweep)
        {
            if (sweep.X > 0)
                boundingBox.Max.X += sweep.X;
            else
                boundingBox.Min.X += sweep.X;

            if (sweep.Y > 0)
                boundingBox.Max.Y += sweep.Y;
            else
                boundingBox.Min.Y += sweep.Y;

            if (sweep.Z > 0)
                boundingBox.Max.Z += sweep.Z;
            else
                boundingBox.Min.Z += sweep.Z;
        }

        /// <summary>
        /// Computes the bounding box of three points.
        /// </summary>
        /// <param name="a">First vertex of the triangle.</param>
        /// <param name="b">Second vertex of the triangle.</param>
        /// <param name="c">Third vertex of the triangle.</param>
        /// <param name="aabb">Bounding box of the triangle.</param>
        public static void GetTriangleBoundingBox(ref OrkEngine3D.Mathematics.Vector3 a, ref OrkEngine3D.Mathematics.Vector3 b, ref OrkEngine3D.Mathematics.Vector3 c, out BoundingBox aabb)
        {
#if !WINDOWS
            aabb = new BoundingBox();
#endif
            //X axis
            if (a.X > b.X && a.X > c.X)
            {
                //A is max
                aabb.Max.X = a.X;
                aabb.Min.X = b.X > c.X ? c.X : b.X;
            }
            else if (b.X > c.X)
            {
                //B is max
                aabb.Max.X = b.X;
                aabb.Min.X = a.X > c.X ? c.X : a.X;
            }
            else
            {
                //C is max
                aabb.Max.X = c.X;
                aabb.Min.X = a.X > b.X ? b.X : a.X;
            }
            //Y axis
            if (a.Y > b.Y && a.Y > c.Y)
            {
                //A is max
                aabb.Max.Y = a.Y;
                aabb.Min.Y = b.Y > c.Y ? c.Y : b.Y;
            }
            else if (b.Y > c.Y)
            {
                //B is max
                aabb.Max.Y = b.Y;
                aabb.Min.Y = a.Y > c.Y ? c.Y : a.Y;
            }
            else
            {
                //C is max
                aabb.Max.Y = c.Y;
                aabb.Min.Y = a.Y > b.Y ? b.Y : a.Y;
            }
            //Z axis
            if (a.Z > b.Z && a.Z > c.Z)
            {
                //A is max
                aabb.Max.Z = a.Z;
                aabb.Min.Z = b.Z > c.Z ? c.Z : b.Z;
            }
            else if (b.Z > c.Z)
            {
                //B is max
                aabb.Max.Z = b.Z;
                aabb.Min.Z = a.Z > c.Z ? c.Z : a.Z;
            }
            else
            {
                //C is max
                aabb.Max.Z = c.Z;
                aabb.Min.Z = a.Z > b.Z ? b.Z : a.Z;
            }
        }






        /// <summary>
        /// Updates the quaternion using RK4 integration.
        /// </summary>
        /// <param name="q">OrkEngine3D.Mathematics.Quaternion to update.</param>
        /// <param name="localInertiaTensorInverse">Local-space inertia tensor of the object being updated.</param>
        /// <param name="angularMomentum">Angular momentum of the object.</param>
        /// <param name="dt">Time since last frame, in seconds.</param>
        /// <param name="newOrientation">New orientation quaternion.</param>
        public static void UpdateOrientationRK4(ref OrkEngine3D.Mathematics.Quaternion q, ref Matrix3x3 localInertiaTensorInverse, ref OrkEngine3D.Mathematics.Vector3 angularMomentum, float dt, out OrkEngine3D.Mathematics.Quaternion newOrientation)
        {
            //TODO: This is a little goofy
            //OrkEngine3D.Mathematics.Quaternion diff = differentiateQuaternion(ref q, ref localInertiaTensorInverse, ref angularMomentum);
            OrkEngine3D.Mathematics.Quaternion d1;
            DifferentiateQuaternion(ref q, ref localInertiaTensorInverse, ref angularMomentum, out d1);
            OrkEngine3D.Mathematics.Quaternion s2;
            QuaternionEx.Multiply(ref d1, dt * .5f, out s2);
            QuaternionEx.Add(ref q, ref s2, out s2);

            OrkEngine3D.Mathematics.Quaternion d2;
            DifferentiateQuaternion(ref s2, ref localInertiaTensorInverse, ref angularMomentum, out d2);
            OrkEngine3D.Mathematics.Quaternion s3;
            QuaternionEx.Multiply(ref d2, dt * .5f, out s3);
            QuaternionEx.Add(ref q, ref s3, out s3);

            OrkEngine3D.Mathematics.Quaternion d3;
            DifferentiateQuaternion(ref s3, ref localInertiaTensorInverse, ref angularMomentum, out d3);
            OrkEngine3D.Mathematics.Quaternion s4;
            QuaternionEx.Multiply(ref d3, dt, out s4);
            QuaternionEx.Add(ref q, ref s4, out s4);

            OrkEngine3D.Mathematics.Quaternion d4;
            DifferentiateQuaternion(ref s4, ref localInertiaTensorInverse, ref angularMomentum, out d4);

            QuaternionEx.Multiply(ref d1, dt / 6, out d1);
            QuaternionEx.Multiply(ref d2, dt / 3, out d2);
            QuaternionEx.Multiply(ref d3, dt / 3, out d3);
            QuaternionEx.Multiply(ref d4, dt / 6, out d4);
            OrkEngine3D.Mathematics.Quaternion added;
            QuaternionEx.Add(ref q, ref d1, out added);
            QuaternionEx.Add(ref added, ref d2, out added);
            QuaternionEx.Add(ref added, ref d3, out added);
            QuaternionEx.Add(ref added, ref d4, out added);
            QuaternionEx.Normalize(ref added, out newOrientation);
        }


        /// <summary>
        /// Finds the change in the rotation state quaternion provided the local inertia tensor and angular velocity.
        /// </summary>
        /// <param name="orientation">Orienatation of the object.</param>
        /// <param name="localInertiaTensorInverse">Local-space inertia tensor of the object being updated.</param>
        /// <param name="angularMomentum">Angular momentum of the object.</param>
        ///  <param name="orientationChange">Change in quaternion.</param>
        public static void DifferentiateQuaternion(ref OrkEngine3D.Mathematics.Quaternion orientation, ref Matrix3x3 localInertiaTensorInverse, ref OrkEngine3D.Mathematics.Vector3 angularMomentum, out OrkEngine3D.Mathematics.Quaternion orientationChange)
        {
            OrkEngine3D.Mathematics.Quaternion normalizedOrientation;
            QuaternionEx.Normalize(ref orientation, out normalizedOrientation);
            Matrix3x3 tempRotMat;
            Matrix3x3.CreateFromQuaternion(ref normalizedOrientation, out tempRotMat);
            Matrix3x3 tempInertiaTensorInverse;
            Matrix3x3.MultiplyTransposed(ref tempRotMat, ref localInertiaTensorInverse, out tempInertiaTensorInverse);
            Matrix3x3.Multiply(ref tempInertiaTensorInverse, ref tempRotMat, out tempInertiaTensorInverse);
            OrkEngine3D.Mathematics.Vector3 halfspin;
            Matrix3x3.Transform(ref angularMomentum, ref tempInertiaTensorInverse, out halfspin);
            Vector3Ex.Multiply(ref halfspin, .5f, out halfspin);
            var halfspinQuaternion = new OrkEngine3D.Mathematics.Quaternion(halfspin.X, halfspin.Y, halfspin.Z, 0);
            QuaternionEx.Multiply(ref halfspinQuaternion, ref normalizedOrientation, out orientationChange);
        }


        /// <summary>
        /// Gets the barycentric coordinates of the point with respect to a triangle's vertices.
        /// </summary>
        /// <param name="p">Point to compute the barycentric coordinates of.</param>
        /// <param name="a">First vertex in the triangle.</param>
        /// <param name="b">Second vertex in the triangle.</param>
        /// <param name="c">Third vertex in the triangle.</param>
        /// <param name="aWeight">Weight of the first vertex.</param>
        /// <param name="bWeight">Weight of the second vertex.</param>
        /// <param name="cWeight">Weight of the third vertex.</param>
        public static void GetBarycentricCoordinates(ref OrkEngine3D.Mathematics.Vector3 p, ref OrkEngine3D.Mathematics.Vector3 a, ref OrkEngine3D.Mathematics.Vector3 b, ref OrkEngine3D.Mathematics.Vector3 c, out float aWeight, out float bWeight, out float cWeight)
        {
            OrkEngine3D.Mathematics.Vector3 ab, ac;
            Vector3Ex.Subtract(ref b, ref a, out ab);
            Vector3Ex.Subtract(ref c, ref a, out ac);
            OrkEngine3D.Mathematics.Vector3 triangleNormal;
            Vector3Ex.Cross(ref ab, ref ac, out triangleNormal);
            float x = triangleNormal.X < 0 ? -triangleNormal.X : triangleNormal.X;
            float y = triangleNormal.Y < 0 ? -triangleNormal.Y : triangleNormal.Y;
            float z = triangleNormal.Z < 0 ? -triangleNormal.Z : triangleNormal.Z;

            float numeratorU, numeratorV, denominator;
            if (x >= y && x >= z)
            {
                //The projection of the triangle on the YZ plane is the largest.
                numeratorU = (p.Y - b.Y) * (b.Z - c.Z) - (b.Y - c.Y) * (p.Z - b.Z); //PBC
                numeratorV = (p.Y - c.Y) * (c.Z - a.Z) - (c.Y - a.Y) * (p.Z - c.Z); //PCA
                denominator = triangleNormal.X;
            }
            else if (y >= z)
            {
                //The projection of the triangle on the XZ plane is the largest.
                numeratorU = (p.X - b.X) * (b.Z - c.Z) - (b.X - c.X) * (p.Z - b.Z); //PBC
                numeratorV = (p.X - c.X) * (c.Z - a.Z) - (c.X - a.X) * (p.Z - c.Z); //PCA
                denominator = -triangleNormal.Y;
            }
            else
            {
                //The projection of the triangle on the XY plane is the largest.
                numeratorU = (p.X - b.X) * (b.Y - c.Y) - (b.X - c.X) * (p.Y - b.Y); //PBC
                numeratorV = (p.X - c.X) * (c.Y - a.Y) - (c.X - a.X) * (p.Y - c.Y); //PCA
                denominator = triangleNormal.Z;
            }

            if (denominator < -1e-9 || denominator > 1e-9)
            {
                denominator = 1 / denominator;
                aWeight = numeratorU * denominator;
                bWeight = numeratorV * denominator;
                cWeight = 1 - aWeight - bWeight;
            }
            else
            {
                //It seems to be a degenerate triangle.
                //In that case, pick one of the closest vertices.
                //MOST of the time, this will happen when the vertices
                //are all very close together (all three points form a single point).
                //Sometimes, though, it could be that it's more of a line.
                //If it's a little inefficient, don't worry- this is a corner case anyway.

                float distance1, distance2, distance3;
                Vector3Ex.DistanceSquared(ref p, ref a, out distance1);
                Vector3Ex.DistanceSquared(ref p, ref b, out distance2);
                Vector3Ex.DistanceSquared(ref p, ref c, out distance3);
                if (distance1 < distance2 && distance1 < distance3)
                {
                    aWeight = 1;
                    bWeight = 0;
                    cWeight = 0;
                }
                else if (distance2 < distance3)
                {
                    aWeight = 0;
                    bWeight = 1;
                    cWeight = 0;
                }
                else
                {
                    aWeight = 0;
                    bWeight = 0;
                    cWeight = 1;
                }
            }


        }




        #endregion
    }
}