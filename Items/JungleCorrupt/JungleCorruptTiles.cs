using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.JungleCorrupt
{
    public class WallJungleCorruptItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Corrupt junge grass wall");
        }

        public override void SetDefaults()
        {
            item.width = 16;
            item.height = 16;
            item.maxStack = 999;
            item.useTurn = true;
            item.autoReuse = true;
            item.useAnimation = 15;
            item.useTime = 10;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.consumable = true;
            item.createWall = WallType<Tiles.JungleCorrupt.WallJungleCorrupt>();
        }
    }
}