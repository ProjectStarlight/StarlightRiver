using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
	class VitricPillarWall : ModTile
	{
		public override string Texture => AssetDirectory.VitricTile + "VitricPillarWall";

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 4, 25, DustType<Dusts.Sand>(), SoundID.Tink, new Color(54, 48, 42));
		}
	}

	class VitricPillarWallItem : QuickTileItem
	{
		public override string Texture => AssetDirectory.VitricTile + "VitricPillarWallItem";

		public VitricPillarWallItem() : base("Vitric Forge Pillar", "Sturdy", "VitricPillarWall", ItemRarityID.White) { }
	}

	class VitricPillarWallShort : ModTile
	{
		public override string Texture => AssetDirectory.VitricTile + "VitricPillarWallShort";

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 4, 11, DustType<Dusts.Sand>(), SoundID.Tink, new Color(54, 48, 42));
		}
	}

	class VitricPillarWallShortItem : QuickTileItem
	{
		public override string Texture => AssetDirectory.VitricTile + "VitricPillarWallItem";

		public VitricPillarWallShortItem() : base("Short Vitric Forge Pillar", "Sturdy", "VitricPillarWallShort", ItemRarityID.White) { }
	}
}