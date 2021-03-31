using StarlightRiver.Content.ArmorEnchantment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace StarlightRiver.Core.Loaders
{
    class ArmorEnchantLoader : ILoadable
    {
        public static List<ArmorEnchantment> Enchantments = new List<ArmorEnchantment>();

        public float Priority => 1.0f;

        public void Load()
        {
            Mod mod = StarlightRiver.Instance;

            foreach (Type t in mod.Code.GetTypes())
            {
                if (t.IsSubclassOf(typeof(ArmorEnchantment)))
                {
                    Enchantments.Add((ArmorEnchantment)Activator.CreateInstance(t));
                }
            }
        }

        public void Unload()
        {
            Enchantments.Clear();
        }
    }
}
