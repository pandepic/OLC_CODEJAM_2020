using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore.Entities
{
    public class Weapon
    {
        public string ProjectileType;
        public TargetType TargetType;
        public MountType MountType;
        public float Range;
        public float Cooldown;
        public float Damage;
        public float MaxAngle;

        public float CurrentCooldown;

        public Ship Target; // used by turret mounted weapons
        public float NextTargetScan;
        public float TargetScanDuration = 200.0f;

        public void ResetCooldown()
        {
            CurrentCooldown = Cooldown + WorldData.RNG.Next(-25, 25);
        }
    }
}
