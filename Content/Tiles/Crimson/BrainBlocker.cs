﻿using StarlightRiver.Content.Bosses.TheThinkerBoss;

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
			return false;
		}
	}
}