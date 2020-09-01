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
            Stance = ShipStance.Aggressive;
            //if (!IsPlayerShip)
            //    Stance = ShipStance.Aggressive;
            //else
            //    Stance = ShipStance.Defensive;

            StateMachine.RegisterState(new ShipPatrolFollowState(this));
            StateMachine.RegisterState(new SmallShipAttackingState(this));
            StateMachine.RegisterState(new ShipIdleState(this));

            if (Stance == ShipStance.Aggressive)
            {
                //EnemyTarget = AIHelper.FindClosestEnemy(this);

                //if (EnemyTarget != null)
                //{
                //    var attacking = StateMachine.GetState<SmallShipAttackingState>();
                //    attacking.Target = EnemyTarget;
                //    StateMachine.Start<SmallShipAttackingState>();
                //}

                StateMachine.Start<ShipIdleState>();
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
            if (Stance == ShipStance.Aggressive)
                AIHelper.SmallAggressiveAttackingShipAI(this);
            else if (Stance == ShipStance.Defensive)
                AIHelper.SmallDefensiveAttackingShipAI(this);

            base.Update(gameTime);
        }
    }
}
