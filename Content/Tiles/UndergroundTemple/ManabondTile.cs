using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Items.Manabonds;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.UndergroundTemple
{
	class ManabondTile : ModTile
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/UndergroundTemple/" + Name;

		public override void SetStaticDefaults()
		{
			TileID.Sets.DrawsWalls[Type] = true;
			QuickBlock.QuickSetFurniture(this, 3, 3, DustID.MagicMirror, SoundID.DD2_DarkMageHealImpact, false, new Color(50, 50, 25), false, false, "Manabond");

			RegisterItemDrop(ModContent.ItemType<BasicManabond>());
		}

		public override void NearbyEffects(int i, int j, bool closer)
		{
			if (Main.rand.NextBool(60))
				Dust.NewDustPerfect(new Vector2(i, j) * 16 + new Vector2(Main.rand.Next(16), Main.rand.Next(16)), ModContent.DustType<Dusts.ArtifactSparkles.BlueArtifactSparkle>(), Vector2.Zero);
		}

		public override void MouseOver(int i, int j)
		{
			Player player = Main.LocalPlayer;
			player.cursorItemIconID = ModContent.ItemType<BasicManabond>();
			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
		}

		public override bool RightClick(int i, int j)
		{
			WorldGen.KillTile(i, j);

			NetMessage.SendTileSquare(Main.myPlayer, i, j, 1, 1);

			return true;
		}
	}
}