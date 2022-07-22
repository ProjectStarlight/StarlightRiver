using StarlightRiver.Content.Tiles.Forest;
using StarlightRiver.Content.Tiles.Palestone;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Tiles.Moonstone;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Terraria.IO;
using StarlightRiver.Core;

namespace StarlightRiver.Core
{
    public class GenerateMoonstone : ILoadable
    {
        public void Load(Mod mod)
        {
            On.Terraria.WorldGen.dropMeteor += DecideMeteor;
        }

        public void Unload()
        {
            On.Terraria.WorldGen.dropMeteor -= DecideMeteor;
        }

		public void DecideMeteor(On.Terraria.WorldGen.orig_dropMeteor orig)
        {
			if (Main.rand.NextBool())
				orig();
			else
				DropMoonstone();
		}
        public static void DropMoonstone() //Consistant naming with vanilla's dropMeteor and meteor methods
        {
			bool flag = true; //TODO: Clean this ripped vanilla code up
			if (Main.netMode == 1)
			{
				return;
			}
			for (int i = 0; i < 255; i++)
			{
				if (Main.player[i].active)
				{
					flag = false;
					break;
				}
			}
			int num = 0;
			float num2 = Main.maxTilesX / 4200;
			int num3 = (int)(400f * num2);
			for (int j = 5; j < Main.maxTilesX - 5; j++)
			{
				for (int k = 5; (double)k < Main.worldSurface; k++)
				{
					if (Main.tile[j, k].HasTile && Main.tile[j, k].TileType == 37)
					{
						num++;
						if (num > num3)
						{
							return;
						}
					}
				}
			}
			float num4 = 600f;
			int num5 = 0;
			while (!flag)
			{
				float num6 = (float)Main.maxTilesX * 0.08f;
				int num7 = Main.rand.Next(150, Main.maxTilesX - 150);
				while ((float)num7 > (float)Main.spawnTileX - num6 && (float)num7 < (float)Main.spawnTileX + num6)
				{
					num7 = Main.rand.Next(150, Main.maxTilesX - 150);
				}
				for (int l = (int)(Main.worldSurface * 0.3); l < Main.maxTilesY; l++)
				{
					Tile tile = Main.tile[num7, l];
					if (!tile.HasTile || !Main.tileSolid[tile.TileType] || TileID.Sets.Platforms[tile.TileType])
					{
						continue;
					}
					int num8 = 0;
					int num9 = 15;
					for (int m = num7 - num9; m < num7 + num9; m++)
					{
						for (int n = l - num9; n < l + num9; n++)
						{
							if (WorldGen.SolidTile(m, n))
							{
								num8++;
								if (Main.tile[m, n].TileType == 189 || Main.tile[m, n].TileType == 202)
								{
									num8 -= 100;
								}
							}
							else if (Main.tile[m, n].LiquidAmount > 0)
							{
								num8--;
							}
						}
					}
					if ((float)num8 >= num4)
					{
						flag = Moonstone(num7, l);
						if (flag)
						{
						}
					}
					else
					{
						num4 -= 0.5f;
					}
					break;
				}
				num5++;
				if (num4 < 100f || num5 >= Main.maxTilesX * 5)
				{
					break;
				}
			}
        }

        private static bool Moonstone(int i, int j)
        {
			bool ignorePlayers = false;
			if (i < 50 || i > Main.maxTilesX - 50)
			{
				return false;
			}
			if (j < 50 || j > Main.maxTilesY - 50)
			{
				return false;
			}
			int num = 35;
			Rectangle rectangle = new Rectangle((i - num) * 16, (j - num) * 16, num * 2 * 16, num * 2 * 16);
			for (int k = 0; k < 255; k++)
			{
				if (Main.player[k].active && !ignorePlayers)
				{
					Rectangle value = new Rectangle((int)(Main.player[k].position.X + (float)(Main.player[k].width / 2) - (float)(NPC.sWidth / 2) - (float)NPC.safeRangeX), (int)(Main.player[k].position.Y + (float)(Main.player[k].height / 2) - (float)(NPC.sHeight / 2) - (float)NPC.safeRangeY), NPC.sWidth + NPC.safeRangeX * 2, NPC.sHeight + NPC.safeRangeY * 2);
					if (rectangle.Intersects(value))
					{
						return false;
					}
				}
			}
			for (int l = 0; l < 200; l++)
			{
				if (Main.npc[l].active)
				{
					Rectangle value2 = new Rectangle((int)Main.npc[l].position.X, (int)Main.npc[l].position.Y, Main.npc[l].width, Main.npc[l].height);
					if (rectangle.Intersects(value2))
					{
						return false;
					}
				}
			}

			#region actual moonstone generation, made by Wombat
			FastNoiseLite noise = new FastNoiseLite();
			noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
			noise.SetFractalType(FastNoiseLite.FractalType.FBm);
			noise.SetFrequency(0.05f);

			float radius = 16;
			float xFrequency = 1.5f;
			float yFrequency = 1;
			float strength = 1;

			for (int y = (int)(j - radius * 1.5f / yFrequency); y <= j + radius * 1.5f / yFrequency; y++)
			{
				for (int x = (int)(i - radius * 1.5f / xFrequency); x <= i + radius * 1.5f / xFrequency; x++)
				{
					if (noise.GetNoise(x * xFrequency, y * yFrequency) <= (1 - (Vector2.Distance(new Vector2((x - i) * xFrequency + i, (y - j) * yFrequency + j), new Vector2(i, j)) / radius)) * strength)
					{
						Framing.GetTileSafely(x, y).HasTile = true;
						Framing.GetTileSafely(x, y).TileType = (ushort)ModContent.TileType<MoonstoneOre>(); 
					}
				}
			}
			#endregion

			return true;
        }
    }
}
