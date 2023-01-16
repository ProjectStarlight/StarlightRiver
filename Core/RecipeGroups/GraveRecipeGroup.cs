using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using StarlightRiver.Content.Items.Geomancer;
using Terraria.Localization;

namespace StarlightRiver.Core.Loaders
{
	class GraveRecipeGroup : IRecipeGroup
    {
        public void AddRecipeGroups()
        {
			RecipeGroup group = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(ItemID.Tombstone), new int[]
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
