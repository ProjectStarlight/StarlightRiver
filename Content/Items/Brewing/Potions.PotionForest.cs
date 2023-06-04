using StarlightRiver.Content.Tiles.Forest;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Brewing
{
	internal class PotionForest : QuickPotion
	{
		public override string Texture => "StarlightRiver/Assets/Items/Brewing/PotionForest";

		public PotionForest() : base("Forest Tonic", "Provides regeneration and immunity to poision", 1800, BuffType<Buffs.ForestTonic>(), 2) { }

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.BottledWater, 1);
			recipe.AddIngredient(ItemType<ForestBerries>(), 5);
			recipe.AddIngredient(ItemType<Ivy>(), 20);
			recipe.AddTile(TileID.Bottles);
			recipe.Register();
		}
	}
}