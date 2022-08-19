using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.Enums;
using StarlightRiver.Core.Loaders;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.ObjectData;
using Terraria;
using static StarlightRiver.Helpers.Helper;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
	public class TempleCandleCrystal : ModTile
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Vitric/TempleDecoration/" + Name;

		public override void SetStaticDefaults()
		{
			Main.tileLighted[Type] = true;
			this.QuickSetFurniture(1, 2, 0, SoundID.Dig, false, new Color(80, 131, 142), false, false, "Candle", AnchorTableTop(1, true));
			ItemDrop = ModContent.ItemType<TempleCandleCrystalItem>();
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			Tile tile = Main.tile[i, j];
			if (tile.TileFrameX == 0)
			{
				r = 0.4f;
				g = 1f;
				b = 0.87f;
			}
		}

		const int height = 2;
		public override void HitWire(int i, int j)
		{
			Tile tile = Main.tile[i, j];
			int topY = j - tile.TileFrameY / 18 % height;
			short frameAdjustment = (short)(tile.TileFrameX > 0 ? -18 : 18);

			for (int h = 0; h < height; h++)
				Main.tile[i, topY + h].TileFrameX += frameAdjustment;

			for (int k = 0; k < height; k++)
				Wiring.SkipWire(i, topY + k);

			if (Main.netMode != NetmodeID.SinglePlayer)
				NetMessage.SendTileSquare(-1, i, topY + (int)Math.Floor(height / 2f), height, TileChangeType.None);
		}
	}

	public class TempleCandleCrystalItem : QuickTileItem
	{
		public TempleCandleCrystalItem() : base("Temple Crystal Candle", "", "TempleCandleCrystal", ItemRarityID.White, "StarlightRiver/Assets/Tiles/Vitric/TempleDecoration/") { }
	}
}
