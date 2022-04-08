using StarlightRiver.Core;
using Terraria;
using Terraria.DataStructures;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Interactive
{
	public class StaminaOrbItem : QuickTileItem
    {
        public StaminaOrbItem() : base("Stamina Orb", "Pass through this to gain stamina!\n5 second cooldown", TileType<StaminaOrb>(), 8, AssetDirectory.InteractiveTile) { }
            
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault(Itemname);
            Tooltip.SetDefault(Itemtooltip);
        }
    }
}