using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core.Systems.DummyTileSystem;
using StarlightRiver.Core.Systems.LightingSystem;
using System;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Crimson
{
	internal class BigSpike : DummyTile
	{
		public override string Texture => AssetDirectory.Invisible;

		public override int DummyType => DummySystem.DummyType<BigSpikeDummy>();

		static readonly (int dx, int dy)[] directions = new (int, int)[]
		{
			(-1, -1), (0, -1), (1, -1),
			(-1,  0),         (1,  0),
			(-1,  1), (0,  1), (1,  1)
		};

		static readonly float[] angles = new float[]
		{
			(float)(Math.PI * 5 / 4), (float)(Math.PI * 3 / 2), (float)(Math.PI * 7 / 4),
			(float)Math.PI,         float.NaN,     0f,
			(float)(Math.PI * 3 / 4), (float)(Math.PI / 2),     (float)(Math.PI / 4)
		};

		public static float GetArrowDirection(bool[,] grid)
		{
			int[] openCounts = new int[8];

			for (int i = 0; i < directions.Length; i++)
			{
				int dx = directions[i].dx;
				int dy = directions[i].dy;
				int count = 0;

				for (int step = 1; step < 3; step++)
				{
					int x = 1 + dx * step;
					int y = 1 + dy * step;

					if (x < 0 || x >= 3 || y < 0 || y >= 3 || grid[x, y])
						break;

					count++;
				}

				openCounts[i] = count;
			}

			int bestIndex = -1;
			int maxOpen = 0;

			for (int i = 0; i < openCounts.Length; i++)
			{
				if (openCounts[i] > maxOpen)
				{
					maxOpen = openCounts[i];
					bestIndex = i;
				}
			}

			if (bestIndex == -1)
			{
				return float.NaN;
			}

			return angles[bestIndex];
		}

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 1, 1, DustID.Bone, SoundID.NPCHit10, new Color(60, 40, 20));
		}

		public override bool SpawnConditions(int i, int j)
		{
			return true;
		}

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
		{
			bool[,] occlusions = new bool[3, 3];

			for (int x = 0; x < 3; x++)
			{
				for (int y = 0; y < 3; y++)
				{
					Tile scan = Framing.GetTileSafely(i - 1 + x, j - 1 + y);
					occlusions[x, y] = scan.HasTile && Main.tileSolid[scan.TileType];
				}
			}

			Tile tile = Framing.GetTileSafely(i, j);
			float angle = GetArrowDirection(occlusions);

			if (float.IsNaN(angle))
			{
				WorldGen.KillTile(i, j);
				return true;
			}

			tile.TileFrameX = (short)(angle * 100);

			return true;
		}
	}

	public class BigSpikeDummy : Dummy
	{
		public BigSpikeDummy() : base(ModContent.TileType<BigSpike>(), 16, 16) { }

		public override void DrawBehindTiles()
		{
			float angle = Parent.TileFrameX / 100f + 1.57f / 2f;

			Texture2D tex = Assets.Tiles.Crimson.BigSpike.Value;
			var source = new Rectangle(0, Parent.TileFrameY * 120, 160, 120);
			var origin = new Vector2(48, 120 - 48);
			var pos = Center - Main.screenPosition;

			LightingBufferRenderer.DrawWithLighting(tex, pos, source, Color.White, angle, origin, 1f);

			var arrow = Assets.MagicPixel.Value;
			Main.spriteBatch.Draw(arrow, new Rectangle((int)pos.X, (int)pos.Y, 100, 2), source, Color.LimeGreen, angle - 1.57f / 2f, origin, 0, 0);
		}
	}
}
