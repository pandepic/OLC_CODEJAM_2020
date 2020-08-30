using Microsoft.Xna.Framework;
using PandaMonogame;
using PandaMonogame.Assets;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore.Entities
{
    public class Entity : IPoolable
    {
        public TexturePackerSprite Sprite;
        public Vector2 Position, Velocity, Origin;
        public float Rotation = 0.0f;
        public float Scale = 1.0f;

        public Rectangle CollisionRect
        {
            get
            {
                return new Rectangle((int)Position.X - (int)Origin.X, (int)Position.Y - (int)Origin.Y, Sprite.SourceRect.Width, Sprite.SourceRect.Height);
            }
        }

        public bool IsAlive { get; set; }

        public virtual void Reset()
        {
        }
    }
}
