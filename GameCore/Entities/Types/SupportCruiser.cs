﻿using GameCore.AI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore.Entities
{
    public class SupportCruiser : Ship
    {
        public SupportCruiser(Ship owner, Vector2 position)
        {
            Owner = owner;
            ShipType = ShipType.SupportCruiser;
            Position = position;

            LoadData();

            if (IsPlayerShip)
            {
                Stance = ShipStance.Defensive;
                DefendTarget = Owner;
            }
            else
            {
                Stance = ShipStance.Aggressive;
            }

            AIHelper.SetupBigWarshipStates(this);
        }

        public override void Update(GameTime gameTime)
        {
            AIHelper.BigWarshipAI(this);
            base.Update(gameTime);
        }
    }
}
