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

			return false;
		}
	}
}