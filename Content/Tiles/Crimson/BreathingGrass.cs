using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Tiles.Crimson
{
	internal class BreathingGrass : ModTile
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = false;
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			var tex = Assets.Tiles.Crimson.BreathingGrass.Value;
			Vector2 pos = (new Vector2(i, j + 1) + Helpers.Helper.TileAdj) * 16;
			Vector2 logicPos = new Vector2(i + 0.5f, j + 1) * 16;
			var color = Lighting.GetColor(i, j); // make sure we only get lighting once since thats expensive

			int seed = (i ^ i * i) / 2;

			void DrawGrass(int seed, Color color, float scale)
			{
				int variant = seed % 8;
				int offset = seed % 14 - 7;

				float breath = (float)Math.Sin((Main.GameUpdateCount + seed % 90) / (60 * 6f) * 6.28f);
				float sway = (float)Math.Sin((Main.GameUpdateCount + seed % (60 * 9)) / (60 * 9f) * 6.28f);
				 
				pos.X += offset;
				logicPos.X += offset;
				pos.Y += 2;

				float baseLen = 4;
				var dist = Vector2.Distance(logicPos, Main.LocalPlayer.Center);

				if (dist < 160)
				{
					baseLen -= 2 * (1f - dist / 160f);
					sway += (logicPos.X < Main.LocalPlayer.Center.X ? 3 : -3) * (1f - dist / 160f);
				}

				Vector2 lastPos = pos;

				for (int k = 0; k < 8; k++)
				{
					Rectangle source = new(k * 8, variant * 6, 6, 4);
					Vector2 origin = new Vector2(0, 2);
					float rot = -1.57f + sway * (0.3f + k * (0.05f + breath * 0.03f));
					float len = (baseLen + breath * 2f) * scale;

					spriteBatch.Draw(tex, lastPos - Main.screenPosition, source, color, rot, origin, scale, 0, 0);

					lastPos += Vector2.UnitX.RotatedBy(rot) * len;
				}
			}

			DrawGrass((seed ^ i * i) / 2, Color.Lerp(color, Color.Black, 0.3f) * 0.8f, 1.5f);
			DrawGrass(seed, color, 1f);
		}
	}
}
