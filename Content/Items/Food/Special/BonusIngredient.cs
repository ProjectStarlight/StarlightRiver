using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace StarlightRiver.Content.Items.Food.Special
{
	public static class FoodRecipieHandler //TODO: Maybe move this somewhere else layer?
	{ 
		public static List<FoodRecipie> Recipes = new List<FoodRecipie>();

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
		int mainType;
		int sideType;
		int sideType2;
		int seasoningType;

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
			var comparison = this; //.NET needs this for some reason???

			return items.Any(n => n.type == comparison.mainType) &&
				items.Any(n => n.type == comparison.sideType) &&
				items.Any(n => n.type == comparison.sideType2) &&
				items.Any(n => n.type == comparison.seasoningType);
		}
	}

	internal abstract class BonusIngredient : Ingredient
	{
		protected BonusIngredient(string tooltip) : base(tooltip, 0, IngredientType.Bonus) { }

		public abstract FoodRecipie Recipie();

		public override void AddRecipes()
		{
			FoodRecipieHandler.Recipes.Add(Recipie());
		}
	}
}
