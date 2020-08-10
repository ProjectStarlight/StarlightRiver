using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.Items.Herbology.Materials;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Crafting
{
    public class OvenItem : QuickTileItem
    {
        public OvenItem() : base("Oven", "Used to bake items", TileType<Tiles.Crafting.Oven>(), 0)
        {
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.StoneBlock, 20);
            recipe.AddIngredient(ItemID.Gel, 5);
            recipe.AddIngredient(ItemID.Wood, 10);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    public class HerbStationItem : QuickTileItem
    {
        public HerbStationItem() : base("Herbologist's Bench", "Used to refine herbs", TileType<Tiles.Crafting.HerbStation>(), 0)
        {
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Wood, 20);
            recipe.AddIngredient(ItemType<Ivy>());
            recipe.AddIngredient(ItemID.Bottle, 1);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    public class CookStationItem : QuickTileItem
    {
        public CookStationItem() : base("Prep Station", "Right click to prepare meals", TileType<Tiles.Crafting.CookStation>(), 0)
        {
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Wood, 20);
            recipe.AddIngredient(RecipeGroupID.IronBar, 5);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}