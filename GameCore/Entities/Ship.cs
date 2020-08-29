using Microsoft.Xna.Framework.Graphics;
using PandaMonogame;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore.Entities
{
    public class Ship : IPoolable
    {
        public Texture2D Texture;
        public bool IsAlive { get; set; }

        public void Reset()
        {
        }
    }
}
