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
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

			Tile tile = Framing.GetTileSafely(i, j);

<<<<<<< HEAD
=======
			//if (!tile.IsActuated)
			//spriteBatch.Draw(tex, (new Vector2(i, j) + Helpers.Helper.TileAdj) * 16 - Main.screenPosition, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), new Color(200, 90, 70) * DeadBrain.ArenaOpacity);

>>>>>>> eb3c8d12fce82701d2256b77c498d5fb1b231b54
			return false;
		}
	}
}