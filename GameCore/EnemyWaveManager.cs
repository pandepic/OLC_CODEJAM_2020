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

        public void Start()
        {
            SpawnPositions.Clear();

            SpawnPositions.Add(new WaveSpawnPosition()
            {
                Name = "North West",
                Position = new Vector2(-1000, -1000),
            });

            SpawnPositions.Add(new WaveSpawnPosition()
            {
                Name = "North East",
                Position = new Vector2(Config.WorldWidth, 0) + new Vector2(1000, -1000),
            });

            SpawnPositions.Add(new WaveSpawnPosition()
            {
                Name = "South West",
                Position = new Vector2(0, Config.WorldHeight) + new Vector2(-1000, 1000),
            });

            SpawnPositions.Add(new WaveSpawnPosition()
            {
                Name = "South East",
                Position = new Vector2(Config.WorldWidth, Config.WorldHeight) + new Vector2(1000, 1000),
            });

            NextWaveTimer = 0;
            CurrentWaveValue = 0;
            NextWaveValue = 0;

            SetNextWave(Config.StartingWaveTimer, Config.StartingWaveValue);
        }

        public void SetNextWave(float time, int addValue)
        {
            NextWaveTimer = time;
            NextWaveValue = CurrentWaveValue + addValue;

            var nextWavePositions = new List<WaveSpawnPosition>();
            foreach (var p in SpawnPositions)
            {
                if (p.Name != NextWavePosition.Name)
                    nextWavePositions.Add(p);
            }

            NextWavePosition = nextWavePositions[WorldData.RNG.Next(0, nextWavePositions.Count)];
        }

        public void SpawnNextWave()
        {
            CurrentWaveValue = NextWaveValue;
            var waveSpawnPosition = NextWavePosition;

            SetNextWave(Config.WaveTimer, Config.IncreasePerWave);
        }

        public void Update(GameTime gameTime)
        {
            NextWaveTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (NextWaveTimer < 0)
                SpawnNextWave();
        }
    }
}
