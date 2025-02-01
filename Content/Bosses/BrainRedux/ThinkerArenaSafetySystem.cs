using StarlightRiver.Content.Tiles.Crimson;
using StructureHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	public struct ThinkerRecord : TagSerializable
	{
		public NPC? thinker;
		public Vector2 arenaPos;
		public List<Point16> changedTiles;

		public static Func<TagCompound, ThinkerRecord> DESERIALIZER = DeserializeData;

		public ThinkerRecord(NPC? thinker, Vector2 arenaPos, List<Point16> changedTiles)
		{
			this.thinker = thinker;
			this.arenaPos = arenaPos;
			this.changedTiles = changedTiles;
		}

		public TagCompound SerializeData()
		{
			return new TagCompound()
			{
				["Pos"] = arenaPos,
				["Tiles"] = changedTiles.Select(n => n.ToVector2()).ToList()
			};
		}

		public static ThinkerRecord DeserializeData(TagCompound tag)
		{
			return new ThinkerRecord(null, tag.Get<Vector2>("Pos"), tag.GetList<Vector2>("changedTiles").Select(n => n.ToPoint16()).ToList());
		}
	}

	internal class ThinkerArenaSafetySystem : ModSystem
	{
		public List<ThinkerRecord> records = new();

		public override void SaveWorldData(TagCompound tag)
		{
			tag["Records"] = records;
		}

		public override void LoadWorldData(TagCompound tag)
		{
			records = tag.GetList<ThinkerRecord>("Records").ToList();
		}

		public override void PostUpdateEverything()
		{
			foreach (ThinkerRecord record in records)
			{
				if (record.thinker is null || !record.thinker.active || record.thinker.type != ModContent.NPCType<TheThinker>())
				{
					ResetArena(record);
					break;
				}
			}
		}

		public void ResetArena(ThinkerRecord record)
		{
			foreach (Point16 point in record.changedTiles)
			{
				Tile tile = Main.tile[(int)record.arenaPos.X / 16 + point.X, (int)record.arenaPos.Y / 16 + point.Y];

				if (tile.IsActuated)
					tile.IsActuated = false;

				if (tile.TileType == ModContent.TileType<BrainBlocker>())
					tile.HasTile = false;
			}

			foreach (NPC npc in Main.npc.Where(n => n.active && n.type == ModContent.NPCType<BrainPlatform>()))
			{
				npc.active = false;
			}

			var thinker = record.thinker?.ModNPC as TheThinker;

			if (thinker != null)
			{
				thinker.platforms.Clear();
				thinker.active = false;
			}

			records.Remove(record);
		}

		public void ResetArena(NPC thinker)
		{
			if (records.Any(n => n.thinker == thinker))
			{
				ThinkerRecord record = records.First(n => n.thinker == thinker);
				ResetArena(record);
			}
		}
	}
}