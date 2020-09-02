using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore.Entities
{
    public class Weapon
    {
        public string ProjectileType;
        public TargetType TargetType;
        public float Range;
        public float Cooldown;
        public float Damage;
        public float MaxAngle;

        public float CurrentCooldown;
    }
}
