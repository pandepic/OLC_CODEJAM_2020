using PandaMonogame.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore
{
    public enum UpgradeType
    {
        MinerCap1, // +5 Max Miners
        MinerCap2, // +10 Max Miners
        MiningRate1, // +5 Mining Rate
        MiningRate2, // +5 Mining Rate
        RepairRate, // +10 Repair Rate
        ShieldRegen1, // +5 Shield Regen
        ShieldRegen2, // +10 Shield Regen
        Warmachine1, // + Warmachine beams
        Warmachine2, // + Warmachine missiles
        Hyperdrive, // Fix Hyperdrive (Victory)
    }

    public class UpgradeManager
    {
        public Dictionary<UpgradeType, int> UpgradeCosts = new Dictionary<UpgradeType, int>()
        {
            { UpgradeType.MinerCap1, 100 },
            { UpgradeType.MinerCap2, 500 },
            { UpgradeType.MiningRate1, 200 },
            { UpgradeType.MiningRate2, 500 },
            { UpgradeType.RepairRate, 200 },
            { UpgradeType.ShieldRegen1, 200 },
            { UpgradeType.ShieldRegen2, 500 },
            { UpgradeType.Warmachine1, 500 },
            { UpgradeType.Warmachine2, 1000 },
            { UpgradeType.Hyperdrive, 2000 },
        };

        public Dictionary<UpgradeType, bool> UpgradesUnlocked = new Dictionary<UpgradeType, bool>()
        {
            { UpgradeType.MinerCap1, false },
            { UpgradeType.MinerCap2, false },
            { UpgradeType.MiningRate1, false },
            { UpgradeType.MiningRate2, false },
            { UpgradeType.RepairRate, false },
            { UpgradeType.ShieldRegen1, false },
            { UpgradeType.ShieldRegen2, false },
            { UpgradeType.Warmachine1, false },
            { UpgradeType.Warmachine2, false },
            { UpgradeType.Hyperdrive, false },
        };

        public Dictionary<UpgradeType, PUIWBasicButton> UpgradeButtons;

        #region menu button methods
        public void UpgradeMinerCap1(params object[] args)
        {
            UnlockUpgrade(UpgradeType.MinerCap1);
        }

        public void UpgradeMinerCap2(params object[] args)
        {
            UnlockUpgrade(UpgradeType.MinerCap2);
        }

        public void UpgradeMiningRate1(params object[] args)
        {
            UnlockUpgrade(UpgradeType.MiningRate1);
        }

        public void UpgradeMiningRate2(params object[] args)
        {
            UnlockUpgrade(UpgradeType.MiningRate2);
        }

        public void UpgradeRepairRate(params object[] args)
        {
            UnlockUpgrade(UpgradeType.RepairRate);
        }

        public void UpgradeShieldRegen1(params object[] args)
        {
            UnlockUpgrade(UpgradeType.ShieldRegen1);
        }

        public void UpgradeShieldRegen2(params object[] args)
        {
            UnlockUpgrade(UpgradeType.ShieldRegen2);
        }

        public void UpgradeWarmachine1(params object[] args)
        {
            UnlockUpgrade(UpgradeType.Warmachine1);
        }

        public void UpgradeWarmachine2(params object[] args)
        {
            UnlockUpgrade(UpgradeType.Warmachine2);
        }

        public void UpgradeHyperdrive(params object[] args)
        {
            UnlockUpgrade(UpgradeType.Hyperdrive);
        }
        #endregion

        public int UpgradePoints;

        public int BonusMaxMiners;
        public int BonusMiningRate;
        public float BonusRepairRate;

        public UpgradeManager()
        {
        }

        public void Load()
        {
            UpgradeButtons = new Dictionary<UpgradeType, PUIWBasicButton>();

            foreach (var kvp in UpgradeCosts)
            {
                var button = GameplayState.Menu.GetWidget<PUIWBasicButton>("btnUpgrade" + kvp.Key.ToString());
                button.SetTooltip(kvp.Value.ToString() + " points", kvp.Value.ToString() + " points" + (kvp.Key == UpgradeType.Hyperdrive ? "\nMust unlock all other upgrades." : ""));
                UpgradeButtons.Add(kvp.Key, button);
            }
        }

        public void UnlockUpgrade(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.Hyperdrive:
                    {
                        var otherUpgrades = true;

                        foreach (var kvp in UpgradesUnlocked)
                        {
                            if (kvp.Key != UpgradeType.Hyperdrive && kvp.Value == false)
                                otherUpgrades = false;
                        }

                        if (!otherUpgrades)
                            return;

                        // TODO : WIN!
                    }
                    break;
            }

            UpgradesUnlocked[type] = true;
            UpgradeButtons[type].Visible = false;
            UpgradeButtons[type].Active = false;
        }
    }
}
