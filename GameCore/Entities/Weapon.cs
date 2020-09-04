using Microsoft.Xna.Framework;
using PandaMonogame.Assets;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore.Entities
{
    public class Weapon
    {
        public Ship Parent;

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

        //public TexturePackerSprite TurretSprite;
        //public Vector2 TurretPosition, TurretOrigin;
        //public float TurretRotation;

        public void ResetCooldown()
        {
            CurrentCooldown = Cooldown + WorldData.RNG.Next(-25, 25);
        }
    }
}
