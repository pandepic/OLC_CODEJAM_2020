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
                BigAttackTarget(ship, newTarget);
                return true;
            }

            return false;
        } // TryBigAttackClosestEnemy

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

        public static void BigAttackTarget(Ship ship, Ship target)
        {
            if (ship.IsPlayerShip == target.IsPlayerShip)
                return;

            ship.DefendPosition = null;
            ship.DefendTarget = target;

            var follow = ship.StateMachine.GetState<ShipFollowingState>();
            follow.Target = ship.DefendTarget;
            ship.SetState(follow);
        } // SmallDefendTarget

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
            ship.StateMachine.RegisterState(new ShipFollowingState(ship));
            ship.StateMachine.RegisterState(new ShipFollowPositionState(ship));
            ship.StateMachine.RegisterState(new ShipIdleState(ship));

            ship.StateMachine.Start<ShipIdleState>();
        } // SetupBigWarshipStates

        public static void BigWarshipAI(Ship ship)
        {
            switch (ship.StateMachine.CurrentState)
            {
                case ShipFollowingState following:
                    {
                        if (following.Target.IsDead)
                        {
                            ship.SetState<ShipIdleState>();
                        }
                    }
                    break;

                case ShipIdleState idle:
                    {
                        if (ship.Stance == ShipStance.Aggressive)
                        {
                            TryBigAttackClosestEnemy(ship);
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
