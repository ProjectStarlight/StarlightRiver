using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Vitric
{
    class GlassIdol : ModItem
    {
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
