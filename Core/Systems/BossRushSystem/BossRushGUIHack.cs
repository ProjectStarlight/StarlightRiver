using StarlightRiver.Content.GUI;
using StarlightRiver.Core.Loaders.UILoading;

namespace StarlightRiver.Core.Systems.BossRushSystem
{
	internal class BossRushGUIHack : ModSystem
	{
		public static bool inMenu;

		public override void Load()
		{
			On.Terraria.Main.DrawMenu += DrawBossMenu;
		}

		private void DrawBossMenu(On.Terraria.Main.orig_DrawMenu orig, Main self, GameTime gameTime)
		{
			if (!inMenu)
			{
				orig(self, gameTime);
				return;
			}

			UILoader.GetUIState<BossRushMenu>().UserInterface.Update(Main._drawInterfaceGameTime);
		}
	}
}
