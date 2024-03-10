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
			(r, g, b) = (0.7f, 0.7f, 0.7f);
		}
	}
}
