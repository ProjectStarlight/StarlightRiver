using Terraria;
using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Core.RecipeGroups
{
	class BugShellRecipeGroup : IRecipeGroup
	{
		public void AddRecipeGroups()
		{
			var group = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(ItemID.RedHusk), new int[]
			{
				ItemID.RedHusk,
				ItemID.CyanHusk,
				ItemID.VioletHusk
			});
			// Registers the new recipe group with the specified name
			RecipeGroup.RegisterGroup("StarlightRiver:BugShells", group);
		}
		public float Priority => 1.2f;
	}
}
