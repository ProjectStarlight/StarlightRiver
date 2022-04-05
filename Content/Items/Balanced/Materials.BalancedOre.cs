using StarlightRiver.Core;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Balanced
{
	public class OreEbony : QuickTileItem
    {
        public OreEbony() : base("OreEbony", "Ebony Ore", "Heavy and impure", "OreEbony", ItemRarityID.Blue, AssetDirectory.BalancedItem) { }
    }

    public class OreIvory : QuickMaterial//Since this cant be placed
    {
        public OreIvory() : base("OreIvory", "Ivory Ore", "Light and pure", 999, 1000, ItemRarityID.LightRed, AssetDirectory.BalancedItem) { }
    }
}