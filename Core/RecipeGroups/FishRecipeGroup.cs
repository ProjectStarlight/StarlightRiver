﻿using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Core.RecipeGroups
{
	class FishRecipeGroup : IRecipeGroup
	{
		public void AddRecipeGroups()
		{
			var group = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(ItemID.Fish), new int[]
			{
				ItemID.AtlanticCod,
				ItemID.Bass,
				ItemID.DoubleCod,
				ItemID.Flounder,
				ItemID.NeonTetra,
				ItemID.RedSnapper,
				ItemID.Salmon,
				ItemID.Trout,
				ItemID.Tuna
			});
			// Registers the new recipe group with the specified name
			RecipeGroup.RegisterGroup("StarlightRiver:Fish", group);
		}
		public float Priority => 1.2f;
	}
}