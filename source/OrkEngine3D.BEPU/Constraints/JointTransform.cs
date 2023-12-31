﻿using System;
using BEPUutilities;
 

namespace BEPUphysics.Constraints
{
    /// <summary>
    /// Defines a three dimensional orthonormal basis used by a constraint.
    /// </summary>
    public class JointBasis3D
    {
        internal OrkEngine3D.Mathematics.Vector3 localPrimaryAxis = Vector3Ex.Backward;
        internal OrkEngine3D.Mathematics.Vector3 localXAxis = Vector3Ex.Right;
        internal OrkEngine3D.Mathematics.Vector3 localYAxis = Vector3Ex.Up;
        internal OrkEngine3D.Mathematics.Vector3 primaryAxis = Vector3Ex.Backward;
        internal Matrix3x3 rotationMatrix = Matrix3x3.Identity;
        internal OrkEngine3D.Mathematics.Vector3 xAxis = Vector3Ex.Right;
        internal OrkEngine3D.Mathematics.Vector3 yAxis = Vector3Ex.Up;

        /// <summary>
        /// Gets the primary axis of the transform in local space.
        /// </summary>
        public OrkEngine3D.Mathematics.Vector3 LocalPrimaryAxis
        {
            get { return localPrimaryAxis; }
        }

        /// <summary>
        /// Gets or sets the local transform of the basis.
        /// </summary>
        public Matrix3x3 LocalTransform
        {
            get
            {
                var toReturn = new Matrix3x3 {Right = localXAxis, Up = localYAxis, Backward = localPrimaryAxis};
                return toReturn;
            }
            set { SetLocalAxes(value); }
        }

        /// <summary>
        /// Gets the X axis of the transform in local space.
        /// </summary>
        public OrkEngine3D.Mathematics.Vector3 LocalXAxis
        {
            get { return localXAxis; }
        }

        /// <summary>
        /// Gets the Y axis of the transform in local space.
        /// </summary>
        public OrkEngine3D.Mathematics.Vector3 LocalYAxis
        {
            get { return localYAxis; }
        }

        /// <summary>
        /// Gets the primary axis of the transform.
        /// </summary>
        public OrkEngine3D.Mathematics.Vector3 PrimaryAxis
        {
            get { return primaryAxis; }
        }

        /// <summary>
        /// Gets or sets the rotation matrix used by the joint transform to convert local space axes to world space.
        /// </summary>
        public Matrix3x3 RotationMatrix
        {
            get { return rotationMatrix; }
            set
            {
                rotationMatrix = value;
                ComputeWorldSpaceAxes();
            }
        }

        /// <summary>
        /// Gets or sets the world transform of the basis.
        /// </summary>
        public Matrix3x3 WorldTransform
        {
            get
            {
                var toReturn = new Matrix3x3 {Right = xAxis, Up = yAxis, Backward = primaryAxis};
                return toReturn;
            }
            set { SetWorldAxes(value); }
        }

        /// <summary>
        /// Gets the X axis of the transform.
        /// </summary>
        public OrkEngine3D.Mathematics.Vector3 XAxis
        {
            get { return xAxis; }
        }

        /// <summary>
        /// Gets the Y axis of the transform.
        /// </summary>
        public OrkEngine3D.Mathematics.Vector3 YAxis
        {
            get { return yAxis; }
        }


        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="primaryAxis">First axis in the transform.  Usually aligned along the main axis of a joint, like the twist axis of a TwistLimit.</param>
        /// <param name="xAxis">Second axis in the transform.</param>
        /// <param name="yAxis">Third axis in the transform.</param>
        /// <param name="rotationMatrix">OrkEngine3D.Mathematics.Matrix4x4 to use to transform the local axes into world space.</param>
        public void SetLocalAxes(OrkEngine3D.Mathematics.Vector3 primaryAxis, OrkEngine3D.Mathematics.Vector3 xAxis, OrkEngine3D.Mathematics.Vector3 yAxis, Matrix3x3 rotationMatrix)
        {
            this.rotationMatrix = rotationMatrix;
            SetLocalAxes(primaryAxis, xAxis, yAxis);
        }


        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="primaryAxis">First axis in the transform.  Usually aligned along the main axis of a joint, like the twist axis of a TwistLimit.</param>
        /// <param name="xAxis">Second axis in the transform.</param>
        /// <param name="yAxis">Third axis in the transform.</param>
        public void SetLocalAxes(OrkEngine3D.Mathematics.Vector3 primaryAxis, OrkEngine3D.Mathematics.Vector3 xAxis, OrkEngine3D.Mathematics.Vector3 yAxis)
        {
            if (Math.Abs(Vector3Ex.Dot(primaryAxis, xAxis)) > Toolbox.BigEpsilon ||
                Math.Abs(Vector3Ex.Dot(primaryAxis, yAxis)) > Toolbox.BigEpsilon ||
                Math.Abs(Vector3Ex.Dot(xAxis, yAxis)) > Toolbox.BigEpsilon)
                throw new ArgumentException("The axes provided to the joint transform do not form an orthonormal basis.  Ensure that each axis is perpendicular to the other two.");

            localPrimaryAxis = OrkEngine3D.Mathematics.Vector3.Normalize(primaryAxis);
            localXAxis = OrkEngine3D.Mathematics.Vector3.Normalize(xAxis);
            localYAxis = OrkEngine3D.Mathematics.Vector3.Normalize(yAxis);
            ComputeWorldSpaceAxes();
        }

        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="matrix">Rotation matrix representing the three axes.
        /// The matrix's backward vector is used as the primary axis.  
        /// The matrix's right vector is used as the x axis.
        /// The matrix's up vector is used as the y axis.</param>
        public void SetLocalAxes(Matrix3x3 matrix)
        {
            if (Math.Abs(Vector3Ex.Dot(matrix.Backward, matrix.Right)) > Toolbox.BigEpsilon ||
                Math.Abs(Vector3Ex.Dot(matrix.Backward, matrix.Up)) > Toolbox.BigEpsilon ||
                Math.Abs(Vector3Ex.Dot(matrix.Right, matrix.Up)) > Toolbox.BigEpsilon)
                throw new ArgumentException("The axes provided to the joint transform do not form an orthonormal basis.  Ensure that each axis is perpendicular to the other two.");

            localPrimaryAxis = OrkEngine3D.Mathematics.Vector3.Normalize(matrix.Backward);
            localXAxis = OrkEngine3D.Mathematics.Vector3.Normalize(matrix.Right);
            localYAxis = OrkEngine3D.Mathematics.Vector3.Normalize(matrix.Up);
            ComputeWorldSpaceAxes();
        }


        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="primaryAxis">First axis in the transform.  Usually aligned along the main axis of a joint, like the twist axis of a TwistLimit.</param>
        /// <param name="xAxis">Second axis in the transform.</param>
        /// <param name="yAxis">Third axis in the transform.</param>
        /// <param name="rotationMatrix">OrkEngine3D.Mathematics.Matrix4x4 to use to transform the local axes into world space.</param>
        public void SetWorldAxes(OrkEngine3D.Mathematics.Vector3 primaryAxis, OrkEngine3D.Mathematics.Vector3 xAxis, OrkEngine3D.Mathematics.Vector3 yAxis, Matrix3x3 rotationMatrix)
        {
            this.rotationMatrix = rotationMatrix;
            SetWorldAxes(primaryAxis, xAxis, yAxis);
        }

        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="primaryAxis">First axis in the transform.  Usually aligned along the main axis of a joint, like the twist axis of a TwistLimit.</param>
        /// <param name="xAxis">Second axis in the transform.</param>
        /// <param name="yAxis">Third axis in the transform.</param>
        public void SetWorldAxes(OrkEngine3D.Mathematics.Vector3 primaryAxis, OrkEngine3D.Mathematics.Vector3 xAxis, OrkEngine3D.Mathematics.Vector3 yAxis)
        {
            if (Math.Abs(Vector3Ex.Dot(primaryAxis, xAxis)) > Toolbox.BigEpsilon ||
                Math.Abs(Vector3Ex.Dot(primaryAxis, yAxis)) > Toolbox.BigEpsilon ||
                Math.Abs(Vector3Ex.Dot(xAxis, yAxis)) > Toolbox.BigEpsilon)
                throw new ArgumentException("The axes provided to the joint transform do not form an orthonormal basis.  Ensure that each axis is perpendicular to the other two.");

            this.primaryAxis = OrkEngine3D.Mathematics.Vector3.Normalize(primaryAxis);
            this.xAxis = OrkEngine3D.Mathematics.Vector3.Normalize(xAxis);
            this.yAxis = OrkEngine3D.Mathematics.Vector3.Normalize(yAxis);
            Matrix3x3.TransformTranspose(ref this.primaryAxis, ref rotationMatrix, out localPrimaryAxis);
            Matrix3x3.TransformTranspose(ref this.xAxis, ref rotationMatrix, out localXAxis);
            Matrix3x3.TransformTranspose(ref this.yAxis, ref rotationMatrix, out localYAxis);
        }

        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="matrix">Rotation matrix representing the three axes.
        /// The matrix's backward vector is used as the primary axis.  
        /// The matrix's right vector is used as the x axis.
        /// The matrix's up vector is used as the y axis.</param>
        public void SetWorldAxes(Matrix3x3 matrix)
        {
            if (Math.Abs(Vector3Ex.Dot(matrix.Backward, matrix.Right)) > Toolbox.BigEpsilon ||
                Math.Abs(Vector3Ex.Dot(matrix.Backward, matrix.Up)) > Toolbox.BigEpsilon ||
                Math.Abs(Vector3Ex.Dot(matrix.Right, matrix.Up)) > Toolbox.BigEpsilon)
                throw new ArgumentException("The axes provided to the joint transform do not form an orthonormal basis.  Ensure that each axis is perpendicular to the other two.");

            primaryAxis = OrkEngine3D.Mathematics.Vector3.Normalize(matrix.Backward);
            xAxis = OrkEngine3D.Mathematics.Vector3.Normalize(matrix.Right);
            yAxis = OrkEngine3D.Mathematics.Vector3.Normalize(matrix.Up);
            Matrix3x3.TransformTranspose(ref this.primaryAxis, ref rotationMatrix, out localPrimaryAxis);
            Matrix3x3.TransformTranspose(ref this.xAxis, ref rotationMatrix, out localXAxis);
            Matrix3x3.TransformTranspose(ref this.yAxis, ref rotationMatrix, out localYAxis);
        }

        internal void ComputeWorldSpaceAxes()
        {
            Matrix3x3.Transform(ref localPrimaryAxis, ref rotationMatrix, out primaryAxis);
            Matrix3x3.Transform(ref localXAxis, ref rotationMatrix, out xAxis);
            Matrix3x3.Transform(ref localYAxis, ref rotationMatrix, out yAxis);
        }
    }

    /// <summary>
    /// Defines a two axes which are perpendicular to each other used by a constraint.
    /// </summary>
    public class JointBasis2D
    {
        internal OrkEngine3D.Mathematics.Vector3 localPrimaryAxis = Vector3Ex.Backward;
        internal OrkEngine3D.Mathematics.Vector3 localXAxis = Vector3Ex.Right;
        internal OrkEngine3D.Mathematics.Vector3 primaryAxis = Vector3Ex.Backward;
        internal Matrix3x3 rotationMatrix = Matrix3x3.Identity;
        internal OrkEngine3D.Mathematics.Vector3 xAxis = Vector3Ex.Right;

        /// <summary>
        /// Gets the primary axis of the transform in local space.
        /// </summary>
        public OrkEngine3D.Mathematics.Vector3 LocalPrimaryAxis
        {
            get { return localPrimaryAxis; }
        }

        /// <summary>
        /// Gets the X axis of the transform in local space.
        /// </summary>
        public OrkEngine3D.Mathematics.Vector3 LocalXAxis
        {
            get { return localXAxis; }
        }

        /// <summary>
        /// Gets the primary axis of the transform.
        /// </summary>
        public OrkEngine3D.Mathematics.Vector3 PrimaryAxis
        {
            get { return primaryAxis; }
        }

        /// <summary>
        /// Gets or sets the rotation matrix used by the joint transform to convert local space axes to world space.
        /// </summary>
        public Matrix3x3 RotationMatrix
        {
            get { return rotationMatrix; }
            set
            {
                rotationMatrix = value;
                ComputeWorldSpaceAxes();
            }
        }

        /// <summary>
        /// Gets the X axis of the transform.
        /// </summary>
        public OrkEngine3D.Mathematics.Vector3 XAxis
        {
            get { return xAxis; }
        }


        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="primaryAxis">First axis in the transform.  Usually aligned along the main axis of a joint, like the twist axis of a TwistLimit.</param>
        /// <param name="xAxis">Second axis in the transform.</param>
        /// <param name="rotationMatrix">OrkEngine3D.Mathematics.Matrix4x4 to use to transform the local axes into world space.</param>
        public void SetLocalAxes(OrkEngine3D.Mathematics.Vector3 primaryAxis, OrkEngine3D.Mathematics.Vector3 xAxis, Matrix3x3 rotationMatrix)
        {
            this.rotationMatrix = rotationMatrix;
            SetLocalAxes(primaryAxis, xAxis);
        }

        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="primaryAxis">First axis in the transform.  Usually aligned along the main axis of a joint, like the twist axis of a TwistLimit.</param>
        /// <param name="xAxis">Second axis in the transform.</param>
        public void SetLocalAxes(OrkEngine3D.Mathematics.Vector3 primaryAxis, OrkEngine3D.Mathematics.Vector3 xAxis)
        {
            if (Math.Abs(Vector3Ex.Dot(primaryAxis, xAxis)) > Toolbox.BigEpsilon)
                throw new ArgumentException("The axes provided to the joint transform are not perpendicular.  Ensure that the specified axes form a valid constraint.");

            localPrimaryAxis = OrkEngine3D.Mathematics.Vector3.Normalize(primaryAxis);
            localXAxis = OrkEngine3D.Mathematics.Vector3.Normalize(xAxis);
            ComputeWorldSpaceAxes();
        }

        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="matrix">Rotation matrix representing the three axes.
        /// The matrix's backward vector is used as the primary axis.  
        /// The matrix's right vector is used as the x axis.</param>
        public void SetLocalAxes(Matrix3x3 matrix)
        {
            if (Math.Abs(Vector3Ex.Dot(matrix.Backward, matrix.Right)) > Toolbox.BigEpsilon)
                throw new ArgumentException("The axes provided to the joint transform are not perpendicular.  Ensure that the specified axes form a valid constraint.");
            localPrimaryAxis = OrkEngine3D.Mathematics.Vector3.Normalize(matrix.Backward);
            localXAxis = OrkEngine3D.Mathematics.Vector3.Normalize(matrix.Right);
            ComputeWorldSpaceAxes();
        }


        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="primaryAxis">First axis in the transform.  Usually aligned along the main axis of a joint, like the twist axis of a TwistLimit.</param>
        /// <param name="xAxis">Second axis in the transform.</param>
        /// <param name="rotationMatrix">OrkEngine3D.Mathematics.Matrix4x4 to use to transform the local axes into world space.</param>
        public void SetWorldAxes(OrkEngine3D.Mathematics.Vector3 primaryAxis, OrkEngine3D.Mathematics.Vector3 xAxis, Matrix3x3 rotationMatrix)
        {
            this.rotationMatrix = rotationMatrix;
            SetWorldAxes(primaryAxis, xAxis);
        }

        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="primaryAxis">First axis in the transform.  Usually aligned along the main axis of a joint, like the twist axis of a TwistLimit.</param>
        /// <param name="xAxis">Second axis in the transform.</param>
        public void SetWorldAxes(OrkEngine3D.Mathematics.Vector3 primaryAxis, OrkEngine3D.Mathematics.Vector3 xAxis)
        {
            if (Math.Abs(Vector3Ex.Dot(primaryAxis, xAxis)) > Toolbox.BigEpsilon)
                throw new ArgumentException("The axes provided to the joint transform are not perpendicular.  Ensure that the specified axes form a valid constraint.");
            this.primaryAxis = OrkEngine3D.Mathematics.Vector3.Normalize(primaryAxis);
            this.xAxis = OrkEngine3D.Mathematics.Vector3.Normalize(xAxis);
            Matrix3x3.TransformTranspose(ref this.primaryAxis, ref rotationMatrix, out localPrimaryAxis);
            Matrix3x3.TransformTranspose(ref this.xAxis, ref rotationMatrix, out localXAxis);
        }

        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="matrix">Rotation matrix representing the three axes.
        /// The matrix's backward vector is used as the primary axis.  
        /// The matrix's right vector is used as the x axis.</param>
        public void SetWorldAxes(Matrix3x3 matrix)
        {
            if (Math.Abs(Vector3Ex.Dot(matrix.Backward, matrix.Right)) > Toolbox.BigEpsilon)
                throw new ArgumentException("The axes provided to the joint transform are not perpendicular.  Ensure that the specified axes form a valid constraint.");
            primaryAxis = OrkEngine3D.Mathematics.Vector3.Normalize(matrix.Backward);
            xAxis = OrkEngine3D.Mathematics.Vector3.Normalize(matrix.Right);
            Matrix3x3.TransformTranspose(ref this.primaryAxis, ref rotationMatrix, out localPrimaryAxis);
            Matrix3x3.TransformTranspose(ref this.xAxis, ref rotationMatrix, out localXAxis);
        }

        internal void ComputeWorldSpaceAxes()
        {
            Matrix3x3.Transform(ref localPrimaryAxis, ref rotationMatrix, out primaryAxis);
            Matrix3x3.Transform(ref localXAxis, ref rotationMatrix, out xAxis);
        }
    }
}