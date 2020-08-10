using StarlightRiver.Items.Herbology.Materials;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Herbology.Potions
{
    internal class PotionForest : QuickPotion
    {
        public PotionForest() : base("Forest Tonic", "Provides regenration and immunity to poision", 1800, BuffType<Buffs.ForestTonic>(), 2)
        {
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.BottledWater, 1);
            recipe.AddIngredient(ItemType<ForestBerries>(), 5);
            recipe.AddIngredient(ItemType<Ivy>(), 20);
            recipe.AddTile(TileType<Tiles.Crafting.HerbStation>());
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}