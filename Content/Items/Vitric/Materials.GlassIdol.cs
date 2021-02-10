using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

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

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<VitricGem>(), 3);
            recipe.AddIngredient(ItemID.Sandstone, 20);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}
