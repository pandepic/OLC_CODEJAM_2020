using PandaMonogame;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace GameCore.Entities
{
    public struct ShipTypeData
    {
        public ShipType Type;
        public string Sprite;

        public float MoveSpeed;
        public float TurnSpeed;

        public float ArmourHP;
        public float ShieldHP;
        public float ShieldRegenRate;

        public Dictionary<ResourceType, int> BuildCost;
        public int BuildTime;

        public List<Weapon> Weapons;
    }

    public static class EntityData
    {
        public static Dictionary<ShipType, ShipTypeData> ShipTypes = new Dictionary<ShipType, ShipTypeData>();

        public static void Load()
        {
            using (var fs = ModManager.Instance.AssetManager.GetFileStreamByAsset("ShipTypes"))
            {
                var doc = XDocument.Load(fs);

                foreach (var el in doc.Root.Elements("Type"))
                {
                    var newType = new ShipTypeData()
                    {
                        Type = el.Attribute("Name").Value.ToEnum<ShipType>(),
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
                                Type = elWeapon.Attribute("Type").Value.ToEnum<WeaponType>(),
                                Range = float.Parse(elWeapon.Attribute("Range").Value),
                                Cooldown = float.Parse(elWeapon.Attribute("Cooldown").Value),
                                Damage = float.Parse(elWeapon.Attribute("Damage").Value),
                            });
                        }
                    }

                    ShipTypes.Add(newType.Type, newType);
                }
            }
        } // Load
    }
}
