using GameCore.AI;
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
    public class BuildQueueItem
    {
        public ShipType ShipType;
        public float Duration;

        public override string ToString()
        {
            return ShipType.ToString() + ": " + (Duration / 1000.0f).ToString("0.0");
        }
    }

    public class Player : Ship
    {
        public List<BuildQueueItem> BuildQueue;
        public Inventory Inventory;

        public Player()
        {
            ShipType = ShipType.HomeShip;

            LoadData();
            IsPlayerShip = true;

            BuildQueue = new List<BuildQueueItem>();
            Inventory = new Inventory();

            StateMachine.RegisterState(new ShipIdleState(this));
            StateMachine.RegisterState(new PlayerTravelingState(this));

            StateMachine.Start<ShipIdleState>();
        }

        public override void Update(GameTime gameTime)
        {
            if (BuildQueue.Count > 0)
            {
                var nextItem = BuildQueue[0];
                nextItem.Duration -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (nextItem.Duration <= 0)
                {
                    BuildQueue.RemoveAt(0);
                    GameplayState.UnitManager.SpawnShip(nextItem.ShipType, Position, this);
                }
            }

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if (BuildQueue.Count > 0)
                spriteBatch.DrawString(Sprites.DefaultFont, BuildQueue[0].ToString(), Position + new Vector2(-200, -200), Color.White);
        }

        public bool BuildShip(ShipType type)
        {
            var data = EntityData.ShipTypes[type];
            
            foreach (var kvp in data.BuildCost)
            {
                if (Inventory.ResourceAmount(kvp.Key) < kvp.Value)
                    return false;
            }

            foreach (var kvp in data.BuildCost)
            {
                Inventory.AddResource(kvp.Key, -kvp.Value);
            }

            var newItem = new BuildQueueItem()
            {
                ShipType = type,
                Duration = data.BuildTime
            };

            BuildQueue.Add(newItem);

            return true;
        }
    }
}
