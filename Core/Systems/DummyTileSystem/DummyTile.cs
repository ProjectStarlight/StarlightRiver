using StarlightRiver.Content.Packets;
using System.Collections.Generic;
using Terraria.DataStructures;

namespace StarlightRiver.Core.Systems.DummyTileSystem
{
	public abstract class DummyTile : ModTile
	{
		public readonly static Dictionary<Point16, Dummy> dummiesByPosition = new();

		public virtual int DummyType { get; }

		public Dummy Dummy(int i, int j)
		{
			return GetDummy(i, j, DummyType);
		}

		/// <summary>
		/// Attempts to find a dummy at the given coordiantes of the given type
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="type"></param>
		/// <returns>The dummy instance or null if one does not exist</returns>
		public static Dummy GetDummy(int i, int j, int type)
		{
			var key = new Point16(i, j);

			if (dummiesByPosition.TryGetValue(key, out Dummy dummy))
			{
				if (dummy.type == type && dummy.active)
					return dummy;
			}

			if (NonDictSearch(i, j, type))
			{
				if (dummiesByPosition.TryGetValue(key, out Dummy dummy2))
				{
					if (dummy2.type == type && dummy2.active)
						return dummy2;
				}
			}

			return null;
		}

		/// <summary>
		/// Attempts to find a dummy at the given coordiantes of the given type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <returns>The dummy instance or null if one does not exist</returns>
		public static Dummy GetDummy<T>(int i, int j) where T : Dummy
		{
			var key = new Point16(i, j);

			if (dummiesByPosition.TryGetValue(key, out Dummy dummy))
			{
				if (dummy is T && dummy.active)
					return dummy;
			}

			if (NonDictSearch<T>(i, j))
			{
				if (dummiesByPosition.TryGetValue(key, out Dummy dummy2))
				{
					if (dummy2 is T && dummy2.active)
						return dummy2;
				}
			}

			return null;
		}

		/// <summary>
		/// Determines if a dummy exists at a given position with the given type or not
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="type"></param>
		/// <returns>If the described dummy is extant locally</returns>
		public static bool DummyExists(int i, int j, int type)
		{
			if (GetDummy(i, j, type) != null)
				return true;

			return NonDictSearch(i, j, type);
		}

		/// <summary>
		/// Determines if a dummy exists at a given position with the given type or not
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <returns>If the described dummy is extant locally</returns>
		public static bool DummyExists<T>(int i, int j) where T : Dummy
		{
			if (GetDummy<T>(i, j) != null)
				return true;

			return NonDictSearch<T>(i, j);
		}

		/// <summary>
		/// searches through dummy entities for a dummy that's not found in the dictionary, and adds it to the dictionary if found
		/// primarily for multiplayer since its more likely to fail to be attached to a key there
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		private static bool NonDictSearch(int i, int j, int type)
		{
			for (int k = 0; k < DummySystem.dummies.Count; k++)
			{
				Dummy dummy = DummySystem.dummies[k];

				if (dummy.active && dummy.type == type && (dummy.position / 16).ToPoint16() == new Point16(i, j))
				{
					var key = new Point16(i, j);
					dummiesByPosition[key] = dummy;
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// searches through dummies for a dummy that's not found in the dictionary, and adds it to the dictionary if found
		/// primarily for multiplayer since its more likely to fail to be attached to a key there
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		private static bool NonDictSearch<T>(int i, int j) where T : Dummy
		{
			for (int k = 0; k < DummySystem.dummies.Count; k++)
			{
				Dummy dummy = DummySystem.dummies[k];

				if (dummy.active && dummy is T && (dummy.position / 16).ToPoint16() == new Point16(i, j))
				{
					var key = new Point16(i, j);
					dummiesByPosition[key] = dummy;
					return true;
				}
			}

			return false;
		}

		public virtual void SafeNearbyEffects(int i, int j, bool closer) { }

		public virtual bool SpawnConditions(int i, int j)
		{
			Tile tile = Main.tile[i, j];
			return tile.TileFrameX == 0 && tile.TileFrameY == 0;
		}

		public sealed override void NearbyEffects(int i, int j, bool closer)
		{
			if (!Main.tileFrameImportant[Type] || SpawnConditions(i, j))
			{
				int type = DummyType;
				Dummy dummy = Dummy(i, j);

				if (dummy is null || !dummy.active)
				{
					if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient)
					{
						var packet = new SpawnDummy(Main.myPlayer, type, i, j);
						packet.Send(-1, -1, false);
						return;
					}

					Vector2 spawnPos = new Vector2(i, j) * 16 + DummySystem.prototypes[type].Size / 2;
					Dummy newDummy = DummySystem.NewDummy(type, spawnPos);

					var key = new Point16(i, j);
					dummiesByPosition[key] = newDummy;
				}
			}

			SafeNearbyEffects(i, j, closer);
		}
	}
}