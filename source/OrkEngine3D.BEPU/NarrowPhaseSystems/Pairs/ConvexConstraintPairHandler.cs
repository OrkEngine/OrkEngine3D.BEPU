﻿using System;
using BEPUphysics.BroadPhaseSystems;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.CollisionTests;
using BEPUphysics.CollisionTests.CollisionAlgorithms.GJK;
using BEPUphysics.CollisionTests.Manifolds;
using BEPUphysics.Constraints.Collision;
using BEPUphysics.PositionUpdating;
using BEPUphysics.Settings;
 
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUutilities;

namespace BEPUphysics.NarrowPhaseSystems.Pairs
{
    ///<summary>
    /// Pair handler that manages a pair of two boxes.
    ///</summary>
    public abstract class ConvexConstraintPairHandler : ConvexPairHandler
    {

        ConvexContactManifoldConstraint contactConstraint = new ConvexContactManifoldConstraint();

        //public override void Initialize(BroadPhaseEntry entryA, BroadPhaseEntry entryB)
        //{
        //    contactConstraint = new ConvexContactManifoldConstraint();

        //    base.Initialize(entryA, entryB);
        //}

        /// <summary>
        /// Gets the contact constraint used by the pair handler.
        /// </summary>
        public override ContactManifoldConstraint ContactConstraint
        {
            get { return contactConstraint; }
        }


        protected internal override void GetContactInformation(int index, out ContactInformation info)
        {
            info.Contact = ContactManifold.contacts.Elements[index];
            //Find the contact's normal force.
            float totalNormalImpulse = 0;
            info.NormalImpulse = 0;
            for (int i = 0; i < contactConstraint.penetrationConstraints.Count; i++)
            {
                totalNormalImpulse += contactConstraint.penetrationConstraints.Elements[i].accumulatedImpulse;
                if (contactConstraint.penetrationConstraints.Elements[i].contact == info.Contact)
                {
                    info.NormalImpulse = contactConstraint.penetrationConstraints.Elements[i].accumulatedImpulse;
                }
            }
            //Compute friction force.  Since we are using central friction, this is 'faked.'
            float radius;
            Vector3Ex.Distance(ref contactConstraint.slidingFriction.manifoldCenter, ref info.Contact.Position, out radius);
            if (totalNormalImpulse > 0)
                info.FrictionImpulse = (info.NormalImpulse / totalNormalImpulse) * (contactConstraint.slidingFriction.accumulatedImpulse.Length() + contactConstraint.twistFriction.accumulatedImpulse * radius);
            else
                info.FrictionImpulse = 0;
            //Compute relative velocity
            OrkEngine3D.Mathematics.Vector3 velocity;
            //If the pair is handling some type of query and does not actually have supporting entities, then consider the velocity contribution to be zero.
            if (EntityA != null)
            {
                Vector3Ex.Subtract(ref info.Contact.Position, ref EntityA.position, out velocity);
                Vector3Ex.Cross(ref EntityA.angularVelocity, ref velocity, out velocity);
                Vector3Ex.Add(ref velocity, ref EntityA.linearVelocity, out info.RelativeVelocity);
            }
            else
                info.RelativeVelocity = new OrkEngine3D.Mathematics.Vector3();

            if (EntityB != null)
            {
                Vector3Ex.Subtract(ref info.Contact.Position, ref EntityB.position, out velocity);
                Vector3Ex.Cross(ref EntityB.angularVelocity, ref velocity, out velocity);
                Vector3Ex.Add(ref velocity, ref EntityB.linearVelocity, out velocity);
                Vector3Ex.Subtract(ref info.RelativeVelocity, ref velocity, out info.RelativeVelocity);
            }


            info.Pair = this;

        }

    }

}
