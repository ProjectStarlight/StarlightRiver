using StarlightRiver.Core.Systems.ChestLootSystem;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
    public class MiscPool : LootPool
    {
        public override void AddLoot()
        {
            AddItem(ModContent.ItemType<BarbedKnife>(), ChestRegionFlags.Surface | ChestRegionFlags.Barrel);
            AddItem(ModContent.ItemType<Cheapskates>(), ChestRegionFlags.Ice);
            AddItem(ModContent.ItemType<Slitherring>(), ChestRegionFlags.Jungle, 0.20f);
            AddItem(ModContent.ItemType<SojournersScarf>(), ChestRegionFlags.Underground | ChestRegionFlags.Surface | ChestRegionFlags.Granite | ChestRegionFlags.Marble, 0.1f);
        }
    }
}