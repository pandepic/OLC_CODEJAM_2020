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
        public TargetType TargetType;
        public float Lifetime;
        public bool IsPlayerProjectile;
        public float Damage;
        public Color Colour;

        public Projectile() { }

        public void SetType(string type)
        {
            ProjectileType = type;

            var data = EntityData.ProjectileTypes[ProjectileType];
            MoveSpeed = data.MoveSpeed;
            TurnSpeed = data.TurnSpeed;
            Lifetime = data.Lifetime;
            Colour = data.Colour;  
            Sprite = TexturePacker.GetSprite("ParticlesAtlas", data.Sprite);
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

            // todo : lead target based on speed
            newProjectile.TargetRotation = AIHelper.GetAngleToTarget(source.Position, source.Rotation, target.Position);

            if (newProjectile.TurnSpeed == 0)
                newProjectile.Rotation = newProjectile.TargetRotation;
            else
                newProjectile.Rotation = source.Rotation;

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

                    //if (target.IsShieldActive)
                    //    collision = Vector2.Distance(projectile.Position, target.Position) <= (target.ShieldRadius * 0.8f);
                    //else
                    //    collision = projectile.CollisionRect.Intersects(target.CollisionRect);

                    collision = Vector2.Distance(projectile.Position, target.Position) <= (target.ShieldRadius * 0.5f);

                    if (collision)
                    {
                        target.TakeDamage(projectile.Damage);
                        DeadProjectiles.Add(projectile);
                    }
                }
            }

            for (var i = 0; i < DeadProjectiles.Count; i++)
                Projectiles.Delete(DeadProjectiles[i]);

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
