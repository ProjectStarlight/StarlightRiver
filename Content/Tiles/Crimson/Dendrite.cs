using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Crimson
{
	internal class DendriteItem : QuickTileItem
	{
		public DendriteItem() : base("Dendrite", "Doyoyouuyuuhuyhh *BRRPT* *Anyuerism*", "Dendrite", 0, "StarlightRiver/Assets/Tiles/Crimson/") { }
	}

	internal class Dendrite : ModTile
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Crimson/" + Name;

		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;

			Main.tileMerge[Type][TileID.Dirt] = true;
			Main.tileMerge[TileID.Dirt][Type] = true;

			Main.tileMerge[Type][TileID.CrimsonGrass] = true;
			Main.tileMerge[TileID.CrimsonGrass][Type] = true;

			Main.tileMerge[Type][TileID.Crimstone] = true;
			Main.tileMerge[TileID.Crimstone][Type] = true;

			HitSound = Terraria.ID.SoundID.Tink;

			DustType = Terraria.ID.DustID.Blood;
			RegisterItemDrop(ModContent.ItemType<DendriteItem>());

			AddMapEntry(new Color(165, 180, 191));
		}
	}
}
