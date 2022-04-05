using StarlightRiver.Content.Tiles.Crafting;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

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
            Recipe recipe = CreateRecipe(3);
            recipe.AddIngredient(ItemID.Daybloom, 1);
            recipe.AddIngredient(ItemID.GrassSeeds, 1);
            recipe.AddIngredient(ItemType<Ivy>(), 5);
            //recipe.AddTile(TileType<HerbStation>());
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
            Recipe recipe = CreateRecipe(3);
            recipe.AddIngredient(ItemID.Deathweed, 1);
            recipe.AddIngredient(ItemID.Shiverthorn, 1);
            recipe.AddIngredient(ItemType<Deathstalk>(), 5);
            //recipe.AddTile(TileType<HerbStation>());
        }
    }
}