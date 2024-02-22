using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Core
{
	class RecipeSystem : ModSystem
	{
		public static List<string> knownRecipies = new();

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

		public static Condition GetCondition(Item result)
		{
			return new Condition(LocalizedText.Empty, () => RecipeSystem.knownRecipies.Contains(result.Name));
		}
	}
}