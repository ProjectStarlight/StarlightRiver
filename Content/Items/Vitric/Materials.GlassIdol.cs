using StarlightRiver.Core;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Vitric
{
	class GlassIdol : ModItem
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Used to worship a powerful guardian.");
        }

        public override void SetDefaults()
        {
            Item.rare = ItemRarityID.Orange;
            Item.maxStack = 20;
            Item.width = 32;
            Item.height = 32;
        }
    }

    class GlassIdolEndless : ModItem
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Gilded Glass Idol");
            Tooltip.SetDefault("Used to worship a powerful guardian.\nInfinite uses");
        }

        public override void SetDefaults()
        {
            Item.rare = ItemRarityID.Orange;
            Item.width = 32;
            Item.height = 32;
        }
    }
}
