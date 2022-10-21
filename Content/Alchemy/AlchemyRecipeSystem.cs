using System;
using System.Collections.Generic;
using System.Linq;

namespace StarlightRiver.Content.Alchemy
{
	public static class AlchemyRecipeSystem
	{
		//static class to handle recipe loading / unloading as well as helper functions for codex integration and determining result of a list of ingredients

		public static List<AlchemyRecipe> recipeList;

		/// <summary>
		/// cache Types of all the alchemy ingredients in use, indexed by ItemId, 
		/// done this way since we want to instantiate new instances b/c there can be multiple cauldrons and we don't want to use reflection every time an ingredient needs to be created
		/// </summary>
		public static Dictionary<int, Type> allIngredientMap;

		public static void Load()
		{
			allIngredientMap = new Dictionary<int, Type>();
			recipeList = new List<AlchemyRecipe>();
			//reflection to discover and cache AlchemyIngredient override types
			Mod Mod = StarlightRiver.Instance;
			foreach (Type type in Mod.Code.GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(AlchemyIngredient)) && t != typeof(GenericAlchemyIngredient)))
			{
				var tempIngredient = (AlchemyIngredient)Activator.CreateInstance(type);
				allIngredientMap.Add(tempIngredient.GetItemID(), type);
			}
		}

		public static void Unload()
		{
			if (allIngredientMap != null)
			{
				allIngredientMap.Clear();
				allIngredientMap = null;
			}

			if (recipeList != null)
			{
				recipeList.Clear();
				recipeList = null;
			}
		}

		public static List<AlchemyRecipe> GetRemainingPossiblities(Item addedItem, List<AlchemyRecipe> previousPossibilities)
		{
			var remainingPossibilities = new List<AlchemyRecipe>();
			foreach (AlchemyRecipe eachRecipe in previousPossibilities)
			{
				if (eachRecipe.CheckItem(addedItem))
					remainingPossibilities.Add(eachRecipe);
			}

			return remainingPossibilities;
		}

		/// <summary>
		/// Creates an instance of AlchemyIngredient based on provided Item and puts the Item itself into the ingredient object
		/// </summary>
		/// <param name="Item"></param>
		/// <returns></returns>
		public static AlchemyIngredient InstantiateIngredient(Item Item)
		{
			bool inMap = allIngredientMap.TryGetValue(Item.type, out Type ingredientClassType);

			AlchemyIngredient instantiatedIngredient;

			if (inMap)
				instantiatedIngredient = (AlchemyIngredient)Activator.CreateInstance(ingredientClassType);
			else
				instantiatedIngredient = new GenericAlchemyIngredient(Item.type);

			instantiatedIngredient.PutIngredient(Item);
			return instantiatedIngredient;
		}
	}
}
