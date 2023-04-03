using System;
using Terraria.ID;

namespace StarlightRiver.Helpers
{
	public static class DustHelper
	{
		public static void DrawDustImage(Vector2 position, int dustType, float size, Texture2D tex, float dustSize = 1f, int Alpha = 0, Color? color = null, bool noGravity = true, float rot = 0.34f)
		{
			if (Main.netMode != NetmodeID.Server)
			{
				float rotation = Main.rand.NextFloat(0 - rot, rot);
				var data = new Color[tex.Width * tex.Height];
				tex.GetData(data);

				for (int i = 0; i < tex.Width; i += 2)
				{
					for (int j = 0; j < tex.Height; j += 2)
					{
						Color alpha = data[j * tex.Width + i];

						if (alpha == new Color(255, 255, 255))
						{
							double dustX = i - tex.Width / 2;
							double dustY = j - tex.Height / 2;
							dustX *= size;
							dustY *= size;
							Dust.NewDustPerfect(position, dustType, new Vector2((float)dustX, (float)dustY).RotatedBy(rotation), Alpha, (Color)color, dustSize).noGravity = noGravity;
						}
					}
				}
			}
		}

		public static void DrawStar(Vector2 position, int dustType, float pointAmount = 5, float mainSize = 1, float dustDensity = 1, float dustSize = 1f, float pointDepthMult = 1f, float pointDepthMultOffset = 0.5f, float randomAmount = 0, float rotationAmount = -1, Color? color = null)
		{
			float rot;

			if (color is null)
				color = Color.White;

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

		public static void DrawCircle(Vector2 position, int dustType, float mainSize = 1, float RatioX = 1, float RatioY = 1, float dustDensity = 1, float dustSize = 1f, float randomAmount = 0, float rotationAmount = 0)
		{
			float rot;

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

				float x = (float)Math.Cos(k + rand) * RatioX;
				float y = (float)Math.Sin(k + rand) * RatioY;
				Dust.NewDustPerfect(position, dustType, new Vector2(x, y).RotatedBy(rot) * mainSize, 0, default, dustSize);
			}
		}

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