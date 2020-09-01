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
            base.Draw(spriteBatch, Color.White);
        }
    }
}
