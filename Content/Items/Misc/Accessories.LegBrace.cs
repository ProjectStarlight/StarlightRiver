using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
	public class LegBrace : SmartAccessory
	{
		//TODO: this
		public override string Texture => AssetDirectory.MiscItem + Name;
		public LegBrace() : base("Leg Brace", "NaN") { }

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<PulseBoots>());
			recipe.AddIngredient(ModContent.ItemType<ShockAbsorber>());
			recipe.AddIngredient(ItemID.FrogLeg);
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
	}
}