using System;
using System.Collections.Generic;
using Terraria;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Alchemy
{
    /// <summary>
    /// base alchemy class, can be directly created if no unique visuals / logic is needed for the recipe
    /// otherwise override and use provided hooks for more advanced logic and visuals
    /// </summary>
    public class AlchemyRecipe
    {

        protected List<Item> requiredIngredients = new List<Item>();
        protected List<int> requiredModifiers = new List<int>();

        protected List<Item> outputItemList = new List<Item>();
        protected Dictionary<int, int> outputCountDict = new Dictionary<int, int>();

        /// <summary>
        /// adds this recipe to the recipe cache now that it is done
        /// make sure to call this after adding all the modifiers / ingredients
        /// </summary>
        public void addRecipe()
        {
            AlchemyRecipeSystem.recipeList.Add(this);
        }

        public void addOutputById(int outputItemId, int outputCount = 1)
        {
            Item newOutput =  new Item();
            newOutput.SetDefaults(outputItemId);
            newOutput.stack = outputCount;
            outputItemList.Add(newOutput);
        }

        /// <summary>
        /// creats an output by a clone of provided item for potential exact matching
        /// </summary>
        /// <param name="item"></param>
        public void addOutputByItem(Item item)
        {
            outputItemList.Add(item.Clone());
        }

        public void addIngredientById(int requiredIngredientId, int inputCount = 1)
        {
            Item newIngredient = new Item();
            newIngredient.SetDefaults(requiredIngredientId);
            newIngredient.stack = inputCount;
            requiredIngredients.Add(newIngredient);
        }

        /// <summary>
        /// creates an ingredient by a clone of provided item for potential exact matching
        /// </summary>
        /// <param name="item"></param>
        public void addIngredientByItem(Item item)
        {
            requiredIngredients.Add(item.Clone());
        }

        public void addRequiredModifier(int requiredModifierTileId)
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
        public virtual bool UpdateReady()
        {
            return false;
        }

        /// <summary>
        /// Runs when player has initiated crafting this recipe with all ingredients added. return true when done crafting.
        /// responsible for spawning in items and visuals.
        /// executes on client and server.
        /// </summary>
        /// <returns></returns>
        public virtual bool UpdateCrafting()
        {
            return true;
        }


        /// <summary>
        /// returns true if an ingredient is part of this recipe, false otherwise
        /// by default only checks item Id, override for stricter checking (like weapon modifier, ensuring minimum amount at insertion time, fields on the item, etc). 
        /// ignores minimum amounts by default under the assumption that player can add the rest at a later step
        /// </summary>
        /// <returns></returns>
        public virtual bool checkIngredient(Item item)
        {
            foreach (Item eachRequiredIngredient in requiredIngredients)
            {
                if (item.type == eachRequiredIngredient.type)
                    return true;
            }

            return false;
        }
    }
}
