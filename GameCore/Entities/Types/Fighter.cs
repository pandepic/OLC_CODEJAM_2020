using GameCore.AI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore.Entities
{
    public class Fighter : Ship
    {
        public Fighter(Ship owner, Vector2 position)
        {
            Owner = owner;
            ShipType = ShipType.Fighter;
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
