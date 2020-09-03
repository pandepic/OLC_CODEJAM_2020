using Microsoft.Xna.Framework;
using PandaMonogame;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace GameCore.Entities
{
    public struct ShipTypeData
    {
        public ShipType ShipType;
        public TargetType TargetType;
        public string Sprite;

        public float MoveSpeed;
        public float TurnSpeed;

        public float ArmourHP;
        public float ShieldHP;
        public float ShieldRegenRate;

        public Dictionary<ResourceType, int> BuildCost;
        public int BuildTime;

        public List<Weapon> Weapons;
        public List<ShipType> TargetPriorities;
    }

    public struct ProjectileTypeData
    {
        public string ProjectileType;
        public string Sprite;
        public Color Colour;
        public float MoveSpeed;
        public float Lifetime;
        public float TurnSpeed;
    }

    public static class EntityData
    {
        public static Dictionary<ShipType, ShipTypeData> ShipTypes = new Dictionary<ShipType, ShipTypeData>();
        public static Dictionary<string, ProjectileTypeData> ProjectileTypes = new Dictionary<string, ProjectileTypeData>();

        public static void Load()
        {
            using (var fs = ModManager.Instance.AssetManager.GetFileStreamByAsset("ShipTypes"))
            {
                var doc = XDocument.Load(fs);

                foreach (var el in doc.Root.Elements("Type"))
                {
                    var newType = new ShipTypeData()
                    {
                        ShipType = el.Attribute("Name").Value.ToEnum<ShipType>(),
                        TargetType = el.Attribute("TargetType").Value.ToEnum<TargetType>(),
                        Sprite = el.Attribute("Sprite").Value,
                        MoveSpeed = float.Parse(el.Element("MoveSpeed").Value),
                        TurnSpeed = float.Parse(el.Element("TurnSpeed").Value),
                        ArmourHP = float.Parse(el.Element("ArmourHP").Value),
                        ShieldHP = float.Parse(el.Element("ShieldHP").Value),
                        ShieldRegenRate = float.Parse(el.Element("ShieldRegenRate").Value),
                        BuildCost = new Dictionary<ResourceType, int>(),
                        Weapons = new List<Weapon>(),
                    };

                    var elBuildCost = el.Element("BuildCost");
                    var elBuildTime = el.Element("BuildTime");

                    if (elBuildCost != null)
                    {
                        foreach (var type in WorldData.ResourceTypes)
                        {
                            var attr = elBuildCost.Attribute(type.ToString());
                            if (attr != null)
                                newType.BuildCost.Add(type, int.Parse(attr.Value));
                        }
                    }

                    if (elBuildTime != null)
                        newType.BuildTime = int.Parse(elBuildTime.Value);

                    var elWeapons = el.Element("Weapons");

                    if (elWeapons != null)
                    {
                        foreach (var elWeapon in elWeapons.Elements("Weapon"))
                        {
                            newType.Weapons.Add(new Weapon()
                            {
                                ProjectileType = elWeapon.Attribute("ProjectileType").Value,
                                TargetType = elWeapon.Attribute("TargetType").Value.ToEnum<TargetType>(),
                                Range = float.Parse(elWeapon.Attribute("Range").Value),
                                Cooldown = float.Parse(elWeapon.Attribute("Cooldown").Value),
                                Damage = float.Parse(elWeapon.Attribute("Damage").Value),
                                MaxAngle = float.Parse(elWeapon.Attribute("MaxAngle").Value),
                            });
                        }
                    }

                    newType.TargetPriorities = new List<ShipType>();

                    var elTargetPriorities = el.Element("TargetPriorities");

                    if (elTargetPriorities != null)
                    {
                        foreach (var priority in elTargetPriorities.Elements("Priority"))
                            newType.TargetPriorities.Add(priority.Attribute("Type").Value.ToEnum<ShipType>());
                    }

                    ShipTypes.Add(newType.ShipType, newType);
                }
            } // load ship types

            Console.WriteLine("Loaded ship types.");

            using (var fs = ModManager.Instance.AssetManager.GetFileStreamByAsset("ProjectileTypes"))
            {
                var doc = XDocument.Load(fs);

                foreach (var projectileType in doc.Root.Elements("Type"))
                {
                    var newType = new ProjectileTypeData()
                    {
                        ProjectileType = projectileType.Attribute("Name").Value,
                        Sprite = projectileType.Attribute("Sprite").Value,
                        Colour = PUIColorConversion.Instance.ToColor(projectileType.Attribute("Colour").Value),
                        MoveSpeed = float.Parse(projectileType.Attribute("MoveSpeed").Value),
                        TurnSpeed = float.Parse(projectileType.Attribute("TurnSpeed").Value),
                        Lifetime = float.Parse(projectileType.Attribute("Lifetime").Value),
                    };

                    ProjectileTypes.Add(newType.ProjectileType, newType);
                }
            } // load projectile types

            Console.WriteLine("Loaded projectile types.");

        } // Load
    }
}
