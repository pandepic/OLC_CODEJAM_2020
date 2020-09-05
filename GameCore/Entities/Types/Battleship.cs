using GameCore.AI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore.Entities
{
    public class Battleship : Ship
    {
        public Battleship(Ship owner, Vector2 position)
        {
            Owner = owner;
            ShipType = ShipType.Battleship;
            Position = position;

            LoadData();
            AIHelper.SetupBigWarshipStates(this);
        }

        public override void Update(GameTime gameTime)
        {
            AIHelper.BigWarshipAI(this, gameTime);
            base.Update(gameTime);
        }
    }
}
