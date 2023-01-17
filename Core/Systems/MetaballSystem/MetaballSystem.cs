using System.Collections.Generic;
using System.Linq;

namespace StarlightRiver.Core.Systems.MetaballSystem
{
	public class MetaballSystem : IOrderedLoadable
	{
		public static int oldScreenWidth = 0;
		public static int oldScreenHeight = 0;

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

		public void UpdateWindowSize(int width, int height)
		{
			Main.QueueMainThreadAction(() => Actors.ForEach(n => n.ResizeTarget(width, height)));

			oldScreenWidth = width;
			oldScreenHeight = height;
		}

		private void DrawTargets(On.Terraria.Main.orig_DrawNPCs orig, Main self, bool behindTiles = false)
		{
			if (behindTiles)
			{
				var toDraw = Actors.Where(n => !n.OverEnemies).ToList();
				toDraw.ForEach(a => a.DrawTarget(Main.spriteBatch));
			}

			orig(self, behindTiles);

			if (!behindTiles)
			{
				var toDraw = Actors.Where(n => n.OverEnemies).ToList();
				toDraw.ForEach(a => a.DrawTarget(Main.spriteBatch));
			}
		}

		private void BuildTargets(On.Terraria.Main.orig_CheckMonoliths orig)
		{
			if (Main.graphics.GraphicsDevice != null)
			{
				if (Main.screenWidth != oldScreenWidth || Main.screenHeight != oldScreenHeight)
					UpdateWindowSize(Main.screenWidth, Main.screenHeight);
			}

			if (Main.spriteBatch != null && Main.graphics.GraphicsDevice != null && !Main.gameMenu)
				Actors.ForEach(a => a.DrawToTarget(Main.spriteBatch, Main.graphics.GraphicsDevice));

			orig();
		}
	}
}
