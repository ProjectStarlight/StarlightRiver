using log4net.Repository.Hierarchy;
using StarlightRiver.Content.Tiles.Permafrost;
using StarlightRiver.Core.Systems.AuroraWaterSystem;
using StarlightRiver.Helpers;
using System;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace StarlightRiver.Core
{
	public partial class StarlightWorld : ModSystem
	{
		public static int permafrostCenter;

		public void PermafrostGen(GenerationProgress progress, GameConfiguration configuration)
		{
			progress.Message = "Permafrost generation";

			int iceLeft = Main.maxTilesX;
			int iceRight = 0;
			int iceBottom = 0;
			int iceTop;

			for (int x = 0; x < Main.maxTilesX; x++)
			{
				for (int y = Main.maxTilesY - 1; y > 0; y--)
				{
					if (y < iceBottom)
						continue;

					if (Main.tile[x, y].TileType == TileID.IceBlock)
					{
						iceBottom = y;
					}
				}
			}

			iceTop = (int)(iceBottom + GenVars.worldSurfaceHigh) / 2;

			for (int x = 0; x < Main.maxTilesX; x++) //Find the ice biome since vanilla dosent track it
			{
				if (x >= iceLeft)
					continue;

				for (int y = iceTop; y < Main.maxTilesY; y++)
				{
					if (Main.tile[x, y].TileType == TileID.IceBlock)
					{
						iceLeft = x;
					}
				}
			}

			for (int x = Main.maxTilesX - 1; x > 0; x--)
			{
				if (x <= iceRight)
					continue;

				for (int y = iceTop; y < Main.maxTilesY; y++)
				{
					if (Main.tile[x, y].TileType == TileID.IceBlock)
					{
						iceRight = x;
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

			int centerX = (iceLeft + iceRight) / 2;
			int centerY = (int)(iceBottom + GenVars.worldSurfaceHigh) / 2;

			// Try shotgun approach
			int retries = 0;
			while (retries < 100)
			{
				retries++;

				if (retries >= 100)
					throw new Exception("Could not place a required structure: Auroracle Arena");

				if (centerX < iceLeft || centerX > iceRight - 109)
					centerX = (iceLeft + iceRight) / 2;

				if (!WorldGenHelper.IsRectangleSafe(new Rectangle(centerX - 40, centerY + 100, 109, 180)))
				{
					centerX = WorldGen.genRand.Next(iceLeft, iceRight - 109);
					centerY = (int)GenVars.worldSurfaceHigh + (int)((iceBottom - (int)GenVars.worldSurfaceHigh) * WorldGen.genRand.NextFloat(0.5f, 0.8f));
					StarlightRiver.Instance.Logger.Info($"World generation attempting to place Auroracle Arena at {centerX}, {centerY} failed, retries left: {100 - retries}");
					continue;
				}
				else
				{
					break;
				}
			}

			squidBossArena = new Rectangle(centerX - 40, centerY + 100, 109, 180);
			StructureHelper.API.Generator.GenerateStructure("Structures/SquidBossArena", new Point16(centerX - 40, centerY + 100), Mod);

			GenVars.structures.AddProtectedStructure(squidBossArena, 20);

			Vector2 oldPos = new Vector2(squidBossArena.Center.X, squidBossArena.Y) * 16;

			//Find locations for and place the touchstone altars which lead to the boss' arena
			for (int k = 1; k <= 3; k++)
			{
				float fraction = k / 4f;
				int yTarget = (int)MathHelper.Lerp(squidBossArena.Y, (float)GenVars.worldSurfaceHigh, fraction);

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

				int iceCenter = (iceLeft + iceRight) / 2;
				int xTarget = iceCenter + WorldGen.genRand.Next(-100, 100);

				int retries2 = 0;
				while (retries2 < 100)
				{
					retries2++;

					if (!Helpers.WorldGenHelper.IsRectangleSafe(new Rectangle(xTarget, yTarget, 32, 32)))
					{
						xTarget = iceCenter + WorldGen.genRand.Next(-100, 100);
						yTarget = (int)MathHelper.Lerp(squidBossArena.Y, (float)GenVars.worldSurfaceHigh, fraction) + WorldGen.genRand.Next(-40, 40);
						continue;
					}
					else
					{
						oldPos = PlaceShrine(new Point16(xTarget, yTarget), 4 - k, oldPos) * 16;
						break;
					}
				}
				// We can continue after a fail here and just skip a shrine, its not ideal as it decreases loot but its better than failing the seed
			}

			bool placedSurfaceShrine = false;

			for (int x = iceLeft; x < iceRight - 40; x++)
			{
				if (placedSurfaceShrine)
					break;

				for (int y = 40; y < Main.maxTilesY - 200; y++)
				{
					Tile tile = Main.tile[x, y];
					Point16 size = StructureHelper.API.Generator.GetStructureDimensions("Structures/AuroraShrine0", Mod);

					if (tile.HasTile && tile.TileType == TileID.SnowBlock && WorldGenHelper.NonSolidScanUp(new Point16(x, y), 20))
					{
						if (WorldGenHelper.GetElevationDeviation(new Point16(x, y), size.X, 20, 5, true) < 5)
						{
							PlaceShrine(new Point16(x, y - size.Y + 8), 0, oldPos);
							placedSurfaceShrine = true;
							break;
						}
					}
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
			int radius = WorldGen.genRand.Next(2, 5);

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

					if (tile.WallType == ModContent.WallType<AuroraBrickWall>())
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
		/// <returns>The position that the next shrines touchstone should lead to</returns>
		private Vector2 PlaceShrine(Point16 topLeft, int variant, Vector2 targetPoint)
		{
			var touchstonePos = new Point16();

			switch (variant)
			{
				case 0: touchstonePos = topLeft + new Point16(24, 21); break;
				case 1: touchstonePos = topLeft + new Point16(11, 15); break;
				case 2: touchstonePos = topLeft + new Point16(12, 12); break;
				case 3: touchstonePos = topLeft + new Point16(14, 12); break;
			}

			StructureHelper.API.Generator.GenerateStructure("Structures/AuroraShrine" + variant, topLeft, Mod);

			var te = TileEntity.ByPosition[touchstonePos] as TouchstoneTileEntity;

			if (te is null)
				return touchstonePos.ToVector2();

			te.targetPoint = targetPoint;

			return touchstonePos.ToVector2();
		}
	}
}