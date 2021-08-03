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
            item.rare = ItemRarityID.Orange;
            item.width = 32;
            item.height = 32;
        }
    }
}
