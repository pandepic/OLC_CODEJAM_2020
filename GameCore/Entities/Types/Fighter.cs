using GameCore.AI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore.Entities
{
    public class Fighter : Ship
    {
        public Fighter(Ship owner, Vector2 position)
        {
            Owner = owner;
            ShipType = ShipType.Fighter;
            Position = position;

            LoadData();

            StateMachine.RegisterState(new ShipPatrolFollowState(this));
            StateMachine.RegisterState(new SmallShipAttackingState(this));
            StateMachine.RegisterState(new ShipIdleState(this));
            StateMachine.RegisterState(new ShipPatrolPositionState(this));

            if (IsPlayerShip)
            {
                Stance = ShipStance.Defensive;
                DefendTarget = Owner;
            }
            else
            {
                Stance = ShipStance.Aggressive;
            }

            StateMachine.Start<ShipIdleState>();
        }
        
        public override void Update(GameTime gameTime)
        {
            AIHelper.SmallAttackingShipAI(this, gameTime);
            base.Update(gameTime);
        }
    }
}
