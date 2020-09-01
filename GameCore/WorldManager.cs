﻿using GameCore.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PandaMonogame;
using PandaMonogame.Assets;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore
{
    public class WorldManager
    {
        public static int StartingMiners = 2;
        public static int WorldWidth = 50000;
        public static int WorldHeight = 50000;
        public static int AsteroidRegionWidth = 250;
        public static int AsteroidRegionHeight = 250;
        public static Vector2 PlayerStartPosition = new Vector2(5000, 5000);

        public ObjectPool<Asteroid> Asteroids;
        public List<Ship> Ships = new List<Ship>();
        public List<Ship> PlayerShips = new List<Ship>();
        public List<Ship> EnemyShips = new List<Ship>();
        public List<Ship> DeadShips = new List<Ship>();
        public Player PlayerEntity;

        //public Texture2D Planet, Background;
        public Texture2D Background;
        public Vector2 ScreenCenter;

        public BasicCamera2D Camera;
        public UnitManager UnitManager;
        GraphicsDevice Graphics;

        public WorldManager()
        {
        }

        public void Setup(GraphicsDevice graphics, BasicCamera2D camera, UnitManager unitManager)
        {
            UnitManager = unitManager;
            UnitManager.WorldManager = this;
            Graphics = graphics;
            Camera = camera;
            ScreenCenter = new Vector2(graphics.PresentationParameters.BackBufferWidth / 2, graphics.PresentationParameters.BackBufferHeight / 2);

            // todo - set world seed
            WorldData.RNG = new Random();

            var bg = 1;// WorldData.RNG.Next(1, 9);

            var planet = ModManager.Instance.AssetManager.LoadTexture2D(graphics, "Planet" + WorldData.RNG.Next(1, 11).ToString(), true);
            var background = ModManager.Instance.AssetManager.LoadTexture2D(graphics, "Background" + bg.ToString(), true);

            Background = new RenderTarget2D(graphics, background.Width, background.Height);

            graphics.SetRenderTarget((RenderTarget2D)Background);
            graphics.Clear(Color.Transparent);

            using (SpriteBatch spriteBatch = new SpriteBatch(graphics))
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                spriteBatch.Draw(background, Vector2.Zero, Color.White); // todo : fix bg not lining up at right edge
                spriteBatch.Draw(planet,
                    new Vector2()
                    {
                        X = background.Width / 2 - planet.Width / 2,
                        Y = background.Height / 2 - planet.Height / 2
                    },
                    Color.White);
                spriteBatch.End();
            }
            graphics.SetRenderTarget(null);

            Asteroids = new ObjectPool<Asteroid>(10000);

            // random asteroid field generation
            for (var x = AsteroidRegionWidth; x < (WorldWidth - AsteroidRegionWidth); x += AsteroidRegionWidth)
            {
                for (var y = AsteroidRegionHeight; y < (WorldHeight - AsteroidRegionHeight); y += AsteroidRegionHeight)
                {
                    if (WorldData.RNG.Next(0, 10) < 2 && Asteroids.LastActiveIndex < (Asteroids.Size - 1))
                    {
                        var newAsteroid = Asteroids.New();
                        newAsteroid.Sprite = TexturePacker.GetSprite("AsteroidsAtlas", "Asteroid" + WorldData.RNG.Next(1, WorldData.AsteroidTypes + 1).ToString());
                        newAsteroid.Position = new Vector2(x + WorldData.RNG.Next(0, 150), y + WorldData.RNG.Next(0, 150));
                        newAsteroid.Origin = new Vector2(newAsteroid.Sprite.SourceRect.Width / 2, newAsteroid.Sprite.SourceRect.Height / 2);
                        newAsteroid.RotationSpeed = (float)PandaUtil.RandomDouble(WorldData.RNG, 0, 8);
                        newAsteroid.Rotation = WorldData.RNG.Next(0, 360);

                        if (WorldData.RNG.Next(0, 10) < 3)
                        {
                            newAsteroid.ResourceType = WorldData.ResourceTypes[WorldData.RNG.Next(0, WorldData.ResourceTypes.Count)];
                            newAsteroid.ResourceCount = WorldData.RNG.Next(50000, 100000);
                        }
                    }
                }
            }

            PlayerEntity = new Player();
            PlayerEntity.Position = PlayerStartPosition;
            PlayerShips.Add(PlayerEntity);

            for (var i = 0; i < StartingMiners; i++)
            {
                UnitManager.SpawnShip(ShipType.Miner, PlayerEntity.Position + new Vector2(WorldData.RNG.Next(-200, 200), WorldData.RNG.Next(-200, 200)), PlayerEntity);
            }

            UnitManager.SpawnShip(ShipType.Fighter, new Vector2(4000, 4000), null);
        }

        ~WorldManager()
        {
            Background.Dispose();
        }

        public void Update(GameTime gameTime)
        {
            for (var i = 0; i <= Asteroids.LastActiveIndex; i++)
            {
                Asteroids[i].Update(gameTime);
            }

            DeadShips.Clear();

            for (var i = 0; i < Ships.Count; i++)
            {
                var ship = Ships[i];
                ship.Update(gameTime);

                if (ship.CurrentArmourHP <= 0)
                    DeadShips.Add(ship);
            }

            foreach (var ship in DeadShips)
                UnitManager.DestroyShip(ship);

            PlayerEntity.Update(gameTime);
        }

        public void DrawWorld (SpriteBatch spriteBatch)
        {
            var camPos = Camera.GetPosition() + Camera.GetOrigin();
            var viewDistance = Graphics.PresentationParameters.BackBufferWidth / (Camera.Zoom * Camera.GetZoomFromZ(Camera.Z, 0f));

            for (var i = 0; i <= Asteroids.LastActiveIndex; i++)
            {
                var asteroid = Asteroids[i];
                var distance = Vector2.Distance(asteroid.Position, camPos);

                // quick hack to do view culling
                if (distance < viewDistance)
                    asteroid.Draw(spriteBatch);
            }

            PlayerEntity.Draw(spriteBatch);

            for (var i = 0; i < Ships.Count; i++)
            {
                var ship = Ships[i];
                var distance = Vector2.Distance(ship.Position, camPos);

                // quick hack to do view culling
                if (distance < viewDistance)
                    ship.Draw(spriteBatch);
            }
        }

        public void DrawScreen(SpriteBatch spriteBatch, float z)
        {
            // 5f is furthest Z defined in _zLevels;
            float backRatio = 1f / (5f - z + 1);
            Vector2 screenHalf = new Vector2(Graphics.PresentationParameters.BackBufferWidth / 2, Graphics.PresentationParameters.BackBufferHeight / 2);
            Vector2 topLeft = new Vector2(-screenHalf.X / backRatio, -screenHalf.Y / backRatio);
            Vector2 bottomRight = new Vector2(WorldWidth + screenHalf.X / backRatio, WorldHeight + screenHalf.Y / backRatio);
            int width = (int)(bottomRight.X - topLeft.X);
            int height = (int)(bottomRight.Y - topLeft.Y);
            int maxSize = Math.Max(width, height); // Preserve square.
            spriteBatch.Draw(
                Background,
                new Rectangle((int)topLeft.X, (int)topLeft.Y, maxSize, maxSize),
                Color.White
            );
        }
    }
}
