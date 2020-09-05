using GameCore.AI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore.Entities
{
    public class MissileFrigate : Ship
    {
        public MissileFrigate(Ship owner, Vector2 position)
        {
            Owner = owner;
            ShipType = ShipType.MissileFrigate;
            Position = position;
            Stance = ShipStance.Defensive;

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
