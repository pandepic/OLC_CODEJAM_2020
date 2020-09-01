using GameCore.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore.AI
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

            // find closest target in priority order
            foreach (var priority in ship.TargetPriorities)
            {
                for (var i = 0; i < targetList.Count; i++)
                {
                    var possibleTarget = targetList[i];
                    if (possibleTarget.Type != priority)
                        continue;

                    var testDistance = Vector2.Distance(ship.Position, possibleTarget.Position);

                    if (newTarget == null || testDistance < distance)
                    {
                        newTarget = possibleTarget;
                        distance = testDistance;
                    }
                }

                if (newTarget != null)
                    return newTarget;
            }

            // if nothing was found from the priority list just find anything at all
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

        public static float GetAngleToTarget(Vector2 position, float rotation, Vector2 target)
        {
            var angle = MathHelper.ToDegrees(MathF.Atan2((target.X - position.X), (position.Y - target.Y)));
            var distance = Math.Abs(rotation - angle);

            var angle360 = angle;
            if (angle360 < 0)
                angle360 = 180.0f + (180.0f + angle360);

            var distance360 = Math.Abs(rotation - angle360);

            if (distance360 < distance)
                angle = angle360;

            return angle;
        }

        public static void SmallDefensiveAttackingShipAI(Ship ship)
        {

        }

        public static void SmallAggressiveAttackingShipAI(Ship ship)
        {
            switch (ship.StateMachine.CurrentState)
            {
                case ShipIdleState idle:
                    {
                        ship.EnemyTarget = FindClosestEnemy(ship);

                        if (ship.EnemyTarget != null)
                        {
                            var attacking = ship.GetState<SmallShipAttackingState>();
                            attacking.Target = ship.EnemyTarget;
                            ship.SetState<SmallShipAttackingState>();
                        }
                    }
                    break;

                case SmallShipAttackingState attacking:
                    {
                        if (ship.EnemyTarget.IsDead)
                        {
                            if (ship.Stance == ShipStance.Aggressive)
                            {
                                ship.EnemyTarget = FindClosestEnemy(ship);

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
        } // SmallAggressiveAttackingShipAI
    }
}
