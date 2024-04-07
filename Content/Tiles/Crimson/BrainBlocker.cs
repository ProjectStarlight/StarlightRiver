using StarlightRiver.Content.Bosses.BrainRedux;

namespace StarlightRiver.Content.Tiles.Crimson
{
	internal class BrainBlocker : ModTile
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Crimson/" + Name;

		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileMergeDirt[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileLighted[Type] = true;

			HitSound = Terraria.ID.SoundID.NPCHit1;
			DustType = Terraria.ID.DustID.Blood;

			MinPick = int.MaxValue;

			AddMapEntry(new Color(167, 180, 191));
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			(r, g, b) = (0.7f * BrainOfCthulu.ArenaOpacity, 0.4f * BrainOfCthulu.ArenaOpacity, 0.2f * BrainOfCthulu.ArenaOpacity);
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			var tex = ModContent.Request<Texture2D>(Texture).Value;

			var tile = Framing.GetTileSafely(i, j);

			spriteBatch.Draw(tex, (new Vector2(i, j) + Helpers.Helper.TileAdj) * 16 - Main.screenPosition, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), new Color(200, 90, 70) * BrainOfCthulu.ArenaOpacity);
			return false;
		}
	}
}
