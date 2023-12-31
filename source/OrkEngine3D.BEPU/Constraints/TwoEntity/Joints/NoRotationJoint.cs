﻿using System;
using BEPUphysics.Entities;
using BEPUutilities;
 

namespace BEPUphysics.Constraints.TwoEntity.Joints
{
    /// <summary>
    /// Constrains two entities so that they cannot rotate relative to each other.
    /// </summary>
    public class NoRotationJoint : Joint, I3DImpulseConstraintWithError, I3DJacobianConstraint
    {
        private OrkEngine3D.Mathematics.Vector3 accumulatedImpulse;
        private OrkEngine3D.Mathematics.Vector3 biasVelocity;
        private Matrix3x3 effectiveMassMatrix;
        private OrkEngine3D.Mathematics.Quaternion initialQuaternionConjugateA;
        private OrkEngine3D.Mathematics.Quaternion initialQuaternionConjugateB;
        private OrkEngine3D.Mathematics.Vector3 error;

        /// <summary>
        /// Constructs a new constraint which prevents relative angular motion between the two connected bodies.
        /// To finish the initialization, specify the connections (ConnectionA and ConnectionB) and the initial orientations
        /// (InitialOrientationA, InitialOrientationB).
        /// This constructor sets the constraint's IsActive property to false by default.
        /// </summary>
        public NoRotationJoint()
        {
            IsActive = false;
        }

        /// <summary>
        /// Constructs a new constraint which prevents relative angular motion between the two connected bodies.
        /// </summary>
        /// <param name="connectionA">First connection of the pair.</param>
        /// <param name="connectionB">Second connection of the pair.</param>
        public NoRotationJoint(Entity connectionA, Entity connectionB)
        {
            ConnectionA = connectionA;
            ConnectionB = connectionB;

            initialQuaternionConjugateA = QuaternionEx.Conjugate(ConnectionA.orientation);
            initialQuaternionConjugateB = QuaternionEx.Conjugate(ConnectionB.orientation);
        }

        /// <summary>
        /// Gets or sets the initial orientation of the first connected entity.
        /// The constraint will try to maintain the relative orientation between the initialOrientationA and initialOrientationB.
        /// </summary>
        public OrkEngine3D.Mathematics.Quaternion InitialOrientationA
        {
            get { return QuaternionEx.Conjugate(initialQuaternionConjugateA); }
            set { initialQuaternionConjugateA = QuaternionEx.Conjugate(value); }
        }

        /// <summary>
        /// Gets or sets the initial orientation of the second connected entity.
        /// The constraint will try to maintain the relative orientation between the initialOrientationA and initialOrientationB.
        /// </summary>
        public OrkEngine3D.Mathematics.Quaternion InitialOrientationB
        {
            get { return QuaternionEx.Conjugate(initialQuaternionConjugateB); }
            set { initialQuaternionConjugateB = QuaternionEx.Conjugate(value); }
        }

        #region I3DImpulseConstraintWithError Members

        /// <summary>
        /// Gets the current relative velocity between the connected entities with respect to the constraint.
        /// </summary>
        public OrkEngine3D.Mathematics.Vector3 RelativeVelocity
        {
            get
            {
                OrkEngine3D.Mathematics.Vector3 velocityDifference;
                Vector3Ex.Subtract(ref connectionB.angularVelocity, ref connectionA.angularVelocity, out velocityDifference);
                return velocityDifference;
            }
        }

        /// <summary>
        /// Gets the total impulse applied by this constraint.
        /// </summary>
        public OrkEngine3D.Mathematics.Vector3 TotalImpulse
        {
            get { return accumulatedImpulse; }
        }

        /// <summary>
        /// Gets the current constraint error.
        /// </summary>
        public OrkEngine3D.Mathematics.Vector3 Error
        {
            get { return error; }
        }

        #endregion

        #region I3DJacobianConstraint Members

        /// <summary>
        /// Gets the linear jacobian entry for the first connected entity.
        /// </summary>
        /// <param name="jacobianX">First linear jacobian entry for the first connected entity.</param>
        /// <param name="jacobianY">Second linear jacobian entry for the first connected entity.</param>
        /// <param name="jacobianZ">Third linear jacobian entry for the first connected entity.</param>
        public void GetLinearJacobianA(out OrkEngine3D.Mathematics.Vector3 jacobianX, out OrkEngine3D.Mathematics.Vector3 jacobianY, out OrkEngine3D.Mathematics.Vector3 jacobianZ)
        {
            jacobianX = Toolbox.ZeroVector;
            jacobianY = Toolbox.ZeroVector;
            jacobianZ = Toolbox.ZeroVector;
        }

        /// <summary>
        /// Gets the linear jacobian entry for the second connected entity.
        /// </summary>
        /// <param name="jacobianX">First linear jacobian entry for the second connected entity.</param>
        /// <param name="jacobianY">Second linear jacobian entry for the second connected entity.</param>
        /// <param name="jacobianZ">Third linear jacobian entry for the second connected entity.</param>
        public void GetLinearJacobianB(out OrkEngine3D.Mathematics.Vector3 jacobianX, out OrkEngine3D.Mathematics.Vector3 jacobianY, out OrkEngine3D.Mathematics.Vector3 jacobianZ)
        {
            jacobianX = Toolbox.ZeroVector;
            jacobianY = Toolbox.ZeroVector;
            jacobianZ = Toolbox.ZeroVector;
        }

        /// <summary>
        /// Gets the angular jacobian entry for the first connected entity.
        /// </summary>
        /// <param name="jacobianX">First angular jacobian entry for the first connected entity.</param>
        /// <param name="jacobianY">Second angular jacobian entry for the first connected entity.</param>
        /// <param name="jacobianZ">Third angular jacobian entry for the first connected entity.</param>
        public void GetAngularJacobianA(out OrkEngine3D.Mathematics.Vector3 jacobianX, out OrkEngine3D.Mathematics.Vector3 jacobianY, out OrkEngine3D.Mathematics.Vector3 jacobianZ)
        {
            jacobianX = Toolbox.RightVector;
            jacobianY = Toolbox.UpVector;
            jacobianZ = Toolbox.BackVector;
        }

        /// <summary>
        /// Gets the angular jacobian entry for the second connected entity.
        /// </summary>
        /// <param name="jacobianX">First angular jacobian entry for the second connected entity.</param>
        /// <param name="jacobianY">Second angular jacobian entry for the second connected entity.</param>
        /// <param name="jacobianZ">Third angular jacobian entry for the second connected entity.</param>
        public void GetAngularJacobianB(out OrkEngine3D.Mathematics.Vector3 jacobianX, out OrkEngine3D.Mathematics.Vector3 jacobianY, out OrkEngine3D.Mathematics.Vector3 jacobianZ)
        {
            jacobianX = Toolbox.RightVector;
            jacobianY = Toolbox.UpVector;
            jacobianZ = Toolbox.BackVector;
        }

        /// <summary>
        /// Gets the mass matrix of the constraint.
        /// </summary>
        /// <param name="outputMassMatrix">Constraint's mass matrix.</param>
        public void GetMassMatrix(out Matrix3x3 outputMassMatrix)
        {
            outputMassMatrix = effectiveMassMatrix;
        }

        #endregion

        /// <summary>
        /// Applies the corrective impulses required by the constraint.
        /// </summary>
        public override float SolveIteration()
        {
            OrkEngine3D.Mathematics.Vector3 velocityDifference;
            Vector3Ex.Subtract(ref connectionB.angularVelocity, ref connectionA.angularVelocity, out velocityDifference);
            OrkEngine3D.Mathematics.Vector3 softnessVector;
            Vector3Ex.Multiply(ref accumulatedImpulse, softness, out softnessVector);

            OrkEngine3D.Mathematics.Vector3 lambda;
            Vector3Ex.Add(ref velocityDifference, ref biasVelocity, out lambda);
            Vector3Ex.Subtract(ref lambda, ref softnessVector, out lambda);
            Matrix3x3.Transform(ref lambda, ref effectiveMassMatrix, out lambda);

            Vector3Ex.Add(ref lambda, ref accumulatedImpulse, out accumulatedImpulse);
            if (connectionA.isDynamic)
            {
                connectionA.ApplyAngularImpulse(ref lambda);
            }
            if (connectionB.isDynamic)
            {
                OrkEngine3D.Mathematics.Vector3 torqueB;
                Vector3Ex.Negate(ref lambda, out torqueB);
                connectionB.ApplyAngularImpulse(ref torqueB);
            }

            return Math.Abs(lambda.X) + Math.Abs(lambda.Y) + Math.Abs(lambda.Z);
        }

        /// <summary>
        /// Initializes the constraint for the current frame.
        /// </summary>
        /// <param name="dt">Time between frames.</param>
        public override void Update(float dt)
        {
            OrkEngine3D.Mathematics.Quaternion quaternionA;
            QuaternionEx.Multiply(ref connectionA.orientation, ref initialQuaternionConjugateA, out quaternionA);
            OrkEngine3D.Mathematics.Quaternion quaternionB;
            QuaternionEx.Multiply(ref connectionB.orientation, ref initialQuaternionConjugateB, out quaternionB);
            QuaternionEx.Conjugate(ref quaternionB, out quaternionB);
            OrkEngine3D.Mathematics.Quaternion intermediate;
            QuaternionEx.Multiply(ref quaternionA, ref quaternionB, out intermediate);


            float angle;
            OrkEngine3D.Mathematics.Vector3 axis;
            QuaternionEx.GetAxisAngleFromQuaternion(ref intermediate, out axis, out angle);

            error.X = axis.X * angle;
            error.Y = axis.Y * angle;
            error.Z = axis.Z * angle;

            float errorReduction;
            springSettings.ComputeErrorReductionAndSoftness(dt, 1 / dt, out errorReduction, out softness);
            errorReduction = -errorReduction;
            biasVelocity.X = errorReduction * error.X;
            biasVelocity.Y = errorReduction * error.Y;
            biasVelocity.Z = errorReduction * error.Z;

            //Ensure that the corrective velocity doesn't exceed the max.
            float length = biasVelocity.LengthSquared();
            if (length > maxCorrectiveVelocitySquared)
            {
                float multiplier = maxCorrectiveVelocity / (float) Math.Sqrt(length);
                biasVelocity.X *= multiplier;
                biasVelocity.Y *= multiplier;
                biasVelocity.Z *= multiplier;
            }

            Matrix3x3.Add(ref connectionA.inertiaTensorInverse, ref connectionB.inertiaTensorInverse, out effectiveMassMatrix);
            effectiveMassMatrix.M11 += softness;
            effectiveMassMatrix.M22 += softness;
            effectiveMassMatrix.M33 += softness;
            Matrix3x3.Invert(ref effectiveMassMatrix, out effectiveMassMatrix);


           
        }

        /// <summary>
        /// Performs any pre-solve iteration work that needs exclusive
        /// access to the members of the solver updateable.
        /// Usually, this is used for applying warmstarting impulses.
        /// </summary>
        public override void ExclusiveUpdate()
        {
            //apply accumulated impulse
            if (connectionA.isDynamic)
            {
                connectionA.ApplyAngularImpulse(ref accumulatedImpulse);
            }
            if (connectionB.isDynamic)
            {
                OrkEngine3D.Mathematics.Vector3 torqueB;
                Vector3Ex.Negate(ref accumulatedImpulse, out torqueB);
                connectionB.ApplyAngularImpulse(ref torqueB);
            }
        } 
    }
}