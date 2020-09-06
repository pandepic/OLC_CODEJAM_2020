using GameCore.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PandaMonogame;
using PandaMonogame.Assets;
using SpriteFontPlus;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace GameCore
{
    public enum SoundType
    {
        Music,
        SoundEffect,
        UI
    }

    public enum GameStateType
    {
        None,
        Startup,
        Menu,
        Loading,
        Play,
        Settings,
        GameOver,
        Exit,
    };

    public enum ResourceType
    {
        None,
        Metal,
        Gas,
        Water,
        Crystal,
        Uranium
    }

    public enum ShipType
    {
        None,
        HomeShip,

        Miner,
        Fighter,
        Bomber,
        RepairShip,
        MissileFrigate,
        BeamFrigate,
        SupportCruiser,
        HeavyCruiser,
        Battleship,
        Carrier,
    }

    public enum ShipStance
    {
        Passive,
        Defensive,
        Aggressive,
    }

    public enum TargetType
    {
        None,
        Small,
        Large,
    }

    public enum MountType
    {
        None,
        Fixed,
        Turret
    }

    public enum ProjectileDeathType
    {
        None,
        Explosion,
    }

    public static class Config
    {
        // player
        public static int StartingMiners;
        public static Vector2 PlayerStartPosition;
        public static int BaseMinerLimit;

        // world
        public static int WorldWidth;
        public static int WorldHeight;
        public static int AsteroidRegionWidth;
        public static int AsteroidRegionHeight;

        // enemy waves
        public static int StartingWaveValue;
        public static int IncreasePerWave;
        public static float StartingWaveTimer; // in ms
        public static float WaveTimer; // in ms
        public static int Difficulty = 5;

        public static void Load()
        {
            using (var fs = ModManager.Instance.AssetManager.GetFileStreamByAsset("Config"))
            {
                var configDoc = XDocument.Load(fs);

                var elPlayer = configDoc.Root.Element("Player");
                var elWorld = configDoc.Root.Element("World");
                var elEnemyWaves = configDoc.Root.Element("EnemyWaves");

                StartingMiners = int.Parse(elPlayer.Attribute("StartingMiners").Value);
                var startPosSplit = elPlayer.Attribute("StartPosition").Value.Split(',');
                PlayerStartPosition = new Vector2(float.Parse(startPosSplit[0]), float.Parse(startPosSplit[1]));
                BaseMinerLimit = int.Parse(elPlayer.Attribute("BaseMinerLimit").Value);

                WorldWidth = int.Parse(elWorld.Attribute("Width").Value);
                WorldHeight = int.Parse(elWorld.Attribute("Height").Value);
                AsteroidRegionWidth = int.Parse(elWorld.Attribute("AsteroidRegionWidth").Value);
                AsteroidRegionHeight = int.Parse(elWorld.Attribute("AsteroidRegionHeight").Value);

                StartingWaveValue = int.Parse(elEnemyWaves.Attribute("StartingValue").Value);
                IncreasePerWave = int.Parse(elEnemyWaves.Attribute("IncreasePerWave").Value);
                StartingWaveTimer = float.Parse(elEnemyWaves.Attribute("StartingTimer").Value) * 1000.0f;
                WaveTimer = float.Parse(elEnemyWaves.Attribute("WaveTimer").Value) * 1000.0f;
            }
        } // Load
    } // Config

    public static class WorldData
    {
        public static Random RNG = new Random();

        public static int GatherRate = 10;

        public static List<ResourceType> ResourceTypes = new List<ResourceType>()
        {
            ResourceType.Metal,
            ResourceType.Gas,
            ResourceType.Water,
            ResourceType.Crystal,
            ResourceType.Uranium
        };

        public static int AsteroidTypes = 5;

        public static void Load()
        {
        }
    }

    public static class Sprites
    {
        public static Texture2D ShieldTexture = null;
        public static DynamicSpriteFont DefaultFont = null;
        public static Color EnemyColour = Color.MediumPurple;

        public static void Load(GraphicsDevice graphics)
        {
            TexturePacker.LoadAsset(graphics, "AsteroidsAtlas", true);
            TexturePacker.LoadAsset(graphics, "ShipsAtlas", true);
            TexturePacker.LoadAsset(graphics, "ParticlesAtlas", true);
            TexturePacker.LoadAsset(graphics, "ResourcesAtlas", false);

            ShieldTexture = ModManager.Instance.AssetManager.LoadTexture2D(graphics, "Shield", true);
            DefaultFont = ModManager.Instance.AssetManager.LoadDynamicSpriteFont("latoblack");
        }
    }
}
