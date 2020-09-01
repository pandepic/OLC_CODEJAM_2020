using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        public float Rotation = 0.0f, TargetRotation = 0.0f;
        public float Scale = 1.0f;
        public float MoveSpeed, TurnSpeed;

        public Vector2 CollisionPos
        {
            get
            {
                return new Vector2(Position.X - Origin.X, Position.Y - Origin.Y);
            }
        }

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

        public void ApplyMovement(GameTime gameTime)
        {
            var delta = gameTime.DeltaTime();

            if (Rotation != TargetRotation)
            {
                if (TargetRotation > Rotation)
                {
                    Rotation += TurnSpeed * delta;
                    if (Rotation > TargetRotation)
                        Rotation = TargetRotation;
                }
                else
                {
                    Rotation -= TurnSpeed * delta;
                    if (Rotation < TargetRotation)
                        Rotation = TargetRotation;
                }
            }

            var forwardVector = new Vector2(0f, -1f);
            var rotaterMatrix = Matrix.CreateRotationZ(MathHelper.ToRadians(Rotation));
            forwardVector = Vector2.TransformNormal(forwardVector, rotaterMatrix);
            Velocity = forwardVector * MoveSpeed;

            Position += Velocity * delta;
        }

        public void Draw(SpriteBatch spriteBatch, Color colour)
        {
            spriteBatch.Draw(
                        Sprite.Texture,
                        Position,
                        Sprite.SourceRect,
                        Color.FromNonPremultiplied(colour.R, colour.G, colour.B, colour.A),
                        MathHelper.ToRadians(Rotation),
                        Origin,
                        Scale,
                        SpriteEffects.None,
                        0.0f
                        );
        }
    }
}
