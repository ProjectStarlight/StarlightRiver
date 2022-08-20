using StarlightRiver.Core.Systems.ChestLootSystem;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Dungeon
{
    public class DungeonPool : LootPool
    {
        public override void AddLoot()
        {
            AddItem(ModContent.ItemType<AquaSapphire>(), ChestRegionFlags.Dungeon, 0.2f, 2, true, 1);
        }
    }
}