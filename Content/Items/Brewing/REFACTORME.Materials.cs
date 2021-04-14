using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Tiles.Crafting;

namespace StarlightRiver.Items.Herbology.Materials
{
    public class BlendForest : QuickMaterial
    {
        public override string Texture => "StarlightRiver/Assets/Items/Brewing/BlendForest";

        public BlendForest() : base("Forest Blend", "Powdered herbs from the Forest", 999, 100, 2)
        {
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Daybloom, 1);
            recipe.AddIngredient(ItemID.GrassSeeds, 1);
            recipe.AddIngredient(mod.ItemType("Ivy"), 5);
            recipe.AddTile(TileType<HerbStation>());
            recipe.SetResult(this, 3);
            recipe.AddRecipe();
        }
    }

    public class BlendEvil : QuickMaterial
    {
        public override string Texture => "StarlightRiver/Assets/Items/Brewing/BlendEvil";

        public BlendEvil() : base("Twisted Blend", "Powdered herbs from Dark Places", 999, 100, 3)
        {
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Deathweed, 1);
            recipe.AddIngredient(ItemID.Shiverthorn, 1);
            recipe.AddIngredient(mod.ItemType("Deathstalk"), 5);
            recipe.AddTile(TileType<HerbStation>());
            recipe.SetResult(this, 3);
            recipe.AddRecipe();
        }
    }
}