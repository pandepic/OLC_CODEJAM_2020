using GameCore.AI;
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
        public Dictionary<ResourceType, int> Inventory;

        public Player()
        {
            Sprite = TexturePacker.GetSprite("ShipsAtlas", "Station2");
            Origin = new Vector2(Sprite.SourceRect.Width / 2, Sprite.SourceRect.Height / 2);

            Inventory = new Dictionary<ResourceType, int>();
            foreach (var res in WorldData.ResourceTypes)
                Inventory.Add(res, 0);

            StateMachine.RegisterState(new ShipIdleState(this));
            StateMachine.RegisterState(new PlayerTravelingState(this));

            StateMachine.Start<ShipIdleState>();
        }
    }
}
