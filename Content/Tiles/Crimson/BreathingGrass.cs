using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
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
			var tile = Main.tile[i, j];

			tile.frameX = (short)(i % 2 * 18);

			tile.frameY = (short)(18 * Main.rand.Next(1, 4));

			var up = Framing.GetTileSafely(i, j - 1);

			if (up.type != Type)
				tile.frameY = 0;

			return false;
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			var up = Framing.GetTileSafely(i, j - 1);
			var down = Framing.GetTileSafely(i, j + 1);

			if (down.type != Type)
				Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.ReverseVine);

			return false;
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			return;

			Tile tile = Framing.GetTileSafely(i, j);
			Texture2D tex = Assets.Tiles.Crimson.BreathingGrass.Value;
			Vector2 pos = new Vector2(i, j + 1) * 16 + Vector2.One * Main.offScreenRange;
			Vector2 logicPos = new Vector2(i, j + 1) * 16;
			Color color = Lighting.GetColor(i, j); // make sure we only get lighting once since thats expensive

			int seed = (i ^ i * i) / 2;

			void DrawGrass(int seed, Color color, float scale)
			{
				int variant = seed % 8;
				int offset = seed % 14 - 7;

				float breath = (float)Math.Sin((Main.GameUpdateCount + seed % 90) / (60 * 6f) * 6.28f);
				float sway = (float)Math.Sin((Main.GameUpdateCount + seed % (60 * 3)) / (60 * 9f) * 6.28f);

				pos.X += offset;
				logicPos.X += offset;
				pos.Y += 2;

				float baseLen = 2;
				float dist = tile.TileFrameX;
				float swayOff = (tile.TileFrameY - 300) * 0.01f;

				float mag = (float)Math.Sin((dist - 20) / 140f * 3.14f);

				if (dist < 160 && dist > 20)
				{
					baseLen += 4 * mag;
					sway += swayOff * mag;
				}

				Vector2 lastPos = pos;

				for (int k = 0; k < 8; k++)
				{
					Rectangle source = new(k * 8, variant * 6, 6, 4);
					Vector2 origin = new Vector2(0, 2);
					float rot = -1.57f + sway * (0.2f + k * (0.04f + breath * 0.02f));
					float len = baseLen * scale;

					spriteBatch.Draw(tex, lastPos - Main.screenPosition, source, color, rot, origin, scale, 0, 0);

					lastPos += Vector2.UnitX.RotatedBy(rot) * len;
				}
			}

			DrawGrass((seed ^ i * i) / 2, Color.Lerp(color, Color.Black, 0.3f) * 0.8f, 1.5f);
			DrawGrass(seed, color, 1f);
		}
	}
}