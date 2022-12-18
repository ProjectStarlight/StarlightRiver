using System;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric
{
	class ForgeInnerDoor : ModTile
	{
		public override string Texture => AssetDirectory.VitricTile + Name;

		public override void SetStaticDefaults()
		{
			MinPick = int.MaxValue;
			TileID.Sets.DrawsWalls[Type] = true;
			QuickBlock.QuickSetFurniture(this, 2, 5, DustType<Dusts.Stamina>(), SoundID.Tink, false, new Color(200, 150, 80), false, true, "Forge Door");
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Vector2 pos = (new Vector2(i, j) + Helpers.Helper.TileAdj) * 16 - Main.screenPosition;
			Texture2D tex = Request<Texture2D>(AssetDirectory.VitricTile + "ForgeInnerDoorGlow").Value;
			Tile tile = Framing.GetTileSafely(i, j);
			var source = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16);

			spriteBatch.Draw(tex, pos, source, Color.White * (0.5f + (float)Math.Sin(StarlightWorld.visualTimer) * 0.25f));
		}
	}

	class ForgeInnerDoorItem : QuickTileItem
	{
		public ForgeInnerDoorItem() : base("Forge Inner Door", "Debug Item", "ForgeInnerDoor", 1, AssetDirectory.Debug, true) { }
	}
}
