using GameCore.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore.AI
{
    public static partial class AIHelper
    {
        public static bool TrySmallAttackClosestEnemy(Ship ship)
        {
            var newTarget = FindClosestEnemy(ship);

            if (newTarget != null)
            {
                return TrySmallAttackTarget(ship, newTarget);
            }

            return false;
        } // TrySmallAttackClosestEnemy

        public static bool TrySmallAttackTarget(Ship ship, Ship target)
        {
            if (ship.IsPlayerShip == target.IsPlayerShip)
                return false;

            var validTarget = false;

            foreach (var weapon in ship.Weapons)
            {
                if (weapon.TargetType == target.TargetType)
                    validTarget = true;
            }

            if (!validTarget)
                return false;

            ship.EnemyTarget = target;
            var attacking = ship.GetState<SmallShipAttackingState>();
            attacking.Target = ship.EnemyTarget;
            ship.SetState(attacking);

            return true;
        } // TrySmallAttackTarget

        public static void SmallDefendTarget(Ship ship, Ship target)
        {
            if (ship.IsPlayerShip != target.IsPlayerShip)
                return;

            ship.DefendPosition = null;
            ship.DefendTarget = target;

            var patrolFollow = ship.StateMachine.GetState<ShipPatrolFollowState>();
            patrolFollow.Target = ship.DefendTarget;
            ship.SetState(patrolFollow);
        } // SmallDefendTarget

        public static void SmallDefendPosition(Ship ship, Vector2 position)
        {
            ship.DefendTarget = null;
            ship.DefendPosition = position;

            var patrolPosition = ship.StateMachine.GetState<ShipPatrolPositionState>();
            patrolPosition.Target = ship.DefendPosition.Value;
            ship.SetState(patrolPosition);
        } // SmallDefendPosition

        public static void SetupSmallAttackingShipStates(Ship ship)
        {
            ship.StateMachine.RegisterState(new ShipPatrolFollowState(ship));
            ship.StateMachine.RegisterState(new SmallShipAttackingState(ship));
            ship.StateMachine.RegisterState(new ShipIdleState(ship));
            ship.StateMachine.RegisterState(new ShipPatrolPositionState(ship));

            if (ship.Owner == null)
            {
                ship.Stance = ShipStance.Aggressive;
            }
            else
            {
                ship.DefendTarget = ship.Owner;
                ship.Stance = ShipStance.Defensive;
            }

            ship.StateMachine.Start<ShipIdleState>();
        } // SetupBigWarshipStates

        public static void SmallAttackingShipAI(Ship ship, GameTime gameTime)
        {
            switch (ship.StateMachine.CurrentState)
            {
                case ShipIdleState idle:
                    {
                        if (ship.Stance == ShipStance.Aggressive)
                        {
                            TrySmallAttackClosestEnemy(ship);
                        }
                        else if (ship.Stance == ShipStance.Defensive)
                        {
                            var patrolFollow = ship.GetState<ShipPatrolFollowState>();
                            patrolFollow.Target = ship.DefendTarget;
                            ship.SetState(patrolFollow);
                            ship.NextDefendScan = ship.DefendScanFrequency;
                        }
                    }
                    break;

                case SmallShipAttackingState attacking:
                    {
                        //ScanForTarget(ship, gameTime);

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
                            else if (ship.Stance == ShipStance.Defensive)
                            {
                                if (ship.DefendTarget != null)
                                {
                                    SmallDefendTarget(ship, ship.DefendTarget);
                                }
                                else if (ship.DefendPosition.HasValue)
                                {
                                    SmallDefendPosition(ship, ship.DefendPosition.Value);
                                }
                                else if (ship.Owner != null)
                                {
                                    SmallDefendTarget(ship, ship.Owner);
                                }
                                else
                                {
                                    ship.SetState<ShipIdleState>();
                                }
                            }
                        }
                    }
                    break;

                case ShipPatrolFollowState patrolFollow:
                    {
                        SmallAttackingScanForTarget(ship, gameTime);
                    }
                    break;

                case ShipPatrolPositionState patrolPosition:
                    {
                        SmallAttackingScanForTarget(ship, gameTime);
                    }
                    break;
            }
        } // SmallAggressiveAttackingShipAI

    } // AIHelper
}
