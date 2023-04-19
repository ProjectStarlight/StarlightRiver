﻿using System;
using Terraria.DataStructures;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Permafrost
{
	class PermafrostIce : ModTile
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Permafrost/PermafrostIce";

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSet(this, 0, DustID.Ice, SoundID.Tink, new Color(90, 208, 232), ItemType<PermafrostIceItem>());
			Main.tileMerge[Type][TileID.SnowBlock] = true;
			Main.tileMerge[TileID.SnowBlock][Type] = true;

			Main.tileMerge[Type][TileID.IceBlock] = true;
			Main.tileMerge[TileID.IceBlock][Type] = true;
		}

		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
		{
			float off = (float)Math.Sin((i + j) * 0.2f) * 300 + (float)Math.Cos(j * 0.15f) * 200;

			float sin2 = (float)Math.Sin(StarlightWorld.visualTimer + off * 0.01f * 0.2f);
			float cos = (float)Math.Cos(StarlightWorld.visualTimer + off * 0.01f);
			var color = new Color(100 * (1 + sin2) / 255f, 140 * (1 + cos) / 255f, 180 / 255f);
			Color light = Lighting.GetColor(i, j);

			drawData.colorTint = new Color(light.R + (int)(color.R * 0.1f), light.G + (int)(color.G * 0.1f), light.B + (int)(color.B * 0.1f));
		}
	}

	class PermafrostIceItem : QuickTileItem
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Permafrost/PermafrostIceItem";

		public PermafrostIceItem() : base("Permafrost Ice", "", "PermafrostIce", ItemRarityID.White) { }
	}
}