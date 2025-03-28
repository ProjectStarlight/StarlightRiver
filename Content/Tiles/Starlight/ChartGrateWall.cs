using System.Reflection;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.Graphics;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Starlight
{
	class ChartGrateWall : ModWall
	{
		public override string Texture => AssetDirectory.StarlightTile + Name;

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetWall(this, DustID.Gold, SoundID.Tink, ModContent.ItemType<ChartGrateWallItem>(), true, new Color(90, 70, 40));
			Main.wallLight[Type] = true;
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Tile tile = Main.tile[i, j];

			var tex = Assets.Tiles.Starlight.ChartGrateWall.Value;
			Lighting.GetCornerColors(i, j, out var vertices);

			var frame = new Rectangle(tile.WallFrameX + i % 6 * 468, tile.WallFrameY + j % 6 * 180, 32, 32);

			Vector2 vector = new Vector2(Main.offScreenRange, Main.offScreenRange);
			Main.tileBatch.Draw(tex, new Vector2(i * 16 - (int)Main.screenPosition.X - 8, j * 16 - (int)Main.screenPosition.Y - 8) + vector, frame, vertices, Vector2.Zero, 1f, SpriteEffects.None);

			return false;
		}
	}

	class ChartGrateWallItem : QuickWallItem
	{
		public override string Texture => AssetDirectory.StarlightTile + Name;

		public ChartGrateWallItem() : base("Chart Grate Wall", "", ModContent.WallType<ChartGrateWall>(), ItemRarityID.White) { }
	}
}
