using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Palestone
{
	internal class PalestoneItem : QuickTileItem
	{
		public PalestoneItem() : base("Palestone", "", "Palestone", 0, AssetDirectory.PalestoneTile) { }
	}

	internal class Palestone : ModTile
	{
		public override string Texture => AssetDirectory.PalestoneTile + Name;

		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileMergeDirt[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileLighted[Type] = true;
			Main.tileStone[Type] = true;
			HitSound = Terraria.ID.SoundID.Tink;

			DustType = Terraria.ID.DustID.Stone;
			ItemDrop = ItemType<PalestoneItem>();

			AddMapEntry(new Color(167, 180, 191));
		}
	}
}