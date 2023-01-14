using StarlightRiver.Content.Tiles.Permafrost;
using StarlightRiver.Core.Systems.AuroraWaterSystem;
using StarlightRiver.Helpers;
using System;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace StarlightRiver.Core
{
	public partial class StarlightWorld : ModSystem
	{
		public static int permafrostCenter;

		private static Vector2 oldPos;

		public void PermafrostGen(GenerationProgress progress, GameConfiguration configuration)
		{
			progress.Message = "Permafrost generation";

			int iceLeft = 0;
			int iceRight = 0;
			int iceBottom = 0;

			for (int x = 0; x < Main.maxTilesX; x++) //Find the ice biome since vanilla dosent track it
			{
				if (iceLeft != 0)
					break;

				for (int y = 0; y < Main.maxTilesY; y++)
				{
					if (Main.tile[x, y].TileType == TileID.IceBlock)
					{
						iceLeft = x;
						break;
					}
				}
			}

			for (int x = Main.maxTilesX - 1; x > 0; x--)
			{
				if (iceRight != 0)
					break;

				for (int y = 0; y < Main.maxTilesY; y++)
				{
					if (Main.tile[x, y].TileType == TileID.IceBlock)
					{
						iceRight = x;
						break;
					}
				}
			}

			for (int y = Main.maxTilesY - 1; y > 0; y--)
			{
				if (Main.tile[iceLeft, y].TileType == TileID.IceBlock)
				{
					iceBottom = y;
					break;
				}
			}

			int center = iceLeft + (iceRight - iceLeft) / 2;
			int centerY = (int)WorldGen.worldSurfaceHigh + (iceBottom - (int)WorldGen.worldSurfaceHigh) / 2;

		TryToGenerateArena:

			if (center < iceLeft || center > iceRight - 109)
				center = iceLeft + (iceRight - iceLeft) / 2;

			for (int x1 = 0; x1 < 109; x1++)
			{
				for (int y1 = 0; y1 < 180; y1++)
				{
					Tile tile = Framing.GetTileSafely(center - 40 + x1, centerY + 100 + y1);

					if (tile.TileType == TileID.BlueDungeonBrick || tile.TileType == TileID.GreenDungeonBrick || tile.TileType == TileID.PinkDungeonBrick)
					{
						center += Main.rand.Next(-1, 2) * 109;
						goto TryToGenerateArena;
					}
				}
			}

			squidBossArena = new Rectangle(center - 40, centerY + 100, 109, 180);
			StructureHelper.Generator.GenerateStructure("Structures/SquidBossArena", new Point16(center - 40, centerY + 100), Mod);

			Vector2 oldPos = new Vector2(squidBossArena.Center.X, squidBossArena.Y) * 16;

			//Find locations for and place the touchstone altars which lead to the boss' arena
			for (int k = 1; k <= 2; k++)
			{
				float fraction = k / 3f;
				int yTarget = (int)Helper.LerpFloat(squidBossArena.Y, (float)WorldGen.worldSurfaceHigh, fraction);

				for (int x = 0; x < Main.maxTilesX; x++)
				{
					if (Main.tile[x, yTarget].TileType == TileID.IceBlock)
					{
						iceLeft = x;
						break;
					}
				}

				for (int x = Main.maxTilesX - 1; x > 0; x--)
				{
					if (Main.tile[x, yTarget].TileType == TileID.IceBlock)
					{
						iceRight = x;
						break;
					}
				}

				int iceCenter = iceLeft + (iceRight - iceLeft) / 2;
				int xTarget = iceCenter + WorldGen.genRand.Next(-100, 100);

				oldPos = PlaceShrine(new Point16(xTarget, yTarget), Main.rand.Next(1, 4), oldPos);
			}

			for (int y = 14; y < Main.maxTilesY - 200; y++)
			{
				if (Main.tile[center, y].HasTile && (Main.tile[center, y].TileType == TileID.SnowBlock || Main.tile[center, y].TileType == TileID.IceBlock))
				{
					PlaceShrine(new Point16(center, y - 12), 0, oldPos, true);
					break;
				}

				if (Main.tile[center, y].HasTile && Main.tileSolid[Main.tile[center, y].TileType])
				{
					center += center > (iceLeft + (iceRight - iceLeft) / 2) ? -10 : 10;
					continue;
				}
			}

			//Place ore
			for (int k = 0; k < 100; k++) //placement attempts
			{
				var point = new Point16(WorldGen.genRand.Next(iceLeft, iceRight), WorldGen.genRand.Next(centerY, iceBottom));

				Tile tile = Framing.GetTileSafely(point.X, point.Y);

				if (tile.HasTile && tile.TileType == TileID.IceBlock)
					PlaceOre(point);
			}

			//Place water
			for (int k = 0; k < 10; k++) //placement attempts
			{
				var point = new Point16(WorldGen.genRand.Next(iceLeft, iceRight), WorldGen.genRand.Next(centerY, iceBottom));

				Tile tile = Framing.GetTileSafely(point.X, point.Y);

				int radius = WorldGen.genRand.Next(14, 30);

				if (SafeForWater(point, radius))
					PlaceWater(point, radius);
			}
		}

		/// <summary>
		/// Places a randomly sized chunk of aurora ice ore, centered at the given coordinates
		/// </summary>
		/// <param name="center">Where to place the ore, in tile coordinates</param>
		private void PlaceOre(Point16 center)
		{
			int radius = Main.rand.Next(2, 5);

			int frameStartX = radius == 4 ? 5 : radius == 3 ? 2 : 0;
			int frameStartY = radius == 4 ? 0 : radius == 3 ? 1 : 2;

			for (int x = center.X; x < center.X + radius; x++)
			{
				for (int y = center.Y; y < center.Y + radius; y++)
				{
					int xRel = x - center.X;
					int yRel = y - center.Y;

					Tile tile = Framing.GetTileSafely(x, y);
					tile.HasTile = true;
					tile.TileType = (ushort)ModContent.TileType<AuroraIce>();
					tile.TileFrameX = (short)((frameStartX + xRel) * 18);
					tile.TileFrameY = (short)((frameStartY + yRel) * 18);

					int r = radius - 1;

					if (xRel == 0 && yRel == 0)
						tile.Slope = SlopeType.SlopeDownRight;

					if (xRel == 0 && yRel == r)
						tile.Slope = SlopeType.SlopeUpRight;

					if (xRel == r && yRel == 0)
						tile.Slope = SlopeType.SlopeDownLeft;

					if (xRel == r && yRel == r)
						tile.Slope = SlopeType.SlopeUpLeft;

					bool dum = false;
					ModContent.GetInstance<AuroraIce>().TileFrame(x, y, ref dum, ref dum);
				}
			}
		}

		/// <summary>
		/// Checks if it is safe to place a sphere of aurora water at a given position and size
		/// </summary>
		/// <param name="center">The center of the prospective sphere, in tile coordinates</param>
		/// <param name="radius">The radius of the prospective sphere, in tiles</param>
		/// <returns></returns>
		private bool SafeForWater(Point16 center, int radius)
		{
			for (int x = -radius; x < radius; x++)
			{
				for (int y = -radius; y < radius; y++)
				{
					Point16 pos = center + new Point16(x, y);
					Tile tile = Framing.GetTileSafely(pos);

					if (tile.HasTile && tile.TileType == ModContent.TileType<AuroraBrick>())
						return false;

					if (tile.TileType == TileID.BlueDungeonBrick || tile.TileType == TileID.GreenDungeonBrick || tile.TileType == TileID.PinkDungeonBrick)
						return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Places a sphere of aurora water in the world
		/// </summary>
		/// <param name="center">The center of the sphere, in tile coordinates</param>
		/// <param name="radius">The radius of the sphere, in tiles</param>
		private void PlaceWater(Point16 center, int radius)
		{
			for (int x = -radius; x < radius; x++)
			{
				for (int y = -radius; y < radius; y++)
				{
					Point16 pos = center + new Point16(x, y);

					if (Vector2.Distance(center.ToVector2(), pos.ToVector2()) <= radius)
						AuroraWaterSystem.PlaceAuroraWater(pos.X, pos.Y);
				}
			}
		}

		/// <summary>
		/// Places a shrine in the world. Returns the position of the touchstone of that shrine
		/// </summary>
		/// <param name="topLeft">The position of the top left of the shrine</param>
		/// <param name="variant">Which variant of the shrine structure to generate</param>
		/// <param name="targetPoint">Where the shrine's touchstone should lead the player when clicked</param>
		/// <param name="hasWisp">If the shrine should spawn with it's touchstone pre-charged</param>
		/// <returns>The position that the next shrines touchstone should lead to</returns>
		private Vector2 PlaceShrine(Point16 topLeft, int variant, Vector2 targetPoint, bool hasWisp = false)
		{
			var touchstonePos = new Point16();

			switch (variant)
			{
				case 0: touchstonePos = topLeft + new Point16(11, 19); break;
				case 1: touchstonePos = topLeft + new Point16(8, 11); break;
				case 2: touchstonePos = topLeft + new Point16(11, 15); break;
				case 3: touchstonePos = topLeft + new Point16(10, 15); break;
				case 4: touchstonePos = topLeft + new Point16(13, 16); break;
			}

			bool genned = StructureHelper.Generator.GenerateMultistructureSpecific("Structures/TouchstoneAltar", topLeft, Mod, variant);

			if (genned)
			{
				var te = TileEntity.ByPosition[touchstonePos] as TouchstoneTileEntity;

				if (te is null)
					return touchstonePos.ToVector2();

				te.targetPoint = targetPoint;
				te.hasWisp = hasWisp;

				return touchstonePos.ToVector2();
			}
			else
			{
				throw new Exception("Failed to generate an altar...");
			}
		}
	}
}
