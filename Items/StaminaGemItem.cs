using Terraria;
using Terraria.DataStructures;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items
{
    public class StaminaOrbItem : QuickTileItem
    {
        public StaminaOrbItem() : base("Stamina Orb", "Pass through this to gain stamina!\n5 second cooldown", TileType<Tiles.Interactive.StaminaOrb>(), 8)
        {
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault(Itemname);
            Tooltip.SetDefault(Itemtooltip);
            Main.RegisterItemAnimation(item.type, new DrawAnimationVertical(3, 6));
        }
    }
}