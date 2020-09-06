using GameCore.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore
{
    public struct WaveSpawnPosition
    {
        public string Name;
        public Vector2 Position;
    }

    public class EnemyWaveManager
    {
        public List<WaveSpawnPosition> SpawnPositions = new List<WaveSpawnPosition>();

        public int CurrentWaveValue, NextWaveValue;
        public float NextWaveTimer;
        public WaveSpawnPosition NextWavePosition;

        public List<ShipType> NewWaveShipTypes = new List<ShipType>();
        public List<ShipType> OtherShipTypes = new List<ShipType>();
        public List<Ship> NewWaveShips = new List<Ship>();

        public void Start()
        {
            SpawnPositions.Clear();
            NewWaveShipTypes.Clear();
            NewWaveShips.Clear();
            OtherShipTypes.Clear();

            SpawnPositions.Add(new WaveSpawnPosition()
            {
                Name = "North West",
                Position = new Vector2(Config.WorldWidth, Config.WorldHeight),//Position = new Vector2(0, 0),
            });

            SpawnPositions.Add(new WaveSpawnPosition()
            {
                Name = "North East",
                Position = new Vector2(Config.WorldWidth, Config.WorldHeight),//Position = new Vector2(Config.WorldWidth, 0),
            });

            SpawnPositions.Add(new WaveSpawnPosition()
            {
                Name = "South West",
                Position = new Vector2(Config.WorldWidth, Config.WorldHeight),//Position = new Vector2(0, Config.WorldHeight),
            });

            SpawnPositions.Add(new WaveSpawnPosition()
            {
                Name = "South East",
                Position = new Vector2(Config.WorldWidth, Config.WorldHeight),
            });

            foreach (var type in EntityData.ShipTypes)
            {
                if (type.Key == ShipType.Fighter || type.Key == ShipType.Bomber || type.Value.WaveValue < 0)
                    continue;

                OtherShipTypes.Add(type.Key);
            }

            NextWaveTimer = 0;
            CurrentWaveValue = 0;
            NextWaveValue = 0;

            SetNextWave(Config.StartingWaveTimer, Config.StartingWaveValue);
        }

        public void SetNextWave(float time, int addValue)
        {
            NextWaveTimer = time;
            NextWaveValue = CurrentWaveValue + addValue;

            //var nextWavePositions = new List<WaveSpawnPosition>();
            //foreach (var p in SpawnPositions)
            //{
            //    if (p.Name != NextWavePosition.Name)
            //        nextWavePositions.Add(p);
            //}

            var newSpawnPosition = SpawnPositions[0];
            var nextSpawnDistance = -1.0f;

            foreach (var p in SpawnPositions)
            {
                var distance = Vector2.Distance(p.Position, GameplayState.WorldManager.PlayerEntity.Position);

                if (nextSpawnDistance == -1.0f || distance < nextSpawnDistance)
                {
                    nextSpawnDistance = distance;
                    newSpawnPosition = p;
                }
            }

            NextWavePosition = newSpawnPosition;
        }

        public void SpawnNextWave()
        {
            CurrentWaveValue = NextWaveValue;
            var waveSpawnPosition = NextWavePosition;

            SetNextWave(Config.WaveTimer, Config.IncreasePerWave);

            var reservedFighterBomber = CurrentWaveValue / 2;
            var otherReserved = CurrentWaveValue - reservedFighterBomber;

            var fighterValue = reservedFighterBomber / 2;
            var bomberValue = reservedFighterBomber - fighterValue;

            var cheapestOther = -1;
            var mostExpensiveOther = -1;

            foreach (var type in OtherShipTypes)
            {
                var data = EntityData.ShipTypes[type];

                if (cheapestOther == -1 || data.WaveValue < cheapestOther)
                    cheapestOther = data.WaveValue;
                if (mostExpensiveOther == -1 || data.WaveValue > mostExpensiveOther)
                    mostExpensiveOther = data.WaveValue;
            }

            var exitSpawnOther = false;

            while (!exitSpawnOther)
            {
                var spawnType = OtherShipTypes[WorldData.RNG.Next(0, OtherShipTypes.Count)];
                var data = EntityData.ShipTypes[spawnType];

                while (data.WaveValue > otherReserved)
                {
                    spawnType = OtherShipTypes[WorldData.RNG.Next(0, OtherShipTypes.Count)];
                    data = EntityData.ShipTypes[spawnType];
                }

                NewWaveShipTypes.Add(spawnType);
                otherReserved -= data.WaveValue;

                exitSpawnOther = otherReserved < cheapestOther;
            }

            // any left over value dump into fighters
            if (otherReserved > 0)
                fighterValue += otherReserved;

            var fighterData = EntityData.ShipTypes[ShipType.Fighter];
            var bomberData = EntityData.ShipTypes[ShipType.Bomber];

            var currentFighterValue = 0;
            while (currentFighterValue < fighterValue)
            {
                NewWaveShipTypes.Add(ShipType.Fighter);
                currentFighterValue += fighterData.WaveValue;
            }

            var currentBomberValue = 0;
            while (currentBomberValue < bomberValue)
            {
                NewWaveShipTypes.Add(ShipType.Bomber);
                currentBomberValue += bomberData.WaveValue;
            }

            Ship flagship = null;
            var spawnFlagship = false;
            var slowestShipSpeed = -1.0f;
            var slowestShipType = ShipType.None;

            foreach (var type in NewWaveShipTypes)
            {
                var data = EntityData.ShipTypes[type];

                if (slowestShipType == ShipType.None || data.MoveSpeed < slowestShipSpeed)
                {
                    slowestShipType = data.ShipType;
                    slowestShipSpeed = data.MoveSpeed;
                }
            }

            foreach (var type in NewWaveShipTypes)
            {
                var data = EntityData.ShipTypes[type];

                if (slowestShipType != ShipType.None && data.MoveSpeed > slowestShipSpeed)
                    spawnFlagship = true;
            }

            if (spawnFlagship)
            {
                var position = waveSpawnPosition.Position + new Vector2(WorldData.RNG.Next(-1000, 1000), WorldData.RNG.Next(-1000, 1000));
                flagship = GameplayState.UnitManager.SpawnShip(slowestShipType, position, null);
                NewWaveShips.Add(flagship);
                NewWaveShipTypes.Remove(slowestShipType);
            }

            for (var i = 0; i < NewWaveShipTypes.Count; i++)
            {
                var data = EntityData.ShipTypes[NewWaveShipTypes[i]];
                var position = waveSpawnPosition.Position + new Vector2(WorldData.RNG.Next(-1000, 1000), WorldData.RNG.Next(-1000, 1000));
                NewWaveShips.Add(GameplayState.UnitManager.SpawnShip(NewWaveShipTypes[i], position, data.MoveSpeed > slowestShipSpeed ? flagship : null));
            }

            NewWaveShipTypes.Clear();
            NewWaveShips.Clear();
        }

        public void Update(GameTime gameTime)
        {
            NextWaveTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (NextWaveTimer < 0)
                SpawnNextWave();
        }
    }
}
