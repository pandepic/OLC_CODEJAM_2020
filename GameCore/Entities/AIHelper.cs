using GameCore.AI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore.Entities
{
    public static class AIHelper
    {
        public static Ship FindClosestEnemy(Ship ship)
        {
            Ship newTarget = null;
            float distance = 0.0f;

            var targetList = GameplayState.WorldManager.EnemyShips;
            if (!ship.IsPlayerShip)
                targetList = GameplayState.WorldManager.PlayerShips;

            for (var i = 0; i < targetList.Count; i++)
            {
                var possibleTarget = targetList[i];
                var testDistance = Vector2.Distance(ship.Position, possibleTarget.Position);

                if (newTarget == null || testDistance < distance)
                {
                    newTarget = possibleTarget;
                    distance = testDistance;
                }
            }

            return newTarget;
        }

        public static void SmallAttackingShipAI(Ship ship)
        {
            switch (ship.StateMachine.CurrentState)
            {
                case SmallShipAttackingState attacking:
                    {
                        if (ship.EnemyTarget.IsDead)
                        {
                            if (ship.Stance == ShipStance.Aggressive)
                            {
                                ship.EnemyTarget = AIHelper.FindClosestEnemy(ship);

                                if (ship.EnemyTarget != null)
                                    attacking.Target = ship.EnemyTarget;
                                else
                                    ship.SetState<ShipIdleState>();
                            }
                            else
                            {
                                if (ship.Owner != null)
                                {
                                    var patrolFollow = ship.StateMachine.GetState<ShipPatrolFollowState>();
                                    patrolFollow.Target = ship.Owner;
                                    ship.SetState<ShipPatrolFollowState>();
                                }
                                else
                                {
                                    ship.SetState<ShipIdleState>();
                                }
                            }
                        }
                    }
                    break;
            }
        } // SmallAttackingShipAI
    }
}
