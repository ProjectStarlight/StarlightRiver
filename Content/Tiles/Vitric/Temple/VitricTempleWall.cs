using Terraria.Graphics;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
	class VitricTempleWall : ModWall
	{
		public override string Texture => AssetDirectory.VitricTile + "VitricTempleWallEdge";

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetWall(this, DustType<Dusts.Sand>(), SoundID.Dig, ItemType<VitricTempleWallItem>(), true, new Color(54, 48, 42));
			DustType = DustType<Dusts.Sand>();
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Texture2D tex = Request<Texture2D>(AssetDirectory.VitricTile + "VitricTempleWall").Value;
			var target = new Vector2(i * 16 - Main.screenPosition.X, j * 16 - Main.screenPosition.Y);
			target += new Vector2(Helpers.Helper.TileAdj.X * 16, Helpers.Helper.TileAdj.Y * 16);
			var source = new Rectangle(i % 14 * 16, j % 25 * 16, 16, 16);

			Tile tile = Framing.GetTileSafely(i, j);

			if (Lighting.NotRetro && !WorldGen.SolidTile(tile))
			{
				Lighting.GetCornerColors(i, j, out VertexColors vertices);
				Main.tileBatch.Draw(tex, target, source, vertices, Vector2.Zero, 1f, SpriteEffects.None);
			}
		}
	}

	class VitricTempleWallItem : QuickWallItem
	{
		public override string Texture => AssetDirectory.VitricTile + "VitricTempleWallItem";

		public VitricTempleWallItem() : base("Vitric Forge Brick Wall", "Sturdy", WallType<VitricTempleWall>(), ItemRarityID.White) { }
	}
}