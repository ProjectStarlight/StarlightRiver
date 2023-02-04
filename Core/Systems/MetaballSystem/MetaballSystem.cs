using System.Collections.Generic;

namespace StarlightRiver.Core.Systems.MetaballSystem
{
	public class MetaballSystem : IOrderedLoadable
	{
		public static List<MetaballActor> Actors = new();

		public float Priority => 1;

		public void Load()
		{
			if (Main.dedServ)
				return;

			On.Terraria.Main.DrawNPCs += DrawTargets;
			On.Terraria.Main.CheckMonoliths += BuildTargets;
		}

		public void Unload()
		{
			On.Terraria.Main.DrawNPCs -= DrawTargets;
			On.Terraria.Main.CheckMonoliths -= BuildTargets;

			Actors = null;
		}

		private void DrawTargets(On.Terraria.Main.orig_DrawNPCs orig, Main self, bool behindTiles = false)
		{
			if (behindTiles)
				Actors.ForEach(a => a.DrawTarget(Main.spriteBatch));

			orig(self, behindTiles);
		}

		private void BuildTargets(On.Terraria.Main.orig_CheckMonoliths orig)
		{
			if (!Main.gameMenu && Main.spriteBatch != null && Main.graphics.GraphicsDevice != null)
				Actors.ForEach(a => a.DrawToTarget(Main.spriteBatch, Main.graphics.GraphicsDevice));

			orig();
		}
	}
}
