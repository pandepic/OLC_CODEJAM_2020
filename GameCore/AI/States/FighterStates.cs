using GameCore.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore.AI.States
{
    /*
     * Fighter states
     * - Patrolling
     * - Attacking target
     */

    public class FighterPatrollingState : ShipStateBase
    {
        public Ship Target;

        public FighterPatrollingState(Ship parentShip) : base("Patrolling", parentShip) { }

        public override void Begin()
        {
            ParentShip.SetDestination(Target.Position);
        }

        public override void Update(GameTime gameTime)
        {
            ParentShip.SetDestination(Target.Position);
        }
    }
}
