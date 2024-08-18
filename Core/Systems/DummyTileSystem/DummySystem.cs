using System;
using System.Collections.Generic;

namespace StarlightRiver.Core.Systems.DummyTileSystem
{
	internal class DummySystem : ModSystem
	{
		public static List<Dummy> dummies = new();
		public static Dictionary<int, Dummy> prototypes = new();
		public static Dictionary<Type, int> types = new();

		public override void Load()
		{
			On_Main.DrawProjectiles += DrawDummies;
			On_Main.DoDraw_DrawNPCsBehindTiles += DrawBehindNPCs;
			On_Player.TileInteractionsCheck += RightClickDummies;
		}

		public override void PostUpdateProjectiles()
		{
			dummies.ForEach(n => n.AI());

			dummies.RemoveAll(n => !n.active);
		}

		private void DrawDummies(On_Main.orig_DrawProjectiles orig, Main self)
		{
			orig(self);

			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.Transform);

			dummies.ForEach(n =>
			{
				if (!n.offscreen)
					n.PostDraw(Lighting.GetColor((n.Center / 16).ToPoint()));
			});

			Main.spriteBatch.End();
		}

		private void DrawBehindNPCs(On_Main.orig_DoDraw_DrawNPCsBehindTiles orig, Main self)
		{
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.Transform);

			dummies.ForEach(n =>
			{
				if (!n.offscreen)
					n.DrawBehindTiles();
			});

			Main.spriteBatch.End();

			orig(self);
		}

		public override void Unload()
		{
			dummies.Clear();
			prototypes.Clear();
			types.Clear();
		}

		public override void OnWorldLoad()
		{
			dummies.Clear();
		}

		/// <summary>
		/// Spawns a new dummy at the given point in the world
		/// </summary>
		/// <param name="type">The numeric type of the dummy to spawn</param>
		/// <param name="pos">The position in the world to spawn the dummy at</param>
		public static Dummy NewDummy(int type, Vector2 pos)
		{
			Dummy dummy = prototypes[type].Clone();
			dummy.active = true;
			dummy.Center = pos;
			dummy.identity = Guid.NewGuid().GetHashCode();

			dummy.SafeSetDefaults();

			dummies.Add(dummy);

			dummy.OnSpawn();

			return dummy;
		}

		/// <summary>
		/// Spawns a new dummy at the given point in the world
		/// </summary>
		/// <typeparam name="T">The type of dummy to spawn</typeparam>
		/// <param name="pos">The position in the world to spawn the dummy at</param>
		public static Dummy NewDummy<T>(Vector2 pos) where T : Dummy
		{
			return NewDummy(DummyType<T>(), pos);
		}

		/// <summary>
		/// Gets the numeric ID of a given dummy type. Note this is consistent only at runtime and based on load order.
		/// </summary>
		/// <typeparam name="T">The type of dummy to get the numeric type of</typeparam>
		/// <returns>The numeric type of T, or -1 if an error occurs</returns>
		public static int DummyType<T>() where T : Dummy
		{
			if (types.ContainsKey(typeof(T)))
				return types[typeof(T)];
			else
				return -1;
		}

		private void RightClickDummies(On_Player.orig_TileInteractionsCheck orig, Player self, int myX, int myY)
		{
			foreach (Dummy dummy in DummySystem.dummies)
			{
				Rectangle? box = dummy.GetClickbox();
				if (box != null && box.Value.Contains(new Point(myX * 16, myY * 16)))
				{
					dummy.RightClickHover(myX, myY);

					if (self.tileInteractAttempted && !self.tileInteractionHappened)
					{
						dummy.RightClick(myX, myY);
						self.tileInteractionHappened = true;
					}

					return;
				}
			}

			orig(self, myX, myY);
		}
	}
}