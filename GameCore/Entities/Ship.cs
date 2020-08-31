using GameCore.AI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PandaMonogame;
using PandaMonogame.Assets;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore.Entities
{
    public class Ship : Entity
    {
        public Ship Owner = null;
        public ShipType Type = ShipType.None;
        public bool IsPlayerShip = false;

        public float TargetRotation = 0.0f;
        public Vector2 Destination;

        public bool Moving = false;
        public float TurnSpeed = 0.0f;
        public float MoveSpeed = 0.0f;

        public SimpleStateMachine StateMachine = new SimpleStateMachine();

        public bool Selected = false;
        public Color SelectionColour = Color.Red;

        public void LoadData()
        {
            var data = EntityData.ShipTypes[Type];
            Sprite = TexturePacker.GetSprite("ShipsAtlas", data.Sprite);
            Origin = new Vector2(Sprite.SourceRect.Width / 2, Sprite.SourceRect.Height / 2);
            MoveSpeed = data.MoveSpeed;
            TurnSpeed = data.TurnSpeed;
        }

        public virtual void Update(GameTime gameTime)
        {
            StateMachine.Update(gameTime);

            Position += Velocity * gameTime.DeltaTime();

            if (Moving)
            {
                TargetRotation = GetAngleToTarget(Destination);

                if (Rotation != TargetRotation)
                {
                    if (TargetRotation > Rotation)
                    {
                        Rotation += TurnSpeed * gameTime.DeltaTime();
                        if (Rotation > TargetRotation)
                            Rotation = TargetRotation;
                    }
                    else
                    {
                        Rotation -= TurnSpeed * gameTime.DeltaTime();
                        if (Rotation < TargetRotation)
                            Rotation = TargetRotation;
                    }
                }

                var forwardVector = new Vector2(0f, -1f);
                var rotaterMatrix = Matrix.CreateRotationZ(MathHelper.ToRadians(Rotation));
                forwardVector = Vector2.TransformNormal(forwardVector, rotaterMatrix);
                Velocity = forwardVector * MoveSpeed;
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                        Sprite.Texture,
                        Position,
                        Sprite.SourceRect,
                        (Selected ? SelectionColour : Color.White),
                        MathHelper.ToRadians(Rotation),
                        Origin,
                        Scale,
                        SpriteEffects.None,
                        0.0f
                        );
        }

        public void SetDestination(Vector2 destination)
        {
            Destination = destination;
            Moving = true;
            TargetRotation = GetAngleToTarget(Destination);
        }

        public float GetAngleToTarget(Vector2 target)
        {
            var angle = MathHelper.ToDegrees(MathF.Atan2((target.X - Position.X), (Position.Y - target.Y)));
            var distance = Math.Abs(Rotation - angle);

            var angle360 = angle;
            if (angle360 < 0)
                angle360 = 180.0f + (180.0f + angle360);

            var distance360 = Math.Abs(Rotation - angle360);

            if (distance360 < distance)
                angle = angle360;

            return angle;
        }

        public void StopMovement()
        {
            Moving = false;
            Velocity = Vector2.Zero;
        }

        public void SetState<T>() where T : SimpleStateBase
        {
            StateMachine.SetState<T>();
        }

        public T GetState<T>() where T : SimpleStateBase
        {
            return StateMachine.GetState<T>();
        }

        public void SetState(string name)
        {
            StateMachine.SetState(name);
        }
    }
}
