using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Tiles;
using StarlightRiver.Content.Tiles.Balanced;

namespace StarlightRiver.Content.Items.Balanced
{
    public class OreEbony : QuickTileItem
    {
        public OreEbony() : base("Ebony Ore", "Heavy and impure", "OreEbony", ItemRarityID.Blue, AssetDirectory.BalancedItem) { }
    }

    public class OreIvory : QuickMaterial//Since this cant be placed
    {
        public OreIvory() : base("Ivory Ore", "Light and pure", 999, 1000, ItemRarityID.LightRed, AssetDirectory.BalancedItem) { }
    }
}