using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Blockers
{
	internal class InvisibleWall : ModTile
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			TileID.Sets.DrawsWalls[Type] = true;
			Main.tileBlockLight[Type] = false;
			MinPick = int.MaxValue;
		}

		public override bool CanExplode(int i, int j)
		{
			return false;
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			if (StarlightRiver.debugMode)
				spriteBatch.Draw(Assets.MagicPixel.Value, new Rectangle(i * 16 - (int)Main.screenPosition.X + Main.offScreenRange, j * 16 - (int)Main.screenPosition.Y + Main.offScreenRange, 16, 16), Color.Red * 0.25f);
		}
	}
}