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
    public class Asteroid : Entity
    {
        public float RotationSpeed = 0.0f;

        public ResourceType ResourceType = ResourceType.None;
        public int ResourceCount = 0;

        public void Update(GameTime gameTime)
        {
            Rotation += RotationSpeed * gameTime.DeltaTime();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                        Sprite.Texture,
                        Position,
                        Sprite.SourceRect,
                        Color.White,
                        MathHelper.ToRadians(Rotation),
                        Origin,
                        Scale,
                        SpriteEffects.None,
                        0.0f
                        );

            //if (ResourceType != ResourceType.None)
            //{
            //    Sprites.DefaultFont.Size = (int)(18.0f / GameplayState.Camera.Zoom);
            //    var resourceString = ResourceType.ToString();
            //    var resourceStringSize = Sprites.DefaultFont.MeasureString(resourceString);
            //    spriteBatch.DrawString(Sprites.DefaultFont, resourceString, Position - new Vector2(resourceStringSize.X / 2, resourceStringSize.Y / 2) - new Vector2(0, Origin.Y + 25), Color.White);
            //}
        }
    }
}
