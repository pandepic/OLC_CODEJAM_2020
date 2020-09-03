using GameCore.AI;
using GameCore.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PandaMonogame;
using PandaMonogame.Assets;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore.Combat
{
    public class Projectile : Entity
    {
        public string ProjectileType = "";
        public ProjectileDeathType DeathType = ProjectileDeathType.None;
        public TargetType TargetType;
        public float Lifetime;
        public bool IsPlayerProjectile;
        public float Damage;
        public Color Colour;
        public Ship Target;

        public Projectile() { }

        public void SetType(string type)
        {
            ProjectileType = type;

            var data = EntityData.ProjectileTypes[ProjectileType];
            DeathType = data.DeathType;
            MoveSpeed = data.MoveSpeed;
            TurnSpeed = data.TurnSpeed;
            Lifetime = data.Lifetime;
            Colour = data.Colour;  
            Sprite = TexturePacker.GetSprite("ParticlesAtlas", data.Sprite);
            Scale = data.Scale;
        }

        public override void Reset()
        {
            Rotation = 0;
            TargetRotation = 0;
            Position = Vector2.Zero;
            Velocity = Vector2.Zero;
            Lifetime = 0;
            ProjectileType = "";
        }
    }

    public class ProjectileManager
    {
        public ObjectPool<Projectile> Projectiles = new ObjectPool<Projectile>(20000, true);

        protected List<Projectile> DeadProjectiles = new List<Projectile>();

        public ProjectileManager() { }

        public void FireProjectile(Weapon weapon, Ship source, Ship target, float damage)
        {
            var newProjectile = Projectiles.New();
            newProjectile.SetType(weapon.ProjectileType);
            newProjectile.TargetType = target.TargetType;
            newProjectile.Damage = damage;
            newProjectile.Position = source.Position; // todo : offset by ship type
            newProjectile.IsPlayerProjectile = source.IsPlayerShip;
            newProjectile.CenterOrigin();

            // todo : lead target based on speed
            newProjectile.TargetRotation = AIHelper.GetAngleToTarget(source.Position, source.Rotation, target.Position);

            if (newProjectile.TurnSpeed == 0)
            {
                newProjectile.Rotation = newProjectile.TargetRotation;
            }
            else
            {
                newProjectile.Rotation = source.Rotation;
                newProjectile.Target = target;
                newProjectile.TargetRotation = AIHelper.GetAngleToTarget(newProjectile.Position, newProjectile.Rotation, newProjectile.Target.Position);
            }
        }

        public void Update(GameTime gameTime)
        {
            for (var i = 0; i <= Projectiles.LastActiveIndex; i++)
            {
                var projectile = Projectiles[i];
                
                projectile.Lifetime -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (projectile.Lifetime <= 0)
                {
                    DeadProjectiles.Add(projectile);
                    continue;
                }

                if (projectile.Target != null && projectile.TurnSpeed > 0)
                    projectile.TargetRotation = AIHelper.GetAngleToTarget(projectile.Position, projectile.Rotation, projectile.Target.Position);

                projectile.ApplyMovement(gameTime);

                var checkList = GameplayState.WorldManager.PlayerShips;
                if (projectile.IsPlayerProjectile)
                    checkList = GameplayState.WorldManager.EnemyShips;

                bool collision = false;

                // todo : raycasting for better collisions
                for (var s = 0; s < checkList.Count && !collision; s++)
                {
                    var target = checkList[s];

                    if (target.TargetType != projectile.TargetType)
                        continue;

                    if (target.IsShieldActive)
                        collision = Vector2.Distance(projectile.Position, target.Position) <= target.ShieldRadius;
                    else
                        collision = Vector2.Distance(projectile.Position, target.Position) <= (target.ShieldRadius * 0.8f);

                    if (collision)
                    {
                        target.TakeDamage(projectile.Damage);
                        DeadProjectiles.Add(projectile);
                    }
                }
            }

            for (var i = 0; i < DeadProjectiles.Count; i++)
            {
                var projectile = DeadProjectiles[i];

                if (projectile.DeathType == ProjectileDeathType.Explosion)
                    GameplayState.EffectsManager.AddExplosion(projectile);

                Projectiles.Delete(projectile);
            }

            DeadProjectiles.Clear();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (var i = 0; i <= Projectiles.LastActiveIndex; i++)
            {
                var projectile = Projectiles[i];
                projectile.Draw(spriteBatch, projectile.Colour);
            }
        }
    }
}
