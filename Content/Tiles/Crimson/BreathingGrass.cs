using System;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ObjectData;

namespace StarlightRiver.Content.Tiles.Crimson
{
	internal class BreathingGrass : ModTile
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Crimson/GrassSway";

		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = false;

			TileID.Sets.MultiTileSway[Type] = true;
			TileID.Sets.ReverseVineThreads[Type] = true;

			TileObjectData.newTile.AnchorBottom = new(AnchorType.AlternateTile | AnchorType.SolidTile, 1, 0);
			TileObjectData.newTile.AnchorAlternateTiles = [TileID.CrimsonGrass, Type];
			QuickBlock.QuickSetFurniture(this, 1, 1, DustID.Blood, SoundID.Grass, Color.Red);
		}

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
		{
			Tile tile = Main.tile[i, j];

			tile.frameX = (short)(i % 2 * 18);

			tile.frameY = (short)(18 * Main.rand.Next(1, 4));

			Tile up = Framing.GetTileSafely(i, j - 1);

			if (up.type != Type)
				tile.frameY = 0;

			return false;
		}

		public override void NearbyEffects(int i, int j, bool closer)
		{
			float absSpeed = Math.Abs(Main.windSpeedCurrent);
			if (absSpeed > 0.4f && Main.rand.NextBool(25) && Main.rand.NextFloat() < (absSpeed - 0.4f))
			{
				Color color = new Color(1f, 0, 0, 0) * absSpeed;

				if (Main.rand.NextBool(3))
					color = new Color(0f, 0, 1f, 0) * absSpeed;

				Dust.NewDustPerfect(new Vector2(i, j - Main.rand.NextFloat(6)) * 16, ModContent.DustType<Dusts.PixelatedEmber>(), new Vector2(Main.windSpeedCurrent * Main.rand.NextFloat(1f, 3.5f), Main.rand.NextFloat(-1, -2)) * Main.rand.NextFloat(2), 0, color, 0.1f);
			}

			Main.instance.TilesRenderer.Wind.GetWindTime(i, j - 2, 40, out int windTimeLeft, out int directionX, out int directionY);
			Vector2 wind = new Vector2(directionX, directionY) * MathF.Sin(windTimeLeft / 40f * 3.14f);

			if (windTimeLeft == 1)
			{
				Color color = new Color(1f, 0, 0, 0);

				if (Main.rand.NextBool(3))
					color = new Color(0f, 0, 1f, 0);

				Dust.NewDustPerfect(new Vector2(i, j - Main.rand.NextFloat(6)) * 16, ModContent.DustType<Dusts.PixelatedEmber>(), new Vector2(wind.X * -15f, Main.rand.NextFloat(-0.5f, -1)) * Main.rand.NextFloat(2), 0, color, 0.1f);
			}
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomNonSolid);
			return false;
		}

		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Tile tile = Framing.GetTileSafely(i, j);
			Texture2D tex = Assets.Tiles.Crimson.BreathingGrass.Value;
			Vector2 pos = new Vector2(i, j + 1) * 16;
			Vector2 logicPos = new Vector2(i, j + 1) * 16;
			Color color = Lighting.GetColor(i, j); // make sure we only get lighting once since thats expensive

			int seed = (i ^ i * i) / 2;

			void DrawGrass(int seed, Color color, float scale)
			{
				int variant = seed % 3;
				int offset = seed % 16 - 8;

				float breath = (float)Math.Sin(Main.windCounter * 4 / (60 * 6f) * 6.28f + i % 20 / 20f * 3.14f) * Main.windSpeedCurrent;
				float sway = 0f;

				Main.instance.TilesRenderer.Wind.GetWindTime(i, j - 2, 40, out int windTimeLeft, out int directionX, out int directionY);
				Vector2 wind = new Vector2(directionX, directionY) * MathF.Sin(windTimeLeft / 40f * 3.14f);

				sway += Main.windSpeedCurrent * scale * 0.08f;
				sway += wind.X * (1f - scale / 20f);

				pos.X += offset;
				logicPos.X += offset;
				pos.Y += 2;

				float baseLen = 8;
				float dist = tile.TileFrameX;
				float swayOff = (tile.TileFrameY - 300) * 0.01f;

				float mag = (float)Math.Sin((dist - 20) / 140f * 3.14f);

				if (dist < 160 && dist > 20)
				{
					baseLen += 4 * mag;
					sway += swayOff * mag;
				}

				Vector2 lastPos = pos;

				int maxSegs = tex.Width / 8;
				int frameStart = maxSegs - (int)scale;

				for (int k = 0; k < scale; k++)
				{
					Rectangle source = new((frameStart + k) * 8, variant * 10, 8, 8);
					var origin = new Vector2(0, 2);
					float rot = -1.57f + sway * (0.2f + k * (0.03f + breath * 0.015f));
					float len = baseLen;

					spriteBatch.Draw(tex, lastPos - Main.screenPosition, source, color, rot, origin, 1, 0, 0);

					lastPos += Vector2.UnitX.RotatedBy(rot) * len;
				}
			}

			DrawGrass((seed ^ i * i) / 3, color * 0.25f, 16f);
			DrawGrass((seed ^ i * i) / 2, color * 0.6f, 12f);
			DrawGrass(seed, color, 8f);
		}
	}
}