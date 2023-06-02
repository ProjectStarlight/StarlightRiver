using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	public class DisinfectantKit : SmartAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public DisinfectantKit() : base("Disinfectant Kit", "Combined effects of the Disinfectant Wipes and Sanitizer Spray\n10% increased critical strike chance when a debuff is active") { }

		public override List<int> ChildTypes =>
			new()
			{
				ModContent.ItemType<DisinfectantWipes>(),
				ModContent.ItemType<SanitizerSpray>()
			};

		public override void SafeUpdateEquip(Player Player)
		{
			for (int i = 0; i < Player.MaxBuffs; i++)
			{
				if (Helper.IsValidDebuff(Player, i))
					return;
			}
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();

			recipe.AddIngredient(ModContent.ItemType<DisinfectantWipes>());
			recipe.AddIngredient(ModContent.ItemType<SanitizerSpray>());

			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
	}
}