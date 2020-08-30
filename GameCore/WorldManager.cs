using Dcrew.Camera;
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
    public class WorldManager
    {
        public static int WorldWidth = 50000;
        public static int WorldHeight = 50000;
        public int AsteroidRegionWidth = 250;
        public int AsteroidRegionHeight = 250;

        public ObjectPool<Asteroid> Asteroids;
        public List<Ship> Ships;
        public Player PlayerEntity;

        //public Texture2D Planet, Background;
        public Texture2D Background;
        public Vector2 ScreenCenter;

        protected BasicCamera2D _camera;

        public Miner TestMiner;

        public WorldManager(GraphicsDevice graphics, BasicCamera2D camera)
        {
            _camera = camera;
            ScreenCenter = new Vector2(graphics.PresentationParameters.BackBufferWidth / 2, graphics.PresentationParameters.BackBufferHeight / 2);

            var rng = new Random();

            var planet = ModManager.Instance.AssetManager.LoadTexture2D(graphics, "Planet" + rng.Next(1, 11).ToString(), true);
            var background = ModManager.Instance.AssetManager.LoadTexture2D(graphics, "Background" + rng.Next(1, 9).ToString(), true);

            Background = new RenderTarget2D(graphics, background.Width, background.Height);
            graphics.SetRenderTarget((RenderTarget2D)Background);
            using (SpriteBatch spriteBatch = new SpriteBatch(graphics))
            {
                graphics.Clear(Color.Transparent);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                spriteBatch.Draw(background, Vector2.Zero, Color.White);
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
            //Ships = new ObjectPool<Ship>(1000);
            Ships = new List<Ship>();

            // random asteroid field generation
            for (var x = AsteroidRegionWidth; x < (WorldWidth - AsteroidRegionWidth); x += AsteroidRegionWidth)
            {
                for (var y = AsteroidRegionHeight; y < (WorldHeight - AsteroidRegionHeight); y += AsteroidRegionHeight)
                {
                    if (rng.Next(0, 10) < 2 && Asteroids.LastActiveIndex < (Asteroids.Size - 1))
                    {
                        var newAsteroid = Asteroids.New();
                        newAsteroid.Sprite = TexturePacker.GetSprite("AsteroidsAtlas", "Asteroid" + rng.Next(1, WorldData.AsteroidTypes + 1).ToString());
                        newAsteroid.Position = new Vector2(x + rng.Next(0, 150), y + rng.Next(0, 150));
                        newAsteroid.Origin = new Vector2(newAsteroid.Sprite.SourceRect.Width / 2, newAsteroid.Sprite.SourceRect.Height / 2);
                        newAsteroid.RotationSpeed = (float)PandaUtil.RandomDouble(rng, 0.0, 0.1);

                        if (rng.Next(0, 10) < 3)
                        {
                            newAsteroid.ResourceType = WorldData.ResourceTypes[rng.Next(0, WorldData.ResourceTypes.Count)];
                            newAsteroid.ResourceCount = rng.Next(50000, 100000);
                        }
                    }
                }
            }

            PlayerEntity = new Player();
            PlayerEntity.Position = new Vector2(500, 500);

            TestMiner = new Miner(PlayerEntity);
            TestMiner.Position = new Vector2(1000, 1000);
            TestMiner.Sprite = TexturePacker.GetSprite("ShipsAtlas", "Fighter1");
            TestMiner.Origin = new Vector2(TestMiner.Sprite.SourceRect.Width / 2, TestMiner.Sprite.SourceRect.Height / 2);
            TestMiner.MoveSpeed = 200.0f;
            TestMiner.TurnSpeed = 75.0f;

            Ships.Add(TestMiner);

            //for (var i = 0; i < 25; i++)
            //{
            //    var newFighter = Ships.New();
            //    newFighter.Sprite = TexturePacker.GetSprite("ShipsAtlas", "Fighter1");
            //    newFighter.Origin = new Vector2(newFighter.Sprite.SourceRect.Width / 2, newFighter.Sprite.SourceRect.Height / 2);
            //    newFighter.Position = new Vector2(rng.Next(2000, 5000), rng.Next(2000, 5000));
            //    newFighter.MoveSpeed = 200.0f;
            //    newFighter.TurnSpeed = 75.0f;
            //}
        }

        public void Update(GameTime gameTime)
        {
            for (var i = 0; i <= Asteroids.LastActiveIndex; i++)
            {
                Asteroids[i].Update(gameTime);
            }

            for (var i = 0; i < Ships.Count; i++)
            {
                //Ships[i].SetDestination(PlayerEntity.Position);
                Ships[i].Update(gameTime);
            }

            PlayerEntity.Update(gameTime);
        }

        public void DrawWorld (SpriteBatch spriteBatch)
        {
            for (var i = 0; i <= Asteroids.LastActiveIndex; i++)
            {
                Asteroids[i].Draw(spriteBatch);
            }

            for (var i = 0; i < Ships.Count; i++)
            {
                Ships[i].Draw(spriteBatch);
            }

            PlayerEntity.Draw(spriteBatch);
        }

        public void DrawScreen(SpriteBatch spriteBatch)
        {
            //var worldSize = new Vector2(WorldWidth * 1.5f, WorldHeight * 1.5f);
            var worldSize = new Vector2(WorldWidth, WorldHeight);
            var bgSize = new Vector2(Background.Width, Background.Height);
            var bgProportionalSize = (float)bgSize.X / (float)worldSize.X;
            float bgZoom = 1.0f - ((1.0f - _camera.Zoom) * bgProportionalSize);

            var screenPosWorld = _camera.ScreenToWorldPosition(Vector2.Zero);

            var backgroundPos = ((screenPosWorld / worldSize) * bgSize) * bgZoom;

            spriteBatch.Draw(
                        Background,
                        -backgroundPos,
                        null,
                        Color.White,
                        0.0f,
                        Vector2.Zero,
                        bgZoom,
                        SpriteEffects.None,
                        0.0f
                        );
        }
    }
}
