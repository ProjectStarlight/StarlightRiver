using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Tiles.Herbology;

namespace StarlightRiver.Items.Herbology
{
    public class GreenhouseGlassItem : QuickTileItem
    {
        public GreenhouseGlassItem() : base("Greenhouse Glass", "Speeds up the growth the plant below it\nNeeds a clear area above it", TileType<GreenhouseGlass>(), 1, AssetDirectory.BrewingItem) { }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Glass, 10);
            recipe.AddIngredient(ItemType<Content.Items.AstralMeteor.AluminumBar>(), 1);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this, 10);
            recipe.AddRecipe();
        }
    }
}