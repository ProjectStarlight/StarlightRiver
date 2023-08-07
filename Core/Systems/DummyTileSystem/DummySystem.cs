using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Core.Systems.DummyTileSystem
{
	internal class DummySystem : ModSystem
	{
		public static List<Dummy> dummies = new();
		public static Dictionary<int, Dummy> prototypes = new();
		public static Dictionary<Type, int> types = new();

		public override void Load()
		{
			On_Main.PostUpdateAllProjectiles += UpdateDummies;
			On_Main.DrawProjectiles += DrawDummies;
			On_Main.DrawCachedProjs += DrawBehindNPCs;
		}

		private void UpdateDummies(On_Main.orig_PostUpdateAllProjectiles orig, Main self)
		{
			dummies.ForEach(n => n.AI());
		}

		private void DrawDummies(On_Main.orig_DrawProjectiles orig, Main self)
		{
			dummies.ForEach(n => n.PostDraw(Lighting.GetColor((n.Center / 16).ToPoint())));
		}

		private void DrawBehindNPCs(On_Main.orig_DrawCachedProjs orig, Main self, List<int> projCache, bool startSpriteBatch)
		{
			orig(self, projCache, startSpriteBatch);

			if (projCache == Main.instance.DrawCacheProjsBehindNPCsAndTiles)
			{
				dummies.ForEach(n => n.DrawBehindTiles());
			}
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
			var dummy = prototypes[type].Clone();
			dummy.active = true;
			dummy.position = pos;
			dummy.identity = Main.rand.Next(int.MaxValue); // Lets hope this is random enough!

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
	}
}
