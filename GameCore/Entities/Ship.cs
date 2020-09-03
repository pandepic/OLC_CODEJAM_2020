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

        public ShipType ShipType = ShipType.None;
        public ShipStance Stance = ShipStance.Passive;
        public TargetType TargetType = TargetType.None;

        public Ship Owner = null;
        public bool IsPlayerShip = false;
        public bool IsDead = false;

        public Vector2 Destination;

        public bool Moving = false;

        public float BaseArmourHP, CurrentArmourHP;
        public float BaseShieldHP, CurrentShieldHP;
        public float BaseShieldRegenRate, CurrentShieldRegenRate;

        public SimpleStateMachine StateMachine = new SimpleStateMachine();

        public List<Weapon> Weapons = new List<Weapon>();
        public float MinWeaponRange = -1, MaxWeaponRange = -1;

        public float DefenceRadius = 2000.0f;
        public float DefendScanFrequency = 500.0f;
        public Ship DefendTarget = null;
        public Vector2? DefendPosition = null;
        public float NextDefendScan;

        public Ship EnemyTarget = null;
        public List<ShipType> TargetPriorities = new List<ShipType>();

        public bool IsSelected = false;
        public Color SelectionColour = Color.Yellow;

        public Dictionary<string, string> SpecialAttributes = new Dictionary<string, string>();

        public void LoadData()
        {
            IsPlayerShip = false;
            if (Owner != null && Owner.IsPlayerShip == true)
                IsPlayerShip = true;

            var data = EntityData.ShipTypes[ShipType];
            TargetType = data.TargetType;
            Sprite = TexturePacker.GetSprite("ShipsAtlas", data.Sprite);
            BaseMoveSpeed = data.MoveSpeed;
            BaseTurnSpeed = data.TurnSpeed;
            MoveSpeed = BaseMoveSpeed;
            TurnSpeed = BaseTurnSpeed;
            CenterOrigin();

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
                    ProjectileType = weapon.ProjectileType,
                    TargetType = weapon.TargetType,
                    Range = weapon.Range,
                    Cooldown = weapon.Cooldown,
                    Damage = weapon.Damage,
                    MaxAngle = weapon.MaxAngle,
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

            foreach (var priority in data.TargetPriorities)
                TargetPriorities.Add(priority);

            ShieldSprite = new Sprite(Sprites.ShieldTexture);
            ShieldSprite.Colour = Color.Turquoise;
            ShieldSprite.Colour.A = 0;
            ActivateShield();

            float shipSize = (Sprite.SourceRect.Width > Sprite.SourceRect.Height ? Sprite.SourceRect.Width : Sprite.SourceRect.Height);
            shipSize *= 1.4f;

            ShieldSprite.Scale = shipSize / ShieldSprite.Width;
            ShieldRadius = ((ShieldSprite.Width * 0.9f) * ShieldSprite.Scale) / 2.0f;

            SpecialAttributes = data.SpecialAttributes;
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

        public void RepairArmour(float amount)
        {
            CurrentArmourHP += amount;
            if (CurrentArmourHP > BaseArmourHP)
                CurrentArmourHP = BaseArmourHP;
        }

        public virtual void Update(GameTime gameTime)
        {
            var delta = gameTime.DeltaTime();

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

                CurrentShieldHP += CurrentShieldRegenRate * delta;
                if (CurrentShieldHP > BaseShieldHP)
                    CurrentShieldHP = BaseShieldHP;
            }

            if (Moving)
                ApplyMovement(gameTime);

            ShieldSprite.Update(gameTime);
            StateMachine.Update(gameTime);

        } // Update

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch, (IsSelected ? SelectionColour : (IsPlayerShip ? Color.White : Sprites.EnemyColour)));
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
            return AIHelper.GetAngleToTarget(Position, Rotation, target);
        }

        public void StopMovement()
        {
            Moving = false;
            Velocity = Vector2.Zero;
        }

        public void SetState(SimpleStateBase state)
        {
            StateMachine.SetState(state);
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
