using GameCore.AI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PandaMonogame;
using PandaMonogame.Assets;
using SpriteFontPlus;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore.Entities
{
    public class Ship : Entity
    {
        public Sprite ShieldSprite;
        public float ShieldRadius;
        public bool IsShieldActive;
        public float ShieldCooldown;

        public ShipType Type = ShipType.None;
        public ShipStance Stance = ShipStance.Passive;

        public Ship Owner = null;
        public bool IsPlayerShip = false;
        public bool IsDead = false;

        public float TargetRotation = 0.0f;
        public Vector2 Destination;

        public bool Moving = false;
        public float TurnSpeed = 0.0f;
        public float MoveSpeed = 0.0f;

        public float BaseArmourHP, CurrentArmourHP;
        public float BaseShieldHP, CurrentShieldHP;
        public float BaseShieldRegenRate, CurrentShieldRegenRate;

        public SimpleStateMachine StateMachine = new SimpleStateMachine();

        public List<Weapon> Weapons = new List<Weapon>();
        public float MinWeaponRange = -1, MaxWeaponRange = -1;

        public Ship EnemyTarget = null;

        public bool Selected = false;
        public Color SelectionColour = Color.Yellow;

        public void LoadData()
        {
            var data = EntityData.ShipTypes[Type];
            Sprite = TexturePacker.GetSprite("ShipsAtlas", data.Sprite);
            Origin = new Vector2(Sprite.SourceRect.Width / 2, Sprite.SourceRect.Height / 2);
            MoveSpeed = data.MoveSpeed;
            TurnSpeed = data.TurnSpeed;
            
            BaseArmourHP = data.ArmourHP;
            BaseShieldHP = data.ShieldHP;
            BaseShieldRegenRate = data.ShieldRegenRate;

            CurrentArmourHP = BaseArmourHP;
            CurrentShieldHP = BaseShieldHP;
            CurrentShieldRegenRate = BaseShieldRegenRate;

            Weapons = new List<Weapon>();

            foreach (var weapon in data.Weapons)
            {
                Weapons.Add(new Weapon()
                {
                    Type = weapon.Type,
                    Range = weapon.Range,
                    Cooldown = weapon.Cooldown,
                    Damage = weapon.Damage,
                    CurrentCooldown = weapon.Cooldown,
                });
            }

            foreach (var weapon in Weapons)
            {
                if (MinWeaponRange == -1 || weapon.Range < MinWeaponRange)
                    MinWeaponRange = weapon.Range;
                if (MaxWeaponRange == -1 || weapon.Range > MaxWeaponRange)
                    MaxWeaponRange = weapon.Range;
            }

            ShieldSprite = new Sprite(Sprites.ShieldTexture);
            ShieldSprite.Colour = Color.Turquoise;
            ShieldSprite.Colour.A = 0;
            ActivateShield();

            float shipSize = (Sprite.SourceRect.Width > Sprite.SourceRect.Height ? Sprite.SourceRect.Width : Sprite.SourceRect.Height);
            shipSize *= 1.4f;

            ShieldSprite.Scale = shipSize / ShieldSprite.Width;
            ShieldRadius = ShieldSprite.Width * ShieldSprite.Scale;
        }

        public void ActivateShield()
        {
            ShieldSprite.BeginFadeEffect(100, 2000);
            IsShieldActive = true;
        }

        public void DeactivateShield()
        {
            ShieldSprite.BeginFadeEffect(0, 2000);
            IsShieldActive = false;
            ShieldCooldown = 2000;
        }

        public void TakeDamage(float amount)
        {
            var shieldDamage = amount;
            var armourDamage = 0.0f;

            if (shieldDamage > CurrentShieldHP)
            {
                armourDamage = shieldDamage - CurrentShieldHP;
                shieldDamage = CurrentShieldHP;
            }

            CurrentShieldHP -= shieldDamage;
            CurrentArmourHP -= armourDamage;
        }

        public virtual void Update(GameTime gameTime)
        {
            var delta = gameTime.DeltaTime();

            ShieldSprite.Update(gameTime);
            StateMachine.Update(gameTime);

            Position += Velocity * delta;

            foreach (var weapon in Weapons)
            {
                weapon.CurrentCooldown -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (weapon.CurrentCooldown <= 0)
                    weapon.CurrentCooldown = 0;
            }

            if (!IsShieldActive)
            {
                ShieldCooldown -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (ShieldCooldown <= 0)
                {
                    ShieldCooldown = 0;
                    ActivateShield();
                }
            }
            else
            {
                if (CurrentShieldHP <= 0)
                {
                    DeactivateShield();
                }
                else
                {
                    CurrentShieldHP += CurrentShieldRegenRate * delta;
                    if (CurrentShieldHP > BaseShieldHP)
                        CurrentShieldHP = BaseShieldHP;
                }
            }

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
        } // Update

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                        Sprite.Texture,
                        Position,
                        Sprite.SourceRect,
                        (Selected ? SelectionColour : (IsPlayerShip ? Color.White : Sprites.EnemyColour)),
                        MathHelper.ToRadians(Rotation),
                        Origin,
                        Scale,
                        SpriteEffects.None,
                        0.0f
                        );

            ShieldSprite.Draw(spriteBatch, Position);
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
