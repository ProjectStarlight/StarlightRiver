using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Vitric
{
    public class VitricOre : QuickMaterial
    {
        public VitricOre() : base("Vitric Ore", "", 999, 200, 2)
        {
        }
    }

    public class VitricGem : QuickMaterial
    {
        public VitricGem() : base("Vitric Gem", "Many Facters Shimmer Within", 250, 500, 2)
        {
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<VitricOre>(), 8);
            recipe.AddTile(TileID.Furnaces);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}