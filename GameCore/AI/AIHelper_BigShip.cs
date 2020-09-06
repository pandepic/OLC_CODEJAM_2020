using GameCore.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore.AI
{
    public static partial class AIHelper
    {
        public static bool TryBigAttackClosestEnemy(Ship ship)
        {
            var newTarget = FindClosestEnemy(ship);

            if (newTarget != null)
            {
                return TryBigAttackTarget(ship, newTarget);
            }

            return false;
        } // TryBigAttackClosestEnemy

        public static bool TryBigAttackTarget(Ship ship, Ship target)
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

            ship.DefendPosition = null;
            ship.DefendTarget = target;

            var follow = ship.StateMachine.GetState<ShipFollowingState>();
            follow.Target = ship.DefendTarget;
            ship.SetState(follow);

            return true;
        } // TryBigAttackTarget

        public static void BigDefendTarget(Ship ship, Ship target)
        {
            if (ship.IsPlayerShip != target.IsPlayerShip)
                return;

            ship.DefendPosition = null;
            ship.DefendTarget = target;

            var follow = ship.StateMachine.GetState<ShipFollowingState>();
            follow.Target = ship.DefendTarget;
            ship.SetState(follow);
        } // SmallDefendTarget

        public static void BigDefendPosition(Ship ship, Vector2 position)
        {
            ship.DefendTarget = null;
            ship.DefendPosition = position;

            var followPosition = ship.StateMachine.GetState<ShipFollowPositionState>();
            followPosition.Target = ship.DefendPosition.Value;
            ship.SetState(followPosition);
        } // SmallDefendPosition

        public static void BigSetFollowState(Ship ship)
        {
            if (ship.DefendTarget != null)
            {
                BigDefendTarget(ship, ship.DefendTarget);
            }
            else if (ship.DefendPosition.HasValue)
            {
                BigDefendPosition(ship, ship.DefendPosition.Value);
            }
            else
            {
                BigDefendTarget(ship, ship.Owner);
            }
        } // BigSetFollowState

        public static void SetupBigWarshipStates(Ship ship)
        {
            if (ship.Owner == null)
            {
                ship.Stance = ShipStance.Aggressive;
            }
            else
            {
                ship.Stance = ShipStance.Defensive;
                ship.DefendTarget = ship.Owner;
            }

            ship.StateMachine.RegisterState(new ShipFollowingState(ship));
            ship.StateMachine.RegisterState(new ShipFollowPositionState(ship));
            ship.StateMachine.RegisterState(new ShipIdleState(ship));

            ship.StateMachine.Start<ShipIdleState>();
        } // SetupBigWarshipStates

        public static void BigWarshipScanForTarget(Ship ship, GameTime gameTime)
        {
            ship.NextDefendScan -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (ship.NextDefendScan <= 0)
            {
                ship.NextDefendScan = 0;

                if (!TryBigAttackClosestEnemy(ship))
                    ship.NextDefendScan = ship.DefendScanFrequency;
            }
        } // BigWarshipScanForTarget

        public static void BigWarshipAI(Ship ship, GameTime gameTime)
        {
            switch (ship.StateMachine.CurrentState)
            {
                case ShipFollowPositionState followPosition:
                    {
                        if (!ship.IsMoving)
                            BigWarshipScanForTarget(ship, gameTime);
                    }
                    break;

                case ShipFollowingState following:
                    {
                        if (following.Target.IsDead)
                        {
                            ship.SetState<ShipIdleState>();
                        }
                        else
                        {
                            if (following.Target.IsPlayerShip)
                                BigWarshipScanForTarget(ship, gameTime);
                        }
                    }
                    break;

                case ShipIdleState idle:
                    {
                        if (ship.Stance == ShipStance.Aggressive)
                        {
                            BigWarshipScanForTarget(ship, gameTime);
                        }
                        else
                        {
                            BigSetFollowState(ship);
                        }
                    }
                    break;
            }
        } // BigWarshipAI
    }
}
