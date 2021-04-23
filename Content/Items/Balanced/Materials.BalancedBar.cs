using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Tiles;
using StarlightRiver.Content.Tiles.Balanced;

namespace StarlightRiver.Content.Items.Balanced
{
    public class EbonyBar : QuickTileItem
    {
        public EbonyBar() : base("Ebony Bar", "Soft and Heavy", "EbonyBar", ItemRarityID.Blue, AssetDirectory.BalancedItem) { } //TODO: Fix place type

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<OreEbony>(), 4);
            recipe.AddTile(TileID.Furnaces);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    public class IvoryBar : QuickTileItem
    {
        public IvoryBar() : base("Ivory Bar", "Hard and Light", "IvoryBar", ItemRarityID.Blue, AssetDirectory.BalancedItem) { } //TODO: Fix place type

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<OreIvory>(), 3);
            recipe.AddTile(TileID.Furnaces);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}