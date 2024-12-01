using StarlightRiver.Content.Items.Moonstone;
using StarlightRiver.Content.Tiles.Crimson;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Vitric
{
	public class DendriteBar : QuickMaterial
	{
		public override string Texture => AssetDirectory.CrimsonItem + Name;

		public DendriteBar() : base("Dendrite Bar", "This bar might be smarter than you...", 9999, 4000, 2) { }

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe()
			.AddIngredient(ModContent.ItemType<DendriteItem>(), 4)
			.AddTile(TileID.Furnaces)
			.Register();
		}
	}

	public class ImaginaryTissue : QuickMaterial
	{
		public override string Texture => AssetDirectory.CrimsonItem + Name;

		public ImaginaryTissue() : base("Imaginary Tissue", "As long as you focus on it, its real", 9999, 10000, 4) { }
	}
}