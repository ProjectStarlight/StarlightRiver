using StarlightRiver.Content.Biomes;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Crimson
{
	internal class DendriteItem : QuickTileItem
	{
		public DendriteItem() : base("Dendrite", "Doyoyouuyuuhuyhh *BRRPT* *Anyuerism*", "Dendrite", 0, "StarlightRiver/Assets/Tiles/Crimson/") { }
	}

	internal class Dendrite : ModTile, ICustomGraymatterDrawOver
	{
		public static int hintTicker = 0;

		public override string Texture => "StarlightRiver/Assets/Tiles/Crimson/" + Name;

		public override bool CanKillTile(int i, int j, ref bool blockDamaged)
		{
			if (StarlightWorld.HasFlag(WorldFlags.ThinkerBossOpen))
			{
				return true;
			}
			else
			{
				hintTicker++;

				if (hintTicker % 10 == 0)
					Main.NewText("This dirt seems unnaturally resilient...", Color.LightGray);

				return false;
			}
		}

		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;

			Main.tileMerge[Type][TileID.CrimsonGrass] = true;
			Main.tileMerge[TileID.CrimsonGrass][Type] = true;

			Main.tileMerge[Type][TileID.Crimstone] = true;
			Main.tileMerge[TileID.Crimstone][Type] = true;

			Main.tileMergeDirt[Type] = true;

			HitSound = Terraria.ID.SoundID.Tink;

			DustType = Terraria.ID.DustID.Dirt;
			RegisterItemDrop(ModContent.ItemType<DendriteItem>());
			GraymatterBiome.grayOverTypes.Add(Type);

			MinPick = 65;

			AddMapEntry(new Color(151, 107, 75));
		}

		public void DrawOverlay(SpriteBatch spriteBatch, int x, int y)
		{
			Tile tile = Main.tile[x, y];

			Texture2D tex = Assets.Tiles.Crimson.DendriteReal.Value;
			spriteBatch.Draw(tex, new Vector2(x, y) * 16 - Main.screenPosition, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), Color.White * 0.8f);
		}
	}
}