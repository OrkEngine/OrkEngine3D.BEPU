﻿using System;
using BEPUphysics.Constraints.TwoEntity.Motors;
using BEPUphysics.Entities;
using BEPUutilities;
 

namespace BEPUphysics.Constraints.SingleEntity
{
    /// <summary>
    /// Constraint which tries to push an entity to a desired location.
    /// </summary>
    public class SingleEntityLinearMotor : SingleEntityConstraint, I3DImpulseConstraintWithError
    {
        private readonly MotorSettings3D settings;

        /// <summary>
        /// Sum of forces applied to the constraint in the past.
        /// </summary>
        private OrkEngine3D.Mathematics.Vector3 accumulatedImpulse = OrkEngine3D.Mathematics.Vector3.Zero;

        private OrkEngine3D.Mathematics.Vector3 biasVelocity;
        private Matrix3x3 effectiveMassMatrix;

        /// <summary>
        /// Maximum impulse that can be applied in a single frame.
        /// </summary>
        private float maxForceDt;

        /// <summary>
        /// Maximum impulse that can be applied in a single frame, squared.
        /// This is computed in the prestep to avoid doing extra multiplies in the more-often called applyImpulse method.
        /// </summary>
        private float maxForceDtSquared;

        private OrkEngine3D.Mathematics.Vector3 error;

        private OrkEngine3D.Mathematics.Vector3 localPoint;

        private OrkEngine3D.Mathematics.Vector3 worldPoint;

        private OrkEngine3D.Mathematics.Vector3 r;
        private float usedSoftness;

        /// <summary>
        /// Gets or sets the entity affected by the constraint.
        /// </summary>
        public override Entity Entity
        {
            get
            {
                return base.Entity;
            }
            set
            {
                if (Entity != value)
                    accumulatedImpulse = new OrkEngine3D.Mathematics.Vector3();
                base.Entity = value;
            }
        }


        /// <summary>
        /// Constructs a new single body linear motor.  This motor will try to move a single entity to a goal velocity or to a goal position.
        /// </summary>
        /// <param name="entity">Entity to affect.</param>
        /// <param name="point">Point in world space attached to the entity that will be motorized.</param>
        public SingleEntityLinearMotor(Entity entity, OrkEngine3D.Mathematics.Vector3 point)
        {
            Entity = entity;
            Point = point;

            settings = new MotorSettings3D(this) {servo = {goal = point}};
            //Not really necessary, just helps prevent 'snapping'.
        }


        /// <summary>
        /// Constructs a new single body linear motor.  This motor will try to move a single entity to a goal velocity or to a goal position.
        /// This constructor will start the motor with isActive = false.
        /// </summary>
        public SingleEntityLinearMotor()
        {
            settings = new MotorSettings3D(this);
            IsActive = false;
        }

        /// <summary>
        /// Point attached to the entity in its local space that is motorized.
        /// </summary>
        public OrkEngine3D.Mathematics.Vector3 LocalPoint
        {
            get { return localPoint; }
            set
            {
                localPoint = value;
                Matrix3x3.Transform(ref localPoint, ref entity.orientationMatrix, out worldPoint);
                Vector3Ex.Add(ref worldPoint, ref entity.position, out worldPoint);
            }
        }

        /// <summary>
        /// Point attached to the entity in world space that is motorized.
        /// </summary>
        public OrkEngine3D.Mathematics.Vector3 Point
        {
            get { return worldPoint; }
            set
            {
                worldPoint = value;
                Vector3Ex.Subtract(ref worldPoint, ref entity.position, out localPoint);
                Matrix3x3.TransformTranspose(ref localPoint, ref entity.orientationMatrix, out localPoint);
            }
        }

        /// <summary>
        /// Gets the motor's velocity and servo settings.
        /// </summary>
        public MotorSettings3D Settings
        {
            get { return settings; }
        }

        #region I3DImpulseConstraintWithError Members

        /// <summary>
        /// Gets the current relative velocity between the connected entities with respect to the constraint.
        /// </summary>
        public OrkEngine3D.Mathematics.Vector3 RelativeVelocity
        {
            get
            {
                OrkEngine3D.Mathematics.Vector3 lambda;
                Vector3Ex.Cross(ref r, ref entity.angularVelocity, out lambda);
                Vector3Ex.Subtract(ref lambda, ref entity.linearVelocity, out lambda);
                return lambda;
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
        /// If the motor is in velocity only mode, error is zero.
        /// </summary>
        public OrkEngine3D.Mathematics.Vector3 Error
        {
            get { return error; }
        }

        #endregion

        /// <summary>
        /// Computes one iteration of the constraint to meet the solver updateable's goal.
        /// </summary>
        /// <returns>The rough applied impulse magnitude.</returns>
        public override float SolveIteration()
        {
            //Compute relative velocity
            OrkEngine3D.Mathematics.Vector3 lambda;
            Vector3Ex.Cross(ref r, ref entity.angularVelocity, out lambda);
            Vector3Ex.Subtract(ref lambda, ref entity.linearVelocity, out lambda);

            //Add in bias velocity
            Vector3Ex.Add(ref biasVelocity, ref lambda, out lambda);

            //Add in softness
            OrkEngine3D.Mathematics.Vector3 softnessVelocity;
            Vector3Ex.Multiply(ref accumulatedImpulse, usedSoftness, out softnessVelocity);
            Vector3Ex.Subtract(ref lambda, ref softnessVelocity, out lambda);

            //In terms of an impulse (an instantaneous change in momentum), what is it?
            Matrix3x3.Transform(ref lambda, ref effectiveMassMatrix, out lambda);

            //Sum the impulse.
            OrkEngine3D.Mathematics.Vector3 previousAccumulatedImpulse = accumulatedImpulse;
            accumulatedImpulse += lambda;

            //If the impulse it takes to get to the goal is too high for the motor to handle, scale it back.
            float sumImpulseLengthSquared = accumulatedImpulse.LengthSquared();
            if (sumImpulseLengthSquared > maxForceDtSquared)
            {
                //max / impulse gives some value 0 < x < 1.  Basically, normalize the vector (divide by the length) and scale by the maximum.
                accumulatedImpulse *= maxForceDt / (float)Math.Sqrt(sumImpulseLengthSquared);

                //Since the limit was exceeded by this corrective impulse, limit it so that the accumulated impulse remains constrained.
                lambda = accumulatedImpulse - previousAccumulatedImpulse;
            }


            entity.ApplyLinearImpulse(ref lambda);
            OrkEngine3D.Mathematics.Vector3 taImpulse;
            Vector3Ex.Cross(ref r, ref lambda, out taImpulse);
            entity.ApplyAngularImpulse(ref taImpulse);

            return (Math.Abs(lambda.X) + Math.Abs(lambda.Y) + Math.Abs(lambda.Z));
        }

        ///<summary>
        /// Performs the frame's configuration step.
        ///</summary>
        ///<param name="dt">Timestep duration.</param>
        public override void Update(float dt)
        {
            //Transform point into world space.
            Matrix3x3.Transform(ref localPoint, ref entity.orientationMatrix, out r);
            Vector3Ex.Add(ref r, ref entity.position, out worldPoint);

            float updateRate = 1 / dt;
            if (settings.mode == MotorMode.Servomechanism)
            {
                Vector3Ex.Subtract(ref settings.servo.goal, ref worldPoint, out error);
                float separationDistance = error.Length();
                if (separationDistance > Toolbox.BigEpsilon)
                {
                    float errorReduction;
                    settings.servo.springSettings.ComputeErrorReductionAndSoftness(dt, updateRate, out errorReduction, out usedSoftness);

                    //The rate of correction can be based on a constant correction velocity as well as a 'spring like' correction velocity.
                    //The constant correction velocity could overshoot the destination, so clamp it.
                    float correctionSpeed = MathHelper.Min(settings.servo.baseCorrectiveSpeed, separationDistance * updateRate) +
                                            separationDistance * errorReduction;

                    Vector3Ex.Multiply(ref error, correctionSpeed / separationDistance, out biasVelocity);
                    //Ensure that the corrective velocity doesn't exceed the max.
                    float length = biasVelocity.LengthSquared();
                    if (length > settings.servo.maxCorrectiveVelocitySquared)
                    {
                        float multiplier = settings.servo.maxCorrectiveVelocity / (float)Math.Sqrt(length);
                        biasVelocity.X *= multiplier;
                        biasVelocity.Y *= multiplier;
                        biasVelocity.Z *= multiplier;
                    }
                }
                else
                {
                    //Wouldn't want to use a bias from an earlier frame.
                    biasVelocity = new OrkEngine3D.Mathematics.Vector3();
                }
            }
            else
            {
                usedSoftness = settings.velocityMotor.softness * updateRate;
                biasVelocity = settings.velocityMotor.goalVelocity;
                error = OrkEngine3D.Mathematics.Vector3.Zero;
            }

            //Compute the maximum force that can be applied this frame.
            ComputeMaxForces(settings.maximumForce, dt);

            //COMPUTE EFFECTIVE MASS MATRIX
            //Transforms a change in velocity to a change in momentum when multiplied.
            Matrix3x3 linearComponent;
            Matrix3x3.CreateScale(entity.inverseMass, out linearComponent);
            Matrix3x3 rACrossProduct;
            Matrix3x3.CreateCrossProduct(ref r, out rACrossProduct);
            Matrix3x3 angularComponentA;
            Matrix3x3.Multiply(ref rACrossProduct, ref entity.inertiaTensorInverse, out angularComponentA);
            Matrix3x3.Multiply(ref angularComponentA, ref rACrossProduct, out angularComponentA);
            Matrix3x3.Subtract(ref linearComponent, ref angularComponentA, out effectiveMassMatrix);

            effectiveMassMatrix.M11 += usedSoftness;
            effectiveMassMatrix.M22 += usedSoftness;
            effectiveMassMatrix.M33 += usedSoftness;

            Matrix3x3.Invert(ref effectiveMassMatrix, out effectiveMassMatrix);

        }

        /// <summary>
        /// Performs any pre-solve iteration work that needs exclusive
        /// access to the members of the solver updateable.
        /// Usually, this is used for applying warmstarting impulses.
        /// </summary>
        public override void ExclusiveUpdate()
        {
            //"Warm start" the constraint by applying a first guess of the solution should be.
            entity.ApplyLinearImpulse(ref accumulatedImpulse);
            OrkEngine3D.Mathematics.Vector3 taImpulse;
            Vector3Ex.Cross(ref r, ref accumulatedImpulse, out taImpulse);
            entity.ApplyAngularImpulse(ref taImpulse);
        }

        /// <summary>
        /// Computes the maxForceDt and maxForceDtSquared fields.
        /// </summary>
        private void ComputeMaxForces(float maxForce, float dt)
        {
            //Determine maximum force
            if (maxForce < float.MaxValue)
            {
                maxForceDt = maxForce * dt;
                maxForceDtSquared = maxForceDt * maxForceDt;
            }
            else
            {
                maxForceDt = float.MaxValue;
                maxForceDtSquared = float.MaxValue;
            }
        }
    }
}