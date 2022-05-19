using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.Forest
{
	internal class ThickTree : ModTile
	{
		public override string Texture => AssetDirectory.ForestTile + Name;

		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileSolid[Type] = false;
			Main.tileAxe[Type] = true;
			ItemDrop = ItemID.Wood;
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			base.PostDraw(i, j, spriteBatch);
		}

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			if (fail || effectOnly)
				return;

			Framing.GetTileSafely(i, j).HasTile = false;

			var left = Framing.GetTileSafely(i - 1, j).TileType == ModContent.TileType<ThickTree>();
			var right = Framing.GetTileSafely(i + 1, j).TileType == ModContent.TileType<ThickTree>();
			var up = Framing.GetTileSafely(i, j - 1).TileType == ModContent.TileType<ThickTree>();
			var down = Framing.GetTileSafely(i, j + 1).TileType == ModContent.TileType<ThickTree>();

			if (left) WorldGen.KillTile(i - 1, j);
			if (right) WorldGen.KillTile(i + 1, j);
			if (up) WorldGen.KillTile(i, j - 1);
			if (down) WorldGen.KillTile(i, j - 1);
		}

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
		{
			short x = 0;
			short y = 0;

			var left = Framing.GetTileSafely(i - 1, j).TileType == ModContent.TileType<ThickTree>();
			var right = Framing.GetTileSafely(i + 1, j).TileType == ModContent.TileType<ThickTree>();
			var up = Framing.GetTileSafely(i, j - 1).TileType == ModContent.TileType<ThickTree>();
			var down = Framing.GetTileSafely(i, j + 1).TileType == ModContent.TileType<ThickTree>();

			if ((up || down))
			{
				if (right)
					x = 0;
				if (left)
					x = 18;

				y = (short)(Main.rand.Next(3) * 18);

				if (Main.rand.NextBool(3))
					x += 18 * 2;
			}

			var tile = Framing.GetTileSafely(i, j);
			tile.TileFrameX = x;
			tile.TileFrameY = y;

			return false;
		}
	}
}
