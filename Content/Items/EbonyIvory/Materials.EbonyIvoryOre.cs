using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Tiles;

namespace StarlightRiver.Content.Items.EbonyIvory
{
    public class OreEbonyItem : QuickTileItem
    {
        public OreEbonyItem() : base("Ebony Ore", "Heavy and Impure", TileType<OreEbony>(), 1, AssetDirectory.EbonyIvoryItem) { }
    }

    public class OreIvoryItem : QuickMaterial
    {
        public OreIvoryItem() : base("Ivory Ore", "Light and Pure", 999, 1000, 4, AssetDirectory.EbonyIvoryItem) { }
    }

    public class BarEbony : QuickTileItem
    {
        public BarEbony() : base("Ebony Bar", "Soft and Heavy", TileType<OreEbony>(), 1, AssetDirectory.EbonyIvoryItem) { } //TODO: Fix place type

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<OreEbonyItem>(), 4);
            recipe.AddTile(TileID.Furnaces);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    public class BarIvory : QuickTileItem
    {
        public BarIvory() : base("Ivory Bar", "Hard and Light", TileType<OreEbony>(), 1, AssetDirectory.EbonyIvoryItem) { } //TODO: Fix place type

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<OreIvoryItem>(), 4);
            recipe.AddTile(TileID.Furnaces);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}