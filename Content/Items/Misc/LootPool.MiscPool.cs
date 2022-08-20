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
            AddItem(ModContent.ItemType<CutlassBus>(), ChestRegionFlags.Underwater);

            AddItem(ModContent.ItemType<SojournersScarf>(), ChestRegionFlags.Underground | ChestRegionFlags.Surface | ChestRegionFlags.Granite | ChestRegionFlags.Marble, 0.1f);
            AddItem(ModContent.ItemType<CoughDrops>(), ChestRegionFlags.Underground, default, 1, true, -1);
            AddItem(ModContent.ItemType<DisinfectantWipes>(), ChestRegionFlags.Underground, default, 1, true, -1);
            AddItem(ModContent.ItemType<SanitizerSpray>(), ChestRegionFlags.Underground, default, 1, true, -1);
        }
    }
}