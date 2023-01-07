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
		public override void Load()
		{
			On.Terraria.WorldGen.StartHardmode += WorldGen_StartHardmode;
		}

		private void WorldGen_StartHardmode(On.Terraria.WorldGen.orig_StartHardmode orig)
		{
			orig();
			UILoader.GetUIState<MessageBox>().Display("Thank you for playing!", "You've reached the current end of Starlight River. Hardmode content is planned and under development, follow us on social media for spoilers and future updates.");
		}
	}
}