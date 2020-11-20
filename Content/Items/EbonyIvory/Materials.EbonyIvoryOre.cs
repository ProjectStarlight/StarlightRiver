using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.EbonyIvory
{
    public class OreEbonyItem : QuickTileItem
    {
        public OreEbonyItem() : base("Ebony Ore", "Heavy and Impure", TileType<Tiles.OreEbony>(), 1) { }
    }

    public class OreIvoryItem : QuickMaterial
    {
        public OreIvoryItem() : base("Ivory Ore", "Light and Pure", 999, 1000, 4) { }
    }

    public class BarEbony : QuickTileItem
    {
        public BarEbony() : base("Ebony Bar", "Soft and Heavy", TileType<Tiles.EbonyBar>(), 1) { }

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
        public BarIvory() : base("Ivory Bar", "Hard and Light", TileType<Tiles.IvoryBar>(), 1) { }

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