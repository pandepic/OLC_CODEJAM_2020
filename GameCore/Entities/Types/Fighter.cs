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
            Type = ShipType.Fighter;
            Position = position;

            LoadData();

            if (!IsPlayerShip)
                Stance = ShipStance.Aggressive;

            StateMachine.RegisterState(new ShipPatrolFollowState(this));
            StateMachine.RegisterState(new SmallShipAttackingState(this));
            StateMachine.RegisterState(new ShipIdleState(this));

            if (Stance == ShipStance.Aggressive)
            {
                EnemyTarget = AIHelper.FindClosestEnemy(this);

                if (EnemyTarget != null)
                {
                    var attacking = StateMachine.GetState<SmallShipAttackingState>();
                    attacking.Target = EnemyTarget;
                    StateMachine.Start<SmallShipAttackingState>();
                }
            }
            else
            {
                var patrolFollow = StateMachine.GetState<ShipPatrolFollowState>();
                patrolFollow.Target = Owner;
                StateMachine.Start<ShipPatrolFollowState>();
            }
        }
        
        public override void Update(GameTime gameTime)
        {
            AIHelper.SmallAttackingShipAI(this);
            base.Update(gameTime);
        }
    }
}
