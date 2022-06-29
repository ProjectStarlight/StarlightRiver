using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.Content.Items.Vitric;

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
                AlchemyIngredient tempIngredient = (AlchemyIngredient)Activator.CreateInstance(type);
                allIngredientMap.Add(tempIngredient.getItemId(), type);
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

        public static List<AlchemyRecipe> getRemainingPossiblities(Item addedItem, List<AlchemyRecipe> previousPossibilities)
        {
            List<AlchemyRecipe> remainingPossibilities = new List<AlchemyRecipe>();
            foreach (AlchemyRecipe eachRecipe in previousPossibilities) 
            {
                if (eachRecipe.checkItem(addedItem))
                    remainingPossibilities.Add(eachRecipe);
            }

            return remainingPossibilities;
        }

        /// <summary>
        /// Creates an instance of AlchemyIngredient based on provided Item and puts the Item itself into the ingredient object
        /// </summary>
        /// <param name="Item"></param>
        /// <returns></returns>
        public static AlchemyIngredient instantiateIngredient(Item Item)
        {
            Type ingredientClassType;
            bool inMap = allIngredientMap.TryGetValue(Item.type, out ingredientClassType);

            AlchemyIngredient instantiatedIngredient ;

            if (inMap)
                instantiatedIngredient = ((AlchemyIngredient)Activator.CreateInstance(ingredientClassType));
            else
                instantiatedIngredient = new GenericAlchemyIngredient(Item.type);

            instantiatedIngredient.putIngredient(Item);
            return instantiatedIngredient;
        }
    }
}
