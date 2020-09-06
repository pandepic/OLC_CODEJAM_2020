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
        public float BaseMoveSpeed, BaseTurnSpeed;
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
                return new Rectangle((int)Position.X - (int)Origin.X, (int)Position.Y - (int)Origin.Y, Width, Height);
            }
        }

        public int Width
        {
            get
            {
                return (int)(Sprite.SourceRect.Width * Scale);
            }
        }

        public int Height
        {
            get
            {
                return (int)(Sprite.SourceRect.Height * Scale);
            }
        }

        public bool IsAlive { get; set; }

        public virtual void Reset()
        {
        }

        public void CenterOrigin()
        {
            Origin = new Vector2(Width / 2, Height / 2);
        }

        public void ApplyMovement(GameTime gameTime)
        {
            var delta = gameTime.DeltaTime();

            if (Rotation != TargetRotation)
            {
                if (Math.Abs(Rotation - TargetRotation) < 1.0f)
                {
                    Rotation = TargetRotation;
                }
                else
                {
                    if (Rotation < TargetRotation)
                    {
                        if (Math.Abs(Rotation - TargetRotation) < 180.0f)
                        {
                            Rotation += TurnSpeed * delta;
                        }
                        else
                        {
                            Rotation -= TurnSpeed * delta;
                        }
                    }
                    else
                    {
                        if (Math.Abs(Rotation - TargetRotation) < 180.0f)
                        {
                            Rotation -= TurnSpeed * delta;
                        }
                        else
                        {
                            Rotation += TurnSpeed * delta;
                        }
                    }
                }

                if (Rotation < 0.0f)
                {
                    Rotation += 360.0f;
                }
                else if (Rotation > 360.0f)
                {
                    Rotation -= 360.0f;
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
