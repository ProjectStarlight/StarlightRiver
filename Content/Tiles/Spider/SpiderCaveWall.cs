using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Spider
{
	internal class SpiderCaveWall : ModWall
	{
		public override string Texture => AssetDirectory.SpiderTile + Name;

		public override void SetStaticDefaults()
		{
			this.QuickSetWall(DustID.Web, SoundID.Dig, ModContent.ItemType<SpiderCaveWallItem>(), false, new Color(40, 40, 40));
		}
	}

	internal class SpiderCaveWallItem : QuickWallItem
	{
		public SpiderCaveWallItem() : base("Spider Nest Wall", "", ModContent.WallType<SpiderCaveWall>(), 0, AssetDirectory.SpiderTile) { }
	}
}
