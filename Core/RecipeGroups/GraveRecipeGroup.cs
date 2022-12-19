using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Core.RecipeGroups
{
	class GraveRecipeGroup : IRecipeGroup
	{
		public void AddRecipeGroups()
		{
			var group = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(ItemID.Tombstone), new int[]
			{
				ItemID.Tombstone,
				ItemID.GraveMarker,
				ItemID.CrossGraveMarker,
				ItemID.Headstone,
				ItemID.Gravestone,
				ItemID.Obelisk,
				ItemID.RichGravestone1,
				ItemID.RichGravestone2,
				ItemID.RichGravestone3,
				ItemID.RichGravestone4,
				ItemID.RichGravestone5
			});
			// Registers the new recipe group with the specified name
			RecipeGroup.RegisterGroup("StarlightRiver:Graves", group);
		}
		public float Priority => 1.2f;
	}
}
