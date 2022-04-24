using StarlightRiver.Content.ArmorEnchantment;
using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace StarlightRiver.Core.Loaders
{
	class ArmorEnchantLoader : IOrderedLoadable
    {
        public static List<ArmorEnchantment> Enchantments;

        public float Priority => 1.0f;

        public void Load()
        {
            Mod Mod = StarlightRiver.Instance;

            Enchantments = new List<ArmorEnchantment>();

            foreach (Type t in Mod.Code.GetTypes())
            {
                if (t.IsSubclassOf(typeof(ArmorEnchantment)))
                {
                    Enchantments.Add((ArmorEnchantment)Activator.CreateInstance(t));
                }
            }
        }

        public void Unload()
        {
            Enchantments = null;
        }
    }
}
