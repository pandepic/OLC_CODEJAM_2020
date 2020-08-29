using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PandaMonogame;
using PandaMonogame.Assets;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore.Entities
{
    public class Player : Ship
    {
        public Player()
        {
            Sprite = TexturePacker.GetSprite("ShipsAtlas", "Station2");
            Origin = new Vector2(Sprite.SourceRect.Width / 2, Sprite.SourceRect.Height / 2);
        }
    }
}
