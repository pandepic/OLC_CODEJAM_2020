using System;
using System.Collections.Generic;
using System.Text;
using GameCore.Entities;
using Microsoft.Xna.Framework;

namespace GameCore.AI
{
    public class MinerTravelingState : ShipStateBase
    {
        public Asteroid Target;

        public MinerTravelingState(Ship parentShip) : base("Traveling", parentShip) { }

        public override void Begin()
        {
            ParentShip.SetDestination(Target.Position);
        }

        public override void End()
        {
        }

        public override void Update(GameTime gameTime)
        {
            if (ParentShip.CollisionRect.Intersects(Target.CollisionRect))
            {
                ParentShip.StopMovement();
                Parent.GetState<MinerMiningState>().Target = Target;
                Parent.SetState<MinerMiningState>();
            }
            else
            {
                ParentShip.SetDestination(Target.Position);
            }
        }
    }

    public class MinerMiningState : ShipStateTimedBase
    {
        public Asteroid Target;

        public MinerMiningState(Ship parentShip) : base("Mining", parentShip)
        {
        }

        public override void Begin()
        {
            Duration = 3000;
        }

        public override void EndDuration()
        {
            Parent.SetState<MinerReturningState>();
        }
    }

    public class MinerReturningState : ShipStateBase
    {
        public Ship Target;

        public MinerReturningState(Ship parentShip) : base("Returning", parentShip) { }

        public override void Begin()
        {
            Target = ParentShip.Owner;
            ParentShip.SetDestination(Target.Position);
        }

        public override void End()
        {
        }

        public override void Update(GameTime gameTime)
        {
            if (ParentShip.CollisionRect.Intersects(Target.CollisionRect))
            {
                ParentShip.StopMovement();
                Parent.SetState<MinerDepositingState>();
            }
            else
            {
                ParentShip.SetDestination(Target.Position);
            }
        }
    }

    public class MinerDepositingState : ShipStateTimedBase
    {
        public MinerDepositingState(Ship parentShip) : base("Depositing", parentShip) { }

        public override void Begin()
        {
            Duration = 2500;
        }

        public override void EndDuration()
        {
            var patrolFollow = Parent.GetState<ShipPatrolFollowState>();
            patrolFollow.Target = ParentShip.Owner;
            Parent.SetState<ShipPatrolFollowState>();
        }
    }
}
