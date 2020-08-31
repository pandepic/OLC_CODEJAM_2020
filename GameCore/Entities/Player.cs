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
        public ShipType Type;
        public float Duration;

        public override string ToString()
        {
            return Type.ToString() + ": " + (Duration / 1000.0f).ToString("0.0");
        }
    }

    public class Player : Ship
    {
        public Queue<BuildQueueItem> BuildQueue;
        public Inventory Inventory;

        public UnitManager UnitManager;

        public Player()
        {
            Type = ShipType.HomeShip;

            LoadData();

            BuildQueue = new Queue<BuildQueueItem>();
            Inventory = new Inventory();

            StateMachine.RegisterState(new ShipIdleState(this));
            StateMachine.RegisterState(new PlayerTravelingState(this));

            StateMachine.Start<ShipIdleState>();
        }

        public override void Update(GameTime gameTime)
        {
            if (BuildQueue.Count > 0)
            {
                var nextItem = BuildQueue.Peek();
                nextItem.Duration -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (nextItem.Duration <= 0)
                {
                    BuildQueue.Dequeue();
                    UnitManager.SpawnShip(nextItem.Type, Position, this);
                }
            }

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if (BuildQueue.Count > 0)
                spriteBatch.DrawString(Sprites.DefaultFont, BuildQueue.Peek().ToString(), Position + new Vector2(-200, -200), Color.White);
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
                Type = type,
                Duration = data.BuildTime
            };

            BuildQueue.Enqueue(newItem);

            return true;
        }
    }
}
