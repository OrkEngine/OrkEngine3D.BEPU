﻿

using BEPUutilities;
namespace BEPUphysics.Paths
{
    /// <summary>
    /// 3D hermite curve that uses the finite difference method to compute tangents.
    /// </summary>
    public class FiniteDifferenceSpline3D : HermiteCurve3D
    {
        /// <summary>
        /// Gets the curve's bounding index information.
        /// </summary>
        /// <param name="minIndex">Index of the minimum control point in the active curve segment.</param>
        /// <param name="maxIndex">Index of the maximum control point in the active curve segment.</param>
        public override void GetCurveIndexBoundsInformation(out int minIndex, out int maxIndex)
        {
            if (ControlPoints.Count > 0)
            {
                minIndex = 0;
                maxIndex = ControlPoints.Count - 1;
            }
            else
            {
                minIndex = -1;
                maxIndex = -1;
            }
        }

        protected override void ComputeTangents()
        {
            if (ControlPoints.Count == 1)
            {
                tangents.Add(OrkEngine3D.Mathematics.Vector3.Zero);
                return;
            }
            if (ControlPoints.Count == 2)
            {
                OrkEngine3D.Mathematics.Vector3 tangent = ControlPoints[1].Value - ControlPoints[0].Value;
                tangents.Add(tangent);
                tangents.Add(tangent);
                return;
            }

            OrkEngine3D.Mathematics.Vector3 tangentA, tangentB;
            OrkEngine3D.Mathematics.Vector3 previous, current, next;
            //First endpoint
            current = ControlPoints[0].Value;
            next = ControlPoints[1].Value;
            Vector3Ex.Subtract(ref next, ref current, out tangentA);
            Vector3Ex.Multiply(ref tangentA, (float)(.5 / (ControlPoints[1].Time - ControlPoints[0].Time)), out tangentA);
            //Vector3Ex.Multiply(ref current, .5f / (controlPoints[0].time), out tangentB);
            //Vector3Ex.Add(ref tangentA, ref tangentB, out tangentA);
            tangents.Add(tangentA);

            for (int i = 1; i < ControlPoints.Count - 1; i++)
            {
                previous = current;
                current = next;
                next = ControlPoints[i + 1].Value;
                Vector3Ex.Subtract(ref next, ref current, out tangentA);
                Vector3Ex.Subtract(ref current, ref previous, out tangentB);
                Vector3Ex.Multiply(ref tangentA, (float)(.5 / (ControlPoints[i + 1].Time - ControlPoints[i].Time)), out tangentA);
                Vector3Ex.Multiply(ref tangentB, (float)(.5 / (ControlPoints[i].Time - ControlPoints[i - 1].Time)), out tangentB);
                Vector3Ex.Add(ref tangentA, ref tangentB, out tangentA);
                tangents.Add(tangentA);
            }

            previous = current;
            current = next;
            Vector3Ex.Negate(ref current, out tangentA);
            Vector3Ex.Subtract(ref current, ref previous, out tangentB);
            int currentIndex = ControlPoints.Count - 1;
            int previousIndex = currentIndex - 1;
            //Vector3Ex.Multiply(ref tangentA, .5f / (-controlPoints[currentIndex].time), out tangentA);
            Vector3Ex.Multiply(ref tangentB, (float)(.5 / (ControlPoints[currentIndex].Time - ControlPoints[previousIndex].Time)), out tangentB);
            //Vector3Ex.Add(ref tangentA, ref tangentB, out tangentA);
            tangents.Add(tangentB);
        }
    }
}