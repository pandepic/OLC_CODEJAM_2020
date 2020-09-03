using GameCore.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore.AI
{
    public static partial class AIHelper
    {
        public static Ship FindClosestEnemy(Ship ship)
        {
            Ship newTarget = null;

            var targetList = GameplayState.WorldManager.EnemyShips;
            if (!ship.IsPlayerShip)
                targetList = GameplayState.WorldManager.PlayerShips;

            // find closest target in priority order
            foreach (var priority in ship.TargetPriorities)
            {
                newTarget = FindClosestEnemyByPriority(targetList, ship, priority);

                if (newTarget != null)
                    return newTarget;
            }

            // if nothing found just try to find any target
            newTarget = FindClosestEnemyByPriority(targetList, ship);

            return newTarget;
        } // FindClosestEnemy

        public static Ship FindClosestEnemyByPriority(List<Ship> targetList, Ship ship, ShipType priority = ShipType.None)
        {
            Ship newTarget = null;
            float distance = 0.0f;

            for (var i = 0; i < targetList.Count; i++)
            {
                var possibleTarget = targetList[i];

                if (possibleTarget.ShipType != priority)
                    continue;

                var weaponMatched = false;

                foreach (var weapon in ship.Weapons)
                {
                    if (weapon.TargetType == possibleTarget.TargetType)
                        weaponMatched = true;
                }

                if (!weaponMatched)
                    continue;

                if (ship.Stance == ShipStance.Defensive && (ship.DefendTarget != null && Vector2.Distance(ship.DefendTarget.Position, possibleTarget.Position) > ship.DefenceRadius))
                    continue;

                if (ship.Stance == ShipStance.Defensive && (ship.DefendPosition.HasValue && Vector2.Distance(ship.DefendPosition.Value, possibleTarget.Position) > ship.DefenceRadius))
                    continue;

                var testDistance = Vector2.Distance(ship.Position, possibleTarget.Position);

                if (newTarget == null || testDistance < distance)
                {
                    newTarget = possibleTarget;
                    distance = testDistance;
                }
            }

            return newTarget;
        } // FindClosestEnemyByPriority

        public static Ship FindClosestDamagedFriend(Ship ship, TargetType targetType = TargetType.Large)
        {
            Ship newTarget = null;
            float distance = 0.0f;

            var targetList = GameplayState.WorldManager.PlayerShips;
            if (!ship.IsPlayerShip)
                targetList = GameplayState.WorldManager.EnemyShips;

            for (var i = 0; i < targetList.Count; i++)
            {
                var possibleTarget = targetList[i];

                if (possibleTarget.TargetType != targetType)
                    continue;

                if (possibleTarget.CurrentArmourHP >= possibleTarget.BaseArmourHP)
                    continue;

                if (ship.DefendTarget != null && Vector2.Distance(ship.DefendTarget.Position, possibleTarget.Position) > ship.DefenceRadius)
                    continue;

                if (ship.DefendPosition.HasValue && Vector2.Distance(ship.DefendPosition.Value, possibleTarget.Position) > ship.DefenceRadius)
                    continue;

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
        } // GetAngleToTarget

        public static Vector2? GetShipDefendTargetPosition(Ship ship)
        {
            if (ship.DefendTarget != null)
                return ship.DefendTarget.Position;
            if (ship.DefendPosition.HasValue)
                return ship.DefendPosition;
            else
                return null;
        } // GetShipDefendTargetPosition

        public static void SmallAttackingScanForTarget(Ship ship, GameTime gameTime)
        {
            ship.NextDefendScan -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (ship.NextDefendScan <= 0)
            {
                ship.NextDefendScan = 0;

                if (!TrySmallAttackClosestEnemy(ship))
                    ship.NextDefendScan = ship.DefendScanFrequency;
            }
        } // ScanForTarget

        public static void DefendTarget(Ship ship, Ship target)
        {
            ship.DefendPosition = null;
            ship.DefendTarget = target;

            var follow = ship.StateMachine.GetState<ShipFollowingState>();
            follow.Target = ship.DefendTarget;
            ship.SetState(follow);
        } // SmallDefendTarget

        public static void DefendPosition(Ship ship, Vector2 position)
        {
            ship.DefendTarget = null;
            ship.DefendPosition = position;

            var followPosition = ship.StateMachine.GetState<ShipFollowPositionState>();
            followPosition.Target = ship.DefendPosition.Value;
            ship.SetState(followPosition);
        } // SmallDefendPosition

    } // AIHelper
}
