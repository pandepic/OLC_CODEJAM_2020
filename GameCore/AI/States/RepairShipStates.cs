using GameCore.Entities;
using Microsoft.Xna.Framework;
using PandaMonogame;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore.AI
{
    public class RepairShipRepairState : ShipStateBase
    {
        public Ship Target;
        public float FollowDistance;
        public float RepairRate;

        public RepairShipRepairState(Ship parentShip) : base("Repairing", parentShip) { }

        public override void Begin()
        {
            FollowDistance = Target.ShieldRadius * 0.9f;
            ParentShip.SetDestination(Target.Position);
        }

        public override void Update(GameTime gameTime)
        {
            if (Vector2.Distance(Target.Position, ParentShip.Position) <= FollowDistance)
            {
                if (Target.Moving)
                {
                    ParentShip.MoveSpeed = Target.MoveSpeed;
                    ParentShip.SetDestination(Target.Position);
                }
                else
                {
                    ParentShip.StopMovement();
                }

                Target.RepairArmour(RepairRate * gameTime.DeltaTime());
            }
            else
            {
                ParentShip.MoveSpeed = ParentShip.BaseMoveSpeed;
                ParentShip.SetDestination(Target.Position);
            }
        }

        public override void End()
        {
            ParentShip.MoveSpeed = ParentShip.BaseMoveSpeed;
        }
    }
}
