using GameCore.AI;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore.Entities
{
    public class Miner : Ship
    {
        public Miner(Ship owner)
        {
            Owner = owner;

            StateMachine.RegisterState(new ShipIdleState(this));
            StateMachine.RegisterState(new MinerTravelingState(this));
            StateMachine.RegisterState(new MinerMiningState(this));
            StateMachine.RegisterState(new MinerReturningState(this));
            StateMachine.RegisterState(new MinerDepositingState(this));
            StateMachine.RegisterState(new ShipFollowingState(this));
            StateMachine.RegisterState(new ShipPatrolFollowState(this));

            var patrolFollow = StateMachine.GetState<ShipPatrolFollowState>();
            patrolFollow.Target = Owner;
            StateMachine.Start<ShipPatrolFollowState>();
        }
    }
}
