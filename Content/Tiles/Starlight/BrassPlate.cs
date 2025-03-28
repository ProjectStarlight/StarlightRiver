using StarlightRiver.Core.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Starlight
{
	class BrassPlate : ModTile
	{
		public override string Texture => AssetDirectory.StarlightTile + Name;

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSet(this, int.MaxValue, DustID.Gold, SoundID.Tink, new Color(255, 210, 150), ModContent.ItemType<BrassPlateItem>());
			MinPick = 100;
		}

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
		{
			Framing.SelfFrame8Way(i, j, Main.tile[i, j], resetFrame);

			Tile tile = Main.tile[i, j];
			tile.TileFrameX += (short)(i % 3 * 324);
			tile.TileFrameY += (short)(j % 3 * 90);

			return false;
		}
	}

	class BrassPlateItem : QuickTileItem
	{
		public override string Texture => AssetDirectory.StarlightTile + Name;

		public BrassPlateItem() : base("Brass Plate", "Composite", "BrassPlate", ItemRarityID.White) { }
	}
}