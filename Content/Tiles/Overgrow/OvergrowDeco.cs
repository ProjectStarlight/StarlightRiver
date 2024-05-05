﻿using StarlightRiver.Core.Systems;
using StarlightRiver.Helpers;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Overgrow
{
	class Rock2x2 : ModTile
	{
		public override string Texture => AssetDirectory.OvergrowTile + "Rock2x2";

		public override void SetStaticDefaults()
		{
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
			TileObjectData.newTile.DrawYOffset = 2;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.RandomStyleRange = 3;
			QuickBlock.QuickSetFurniture(this, 2, 2, DustID.Stone, SoundID.Tink, false, new Color(50, 50, 75));
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Tile tile = Framing.GetTileSafely(i, j);
			Texture2D tex = Assets.Tiles.Overgrow.Rock2x2Glow.Value;
			Vector2 pos = (new Vector2(i, j) + Helper.TileAdj) * 16 - Main.screenPosition;

			spriteBatch.Draw(tex, pos, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), Color.White);
			Lighting.AddLight(new Vector2(i, j) * 16, new Vector3(110, 200, 225) * 0.0015f);
		}
	}

	[SLRDebug]
	class Rock2x2Item : QuickTileItem
	{
		public override string Texture => AssetDirectory.OvergrowTile + Name;

		public Rock2x2Item() : base("2x2 rock placer", "It places... Rocks", "Rock2x2", 7) { }
	}
}