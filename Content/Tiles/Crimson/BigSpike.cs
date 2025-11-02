using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core.Systems.DummyTileSystem;
using StarlightRiver.Core.Systems.LightingSystem;
using System;
using Terraria.ID;
using Terraria.ObjectData;

namespace StarlightRiver.Content.Tiles.Crimson
{
	internal class BigSpike : DummyTile
	{
		public override string Texture => AssetDirectory.Invisible;

		public override int DummyType => DummySystem.DummyType<BigSpikeDummy>();

		public override void SetStaticDefaults()
		{
			TileObjectData.newTile.AnchorBottom = new(Terraria.Enums.AnchorType.SolidTile, 1, 0);
			TileObjectData.newTile.AnchorLeft = new(Terraria.Enums.AnchorType.SolidTile, 1, 0);
			TileObjectData.newTile.AnchorRight = new(Terraria.Enums.AnchorType.SolidTile, 1, 0);
			TileObjectData.newTile.AnchorTop = new(Terraria.Enums.AnchorType.SolidTile, 1, 0);

			QuickBlock.QuickSetFurniture(this, 1, 1, DustID.Bone, SoundID.NPCHit1, new Color(60, 40, 20));
		}

		public override bool SpawnConditions(int i, int j)
		{
			return true;
		}

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
		{
			Vector2 final = Vector2.Zero;
			int count = 0;

			for (int x = 0; x < 3; x++)
			{
				for (int y = 0; y < 3; y++)
				{
					if (x == 1 && y == 1)
						continue;

					Tile scan = Framing.GetTileSafely(i - 1 + x, j - 1 + y);
					bool occluded = scan.HasTile && Main.tileSolid[scan.TileType];

					if (!occluded)
						final += Vector2.Normalize(new Vector2(x - 1, y - 1));
					else
						count++;
				}
			}

			Tile tile = Framing.GetTileSafely(i, j);
			float angle = final.ToRotation();

			if (float.IsNaN(angle) || count <= 0)
			{
				WorldGen.KillTile(i, j);
				return false;
			}

			tile.TileFrameX = (short)(angle % 6.28f * 100);

			if (WorldGen.generatingWorld)
				tile.TileFrameY = (short)Main.rand.Next(6);

			return false;
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
			var origin = new Vector2(48, 120 - 64);
			Vector2 pos = Center - Main.screenPosition;

			LightingBufferRenderer.DrawWithLighting(tex, pos, source, Color.White, angle - 0.25f, origin, 1f);
		}
	}
}