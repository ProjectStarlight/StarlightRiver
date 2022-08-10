using StarlightRiver.Core.Systems.ChestLootSystem;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Desert
{
    public class AnkhPool : LootPool
    {
        public override void AddLoot()
        {
            AddItem(ModContent.ItemType<DefiledAnkh>(), ChestRegionFlags.Ankh, 1, 1, true, 0);
        }
    }
}