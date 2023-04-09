using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using System.Collections.Generic;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	public class MartialBook : SmartAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public MartialBook() : base("Martial Encyclopedia", "Combined effects of Tiger, Snake, and Mantis Techniques\n+10% melee critical damage\nQuit, don't quit... Noodles, don't noodles.") { }

		public override List<int> ChildTypes =>
			new()
			{
				ModContent.ItemType<SwordBook>(),
				ModContent.ItemType<SpearBook>(),
				ModContent.ItemType<AxeBook>()
			};

		public override void SafeUpdateEquip(Player Player)
		{
			Player.GetModPlayer<CritMultiPlayer>().MeleeCritMult += 0.1f;
		}

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Orange;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();

			recipe.AddIngredient(ModContent.ItemType<SwordBook>());
			recipe.AddIngredient(ModContent.ItemType<SpearBook>());
			recipe.AddIngredient(ModContent.ItemType<AxeBook>());

			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
	}
}
