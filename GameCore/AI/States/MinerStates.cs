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
        public Vector2 RandomOffset;

        public MinerTravelingState(Ship parentShip) : base("Traveling", parentShip) { }

        public override void Begin()
        {
            var ship = (Miner)ParentShip;
            ship.CurrentMiningTarget = Target;

            RandomOffset = new Vector2(WorldData.RNG.Next(-25, 25), WorldData.RNG.Next(-25, 25));

            ParentShip.SetDestination(Target.Position + RandomOffset);
        }

        public override void End()
        {
        }

        public override void Update(GameTime gameTime)
        {
            if (Vector2.Distance(ParentShip.Position, Target.Position + RandomOffset) <= 50)
            {
                ParentShip.StopMovement();
                Parent.GetState<MinerMiningState>().Target = Target;
                Parent.SetState<MinerMiningState>();

                GameplayState.EffectsManager.AddExplosion(Target.Position + RandomOffset, Color.OrangeRed, 1.0f, 3000);
            }
            else
            {
                ParentShip.SetDestination(Target.Position + RandomOffset);
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
            var ship = (Miner)ParentShip;
            ship.Inventory.AddResource(Target.ResourceType, ((Miner)ParentShip).GatherRate);
            Parent.SetState<MinerReturningState>();
        }
    }

    public class MinerReturningState : ShipStateBase
    {
        public Ship Target;
        public Vector2 RandomOffset;

        public MinerReturningState(Ship parentShip) : base("Returning", parentShip) { }

        public override void Begin()
        {
            RandomOffset = new Vector2(WorldData.RNG.Next(-50, 50), WorldData.RNG.Next(-50, 50));
            Target = ParentShip.Owner;
            ParentShip.SetDestination(Target.Position + RandomOffset);
        }

        public override void End()
        {
        }

        public override void Update(GameTime gameTime)
        {
            if (Vector2.Distance(ParentShip.Position, Target.Position + RandomOffset) <= 50)
            {
                ParentShip.StopMovement();
                Parent.SetState<MinerDepositingState>();
            }
            else
            {
                ParentShip.SetDestination(Target.Position + RandomOffset);
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
            var ship = (Miner)ParentShip;
            var owner = (Player)ship.Owner;

            ship.Inventory.GiveAll(owner.Inventory);
            
            if (ship.CurrentMiningTarget == null)
            {
                var patrolFollow = Parent.GetState<ShipPatrolFollowState>();
                patrolFollow.Target = ParentShip.Owner;
                Parent.SetState<ShipPatrolFollowState>();
            }
            else
            {
                var traveling = Parent.GetState<MinerTravelingState>();
                traveling.Target = ship.CurrentMiningTarget;
                Parent.SetState<MinerTravelingState>();
            }
        }
    }
}
