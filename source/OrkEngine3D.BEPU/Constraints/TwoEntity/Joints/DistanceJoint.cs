using System;
using BEPUphysics.Entities;

using BEPUutilities;

namespace BEPUphysics.Constraints.TwoEntity.Joints
{
    /// <summary>
    /// Constraint which tries to maintain the distance between points on two entities.
    /// </summary>
    public class DistanceJoint : Joint, I1DImpulseConstraintWithError, I1DJacobianConstraint
    {
        private float accumulatedImpulse;
        private OrkEngine3D.Mathematics.Vector3 anchorA;

        private OrkEngine3D.Mathematics.Vector3 anchorB;
        private float biasVelocity;
        private OrkEngine3D.Mathematics.Vector3 jAngularA, jAngularB;
        private OrkEngine3D.Mathematics.Vector3 jLinearA, jLinearB;

        /// <summary>
        /// Distance maintained between the anchors.
        /// </summary>
        protected float distance;

        private float error;

        private OrkEngine3D.Mathematics.Vector3 localAnchorA;

        private OrkEngine3D.Mathematics.Vector3 localAnchorB;


        private OrkEngine3D.Mathematics.Vector3 offsetA, offsetB;
        private float velocityToImpulse;

        /// <summary>
        /// Constructs a distance joint.
        /// To finish the initialization, specify the connections (ConnectionA and ConnectionB) 
        /// as well as the anchors (WorldAnchorA, WorldAnchorB or LocalAnchorA, LocalAnchorB)
        /// and the desired Distance.
        /// This constructor sets the constraint's IsActive property to false by default.
        /// </summary>
        public DistanceJoint()
        {
            IsActive = false;
            ConnectionA = connectionA;
            ConnectionB = connectionB;

            Distance = (anchorA - anchorB).Length();

            WorldAnchorA = anchorA;
            WorldAnchorB = anchorB;
        }

        /// <summary>
        /// Constructs a distance joint.
        /// </summary>
        /// <param name="connectionA">First body connected to the distance joint.</param>
        /// <param name="connectionB">Second body connected to the distance joint.</param>
        /// <param name="anchorA">Connection to the distance joint from the first connected body in world space.</param>
        /// <param name="anchorB"> Connection to the distance joint from the second connected body in world space.</param>
        public DistanceJoint(Entity connectionA, Entity connectionB, OrkEngine3D.Mathematics.Vector3 anchorA, OrkEngine3D.Mathematics.Vector3 anchorB)
        {
            ConnectionA = connectionA;
            ConnectionB = connectionB;

            Distance = (anchorA - anchorB).Length();

            WorldAnchorA = anchorA;
            WorldAnchorB = anchorB;
        }

        /// <summary>
        /// Gets or sets the distance maintained between the anchors.
        /// </summary>
        public float Distance
        {
            get { return distance; }
            set { distance = Math.Max(0, value); }
        }

        /// <summary>
        /// Gets or sets the first entity's connection point in local space.
        /// </summary>
        public OrkEngine3D.Mathematics.Vector3 LocalAnchorA
        {
            get { return localAnchorA; }
            set
            {
                localAnchorA = value;
                Matrix3x3.Transform(ref localAnchorA, ref connectionA.orientationMatrix, out anchorA);
                anchorA += connectionA.position;
            }
        }

        /// <summary>
        /// Gets or sets the first entity's connection point in local space.
        /// </summary>
        public OrkEngine3D.Mathematics.Vector3 LocalAnchorB
        {
            get { return localAnchorB; }
            set
            {
                localAnchorB = value;
                Matrix3x3.Transform(ref localAnchorB, ref connectionB.orientationMatrix, out anchorB);
                anchorB += connectionB.position;
            }
        }

        /// <summary>
        /// Gets or sets the connection to the distance constraint from the first connected body in world space.
        /// </summary>
        public OrkEngine3D.Mathematics.Vector3 WorldAnchorA
        {
            get { return anchorA; }
            set
            {
                anchorA = value;
                localAnchorA = QuaternionEx.Transform(anchorA - connectionA.position, QuaternionEx.Conjugate(connectionA.orientation));
            }
        }

        /// <summary>
        /// Gets or sets the connection to the distance constraint from the second connected body in world space.
        /// </summary>
        public OrkEngine3D.Mathematics.Vector3 WorldAnchorB
        {
            get { return anchorB; }
            set
            {
                anchorB = value;
                localAnchorB = QuaternionEx.Transform(anchorB - connectionB.position, QuaternionEx.Conjugate(connectionB.orientation));
            }
        }

        #region I1DImpulseConstraintWithError Members

        /// <summary>
        /// Gets the current relative velocity between the connected entities with respect to the constraint.
        /// </summary>
        public float RelativeVelocity
        {
            get
            {
                float lambda, dot;
                Vector3Ex.Dot(ref jLinearA, ref connectionA.linearVelocity, out lambda);
                Vector3Ex.Dot(ref jAngularA, ref connectionA.angularVelocity, out dot);
                lambda += dot;
                Vector3Ex.Dot(ref jLinearB, ref connectionB.linearVelocity, out dot);
                lambda += dot;
                Vector3Ex.Dot(ref jAngularB, ref connectionB.angularVelocity, out dot);
                lambda += dot;
                return lambda;
            }
        }


        /// <summary>
        /// Gets the total impulse applied by this constraint.
        /// </summary>
        public float TotalImpulse
        {
            get { return accumulatedImpulse; }
        }

        /// <summary>
        /// Gets the current constraint error.
        /// </summary>
        public float Error
        {
            get { return error; }
        }

        #endregion

        #region I1DJacobianConstraint Members

        /// <summary>
        /// Gets the linear jacobian entry for the first connected entity.
        /// </summary>
        /// <param name="jacobian">Linear jacobian entry for the first connected entity.</param>
        public void GetLinearJacobianA(out OrkEngine3D.Mathematics.Vector3 jacobian)
        {
            jacobian = jLinearA;
        }

        /// <summary>
        /// Gets the linear jacobian entry for the second connected entity.
        /// </summary>
        /// <param name="jacobian">Linear jacobian entry for the second connected entity.</param>
        public void GetLinearJacobianB(out OrkEngine3D.Mathematics.Vector3 jacobian)
        {
            jacobian = jLinearB;
        }

        /// <summary>
        /// Gets the angular jacobian entry for the first connected entity.
        /// </summary>
        /// <param name="jacobian">Angular jacobian entry for the first connected entity.</param>
        public void GetAngularJacobianA(out OrkEngine3D.Mathematics.Vector3 jacobian)
        {
            jacobian = jAngularA;
        }

        /// <summary>
        /// Gets the angular jacobian entry for the second connected entity.
        /// </summary>
        /// <param name="jacobian">Angular jacobian entry for the second connected entity.</param>
        public void GetAngularJacobianB(out OrkEngine3D.Mathematics.Vector3 jacobian)
        {
            jacobian = jAngularB;
        }

        /// <summary>
        /// Gets the mass matrix of the constraint.
        /// </summary>
        /// <param name="outputMassMatrix">Constraint's mass matrix.</param>
        public void GetMassMatrix(out float outputMassMatrix)
        {
            outputMassMatrix = velocityToImpulse;
        }

        #endregion

        /// <summary>
        /// Calculates and applies corrective impulses.
        /// Called automatically by space.
        /// </summary>
        public override float SolveIteration()
        {
            //Compute the current relative velocity.
            float lambda, dot;
            Vector3Ex.Dot(ref jLinearA, ref connectionA.linearVelocity, out lambda);
            Vector3Ex.Dot(ref jAngularA, ref connectionA.angularVelocity, out dot);
            lambda += dot;
            Vector3Ex.Dot(ref jLinearB, ref connectionB.linearVelocity, out dot);
            lambda += dot;
            Vector3Ex.Dot(ref jAngularB, ref connectionB.angularVelocity, out dot);
            lambda += dot;

            //Add in the constraint space bias velocity
            lambda = -lambda + biasVelocity - softness * accumulatedImpulse;

            //Transform to an impulse
            lambda *= velocityToImpulse;

            //Accumulate impulse
            accumulatedImpulse += lambda;

            //Apply the impulse
            OrkEngine3D.Mathematics.Vector3 impulse;
            if (connectionA.isDynamic)
            {
                Vector3Ex.Multiply(ref jLinearA, lambda, out impulse);
                connectionA.ApplyLinearImpulse(ref impulse);
                Vector3Ex.Multiply(ref jAngularA, lambda, out impulse);
                connectionA.ApplyAngularImpulse(ref impulse);
            }
            if (connectionB.isDynamic)
            {
                Vector3Ex.Multiply(ref jLinearB, lambda, out impulse);
                connectionB.ApplyLinearImpulse(ref impulse);
                Vector3Ex.Multiply(ref jAngularB, lambda, out impulse);
                connectionB.ApplyAngularImpulse(ref impulse);
            }

            return (Math.Abs(lambda));
        }

        /// <summary>
        /// Calculates necessary information for velocity solving.
        /// </summary>
        /// <param name="dt">Time in seconds since the last update.</param>
        public override void Update(float dt)
        {
            //Transform the anchors and offsets into world space.
            Matrix3x3.Transform(ref localAnchorA, ref connectionA.orientationMatrix, out offsetA);
            Matrix3x3.Transform(ref localAnchorB, ref connectionB.orientationMatrix, out offsetB);
            Vector3Ex.Add(ref connectionA.position, ref offsetA, out anchorA);
            Vector3Ex.Add(ref connectionB.position, ref offsetB, out anchorB);

            //Compute the distance.
            OrkEngine3D.Mathematics.Vector3 separation;
            Vector3Ex.Subtract(ref anchorB, ref anchorA, out separation);
            float currentDistance = separation.Length();

            //Compute jacobians
            if (currentDistance > Toolbox.Epsilon)
            {
                jLinearB.X = separation.X / currentDistance;
                jLinearB.Y = separation.Y / currentDistance;
                jLinearB.Z = separation.Z / currentDistance;
            }
            else
                jLinearB = Toolbox.ZeroVector;

            jLinearA.X = -jLinearB.X;
            jLinearA.Y = -jLinearB.Y;
            jLinearA.Z = -jLinearB.Z;

            Vector3Ex.Cross(ref offsetA, ref jLinearB, out jAngularA);
            //Still need to negate angular A.  It's done after the effective mass matrix.
            Vector3Ex.Cross(ref offsetB, ref jLinearB, out jAngularB);


            //Compute effective mass matrix
            if (connectionA.isDynamic && connectionB.isDynamic)
            {
                OrkEngine3D.Mathematics.Vector3 aAngular;
                Matrix3x3.Transform(ref jAngularA, ref connectionA.localInertiaTensorInverse, out aAngular);
                Vector3Ex.Cross(ref aAngular, ref offsetA, out aAngular);
                OrkEngine3D.Mathematics.Vector3 bAngular;
                Matrix3x3.Transform(ref jAngularB, ref connectionB.localInertiaTensorInverse, out bAngular);
                Vector3Ex.Cross(ref bAngular, ref offsetB, out bAngular);
                Vector3Ex.Add(ref aAngular, ref bAngular, out aAngular);
                Vector3Ex.Dot(ref aAngular, ref jLinearB, out velocityToImpulse);
                velocityToImpulse += connectionA.inverseMass + connectionB.inverseMass;
            }
            else if (connectionA.isDynamic)
            {
                OrkEngine3D.Mathematics.Vector3 aAngular;
                Matrix3x3.Transform(ref jAngularA, ref connectionA.localInertiaTensorInverse, out aAngular);
                Vector3Ex.Cross(ref aAngular, ref offsetA, out aAngular);
                Vector3Ex.Dot(ref aAngular, ref jLinearB, out velocityToImpulse);
                velocityToImpulse += connectionA.inverseMass;
            }
            else if (connectionB.isDynamic)
            {
                OrkEngine3D.Mathematics.Vector3 bAngular;
                Matrix3x3.Transform(ref jAngularB, ref connectionB.localInertiaTensorInverse, out bAngular);
                Vector3Ex.Cross(ref bAngular, ref offsetB, out bAngular);
                Vector3Ex.Dot(ref bAngular, ref jLinearB, out velocityToImpulse);
                velocityToImpulse += connectionB.inverseMass;
            }
            else
            {
                //No point in trying to solve with two kinematics.
                isActiveInSolver = false;
                accumulatedImpulse = 0;
                return;
            }

            float errorReduction;
            springSettings.ComputeErrorReductionAndSoftness(dt, 1 / dt, out errorReduction, out softness);

            velocityToImpulse = 1 / (softness + velocityToImpulse);
            //Finish computing jacobian; it's down here as an optimization (since it didn't need to be negated in mass matrix)
            jAngularA.X = -jAngularA.X;
            jAngularA.Y = -jAngularA.Y;
            jAngularA.Z = -jAngularA.Z;

            //Compute bias velocity
            error = distance - currentDistance;
            biasVelocity = MathHelper.Clamp(error * errorReduction, -maxCorrectiveVelocity, maxCorrectiveVelocity);



        }

        /// <summary>
        /// Performs any pre-solve iteration work that needs exclusive
        /// access to the members of the solver updateable.
        /// Usually, this is used for applying warmstarting impulses.
        /// </summary>
        public override void ExclusiveUpdate()
        {
            //Warm starting
            OrkEngine3D.Mathematics.Vector3 impulse;
            if (connectionA.isDynamic)
            {
                Vector3Ex.Multiply(ref jLinearA, accumulatedImpulse, out impulse);
                connectionA.ApplyLinearImpulse(ref impulse);
                Vector3Ex.Multiply(ref jAngularA, accumulatedImpulse, out impulse);
                connectionA.ApplyAngularImpulse(ref impulse);
            }
            if (connectionB.isDynamic)
            {
                Vector3Ex.Multiply(ref jLinearB, accumulatedImpulse, out impulse);
                connectionB.ApplyLinearImpulse(ref impulse);
                Vector3Ex.Multiply(ref jAngularB, accumulatedImpulse, out impulse);
                connectionB.ApplyAngularImpulse(ref impulse);
            }
        }
    }
}