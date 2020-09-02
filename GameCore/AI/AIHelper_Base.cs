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

        public static void ScanForTarget(Ship ship, GameTime gameTime)
        {
            ship.NextDefendScan -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (ship.NextDefendScan <= 0)
            {
                ship.NextDefendScan = 0;

                if (!TrySmallAttackClosestEnemy(ship))
                    ship.NextDefendScan = ship.DefendScanFrequency;
            }
        } // ScanForTarget

    } // AIHelper
}
