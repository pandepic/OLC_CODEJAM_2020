using GameCore.AI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore.Entities
{
    public class Carrier : Ship
    {
        public int FighterHangar, BomberHangar;
        public float FighterBuildTime, BomberBuildTime;
        public ShipTypeData FighterData, BomberData;

        public List<Fighter> Fighters = new List<Fighter>();
        public List<Bomber> Bombers = new List<Bomber>();

        public Carrier(Ship owner, Vector2 position)
        {
            Owner = owner;
            ShipType = ShipType.Carrier;
            Position = position;

            LoadData();

            FighterHangar = int.Parse(SpecialAttributes["FighterHangar"]);
            BomberHangar = int.Parse(SpecialAttributes["BomberHangar"]);

            FighterData = EntityData.ShipTypes[ShipType.Fighter];
            BomberData = EntityData.ShipTypes[ShipType.Bomber];
            FighterBuildTime = FighterData.BuildTime;
            BomberBuildTime = BomberData.BuildTime;

            AIHelper.SetupBigWarshipStates(this);
        } // Carrier

        public override void Update(GameTime gameTime)
        {
            if (Fighters.Count < FighterHangar)
            {
                FighterBuildTime -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (FighterBuildTime <= 0)
                {
                    var newFighter = GameplayState.UnitManager.SpawnShip(ShipType.Fighter, Position + new Vector2(WorldData.RNG.Next(-50, 50), WorldData.RNG.Next(-50, 50)), this);
                    newFighter.IsSelectable = false;
                    Fighters.Add((Fighter)newFighter);
                    
                    FighterBuildTime = FighterData.BuildTime;
                }
            }

            if (Bombers.Count < BomberHangar)
            {
                BomberBuildTime -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (BomberBuildTime <= 0)
                {
                    var newBomber = GameplayState.UnitManager.SpawnShip(ShipType.Bomber, Position + new Vector2(WorldData.RNG.Next(-50, 50), WorldData.RNG.Next(-50, 50)), this);
                    newBomber.IsSelectable = false;
                    Bombers.Add((Bomber)newBomber);
                    
                    BomberBuildTime = BomberData.BuildTime;
                }
            }

            AIHelper.BigWarshipAI(this, gameTime);
            base.Update(gameTime);
        } // Update

        public void ReparentChildren()
        {
            foreach (var fighter in Fighters)
            {
                fighter.Owner = Owner;

                if (fighter.Owner == null)
                    fighter.Stance = ShipStance.Aggressive;
                else
                    fighter.Stance = ShipStance.Defensive;

                fighter.IsSelectable = true;
            }

            foreach (var bomber in Bombers)
            {
                bomber.Owner = Owner;

                if (bomber.Owner == null)
                    bomber.Stance = ShipStance.Aggressive;
                else
                    bomber.Stance = ShipStance.Defensive;

                bomber.IsSelectable = true;
            }
        } // ReparentChildren
    }
}
