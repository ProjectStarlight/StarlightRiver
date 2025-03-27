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
	class ChartGrate : ModTile
	{
		public override string Texture => AssetDirectory.StarlightTile + Name;

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSet(this, int.MaxValue, DustID.Gold, SoundID.Tink, new Color(255, 210, 150), ModContent.ItemType<ChartGrateItem>());
			Main.tileBlockLight[Type] = false;
			MinPick = 100;
		}

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
		{
			Framing.SelfFrame8Way(i, j, Main.tile[i, j], resetFrame);

			Tile tile = Main.tile[i, j];
			tile.TileFrameX += (short)(i % 6 * 324);
			tile.TileFrameY += (short)(j % 6 * 90);

			return false;
		}
	}

	class ChartGrateItem : QuickTileItem
	{
		public override string Texture => AssetDirectory.StarlightTile + Name;

		public ChartGrateItem() : base("Chart Grate", "Composite", "ChartGrate", ItemRarityID.White) { }
	}
}