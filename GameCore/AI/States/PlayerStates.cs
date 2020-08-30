using GameCore.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore.AI
{
    public class PlayerTravelingState : ShipStateBase
    {
        public Vector2 Target;

        public PlayerTravelingState(Ship parentShip) : base("Traveling", parentShip) { }

        public override void Begin()
        {
            ParentShip.SetDestination(Target);
        }

        public override void End()
        {
        }

        public override void Update(GameTime gameTime)
        {
            if (Vector2.Distance(ParentShip.Position, Target) <= 25.0f)
            {
                ParentShip.StopMovement();
                Parent.SetState("Idle");
            }
        }
    }
}
