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
        public int WorldWidth = 50000;
        public int WorldHeight = 50000;
        public int AsteroidRegionWidth = 250;
        public int AsteroidRegionHeight = 250;

        public ObjectPool<Asteroid> Asteroids;
        public ObjectPool<Ship> Ships;
        public Player PlayerEntity;

        public WorldManager()
        {
            var rng = new Random();

            Asteroids = new ObjectPool<Asteroid>(10000);
            Ships = new ObjectPool<Ship>(1000);

            var asteroidTypeCount = WorldData.AsteroidTypes.Count;

            for (var x = 0; x < WorldWidth; x += AsteroidRegionWidth)
            {
                for (var y = 0; y < WorldHeight; y += AsteroidRegionHeight)
                {
                    if (rng.Next(0, 10) < 2 && Asteroids.LastActiveIndex < (Asteroids.Size - 1))
                    {
                        var newAsteroid = Asteroids.New();
                        newAsteroid.Texture = Sprites.AsteroidsTexture;
                        newAsteroid.TypeData = WorldData.AsteroidTypeDataDB[WorldData.AsteroidTypes[rng.Next(0, asteroidTypeCount)]];
                        newAsteroid.Position = new Vector2(x, y);
                        newAsteroid.Origin = new Vector2(newAsteroid.TypeData.SourceRect.Width / 2, newAsteroid.TypeData.SourceRect.Height / 2);
                        newAsteroid.RotationSpeed = (float)PandaUtil.RandomDouble(rng, 0.0, 0.1);
                    }
                }
            }

            PlayerEntity = new Player();
            PlayerEntity.Position = new Vector2(500, 500);

            for (var i = 0; i < 25; i++)
            {
                var newFighter = Ships.New();
                newFighter.Sprite = TexturePacker.GetSprite("ShipsAtlas", "Fighter1");
                newFighter.Origin = new Vector2(newFighter.Sprite.SourceRect.Width / 2, newFighter.Sprite.SourceRect.Height / 2);
                newFighter.Position = new Vector2(rng.Next(2000, 5000), rng.Next(2000, 5000));
                newFighter.MoveSpeed = 200.0f;
                newFighter.TurnSpeed = 75.0f;
            }
        }

        public void Update(GameTime gameTime)
        {
            for (var i = 0; i <= Asteroids.LastActiveIndex; i++)
            {
                Asteroids[i].Update(gameTime);
            }

            for (var i = 0; i <= Ships.LastActiveIndex; i++)
            {
                Ships[i].SetDestination(PlayerEntity.Position);
                Ships[i].Update(gameTime);
            }

            PlayerEntity.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (var i = 0; i <= Asteroids.LastActiveIndex; i++)
            {
                Asteroids[i].Draw(spriteBatch);
            }

            for (var i = 0; i <= Ships.LastActiveIndex; i++)
            {
                Ships[i].Draw(spriteBatch);
            }

            PlayerEntity.Draw(spriteBatch);
        }
    }
}
