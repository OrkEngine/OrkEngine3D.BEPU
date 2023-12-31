﻿using System;
using BEPUphysics.Entities;
using BEPUutilities;
 

namespace BEPUphysics.Constraints.SingleEntity
{
    /// <summary>
    /// Prevents the target entity from moving faster than the specified speeds.
    /// </summary>
    public class MaximumAngularSpeedConstraint : SingleEntityConstraint, I3DImpulseConstraint
    {
        private Matrix3x3 effectiveMassMatrix;
        private float maxForceDt = float.MaxValue;
        private float maxForceDtSquared = float.MaxValue;
        private OrkEngine3D.Mathematics.Vector3 accumulatedImpulse;
        private float maximumForce = float.MaxValue;
        private float maximumSpeed;
        private float maximumSpeedSquared;

        private float softness = .00001f;
        private float usedSoftness;

        /// <summary>
        /// Constructs a maximum speed constraint.
        /// Set its Entity and MaximumSpeed to complete the configuration.
        /// IsActive also starts as false with this constructor.
        /// </summary>
        public MaximumAngularSpeedConstraint()
        {
            IsActive = false;
        }

        /// <summary>
        /// Constructs a maximum speed constraint.
        /// </summary>
        /// <param name="e">Affected entity.</param>
        /// <param name="maxSpeed">Maximum angular speed allowed.</param>
        public MaximumAngularSpeedConstraint(Entity e, float maxSpeed)
        {
            Entity = e;
            MaximumSpeed = maxSpeed;
        }

        /// <summary>
        /// Gets and sets the maximum impulse that the constraint will attempt to apply when satisfying its requirements.
        /// This field can be used to simulate friction in a constraint.
        /// </summary>
        public float MaximumForce
        {
            get
            {
                if (maximumForce > 0)
                {
                    return maximumForce;
                }
                return 0;
            }
            set { maximumForce = value >= 0 ? value : 0; }
        }

        /// <summary>
        /// Gets or sets the maximum angular speed that this constraint allows.
        /// </summary>
        public float MaximumSpeed
        {
            get { return maximumSpeed; }
            set
            {
                maximumSpeed = MathHelper.Max(0, value);
                maximumSpeedSquared = maximumSpeed * maximumSpeed;
            }
        }


        /// <summary>
        /// Gets and sets the softness of this constraint.
        /// Higher values of softness allow the constraint to be violated more.
        /// Must be greater than zero.
        /// Sometimes, if a joint system is unstable, increasing the softness of the involved constraints will make it settle down.
        /// For motors, softness can be used to implement damping.  For a damping constant k, the appropriate softness is 1/k.
        /// </summary>
        public float Softness
        {
            get { return softness; }
            set { softness = Math.Max(0, value); }
        }

        #region I3DImpulseConstraint Members

        /// <summary>
        /// Gets the current relative velocity between the connected entities with respect to the constraint.
        /// </summary>
        OrkEngine3D.Mathematics.Vector3 I3DImpulseConstraint.RelativeVelocity
        {
            get { return entity.angularVelocity; }
        }

        /// <summary>
        /// Gets the total impulse applied by the constraint.
        /// </summary>
        public OrkEngine3D.Mathematics.Vector3 TotalImpulse
        {
            get { return accumulatedImpulse; }
        }

        #endregion

        /// <summary>
        /// Calculates and applies corrective impulses.
        /// Called automatically by space.
        /// </summary>
        public override float SolveIteration()
        {
            float angularSpeed = entity.angularVelocity.LengthSquared();
            if (angularSpeed > maximumSpeedSquared)
            {
                angularSpeed = (float)Math.Sqrt(angularSpeed);
                OrkEngine3D.Mathematics.Vector3 impulse;
                //divide by angularSpeed to normalize the velocity.
                //Multiply by angularSpeed - maximumSpeed to get the 'velocity change vector.'
                Vector3Ex.Multiply(ref entity.angularVelocity, -(angularSpeed - maximumSpeed) / angularSpeed, out impulse);

                //incorporate softness
                OrkEngine3D.Mathematics.Vector3 softnessImpulse;
                Vector3Ex.Multiply(ref accumulatedImpulse, usedSoftness, out softnessImpulse);
                Vector3Ex.Subtract(ref impulse, ref softnessImpulse, out impulse);

                //Transform into impulse
                Matrix3x3.Transform(ref impulse, ref effectiveMassMatrix, out impulse);


                //Accumulate
                OrkEngine3D.Mathematics.Vector3 previousAccumulatedImpulse = accumulatedImpulse;
                Vector3Ex.Add(ref accumulatedImpulse, ref impulse, out accumulatedImpulse);
                float forceMagnitude = accumulatedImpulse.LengthSquared();
                if (forceMagnitude > maxForceDtSquared)
                {
                    //max / impulse gives some value 0 < x < 1.  Basically, normalize the vector (divide by the length) and scale by the maximum.
                    float multiplier = maxForceDt / (float)Math.Sqrt(forceMagnitude);
                    accumulatedImpulse.X *= multiplier;
                    accumulatedImpulse.Y *= multiplier;
                    accumulatedImpulse.Z *= multiplier;

                    //Since the limit was exceeded by this corrective impulse, limit it so that the accumulated impulse remains constrained.
                    impulse.X = accumulatedImpulse.X - previousAccumulatedImpulse.X;
                    impulse.Y = accumulatedImpulse.Y - previousAccumulatedImpulse.Y;
                    impulse.Z = accumulatedImpulse.Z - previousAccumulatedImpulse.Z;
                }

                entity.ApplyAngularImpulse(ref impulse);


                return (Math.Abs(impulse.X) + Math.Abs(impulse.Y) + Math.Abs(impulse.Z));
            }

            return 0;
        }

        /// <summary>
        /// Calculates necessary information for velocity solving.
        /// Called automatically by space.
        /// </summary>
        /// <param name="dt">Time in seconds since the last update.</param>
        public override void Update(float dt)
        {
            usedSoftness = softness / dt;

            effectiveMassMatrix = entity.inertiaTensorInverse;

            effectiveMassMatrix.M11 += usedSoftness;
            effectiveMassMatrix.M22 += usedSoftness;
            effectiveMassMatrix.M33 += usedSoftness;

            Matrix3x3.Invert(ref effectiveMassMatrix, out effectiveMassMatrix);

            //Determine maximum force
            if (maximumForce < float.MaxValue)
            {
                maxForceDt = maximumForce * dt;
                maxForceDtSquared = maxForceDt * maxForceDt;
            }
            else
            {
                maxForceDt = float.MaxValue;
                maxForceDtSquared = float.MaxValue;
            }

        }


        public override void ExclusiveUpdate()
        {

            //Can't do warmstarting due to the strangeness of this constraint (not based on a position error, nor is it really a motor).
            accumulatedImpulse = Toolbox.ZeroVector;
        }
    }
}