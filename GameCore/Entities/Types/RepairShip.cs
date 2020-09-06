using GameCore.AI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore.Entities
{
    public class RepairShip : Ship
    {
        public float RepairRate;

        public RepairShip(Ship owner, Vector2 position)
        {
            Owner = owner;
            ShipType = ShipType.RepairShip;
            Position = position;
            DefendTarget = Owner;
            Stance = ShipStance.Passive;

            LoadData();

            RepairRate = float.Parse(SpecialAttributes["RepairRate"]) + GameplayState.UpgradeManager.BonusRepairRate;

            StateMachine.RegisterState(new ShipFollowingState(this));
            StateMachine.RegisterState(new ShipFollowPositionState(this));
            StateMachine.RegisterState(new RepairShipRepairState(this));
            StateMachine.RegisterState(new ShipIdleState(this));

            StateMachine.Start<ShipIdleState>();
        }

        public override void Update(GameTime gameTime)
        {
            switch (StateMachine.CurrentState)
            {
                case ShipFollowingState following:
                    {
                        if (following.Target.IsDead)
                        {
                            StateMachine.SetState<ShipIdleState>();
                        }

                        ScanForTarget(gameTime);
                    }
                    break;

                case ShipFollowPositionState followPosition:
                    {
                        ScanForTarget(gameTime);
                    }
                    break;

                case ShipIdleState idle:
                    {
                        AIHelper.BigSetFollowState(this);
                    }
                    break;

                case RepairShipRepairState repair:
                    {
                        if (repair.Target.IsDead || repair.Target.CurrentArmourHP >= repair.Target.BaseArmourHP)
                        {
                            AIHelper.BigSetFollowState(this);
                        }
                    }
                    break;
            }

            base.Update(gameTime);
        } // Update

        public void ScanForTarget(GameTime gameTime)
        {
            NextDefendScan -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (NextDefendScan <= 0)
            {
                NextDefendScan = 0;

                var newTarget = AIHelper.FindClosestDamagedFriend(this);

                if (newTarget != null)
                {
                    var repair = StateMachine.GetState<RepairShipRepairState>();
                    repair.Target = newTarget;
                    repair.RepairRate = RepairRate;
                    StateMachine.SetState(repair);
                }
                else
                {
                    NextDefendScan = DefendScanFrequency;
                }
            }
        } // ScanForTarget

        public void SetRepairTarget(Ship target)
        {
            var repair = StateMachine.GetState<RepairShipRepairState>();
            repair.Target = target;
            repair.RepairRate = RepairRate;
            StateMachine.SetState(repair);
        }
    }
}
