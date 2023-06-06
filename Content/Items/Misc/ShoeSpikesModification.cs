using System.Collections.Generic;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	public class ShoeSpikesModification : GlobalItem, IPostLoadable
	{
		private static readonly List<int> ShoeSpikeAccessories = new();

		public override void ModifyTooltips(Item Item, List<TooltipLine> tooltips)
		{
			if (ItemIsDerivativeOfShoeSpikes(Item))
			{
				var tooltipLine = new TooltipLine(Mod, "StarlightRiver:ShoeSpikesInfo", "Massively increased acceleration when touching the ground");
				tooltips.Add(tooltipLine);
			}
		}

		public override void UpdateAccessory(Item Item, Player Player, bool hideVisual)
		{
			if (ItemIsDerivativeOfShoeSpikes(Item) && Player.velocity.Y == 0)
			{
				Player.runAcceleration *= 3;
				Player.runSlowdown *= 3;
			}
		}

		public override void AddRecipes()
		{
			var recipe = Recipe.Create(ItemID.ShoeSpikes);

			recipe.AddIngredient(ItemID.Silk, 15);
			recipe.AddRecipeGroup(RecipeGroupID.IronBar, 10);
			recipe.AddTile(TileID.TinkerersWorkbench);
		}

		private static bool ItemIsDerivativeOfShoeSpikes(Item item)
		{
			if (item.type == ItemID.ShoeSpikes || item.type == ItemID.ClimbingClaws || item.type == ItemID.MasterNinjaGear)
				return true;

			return ShoeSpikeAccessories.Contains(item.type);
		}

		public void PostLoad()
		{
			for (int i = 0; i < Recipe.numRecipes; i++)
			{
				Recipe recipe = Main.recipe[i];

				if (recipe.TryGetIngredient(ItemID.ShoeSpikes, out Item item))
				{
					ShoeSpikeAccessories.Add(item.type);
				}

				if (recipe.TryGetIngredient(ItemID.ClimbingClaws, out Item item2))
				{
					ShoeSpikeAccessories.Add(item2.type);
				}

				if (recipe.TryGetIngredient(ItemID.TigerClimbingGear, out Item item3))
				{
					ShoeSpikeAccessories.Add(item3.type);
				}
			}
		}

		public void PostLoadUnload()
		{
			ShoeSpikeAccessories.Clear();
		}
	}
}