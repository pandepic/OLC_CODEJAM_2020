using GameCore.Combat;
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
        public Entity Anchor;
        public Vector2 AnchorOffset;

        public bool IsAlive { get; set; }

        public void Reset()
        {
            Duration = 0;
            Scale = 1;
        }
    }

    public class EffectsManager
    {
        public const float ExplosionDuration = 800.0f;

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

                if (!explosion.Sprite.IsFading)
                    explosion.Sprite.BeginFadeEffect(0, (ExplosionDuration / 2));

                if (explosion.Duration <= 0)
                    DeadExplosionEffects.Add(explosion);
            }

            foreach (var e in DeadExplosionEffects)
                ExplosionEffects.Delete(e);

            DeadExplosionEffects.Clear();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (var i = 0; i <= ExplosionEffects.LastActiveIndex; i++)
            {
                var explosion = ExplosionEffects[i];
                var position = explosion.Position;

                if (explosion.Anchor != null)
                {
                    position = explosion.Anchor.Position + explosion.AnchorOffset;
                    //anchorOffset = explosion.AnchorOffset;// explosion.Anchor.CollisionPos - explosion.Position;
                }

                explosion.Sprite.Scale = explosion.Scale;
                explosion.Sprite.Draw(spriteBatch, position);
            }
        }

        public void AddExplosion(Entity entity, Entity anchor = null, float explosionSize = 6.0f, float duration = 800.0f)
        {
            AddExplosion(entity, Color.OrangeRed, anchor, explosionSize, duration);
        }

        public void AddExplosion(Entity entity, Color color, Entity anchor = null, float explosionSize = 6.0f, float duration = 800.0f)
        {
            var scaleBy = (float)entity.Sprite.SourceRect.Width;
            if (entity.Sprite.SourceRect.Height > scaleBy)
                scaleBy = (float)entity.Sprite.SourceRect.Height;

            scaleBy *= explosionSize;

            var newExplosion = ExplosionEffects.New();
            newExplosion.Sprite = new Sprite(ExplosionSprite.Texture);
            newExplosion.Duration = ExplosionDuration;
            newExplosion.Scale = scaleBy / (float)ExplosionSprite.SourceRect.Width;
            newExplosion.Position = entity.Position;
            newExplosion.Anchor = anchor;

            newExplosion.Sprite.SourceRect = ExplosionSprite.SourceRect;
            newExplosion.Sprite.Center = new Vector2(ExplosionSprite.SourceRect.Width / 2, ExplosionSprite.SourceRect.Height / 2);
            newExplosion.Sprite.Colour = color;
            newExplosion.Sprite.Colour.A = 120;

            if (anchor != null)
                newExplosion.AnchorOffset = newExplosion.Position - anchor.Position;

            newExplosion.Sprite.BeginFadeEffect(200, (ExplosionDuration / 2));
        }
    }
}
