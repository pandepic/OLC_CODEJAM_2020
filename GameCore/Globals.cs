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
        public static int StartingMiners = 3;
        public static int WorldWidth = 50000;
        public static int WorldHeight = 50000;
        public static int AsteroidRegionWidth = 500;
        public static int AsteroidRegionHeight = 500;
        public static Vector2 PlayerStartPosition = new Vector2(5000, 5000);

        public static void Load()
        {
            var configDoc = XDocument.Load("Assets/config.xml");
        }
    }

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
