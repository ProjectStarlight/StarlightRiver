using StarlightRiver.Content.Biomes;
using System;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Vitric
{
	class VitricNPCSpawner : PlayerTicker
	{
		public override int TickFrequency => 30;

		public override bool Active(Player Player)
		{
			return Player.InModBiome(GetInstance<VitricDesertBiome>());
		}

		public override void Tick(Player Player)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return; //only spawn for singlePlayer or on the server

			if (Main.npc.Any(n => n.active && n.boss))
				return; //No magmites during bosses

			if (Main.rand.NextBool(4) && Main.npc.Count(n => n.active && n.type == NPCType<MagmitePassive>()) < 5)
			{
				Point16 coords = FindTile(((Player.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(200, 500)) / 16).ToPoint16(), n => !n.HasTile && n.LiquidType == LiquidID.Lava && n.LiquidAmount > 0, 10, 2, 2);

				if (coords != Point16.Zero)
					NPC.NewNPC(NPC.GetSource_NaturalSpawn(), coords.X * 16, coords.Y * 16, NPCType<MagmitePassive>(), 0, -1);
			}

			if (Main.rand.NextBool(4) && Main.npc.Count(n => n.active && n.type == NPCType<MagmiteSmol>()) < 3)
			{
				Point16 coords = FindTile(((Player.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(200, 500)) / 16).ToPoint16(), n => !n.HasTile && n.LiquidType == LiquidID.Lava && n.LiquidAmount > 0, 10, 2, 2);

				if (coords != Point16.Zero)
					NPC.NewNPC(NPC.GetSource_NaturalSpawn(), coords.X * 16, coords.Y * 16, NPCType<MagmiteSmol>(), 0, -1);
			}

			if (Main.rand.NextBool(4) && Main.npc.Count(n => n.active && n.type == NPCType<MagmiteLarge>()) < 1)
			{
				Point16 coords = FindTile(((Player.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(200, 500)) / 16).ToPoint16(), n => !n.HasTile && n.LiquidType == LiquidID.Lava && n.LiquidAmount > 0, 10, 2, 2);

				if (coords != Point16.Zero)
					NPC.NewNPC(NPC.GetSource_NaturalSpawn(), coords.X * 16, coords.Y * 16, NPCType<MagmiteLarge>(), 0, -1);
			}
		}

		// These were re-abosrbed from helper since it was only ever used here. Move these to some kind of
		// tile finding util or worldgen utils later if needed
		public static Point16 FindTile(Point16 start, Func<Tile, bool> condition, int radius = 30, int w = 1, int h = 1)
		{
			Point16 output = Point16.Zero;

			for (int x = 0; x < radius; x++)
			{
				for (int y = 0; y < radius; y++)
				{
					Point16 check1 = start + new Point16(x, y);
					Point16 attempt1 = CheckTiles(check1, condition, w, h);
					if (attempt1 != Point16.Zero)
						return attempt1;

					Point16 check2 = start + new Point16(-x, y);
					Point16 attempt2 = CheckTiles(check2, condition, w, h);
					if (attempt2 != Point16.Zero)
						return attempt2;

					Point16 check3 = start + new Point16(x, -y);
					Point16 attempt3 = CheckTiles(check3, condition, w, h);
					if (attempt3 != Point16.Zero)
						return attempt3;

					Point16 check4 = start + new Point16(-x, -y);
					Point16 attempt4 = CheckTiles(check4, condition, w, h);
					if (attempt4 != Point16.Zero)
						return attempt4;
				}
			}

			return output;
		}

		private static Point16 CheckTiles(Point16 check, Func<Tile, bool> condition, int w, int h)
		{
			if (WorldGen.InWorld(check.X, check.Y))
			{
				for (int x = 0; x < w; x++)
				{
					for (int y = 0; y < h; y++)
					{
						Tile checkTile = Framing.GetTileSafely(check.X + x, check.Y + y);

						if (!condition(checkTile))
							return Point16.Zero;
					}
				}

				return check;
			}

			return Point16.Zero;
		}
	}
}