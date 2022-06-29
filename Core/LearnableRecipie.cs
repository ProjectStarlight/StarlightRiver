using Terraria.ModLoader;
using Terraria;
using Terraria.ModLoader.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace StarlightRiver.Core
{
	class LearnableRecipe : GlobalRecipe 
    {
		public override bool RecipeAvailable(Recipe recipe)
		{
			if (recipe.createItem.ModItem is null)
				return base.RecipeAvailable(recipe);

			return !RecipeSystem.allHiddenRecipes.Contains(recipe.createItem.ModItem.Name) || RecipeSystem.knownRecipies.Contains(recipe.createItem.ModItem.Name); //TODO: This is probably an optimization nightmare, figure something out
		}
    }

    class RecipeSystem : ModSystem
	{
		public static List<string> allHiddenRecipes = new List<string>();
		public static List<string> knownRecipies = new List<string>();

		public override void OnWorldLoad()
		{
			knownRecipies = new List<string>();
		}

		public override void SaveWorldData(TagCompound tag)
		{
			tag["Recipies"] = knownRecipies;
		}

		public override void LoadWorldData(TagCompound tag)
		{
			knownRecipies = (List<string>)tag.GetList<string>("Recipies");
		}

		public static void LearnRecipie(string key)
		{
			//this is set up in a way where the stored key should be the same as the display name, there is no real reason to differentiate as the entirety of the data stored is a string list.
			if (!knownRecipies.Contains(key))
			{
				knownRecipies.Add(key);
				CombatText.NewText(Main.LocalPlayer.Hitbox, Color.Tan, "Learned Recipie: " + key);
			}
		}
	}
}
