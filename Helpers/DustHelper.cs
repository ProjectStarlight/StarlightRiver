using System;
using Terraria.ID;

namespace StarlightRiver.Helpers
{
	public static class DustHelper
	{
		/// <summary>
		/// Spawns dust in a pattern determined by a Texture
		/// </summary>
		/// <param name="position"></param>
		/// <param name="dustType"></param>
		/// <param name="size"></param>
		/// <param name="tex"></param>
		/// <param name="dustSize"></param>
		/// <param name="Alpha"></param>
		/// <param name="color"></param>
		/// <param name="noGravity"></param>
		/// <param name="rot"></param>
		public static void SpawnImagePattern(Vector2 position, int dustType, float size, Texture2D tex, float dustSize = 1f, int Alpha = 0, Color? color = null, bool noGravity = true, float rot = 0.34f, float chance = 1f)
		{
			if (Main.netMode != NetmodeID.Server)
			{
				float rotation = Main.rand.NextFloat(0 - rot, rot);
				var data = new Color[tex.Width * tex.Height];
				tex.GetData(data);

				color ??= Color.White;

				for (int i = 0; i < tex.Width; i += 2)
				{
					for (int j = 0; j < tex.Height; j += 2)
					{
						if (Main.rand.NextFloat() < chance)
						{
							Color alpha = data[j * tex.Width + i];

							if (alpha == new Color(255, 255, 255))
							{
								double dustX = i - tex.Width / 2;
								double dustY = j - tex.Height / 2;
								dustX *= size;
								dustY *= size;

								var d = Dust.NewDustPerfect(position, dustType, new Vector2((float)dustX, (float)dustY).RotatedBy(rotation), Alpha, (Color)color, dustSize);

								if (d != null)
									d.noGravity = noGravity;
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Spawns a still set of dusts from an image
		/// </summary>
		/// <param name="position"></param>
		/// <param name="dustType"></param>
		/// <param name="size"></param>
		/// <param name="tex"></param>
		/// <param name="dustSize"></param>
		/// <param name="Alpha"></param>
		/// <param name="color"></param>
		/// <param name="noGravity"></param>
		/// <param name="rot"></param>
		/// <param name="chance"></param>
		public static void SpawnStillImagePattern(Vector2 position, int dustType, float size, Texture2D tex, float dustSize = 1f, int Alpha = 0, Color? color = null, bool noGravity = true, float randomVel = 0f, float chance = 1f)
		{
			if (Main.netMode != NetmodeID.Server)
			{
				var data = new Color[tex.Width * tex.Height];
				tex.GetData(data);

				color ??= Color.White;

				for (int i = 0; i < tex.Width; i += 2)
				{
					for (int j = 0; j < tex.Height; j += 2)
					{
						if (Main.rand.NextFloat() < chance)
						{
							Color alpha = data[j * tex.Width + i];

							if (alpha == new Color(255, 255, 255))
							{
								float dustX = i - tex.Width / 2;
								float dustY = j - tex.Height / 2;
								dustX *= size;
								dustY *= size;

								var d = Dust.NewDustPerfect(position + new Vector2(dustX, dustY), dustType, Vector2.UnitX.RotatedByRandom(6.28f) * randomVel, Alpha, (Color)color, dustSize);

								if (d != null)
									d.noGravity = noGravity;
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Spawns dust in a star pattern
		/// </summary>
		/// <param name="position"></param>
		/// <param name="dustType"></param>
		/// <param name="pointAmount"></param>
		/// <param name="mainSize"></param>
		/// <param name="dustDensity"></param>
		/// <param name="dustSize"></param>
		/// <param name="pointDepthMult"></param>
		/// <param name="pointDepthMultOffset"></param>
		/// <param name="randomAmount"></param>
		/// <param name="rotationAmount"></param>
		/// <param name="color"></param>
		public static void SpawnStarPattern(Vector2 position, int dustType, float pointAmount = 5, float mainSize = 1, float dustDensity = 1, float dustSize = 1f, float pointDepthMult = 1f, float pointDepthMultOffset = 0.5f, float randomAmount = 0, float rotationAmount = -1, Color? color = null)
		{
			float rot;

			color ??= Color.White;

			if (rotationAmount < 0)
				rot = Main.rand.NextFloat(0, (float)Math.PI * 2);
			else
				rot = rotationAmount;

			float density = 1 / dustDensity * 0.1f;

			for (float k = 0; k < 6.28f; k += density)
			{
				float rand = 0;

				if (randomAmount > 0)
					rand = Main.rand.NextFloat(-0.01f, 0.01f) * randomAmount;

				float x = (float)Math.Cos(k + rand);
				float y = (float)Math.Sin(k + rand);
				float mult = Math.Abs(k * (pointAmount / 2) % (float)Math.PI - (float)Math.PI / 2) * pointDepthMult + pointDepthMultOffset;//triangle wave function
				Dust.NewDustPerfect(position, dustType, new Vector2(x, y).RotatedBy(rot) * mult * mainSize, 0, (Color)color, dustSize);
			}
		}

		/// <summary>
		/// Spawns dust in a lightning bolt pattern between two points
		/// </summary>
		/// <param name="point1"></param>
		/// <param name="point2"></param>
		/// <param name="dusttype"></param>
		/// <param name="scale"></param>
		/// <param name="armLength"></param>
		/// <param name="color"></param>
		/// <param name="frequency"></param>
		public static void SpawnElectricityPattern(Vector2 point1, Vector2 point2, int dusttype, float scale = 1, int armLength = 30, Color color = default, float frequency = 0.05f)
		{
			int nodeCount = (int)Vector2.Distance(point1, point2) / armLength;
			var nodes = new Vector2[nodeCount + 1];

			nodes[nodeCount] = point2; //adds the end as the last point

			for (int k = 1; k < nodes.Length; k++)
			{
				//Sets all intermediate nodes to their appropriate randomized dot product positions
				nodes[k] = Vector2.Lerp(point1, point2, k / (float)nodeCount) +
					(k == nodes.Length - 1 ? Vector2.Zero : Vector2.Normalize(point1 - point2).RotatedBy(1.58f) * Main.rand.NextFloat(-armLength / 2, armLength / 2));

				//Spawns the dust between each node
				Vector2 prevPos = k == 1 ? point1 : nodes[k - 1];
				for (float i = 0; i < 1; i += frequency)
				{
					Dust.NewDustPerfect(Vector2.Lerp(prevPos, nodes[k], i), dusttype, Vector2.Zero, 0, color, scale);
				}
			}
		}

		/// <summary>
		/// Gets the type of dust that a tile would emit
		/// </summary>
		/// <param name="tile"></param>
		/// <param name="dusttype"></param>
		/// <returns></returns>
		public static int TileDust(Tile tile, ref int dusttype)
		{
			switch (tile.TileType)
			{
				case TileID.Stone: dusttype = DustID.Stone; break;
				case TileID.Sand: case TileID.Sandstone: dusttype = 32; break;
				case TileID.Granite: dusttype = DustID.Granite; break;
				case TileID.Marble: dusttype = DustID.Marble; break;
				case TileID.Grass: case TileID.JungleGrass: dusttype = DustID.Grass; break;
				case TileID.MushroomGrass: case TileID.MushroomBlock: dusttype = 96; break;

				default:
					if (TileID.Sets.Crimson[tile.TileType])
						dusttype = DustID.Blood;

					if (TileID.Sets.Corrupt[tile.TileType])
						dusttype = 14;

					if (TileID.Sets.Ices[tile.TileType] || TileID.Sets.IcesSnow[tile.TileType])
						dusttype = DustID.Ice;

					if (TileID.Sets.Snow[tile.TileType] || tile.TileType == TileID.Cloud || tile.TileType == TileID.RainCloud)
						dusttype = 51;

					break;
			}

			return dusttype;
		}
	}
}