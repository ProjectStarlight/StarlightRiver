using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric
{
	internal class VitricSandWall : ModWall
	{
		public override string Texture => AssetDirectory.VitricTile + Name;

		public override void SetStaticDefaults()
		{
			WallID.Sets.Conversion.HardenedSand[Type] = true;
			QuickBlock.QuickSetWall(this, DustID.Copper, SoundID.Dig, ItemType<VitricSandWallItem>(), false, new Color(114, 78, 80));
		}
	}

	internal class VitricSandWallItem : QuickWallItem
	{
		public VitricSandWallItem() : base("Vitric Sand Wall", "", WallType<VitricSandWall>(), 0, AssetDirectory.VitricTile) { }
	}
}