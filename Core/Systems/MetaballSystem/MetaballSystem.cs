using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace StarlightRiver.Core.Systems.MetaballSystem
{
	public class MetaballSystem : IOrderedLoadable
	{
		public static Semaphore actorsSem = new(1, 1);

		public static List<MetaballActor> actors = new();

		//We intentionally load after screen targets here so our extra RT swapout applies after the default ones.
		public float Priority => 1.1f;

		public void Load()
		{
			if (Main.dedServ)
				return;

			Terraria.On_Main.DrawNPCs += DrawTargets;
			Terraria.On_Main.CheckMonoliths += BuildTargets;
		}

		public void Unload()
		{
			Terraria.On_Main.DrawNPCs -= DrawTargets;
			Terraria.On_Main.CheckMonoliths -= BuildTargets;

			actorsSem.WaitOne();
			actors = null;
			actorsSem.Release();
		}

		private void DrawTargets(Terraria.On_Main.orig_DrawNPCs orig, Main self, bool behindTiles = false)
		{
			if (behindTiles)
			{
				actorsSem.WaitOne();
				var toDraw = actors.Where(n => !n.OverEnemies).ToList();
				toDraw.ForEach(a => a.DrawTarget(Main.spriteBatch));
				actorsSem.Release();
			}

			orig(self, behindTiles);

			if (!behindTiles)
			{
				actorsSem.WaitOne();
				var toDraw = actors.Where(n => n.OverEnemies).ToList();
				toDraw.ForEach(a => a.DrawTarget(Main.spriteBatch));
				actorsSem.Release();
			}
		}

		private void BuildTargets(Terraria.On_Main.orig_CheckMonoliths orig)
		{
			orig();

			if (!Main.gameMenu && Main.spriteBatch != null && Main.graphics.GraphicsDevice != null)
			{
				actorsSem.WaitOne();
				actors.ForEach(a => a.DrawToTarget(Main.spriteBatch, Main.graphics.GraphicsDevice));
				actorsSem.Release();
			}
		}
	}
}