using System;
using System.Collections.Generic;
using Terraria.DataStructures;

namespace StarlightRiver.Content.Alchemy
{
	/// <summary>
	/// base alchemy class, can be directly created if no unique visuals / logic is needed for the recipe
	/// otherwise override and use provided hooks for more advanced logic and visuals
	/// </summary>
	public class AlchemyRecipe
	{

		protected Dictionary<int, Item> requiredIngredientsMap = new();
		protected List<int> requiredModifiers = new();

		protected List<Item> outputItemList = new();

		/// <summary>
		/// adds this recipe to the recipe cache now that it is done
		/// make sure to call this after adding all the modifiers / ingredients
		/// </summary>
		public void AddRecipe()
		{
			AlchemyRecipeSystem.recipeList.Add(this);
		}

		public void AddOutputById(int outputItemId, int outputCount = 1)
		{
			var newOutput = new Item();
			newOutput.SetDefaults(outputItemId);
			newOutput.stack = outputCount;
			outputItemList.Add(newOutput);
		}

		/// <summary>
		/// creats an output by a clone of provided Item for potential exact matching
		/// </summary>
		/// <param name="Item"></param>
		public void AddOutputByItem(Item Item)
		{
			outputItemList.Add(Item.Clone());
		}

		public void AddIngredientById(int requiredIngredientId, int inputCount = 1)
		{
			var newIngredient = new Item();
			newIngredient.SetDefaults(requiredIngredientId);
			newIngredient.stack = inputCount;
			requiredIngredientsMap.Add(requiredIngredientId, newIngredient);
		}

		/// <summary>
		/// creates an ingredient by a clone of provided Item for potential exact matching
		/// </summary>
		/// <param name="Item"></param>
		public void AddIngredientByItem(Item Item)
		{
			requiredIngredientsMap.Add(Item.type, Item.Clone());
		}

		public void AddRequiredModifier(int requiredModifierTileId)
		{
			requiredModifiers.Add(requiredModifierTileId);
		}

		/// <summary>
		/// Runs when this recipe is the only possible remaining recipe from the current set of ingredients and has enough of each ingredient to be valid.
		/// return true to block any individual ingredient code from running.
		/// executes on client and server.
		/// by default does no logic and returns false.
		/// </summary>
		/// <returns></returns>
		public virtual bool UpdateReady(AlchemyWrapper wrapper)
		{
			return false;
		}

		/// <summary>
		/// runs when this recipe is the only possible remaining recipe but the current set of ingredients is NOT valid for atleast one craft.
		/// return true to block any individual ingredient code from running.
		/// executes on client and server.
		/// by default does not logic and returns false.
		/// </summary>
		/// <returns></returns>
		public virtual bool UpdateAlmostReady(AlchemyWrapper wrapper)
		{
			//TODO: maybe default to some kind of way to indicate to the Player they are on the right track but missing quantity / certain Items
			return false;
		}

		/// <summary>
		/// Runs when Player has initiated crafting this recipe with all ingredients added. return true to skip individual ingredient code from running.
		/// responsible for spawning in Items and visuals.
		/// executes on client and server. return true to stop individual ingredient code from running.
		/// by default consumes ingredients and spawns output instantly
		/// </summary>
		/// <returns></returns>
		public virtual bool UpdateCrafting(AlchemyWrapper wrapper, List<AlchemyIngredient> currentingredients, CauldronDummyAbstract cauldronDummy)
		{
			foreach (Item eachOutputItem in outputItemList)
			{
				Item.NewItem(new EntitySource_WorldEvent(), wrapper.cauldronRect, eachOutputItem.type, eachOutputItem.stack * wrapper.currentBatchSize);
			}

			foreach (AlchemyIngredient eachIngredient in currentingredients)
			{
				requiredIngredientsMap.TryGetValue(eachIngredient.storedItem.type, out Item requiredItem);

				eachIngredient.storedItem.stack -= requiredItem.stack * wrapper.currentBatchSize;
			}

			cauldronDummy.DumpIngredients();

			return false;
		}

		/// <summary>
		/// returns true if an Item is part of this recipe, false otherwise.
		/// by default only checks Item Id, override for stricter checking (like weapon modifier, ensuring minimum amount at insertion time, fields on the Item, etc). 
		/// ignores minimum amounts by default under the assumption that Player can add the rest at a later step
		/// </summary>
		/// <returns></returns>
		public virtual bool CheckItem(Item Item)
		{
			return requiredIngredientsMap.ContainsKey(Item.type);
		}

		/// <summary>
		/// returns a number for the amount of times this ingredient can be batched in the recipe 0 if invalid/insufficient.
		/// Used for ensuring the ingredient is valid and in proper stack size right before initializing the craft.
		/// By default only checks id and stack. override if needs stricter checking (like weapon modifier, maximums, split Item stacks etc.).
		/// </summary>
		/// <param name="ingredient"></param>
		/// <returns></returns>
		public virtual int CheckIngredientBatch(AlchemyIngredient ingredient)
		{
			if (requiredIngredientsMap.ContainsKey(ingredient.storedItem.type))
			{
				requiredIngredientsMap.TryGetValue(ingredient.storedItem.type, out Item requiredItem);

				return ingredient.storedItem.stack / requiredItem.stack;
			}

			return 0;
		}

		/// <summary>
		/// Returns number of times this recipe can craft if all the conditions are correct for the this recipe, otherwise 0
		/// </summary>
		/// <param name="currentIngredients"></param>
		/// <param name="currentModifiers"></param>
		/// <returns></returns>
		public virtual int GetCraftBatchSize(List<AlchemyIngredient> currentIngredients, List<int> currentModifiers)
		{
			//if they don't match in count its not possible for it to be complete so we short circuit and avoid needing to iterate through lists multiple times
			if (currentIngredients.Count != requiredIngredientsMap.Count)
				return 0;
			if (currentModifiers.Count != requiredModifiers.Count)
				return 0;

			int batchSize = int.MaxValue;
			foreach (AlchemyIngredient eachCurrentIngredient in currentIngredients)
			{
				batchSize = Math.Min(batchSize, CheckIngredientBatch(eachCurrentIngredient));
			}

			return batchSize;
		}
	}
}