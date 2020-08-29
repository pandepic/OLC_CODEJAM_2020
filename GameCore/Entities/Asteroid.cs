using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PandaMonogame;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore.Entities
{
    public class AsteroidTypeData
    {
        public AsteroidType Type;
        public Rectangle SourceRect;
    }

    public class Asteroid : IPoolable
    {
        public Texture2D Texture;
        public Vector2 Position, Origin;
        public AsteroidTypeData TypeData;
        public float Rotation = 0.0f;
        public float Scale = 1.0f;
        public float RotationSpeed = 0.0f;

        public bool IsAlive { get; set; }

        public void Reset()
        {
        }

        public void Update(GameTime gameTime)
        {
            Rotation += RotationSpeed * gameTime.DeltaTime();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                        Texture,
                        Position,
                        TypeData.SourceRect,
                        Color.White,
                        Rotation,
                        Origin,
                        Scale,
                        SpriteEffects.None,
                        0.0f
                        );
        }
    }
}
