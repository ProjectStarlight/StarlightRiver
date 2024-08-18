using StarlightRiver.Core.Systems;
using Terraria.Graphics;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
	class VitricTempleWall : ModWall
	{
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
			Tile tile = Main.tile[i, j];

			var frame = new Rectangle(i % 14 * 16, j % 25 * 16, 16, 16);
			var frame2 = new Rectangle(tile.WallFrameX, tile.WallFrameY, 32, 32);

			Lighting.GetCornerColors(i, j, out VertexColors vertices);

			if (!(frame2.Intersects(new Rectangle(36, 36, 36 * 3, 36)) || frame2.Intersects(new Rectangle(36 * 6, 36, 36 * 3, 36 * 2)) || frame2.Intersects(new Rectangle(36 * 10, 0, 36 * 2, 36 * 3))))
				Main.tileBatch.Draw(Assets.Tiles.Vitric.VitricTempleWallEdge.Value, new Vector2(i * 16 - (int)Main.screenPosition.X + Main.offScreenRange - 8, j * 16 - (int)Main.screenPosition.Y + Main.offScreenRange - 8), frame2, vertices, Vector2.Zero, 1f, SpriteEffects.None);

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