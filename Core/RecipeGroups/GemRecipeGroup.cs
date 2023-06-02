using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Core.RecipeGroups
{
	class GemRecipeGroup : IRecipeGroup
	{
		public void AddRecipeGroups()
		{
			//todo add translation for Gemstone
			//todo possibly add mod compat
			var group = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " " + "Gemstone", new int[]
			{
				ItemID.Topaz,
				ItemID.Amethyst,
				ItemID.Sapphire,
				ItemID.Emerald,
				ItemID.Ruby,
				ItemID.Diamond,
				ItemID.Amber
			});
			// Registers the new recipe group with the specified name
			RecipeGroup.RegisterGroup("StarlightRiver:Gems", group);
		}
		public float Priority => 1.2f;
	}
}