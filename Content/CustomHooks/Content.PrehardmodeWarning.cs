using StarlightRiver.Core.Loaders.UILoading;
using StarlightRiver.Content.GUI;
using StarlightRiver.Core.Systems.CameraSystem;
using System;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Content.CustomHooks
{
	class PrehardmodeWarning : HookGroup
	{
		//Swaps the vanilla meteor events out, could create conflicts if other mods attempt the same but shouldnt be anything fatal
		public override void Load()
		{
			On.Terraria.WorldGen.StartHardmode += WorldGen_StartHardmode;
		}

		private void WorldGen_StartHardmode(On.Terraria.WorldGen.orig_StartHardmode orig)
		{
			orig();
			UILoader.GetUIState<MessageBox>().Display("Thank you for playing!", "You have reached the end of Starlight River. While there will be hardmode content in the future, there currently isn't any. Follow us on social media for more information on future content.");
		}
	}
}