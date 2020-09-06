using GameCore.AI;
using Microsoft.Xna.Framework;
using PandaMonogame.Assets;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore.Entities
{
    public class Miner : Ship
    {
        public Inventory Inventory;
        public Asteroid CurrentMiningTarget = null;
        public int GatherRate;

        public Miner(Ship owner, Vector2 position)
        {
            Owner = owner;
            ShipType = ShipType.Miner;
            Position = position;

            LoadData();

            GatherRate = int.Parse(SpecialAttributes["GatherRate"]);

            Inventory = new Inventory();

            StateMachine.RegisterState(new ShipIdleState(this));
            StateMachine.RegisterState(new MinerTravelingState(this));
            StateMachine.RegisterState(new MinerMiningState(this));
            StateMachine.RegisterState(new MinerReturningState(this));
            StateMachine.RegisterState(new MinerDepositingState(this));
            StateMachine.RegisterState(new ShipFollowingState(this));
            StateMachine.RegisterState(new ShipPatrolFollowState(this));
            StateMachine.RegisterState(new ShipPatrolPositionState(this));

            var patrolFollow = StateMachine.GetState<ShipPatrolFollowState>();
            patrolFollow.Target = Owner;
            StateMachine.Start<ShipPatrolFollowState>();
        }
    }
}
