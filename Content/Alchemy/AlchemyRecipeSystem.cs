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

namespace StarlightRiver.Content.Alchemy
{
    public static class AlchemyRecipeSystem
    {
        //static class to handle recipe loading / unloading as well as helper functions for codex integration and determining result of a list of ingredients

        public static List<AlchemyRecipe> recipeList;

        /// <summary>
        /// cache Types of all the alchemy ingredients in use, indexed by itemId, 
        /// done this way since we want to instantiate new instances b/c there can be multiple cauldrons and we don't want to use reflection every time an ingredient needs to be created
        /// </summary>
        public static Dictionary<int, Type> allIngredientMap;

        public static void Load()
        {
            allIngredientMap = new Dictionary<int, Type>();
            recipeList = new List<AlchemyRecipe>();
            //reflection to discover and cache AlchemyIngredient override types
            Mod mod = StarlightRiver.Instance;
            foreach (Type type in mod.Code.GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(AlchemyIngredient)) && t != typeof(GenericAlchemyIngredient)))
            {
                AlchemyIngredient tempIngredient = (AlchemyIngredient)Activator.CreateInstance(type);
                allIngredientMap.Add(tempIngredient.getItemId(), type);
            }

            //todo: remove this later
            AlchemyRecipe testRecipe = new AlchemyRecipe();
            testRecipe.addIngredientById(ItemID.DirtBlock);
            testRecipe.addIngredientById(ItemID.LivingFireBlock);
            testRecipe.addIngredientById(ItemID.DirtWall);
            testRecipe.addIngredientById(ItemID.IronBar);
            testRecipe.addOutputById(ItemID.WoodenSword);
            testRecipe.addRecipe();
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
                if (eachRecipe.checkIngredient(addedItem))
                    remainingPossibilities.Add(eachRecipe);
            }

            return remainingPossibilities;
        }

        /// <summary>
        /// Creates an instance of AlchemyIngredient based on provided item and puts the item itself into the ingredient object
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static AlchemyIngredient instantiateIngredient(Item item)
        {
            Type ingredientClassType;
            bool inMap = allIngredientMap.TryGetValue(item.type, out ingredientClassType);

            AlchemyIngredient instantiatedIngredient ;

            if (inMap)
                instantiatedIngredient = ((AlchemyIngredient)Activator.CreateInstance(ingredientClassType));
            else
                instantiatedIngredient = new GenericAlchemyIngredient(item.type);

            instantiatedIngredient.putIngredient(item);
            return instantiatedIngredient;
        }
    }
}
