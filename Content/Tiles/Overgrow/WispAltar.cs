using Terraria.ID;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Overgrow
{
	class WispAltarL : ModTile
	{
		public override string Texture => AssetDirectory.OvergrowTile + "WispAltarL";

		public override void SetStaticDefaults()
		{
			TileObjectData.newTile.DrawYOffset = 2;
			QuickBlock.QuickSetFurniture(this, 6, 11, DustType<Dusts.GoldNoMovement>(), SoundID.Tink, false, new Color(200, 200, 200));
		}
	}

	class WispAltarLItem : QuickTileItem
	{
		public override string Texture => AssetDirectory.Debug;

		public WispAltarLItem() : base("Wisp Altar L Placer", "Debug Item", "WispAltarL", -1) { }
	}

	class WispAltarR : ModTile
	{
		public override string Texture => AssetDirectory.OvergrowTile + "WispAltarR";

		public override void SetStaticDefaults()
		{
			TileObjectData.newTile.DrawYOffset = 2;
			QuickBlock.QuickSetFurniture(this, 6, 11, DustType<Dusts.GoldNoMovement>(), SoundID.Tink, false, new Color(200, 200, 200));
		}
	}

	class WispAltarRItem : QuickTileItem
	{
		public override string Texture => AssetDirectory.Debug;

		public WispAltarRItem() : base("Wisp Altar R Placer", "Debug Item", "WispAltarR", -1) { }
	}
}