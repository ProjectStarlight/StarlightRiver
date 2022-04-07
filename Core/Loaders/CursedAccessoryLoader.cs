using StarlightRiver.Content.ArmorEnchantment;
using StarlightRiver.Content.Items.BaseTypes;
using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace StarlightRiver.Core.Loaders
{
	class CursedAccessoryLoader : IOrderedLoadable
    {
        public float Priority => 1f;
        public void Load()
        {
            CursedAccessory.LoadSystem();
        }

        public void Unload()
        {
            CursedAccessory.UnloadSystem();
        }
    }
}
