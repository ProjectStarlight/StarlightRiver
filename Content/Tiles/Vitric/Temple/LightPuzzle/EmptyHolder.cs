using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Vitric.Temple.LightPuzzle
{
	internal class EmptyHolder : ModTile
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 3, 3, DustID.Copper, SoundID.Tink, new Color(100, 100, 50));
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Tile tile = Framing.GetTileSafely(i, j);

			if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
			{
				Vector2 pos = (new Vector2(i, j) + Helpers.Helper.TileAdj) * 16 + Vector2.One * 24;
				Color lighting = Lighting.GetColor(i, j);

				Texture2D texUnder = ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "MirrorUnder").Value;
				Main.spriteBatch.Draw(texUnder, pos - Main.screenPosition, null, lighting, 0, texUnder.Size() / 2, 1, 0, 0);
			}
		}
	}
}