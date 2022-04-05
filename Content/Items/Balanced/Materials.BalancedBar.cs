using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Balanced
{
	public class EbonyBar : QuickTileItem
    {
        public EbonyBar() : base("Ebony Bar", "Soft and heavy", "EbonyBar", ItemRarityID.Blue, AssetDirectory.BalancedItem) { } //TODO: Fix place type

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemType<OreEbony>(), 4);
            recipe.AddTile(TileID.Furnaces);
        }
    }

    public class IvoryBar : QuickTileItem
    {
        public IvoryBar() : base("Ivory Bar", "Hard and light", "IvoryBar", ItemRarityID.Blue, AssetDirectory.BalancedItem) { } //TODO: Fix place type

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemType<OreIvory>(), 3);
            recipe.AddTile(TileID.Furnaces);
        }
    }
}