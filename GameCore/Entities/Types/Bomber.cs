using GameCore.AI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore.Entities
{
    public class Bomber : Ship
    {
        public Bomber(Ship owner, Vector2 position)
        {
            Owner = owner;
            ShipType = ShipType.Bomber;
            Position = position;

            LoadData();
            AIHelper.SetupSmallAttackingShipStates(this);
        }

        public override void Update(GameTime gameTime)
        {
            AIHelper.SmallAttackingShipAI(this, gameTime);
            base.Update(gameTime);
        }
    }
}
