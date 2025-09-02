using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Starlight
{
	internal class BlinkbrickWall : ModWall
	{
		public override string Texture => AssetDirectory.StarlightTile + Name;

		public override void SetStaticDefaults() { this.QuickSetWall(DustID.Stone, SoundID.Tink.WithPitchOffset(-0.5f), ModContent.ItemType<BlinkbrickWallItem>(), true, new Color(20, 25, 40)); }
	}

	class BlinkbrickWallItem : QuickWallItem
	{
		public BlinkbrickWallItem() : base("Blinkbrick Wall", "", ModContent.WallType<BlinkbrickWall>(), ItemRarityID.White, AssetDirectory.StarlightTile) { }
	}
}