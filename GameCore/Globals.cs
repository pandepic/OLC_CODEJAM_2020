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
        Exit
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
        Repair,
        Fighter
    }

    public enum ShipStance
    {
        Passive,
        Defensive,
        Aggressive,
    }

    public enum WeaponType
    {
        None,
        BurstLaser,
    }

    public static class Screen
    {
        public static Vector2 GetMousePosition()
        {
            var mouseState = Mouse.GetState();
            return new Vector2(mouseState.Position.X, mouseState.Position.Y);
        }
    }

    public static class WorldData
    {
        public static Random RNG = new Random();

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
        public static Texture2D AsteroidsTexture = null;
        public static Texture2D ShipsTexture = null;
        public static DynamicSpriteFont DefaultFont = null;
        public static Color EnemyColour = Color.Turquoise;

        public static void Load(GraphicsDevice graphics)
        {
            TexturePacker.LoadAsset(graphics, "AsteroidsAtlas");
            TexturePacker.LoadAsset(graphics, "ShipsAtlas");

            AsteroidsTexture = ModManager.Instance.AssetManager.LoadTexture2D(graphics, "AsteroidsTexture");
            ShipsTexture = ModManager.Instance.AssetManager.LoadTexture2D(graphics, "ShipsTexture");

            DefaultFont = ModManager.Instance.AssetManager.LoadDynamicSpriteFont("latoblack");
        }
    }
}
