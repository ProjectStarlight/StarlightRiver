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
            AddItem(ModContent.ItemType<Sling>(), ChestRegionFlags.Surface | ChestRegionFlags.Barrel);
            AddItem(ModContent.ItemType<BarbedKnife>(), ChestRegionFlags.Surface | ChestRegionFlags.Barrel);
            AddItem(ModContent.ItemType<Cheapskates>(), ChestRegionFlags.Ice);
            AddItem(ModContent.ItemType<Slitherring>(), ChestRegionFlags.Jungle, 0.20f);
            AddItem(ModContent.ItemType<SojournersScarf>(), ChestRegionFlags.Underground | ChestRegionFlags.Surface | ChestRegionFlags.Granite | ChestRegionFlags.Marble, 0.1f);
            AddItem(ModContent.ItemType<ArchaeologistsMap>(), ChestRegionFlags.Underground | ChestRegionFlags.Ice | ChestRegionFlags.Desert | ChestRegionFlags.Marble | ChestRegionFlags.Granite | ChestRegionFlags.Jungle | ChestRegionFlags.JungleShrine | ChestRegionFlags.Underworld | ChestRegionFlags.JungleShrine | ChestRegionFlags.Mushroom | ChestRegionFlags.TrappedUnderground, 0.2f, 1, false, -1);
        }
    }
}