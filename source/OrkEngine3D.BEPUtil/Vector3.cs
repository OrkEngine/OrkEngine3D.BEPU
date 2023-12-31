﻿using System;
using System.Runtime.CompilerServices;

namespace BEPUutilities
{
    /// <summary>
    /// Provides XNA-like 3D vector math.
    /// </summary>
    public static class Vector3Ex
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Normalize(this OrkEngine3D.Mathematics.Vector3 vec)
        {
            OrkEngine3D.Mathematics.Vector3.Normalize(vec);
        }

        /// <summary>
        /// Computes the dot product of two vectors.
        /// </summary>
        /// <param name="a">First vector in the product.</param>
        /// <param name="b">Second vector in the product.</param>
        /// <returns>Resulting dot product.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(OrkEngine3D.Mathematics.Vector3 a, OrkEngine3D.Mathematics.Vector3 b)
        {
            return OrkEngine3D.Mathematics.Vector3.Dot(a, b);
        }

        /// <summary>
        /// Computes the dot product of two vectors.
        /// </summary>
        /// <param name="a">First vector in the product.</param>
        /// <param name="b">Second vector in the product.</param>
        /// <param name="product">Resulting dot product.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Dot(ref OrkEngine3D.Mathematics.Vector3 a, ref OrkEngine3D.Mathematics.Vector3 b, out float product)
        {
            product = OrkEngine3D.Mathematics.Vector3.Dot(a, b);
        }
        /// <summary>
        /// Adds two vectors together.
        /// </summary>
        /// <param name="a">First vector to add.</param>
        /// <param name="b">Second vector to add.</param>
        /// <param name="sum">Sum of the two vectors.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(ref OrkEngine3D.Mathematics.Vector3 a, ref OrkEngine3D.Mathematics.Vector3 b, out OrkEngine3D.Mathematics.Vector3 sum)
        {
            sum.X = a.X + b.X;
            sum.Y = a.Y + b.Y;
            sum.Z = a.Z + b.Z;
        }
        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name="a">Vector to subtract from.</param>
        /// <param name="b">Vector to subtract from the first vector.</param>
        /// <param name="difference">Result of the subtraction.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Subtract(ref OrkEngine3D.Mathematics.Vector3 a, ref OrkEngine3D.Mathematics.Vector3 b, out OrkEngine3D.Mathematics.Vector3 difference)
        {
            difference.X = a.X - b.X;
            difference.Y = a.Y - b.Y;
            difference.Z = a.Z - b.Z;
        }
        /// <summary>
        /// Scales a vector.
        /// </summary>
        /// <param name="v">Vector to scale.</param>
        /// <param name="scale">Amount to scale.</param>
        /// <param name="result">Scaled vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Multiply(ref OrkEngine3D.Mathematics.Vector3 v, float scale, out OrkEngine3D.Mathematics.Vector3 result)
        {
            result.X = v.X * scale;
            result.Y = v.Y * scale;
            result.Z = v.Z * scale;
        }
        /// <summary>
        /// Divides a vector's components by some amount.
        /// </summary>
        /// <param name="v">Vector to divide.</param>
        /// <param name="divisor">Value to divide the vector's components.</param>
        /// <param name="result">Result of the division.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Divide(ref OrkEngine3D.Mathematics.Vector3 v, float divisor, out OrkEngine3D.Mathematics.Vector3 result)
        {
            float inverse = 1 / divisor;
            result.X = v.X * inverse;
            result.Y = v.Y * inverse;
            result.Z = v.Z * inverse;
        }

        /// <summary>
        /// Computes the squared distance between two vectors.
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <param name="distanceSquared">Squared distance between the two vectors.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DistanceSquared(ref OrkEngine3D.Mathematics.Vector3 a, ref OrkEngine3D.Mathematics.Vector3 b, out float distanceSquared)
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;
            float z = a.Z - b.Z;
            distanceSquared = x * x + y * y + z * z;
        }

        /// <summary>
        /// Computes the squared distance between two vectors.
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <returns>Squared distance between the two vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceSquared(OrkEngine3D.Mathematics.Vector3 a, OrkEngine3D.Mathematics.Vector3 b)
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;
            float z = a.Z - b.Z;
            return x * x + y * y + z * z;
        }


        /// <summary>
        /// Computes the distance between two two vectors.
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <param name="distance">Distance between the two vectors.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Distance(ref OrkEngine3D.Mathematics.Vector3 a, ref OrkEngine3D.Mathematics.Vector3 b, out float distance)
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;
            float z = a.Z - b.Z;
            distance = (float)Math.Sqrt(x * x + y * y + z * z);
        }
        /// <summary>
        /// Computes the distance between two two vectors.
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <returns>Distance between the two vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(OrkEngine3D.Mathematics.Vector3 a, OrkEngine3D.Mathematics.Vector3 b)
        {
            float toReturn;
            Distance(ref a, ref b, out toReturn);
            return toReturn;
        }


        /// <summary>
        /// Gets the up vector (0,1,0).
        /// </summary>
        public static OrkEngine3D.Mathematics.Vector3 Up
        {
            get
            {
                return new OrkEngine3D.Mathematics.Vector3()
                {
                    X = 0,
                    Y = 1,
                    Z = 0
                };
            }
        }

        /// <summary>
        /// Gets the down vector (0,-1,0).
        /// </summary>
        public static OrkEngine3D.Mathematics.Vector3 Down
        {
            get
            {
                return new OrkEngine3D.Mathematics.Vector3()
                {
                    X = 0,
                    Y = -1,
                    Z = 0
                };
            }
        }

        /// <summary>
        /// Gets the right vector (1,0,0).
        /// </summary>
        public static OrkEngine3D.Mathematics.Vector3 Right
        {
            get
            {
                return new OrkEngine3D.Mathematics.Vector3()
                {
                    X = 1,
                    Y = 0,
                    Z = 0
                };
            }
        }

        /// <summary>
        /// Gets the left vector (-1,0,0).
        /// </summary>
        public static OrkEngine3D.Mathematics.Vector3 Left
        {
            get
            {
                return new OrkEngine3D.Mathematics.Vector3()
                {
                    X = -1,
                    Y = 0,
                    Z = 0
                };
            }
        }

        /// <summary>
        /// Gets the forward vector (0,0,-1).
        /// </summary>
        public static OrkEngine3D.Mathematics.Vector3 Forward
        {
            get
            {
                return new OrkEngine3D.Mathematics.Vector3()
                {
                    X = 0,
                    Y = 0,
                    Z = -1
                };
            }
        }

        /// <summary>
        /// Gets the back vector (0,0,1).
        /// </summary>
        public static OrkEngine3D.Mathematics.Vector3 Backward
        {
            get
            {
                return new OrkEngine3D.Mathematics.Vector3()
                {
                    X = 0,
                    Y = 0,
                    Z = 1
                };
            }
        }

        ///// <summary>
        ///// Computes the cross product between two vectors.
        ///// </summary>
        ///// <param name="a">First vector.</param>
        ///// <param name="b">Second vector.</param>
        ///// <returns>Cross product of the two vectors.</returns>
        //public static OrkEngine3D.Mathematics.Vector3 Cross(OrkEngine3D.Mathematics.Vector3 a, OrkEngine3D.Mathematics.Vector3 b)
        //{
        //    OrkEngine3D.Mathematics.Vector3 toReturn;
        //    Vector3Ex.Cross(ref a, ref b, out toReturn);
        //    return toReturn;
        //}
        /// <summary>
        /// Computes the cross product between two vectors.
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <param name="result">Cross product of the two vectors.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Cross(ref OrkEngine3D.Mathematics.Vector3 a, ref OrkEngine3D.Mathematics.Vector3 b, out OrkEngine3D.Mathematics.Vector3 result)
        {
            float resultX = a.Y * b.Z - a.Z * b.Y;
            float resultY = a.Z * b.X - a.X * b.Z;
            float resultZ = a.X * b.Y - a.Y * b.X;
            result.X = resultX;
            result.Y = resultY;
            result.Z = resultZ;
        }

        ///// <summary>
        ///// Normalizes the given vector.
        ///// </summary>
        ///// <param name="v">Vector to normalize.</param>
        ///// <returns>Normalized vector.</returns>
        //public static OrkEngine3D.Mathematics.Vector3 Normalize(OrkEngine3D.Mathematics.Vector3 v)
        //{
        //    OrkEngine3D.Mathematics.Vector3 toReturn;
        //    OrkEngine3D.Mathematics.Vector3.Normalize(ref v, out toReturn);
        //    return toReturn;
        //}

        /// <summary>
        /// Normalizes the given vector.
        /// </summary>
        /// <param name="v">Vector to normalize.</param>
        /// <param name="result">Normalized vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Normalize(ref OrkEngine3D.Mathematics.Vector3 v, out OrkEngine3D.Mathematics.Vector3 result)
        {
            result = OrkEngine3D.Mathematics.Vector3.Normalize(v);
        }

        /// <summary>
        /// Negates a vector.
        /// </summary>
        /// <param name="v">Vector to negate.</param>
        /// <param name="negated">Negated vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Negate(ref OrkEngine3D.Mathematics.Vector3 v, out OrkEngine3D.Mathematics.Vector3 negated)
        {
            negated = OrkEngine3D.Mathematics.Vector3.Negate(v);
        }

        /// <summary>
        /// Computes the absolute value of the input vector.
        /// </summary>
        /// <param name="v">Vector to take the absolute value of.</param>
        /// <param name="result">Vector with nonnegative elements.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Abs(ref OrkEngine3D.Mathematics.Vector3 v, out OrkEngine3D.Mathematics.Vector3 result)
        {
            result = OrkEngine3D.Mathematics.Vector3.Abs(v);
        }

        ///// <summary>
        ///// Computes the absolute value of the input vector.
        ///// </summary>
        ///// <param name="v">Vector to take the absolute value of.</param>
        ///// <returns>Vector with nonnegative elements.</returns>
        //public static OrkEngine3D.Mathematics.Vector3 Abs(OrkEngine3D.Mathematics.Vector3 v)
        //{
        //    OrkEngine3D.Mathematics.Vector3 result;
        //    Abs(ref v, out result);
        //    return result;
        //}

        /// <summary>
        /// Creates a vector from the lesser values in each vector.
        /// </summary>
        /// <param name="a">First input vector to compare values from.</param>
        /// <param name="b">Second input vector to compare values from.</param>
        /// <param name="min">Vector containing the lesser values of each vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Min(ref OrkEngine3D.Mathematics.Vector3 a, ref OrkEngine3D.Mathematics.Vector3 b, out OrkEngine3D.Mathematics.Vector3 min)
        {
            min = OrkEngine3D.Mathematics.Vector3.Min(a, b);
        }

        /// <summary>
        /// Creates a vector from the lesser values in each vector.
        /// </summary>
        /// <param name="a">First input vector to compare values from.</param>
        /// <param name="b">Second input vector to compare values from.</param>
        /// <returns>Vector containing the lesser values of each vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static OrkEngine3D.Mathematics.Vector3 Min(OrkEngine3D.Mathematics.Vector3 a, OrkEngine3D.Mathematics.Vector3 b)
        {
            return OrkEngine3D.Mathematics.Vector3.Min(a, b);
        }


        /// <summary>
        /// Creates a vector from the greater values in each vector.
        /// </summary>
        /// <param name="a">First input vector to compare values from.</param>
        /// <param name="b">Second input vector to compare values from.</param>
        /// <param name="max">Vector containing the greater values of each vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Max(ref OrkEngine3D.Mathematics.Vector3 a, ref OrkEngine3D.Mathematics.Vector3 b, out OrkEngine3D.Mathematics.Vector3 max)
        {
            max = OrkEngine3D.Mathematics.Vector3.Max(a, b);
        }

        /// <summary>
        /// Creates a vector from the greater values in each vector.
        /// </summary>
        /// <param name="a">First input vector to compare values from.</param>
        /// <param name="b">Second input vector to compare values from.</param>
        /// <returns>Vector containing the greater values of each vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static OrkEngine3D.Mathematics.Vector3 Max(OrkEngine3D.Mathematics.Vector3 a, OrkEngine3D.Mathematics.Vector3 b)
        {
            return OrkEngine3D.Mathematics.Vector3.Max(a, b);
        }

        /// <summary>
        /// Computes an interpolated state between two vectors.
        /// </summary>
        /// <param name="start">Starting location of the interpolation.</param>
        /// <param name="end">Ending location of the interpolation.</param>
        /// <param name="interpolationAmount">Amount of the end location to use.</param>
        /// <returns>Interpolated intermediate state.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static OrkEngine3D.Mathematics.Vector3 Lerp(OrkEngine3D.Mathematics.Vector3 start, OrkEngine3D.Mathematics.Vector3 end, float interpolationAmount)
        {
            OrkEngine3D.Mathematics.Vector3 toReturn;
            Lerp(ref start, ref end, interpolationAmount, out toReturn);
            return toReturn;
        }
        /// <summary>
        /// Computes an interpolated state between two vectors.
        /// </summary>
        /// <param name="start">Starting location of the interpolation.</param>
        /// <param name="end">Ending location of the interpolation.</param>
        /// <param name="interpolationAmount">Amount of the end location to use.</param>
        /// <param name="result">Interpolated intermediate state.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Lerp(ref OrkEngine3D.Mathematics.Vector3 start, ref OrkEngine3D.Mathematics.Vector3 end, float interpolationAmount, out OrkEngine3D.Mathematics.Vector3 result)
        {
            float startAmount = 1 - interpolationAmount;
            result.X = start.X * startAmount + end.X * interpolationAmount;
            result.Y = start.Y * startAmount + end.Y * interpolationAmount;
            result.Z = start.Z * startAmount + end.Z * interpolationAmount;
        }

        /// <summary>
        /// Computes an intermediate location using hermite interpolation.
        /// </summary>
        /// <param name="value1">First position.</param>
        /// <param name="tangent1">Tangent associated with the first position.</param>
        /// <param name="value2">Second position.</param>
        /// <param name="tangent2">Tangent associated with the second position.</param>
        /// <param name="interpolationAmount">Amount of the second point to use.</param>
        /// <param name="result">Interpolated intermediate state.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Hermite(ref OrkEngine3D.Mathematics.Vector3 value1, ref OrkEngine3D.Mathematics.Vector3 tangent1, ref OrkEngine3D.Mathematics.Vector3 value2, ref OrkEngine3D.Mathematics.Vector3 tangent2, float interpolationAmount, out OrkEngine3D.Mathematics.Vector3 result)
        {
            float weightSquared = interpolationAmount * interpolationAmount;
            float weightCubed = interpolationAmount * weightSquared;
            float value1Blend = 2 * weightCubed - 3 * weightSquared + 1;
            float tangent1Blend = weightCubed - 2 * weightSquared + interpolationAmount;
            float value2Blend = -2 * weightCubed + 3 * weightSquared;
            float tangent2Blend = weightCubed - weightSquared;
            result.X = value1.X * value1Blend + value2.X * value2Blend + tangent1.X * tangent1Blend + tangent2.X * tangent2Blend;
            result.Y = value1.Y * value1Blend + value2.Y * value2Blend + tangent1.Y * tangent1Blend + tangent2.Y * tangent2Blend;
            result.Z = value1.Z * value1Blend + value2.Z * value2Blend + tangent1.Z * tangent1Blend + tangent2.Z * tangent2Blend;
        }
        /// <summary>
        /// Computes an intermediate location using hermite interpolation.
        /// </summary>
        /// <param name="value1">First position.</param>
        /// <param name="tangent1">Tangent associated with the first position.</param>
        /// <param name="value2">Second position.</param>
        /// <param name="tangent2">Tangent associated with the second position.</param>
        /// <param name="interpolationAmount">Amount of the second point to use.</param>
        /// <returns>Interpolated intermediate state.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static OrkEngine3D.Mathematics.Vector3 Hermite(OrkEngine3D.Mathematics.Vector3 value1, OrkEngine3D.Mathematics.Vector3 tangent1, OrkEngine3D.Mathematics.Vector3 value2, OrkEngine3D.Mathematics.Vector3 tangent2, float interpolationAmount)
        {
            OrkEngine3D.Mathematics.Vector3 toReturn;
            Hermite(ref value1, ref tangent1, ref value2, ref tangent2, interpolationAmount, out toReturn);
            return toReturn;
        }
    }
}
