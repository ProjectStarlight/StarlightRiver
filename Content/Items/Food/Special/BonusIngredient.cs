using StarlightRiver.Content.Items.Utility;
using System.Collections.Generic;
using System.Linq;

namespace StarlightRiver.Content.Items.Food.Special
{
	public static class FoodRecipieHandler //TODO: Maybe move this somewhere else layer?
	{
		public static List<FoodRecipie> Recipes = new();

		public static Item GetFromRecipie(FoodRecipie recipie)
		{
			var item = new Item();
			item.SetDefaults(recipie.result);
			return item;
		}
	}

	public struct FoodRecipie
	{
		public int result;
		public int mainType;
		public int sideType;
		public int sideType2;
		public int seasoningType;

		public FoodRecipie(int result, int main, int side, int side2, int seasoning)
		{
			this.result = result;
			mainType = main;
			sideType = side;
			sideType2 = side2;
			seasoningType = seasoning;
		}

		public bool Matches(List<Item> items)
		{
			FoodRecipie comparison = this; //.NET needs this for some reason???

			return items.Any(n => n.type == comparison.mainType) &&
				items.Any(n => n.type == comparison.sideType) &&
				items.Any(n => n.type == comparison.sideType2) &&
				items.Any(n => n.type == comparison.seasoningType);
		}

		public List<int> AsList()
		{
			return new List<int>()
			{
				mainType,
				sideType,
				sideType2,
				seasoningType
			};
		}
	}

	internal abstract class BonusIngredient : Ingredient
	{
		protected BonusIngredient(string tooltip) : base(tooltip, 0, IngredientType.Bonus) { }

		public abstract FoodRecipie Recipie();

		public override void AddRecipes()
		{
			FoodRecipieHandler.Recipes.Add(Recipie());
			ChefBag.specialTypes.Add(Item.type);
		}
	}
}
