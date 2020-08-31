using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore.Entities
{
    public class Inventory
    {
        public Dictionary<ResourceType, int> Resources;

        public Inventory()
        {
            Resources = new Dictionary<ResourceType, int>();
            foreach (var res in WorldData.ResourceTypes)
                Resources.Add(res, 0);
        }

        public void AddResource(ResourceType type, int amount)
        {
            Resources[type] += amount;
        }

        public int ResourceAmount(ResourceType type)
        {
            return Resources[type];
        }

        public void GiveResource(ResourceType type, int amount, Inventory target)
        {
            if (Resources[type] < amount)
                amount = Resources[type];

            target.Resources[type] += amount;
            Resources[type] -= amount;
        }

        public void GiveAll(Inventory target)
        {
            foreach (var kvp in Resources)
            {
                target.AddResource(kvp.Key, kvp.Value);
            }
        }

        public void AddAll(int amount)
        {
            foreach (var kvp in Resources)
            {
                Resources[kvp.Key] = kvp.Value + amount;
            }
        }
    }
}
