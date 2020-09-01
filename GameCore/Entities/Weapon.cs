using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore.Entities
{
    public class Weapon
    {
        public WeaponType Type;
        public float Range;
        public float Cooldown;
        public float Damage;

        public float CurrentCooldown;
    }
}
