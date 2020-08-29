using GameCore.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PandaMonogame;
using PandaMonogame.Assets;
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

    public enum AsteroidType
    {
        Iron,
        Copper,
        Uranium
    }

    public static class WorldData
    {
        public static Dictionary<AsteroidType, AsteroidTypeData> AsteroidTypeDataDB;
        public static List<AsteroidType> AsteroidTypes;

        public static void Load()
        {
            AsteroidTypeDataDB = new Dictionary<AsteroidType, AsteroidTypeData>();
            AsteroidTypes = new List<AsteroidType>();

            AsteroidTypeDataDB.Add(AsteroidType.Iron, new AsteroidTypeData()
            {
                SourceRect = Sprites.Asteroid1,
                Type = AsteroidType.Iron,
            });

            AsteroidTypeDataDB.Add(AsteroidType.Copper, new AsteroidTypeData()
            {
                SourceRect = Sprites.Asteroid3,
                Type = AsteroidType.Copper,
            });

            AsteroidTypeDataDB.Add(AsteroidType.Uranium, new AsteroidTypeData()
            {
                SourceRect = Sprites.Asteroid5,
                Type = AsteroidType.Uranium,
            });

            AsteroidTypes.Add(AsteroidType.Iron);
            AsteroidTypes.Add(AsteroidType.Copper);
            AsteroidTypes.Add(AsteroidType.Uranium);
        }
    }

    public static class Sprites
    {
        public static Rectangle Asteroid1 = Rectangle.Empty;
        public static Rectangle Asteroid2 = Rectangle.Empty;
        public static Rectangle Asteroid3 = Rectangle.Empty;
        public static Rectangle Asteroid4 = Rectangle.Empty;
        public static Rectangle Asteroid5 = Rectangle.Empty;
        public static Rectangle Asteroid6 = Rectangle.Empty;
        
        public static Texture2D AsteroidsTexture = null;
        public static Texture2D ShipsTexture = null;

        public static void Load(GraphicsDevice graphics)
        {
            TexturePacker.LoadAsset(graphics, "AsteroidsAtlas");
            TexturePacker.LoadAsset(graphics, "ShipsAtlas");

            AsteroidsTexture = ModManager.Instance.AssetManager.LoadTexture2D(graphics, "AsteroidsTexture");
            ShipsTexture = ModManager.Instance.AssetManager.LoadTexture2D(graphics, "ShipsTexture");

            Asteroid1 = TexturePacker.GetSourceRect("AsteroidsAtlas", "Asteroid1");
            Asteroid2 = TexturePacker.GetSourceRect("AsteroidsAtlas", "Asteroid2");
            Asteroid3 = TexturePacker.GetSourceRect("AsteroidsAtlas", "Asteroid3");
            Asteroid4 = TexturePacker.GetSourceRect("AsteroidsAtlas", "Asteroid4");
            Asteroid5 = TexturePacker.GetSourceRect("AsteroidsAtlas", "Asteroid5");
            Asteroid6 = TexturePacker.GetSourceRect("AsteroidsAtlas", "Asteroid6");
        }

    }
}
