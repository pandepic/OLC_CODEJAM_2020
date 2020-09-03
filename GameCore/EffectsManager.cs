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
    public class ExplosionEffect : IPoolable
    {
        public float Duration;
        public float Scale;
        public Vector2 Position;
        public Sprite Sprite;

        public bool IsAlive { get; set; }

        public void Reset()
        {
            Duration = 0;
            Scale = 1;
        }
    }

    public class EffectsManager
    {
        public TexturePackerSprite ExplosionSprite;
        public List<ExplosionEffect> DeadExplosionEffects = new List<ExplosionEffect>();

        public ObjectPool<ExplosionEffect> ExplosionEffects = new ObjectPool<ExplosionEffect>(1000, true);

        public EffectsManager()
        {
            ExplosionSprite = TexturePacker.GetSprite("ParticlesAtlas", "BigFlare");
        }

        public void Update(GameTime gameTime)
        {
            for (var i = 0; i <= ExplosionEffects.LastActiveIndex; i++)
            {
                var explosion = ExplosionEffects[i];
                explosion.Sprite.Update(gameTime);
                explosion.Duration -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (explosion.Duration <= 1000)
                    explosion.Sprite.BeginFadeEffect(0, 1000);
                else if (explosion.Duration <= 0)
                    DeadExplosionEffects.Add(explosion);
            }

            foreach (var e in DeadExplosionEffects)
                ExplosionEffects.Delete(e);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (var i = 0; i <= ExplosionEffects.LastActiveIndex; i++)
            {
                var explosion = ExplosionEffects[i];
                explosion.Sprite.Draw(spriteBatch, explosion.Position);
            }
        }

        public void AddExplosion(Ship ship)
        {
            var scaleBy = (float)ship.Sprite.SourceRect.Width;
            if (ship.Sprite.SourceRect.Height > scaleBy)
                scaleBy = (float)ship.Sprite.SourceRect.Height;

            var newExplosion = ExplosionEffects.New();
            newExplosion.Sprite = new Sprite(ExplosionSprite.Texture);
            newExplosion.Duration = 2000;
            newExplosion.Scale = ((float)ExplosionSprite.SourceRect.Width / scaleBy) * 1.2f;
            newExplosion.Position = ship.Position;

            newExplosion.Sprite.SourceRect = ExplosionSprite.SourceRect;
            newExplosion.Sprite.Center = new Vector2(ExplosionSprite.SourceRect.Width / 2, ExplosionSprite.SourceRect.Height / 2);
            newExplosion.Sprite.Colour = Color.OrangeRed;
            newExplosion.Sprite.Colour.A = 0;

            newExplosion.Sprite.BeginFadeEffect(200, 1000);
        }
    }
}
