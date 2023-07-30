using StarlightRiver.Content.Configs;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;

namespace StarlightRiver.Core.Systems
{
	internal class MaterialIndicatorSystem : ModSystem
	{
		public static List<int> slrMaterials = new();

		public override void PostSetupRecipes()
		{
			for (int i = 1; i < ItemLoader.ItemCount; i++)
			{
				Item item = ContentSamples.ItemsByType[i];

				if (IsMaterialInSLR(item))
					slrMaterials.Add(item.type);
			}
		}

		private bool IsMaterialInSLR(Item item)
		{
			if (item.ModItem is not null) // Will only add indicator to vanilla items
				return false;

			for (int i = 0; i < Main.recipe.Length; i++)
			{
				Recipe recipe = Main.recipe[i];

				if (recipe is not null && recipe.Mod == StarlightRiver.Instance && (recipe.HasIngredient(item.type) || recipe.acceptedGroups.Exists(x => GetRecipeGroup(x).ValidItems.Contains(item.type))))
					return true;
			}

			return false;
		}

		private RecipeGroup GetRecipeGroup(int id)
		{
			if (RecipeGroup.recipeGroups.TryGetValue(id, out RecipeGroup value))
				return value;

			return null;
		}
	}

	internal class AddIndicatorToTooltips : GlobalItem
	{
		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			if (Main.gameMenu)
				return;

			if (!ModContent.GetInstance<GUIConfig>().AddMaterialIndicator)
				return;

			if (!MaterialIndicatorSystem.slrMaterials.Contains(item.type))
				return;

			TooltipLine materialLine = tooltips.FirstOrDefault(x => x.Name == "Material");

			if (materialLine is not null)
				materialLine.Text += $" [i/s1:{ModContent.ItemType<MaterialIndicatorIcon>()}]";
		}
	}

	internal class MaterialIndicatorIcon : ModItem
	{
		public override string Texture => AssetDirectory.GUI + "MaterialIndicator";

		public override void SetStaticDefaults()
		{
			// ItemID.Sets.Deprecated[Item.type] = true; // This is what vanilla uses for items like SleepingIcon, but sadly chat tags can't be used with deprecated items
			ItemID.Sets.ItemsThatShouldNotBeInInventory[Item.type] = true;
		}
	}
}