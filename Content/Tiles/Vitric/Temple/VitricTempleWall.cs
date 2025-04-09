using StarlightRiver.Core.Systems;
using Terraria.Graphics;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
	class VitricTempleWall : ModWall
	{
		// These nasty magic numbers are a hack for the framing!
		const int r1XMin = 36, r1XMax = 144, r1YMin = 36, r1YMax = 72;
		const int r2XMin = 216, r2XMax = 324, r2YMin = 36, r2YMax = 108;
		const int r3XMin = 360, r3XMax = 432, r3YMin = 0, r3YMax = 108;

		public override string Texture => AssetDirectory.VitricTile + "VitricTempleWall";

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetWall(this, DustType<Dusts.Sand>(), SoundID.Dig, ItemType<VitricTempleWallItem>(), true, new Color(54, 48, 42));
			DustType = DustType<Dusts.Sand>();
		}

		public override bool CanExplode(int i, int j)
		{
			return false;
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Lighting.GetCornerColors(i, j, out VertexColors vertices);

			Tile tile = Main.tile[i, j];

			var frame = new Rectangle(i % 14 * 16, j % 25 * 16, 16, 16);

			int fx = tile.WallFrameX;
			int fy = tile.WallFrameY;
			int fxMax = fx + 32;
			int fyMax = fy + 32;

			bool inRegion1 = fxMax > r1XMin && fx < r1XMax && fyMax > r1YMin && fy < r1YMax;
			bool inRegion2 = fxMax > r2XMin && fx < r2XMax && fyMax > r2YMin && fy < r2YMax;
			bool inRegion3 = fxMax > r3XMin && fx < r3XMax && fyMax > r3YMin && fy < r3YMax;

			if (!(inRegion1 || inRegion2 || inRegion3))
				Main.tileBatch.Draw(Assets.Tiles.Vitric.VitricTempleWallEdge.Value, new Vector2(i * 16 - (int)Main.screenPosition.X + Main.offScreenRange - 8, j * 16 - (int)Main.screenPosition.Y + Main.offScreenRange - 8), new Rectangle(tile.WallFrameX, tile.WallFrameY, 32, 32), vertices, Vector2.Zero, 1f, SpriteEffects.None);

			Main.tileBatch.Draw(Assets.Tiles.Vitric.VitricTempleWall.Value, new Vector2(i * 16 - (int)Main.screenPosition.X + Main.offScreenRange, j * 16 - (int)Main.screenPosition.Y + Main.offScreenRange), frame, vertices, Vector2.Zero, 1f, SpriteEffects.None);

			return false;
		}
	}

	[SLRDebug]
	class VitricTempleWallItem : QuickWallItem
	{
		public override string Texture => AssetDirectory.VitricTile + "VitricTempleWallItem";

		public VitricTempleWallItem() : base("Vitric Forge Brick Wall (Danger)", "{{Debug}} item", WallType<VitricTempleWall>(), ItemRarityID.White) { }
	}

	class VitricTempleWallSafe : VitricTempleWall { }

	class VitricTempleWallSafeItem : QuickWallItem
	{
		public override string Texture => AssetDirectory.VitricTile + "VitricTempleWallItem";

		public VitricTempleWallSafeItem() : base("Vitric Forge Brick Wall (Safe)", "Sturdy", WallType<VitricTempleWallSafe>(), ItemRarityID.White) { }
	}
}