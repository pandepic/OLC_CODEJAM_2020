using GameCore.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore.AI
{
    public static partial class AIHelper
    {
        public static Ship FindClosestEnemy(Ship ship, float? checkDistance = null)
        {
            (Ship, Ship) newTargets = (null, null);

            var targetList = GameplayState.WorldManager.EnemyShips;
            if (!ship.IsPlayerShip)
                targetList = GameplayState.WorldManager.PlayerShips;

            // find closest target in priority order
            foreach (var priority in ship.TargetPriorities)
            {
                newTargets = FindClosestEnemyByPriority(targetList, ship, priority, checkDistance);

                if (newTargets.Item1 != null)
                {
                    if (newTargets.Item2 == null)
                        return newTargets.Item1;
                    else
                    {
                        if (WorldData.RNG.Next(1, 3) == 1)
                            return newTargets.Item1;
                        else
                            return newTargets.Item2;
                    }
                }
            }

            // if nothing found just try to find any target
            newTargets = FindClosestEnemyByPriority(targetList, ship, ShipType.None, checkDistance);

            if (newTargets.Item1 != null)
            {
                if (newTargets.Item2 == null)
                    return newTargets.Item1;
                else
                {
                    if (WorldData.RNG.Next(1, 3) == 1)
                        return newTargets.Item1;
                    else
                        return newTargets.Item2;
                }
            }

            return null;

        } // FindClosestEnemy

        public static (Ship, Ship) FindClosestEnemyByPriority(List<Ship> targetList, Ship ship, ShipType priority = ShipType.None, float? checkDistance = null)
        {
            (Ship, Ship) newTarget = (null, null);
            (float, float) distance = (0.0f, 0.0f);

            for (var i = 0; i < targetList.Count; i++)
            {
                var possibleTarget = targetList[i];

                if (priority != ShipType.None && possibleTarget.ShipType != priority)
                    continue;

                var weaponMatched = false;

                foreach (var weapon in ship.Weapons)
                {
                    if (weapon.TargetType == possibleTarget.TargetType)
                        weaponMatched = true;
                }

                if (!weaponMatched)
                    continue;

                if (!checkDistance.HasValue)
                {
                    if (ship.Stance == ShipStance.Defensive && (ship.DefendTarget != null && Vector2.Distance(ship.DefendTarget.Position, possibleTarget.Position) > ship.DefenceRadius))
                        continue;

                    if (ship.Stance == ShipStance.Defensive && (ship.DefendPosition.HasValue && Vector2.Distance(ship.DefendPosition.Value, possibleTarget.Position) > ship.DefenceRadius))
                        continue;
                }

                var testDistance = Vector2.Distance(ship.Position, possibleTarget.Position);

                if (checkDistance.HasValue && testDistance > checkDistance.Value)
                    continue;

                if (newTarget.Item1 == null || testDistance < distance.Item1)
                {
                    newTarget.Item1 = possibleTarget;
                    distance.Item1 = testDistance;
                }
                else if (newTarget.Item2 == null || testDistance < distance.Item2)
                {
                    newTarget.Item2 = possibleTarget;
                    distance.Item2 = testDistance;
                }
            }

            return newTarget;
        } // FindClosestEnemyByPriority

        public static Ship FindClosestEnemy(Ship ship, Weapon turret)
        {
            (Ship, Ship) newTargets = (null, null);

            var targetList = GameplayState.WorldManager.EnemyShips;
            if (!ship.IsPlayerShip)
                targetList = GameplayState.WorldManager.PlayerShips;

            // find closest target in priority order
            foreach (var priority in ship.TargetPriorities)
            {
                newTargets = FindClosestEnemyByPriority(targetList, ship, turret, priority);

                if (newTargets.Item1 != null)
                {
                    if (newTargets.Item2 == null)
                        return newTargets.Item1;
                    else
                    {
                        if (WorldData.RNG.Next(1, 3) == 1)
                            return newTargets.Item1;
                        else
                            return newTargets.Item2;
                    }
                }
            }

            // if nothing found just try to find any target
            newTargets = FindClosestEnemyByPriority(targetList, ship, turret, ShipType.None);

            if (newTargets.Item1 != null)
            {
                if (newTargets.Item2 == null)
                    return newTargets.Item1;
                else
                {
                    if (WorldData.RNG.Next(1, 3) == 1)
                        return newTargets.Item1;
                    else
                        return newTargets.Item2;
                }
            }

            return null;

        } // FindClosestEnemy

        public static (Ship, Ship) FindClosestEnemyByPriority(List<Ship> targetList, Ship ship, Weapon turret, ShipType priority = ShipType.None)
        {
            (Ship, Ship) newTarget = (null, null);
            (float, float) distance = (0.0f, 0.0f);

            for (var i = 0; i < targetList.Count; i++)
            {
                var possibleTarget = targetList[i];

                if (priority != ShipType.None && possibleTarget.ShipType != priority)
                    continue;

                if (possibleTarget.TargetType != turret.TargetType)
                    continue;

                var testDistance = Vector2.Distance(ship.Position, possibleTarget.Position);

                if (testDistance > turret.Range)
                    continue;

                if (newTarget.Item1 == null || testDistance < distance.Item1)
                {
                    newTarget.Item1 = possibleTarget;
                    distance.Item1 = testDistance;
                }
                else if (newTarget.Item2 == null || testDistance < distance.Item2)
                {
                    newTarget.Item2 = possibleTarget;
                    distance.Item2 = testDistance;
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

        public static void HandleTurret(Ship ship, Weapon turret, GameTime gameTime)
        {
            if (turret.Target == null || turret.Target.IsDead || Vector2.Distance(ship.Position, turret.Target.Position) > turret.Range)
            {
                turret.NextTargetScan -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (turret.NextTargetScan <= 0)
                {
                    turret.Target = FindClosestEnemy(ship, turret);
                    turret.NextTargetScan = turret.TargetScanDuration;
                }
            }

            if (turret.Target != null && turret.CurrentCooldown <= 0)
            {
                //turret.TurretRotation = GetAngleToTarget(ship.Position + turret.TurretPosition, 0.0f, turret.Target.Position);
                GameplayState.ProjectileManager.FireProjectile(turret, ship, turret.Target, turret.Damage);
                turret.ResetCooldown();
            }
        }

    } // AIHelper
}
