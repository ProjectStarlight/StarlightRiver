using StarlightRiver.Core.Loaders.TileLoading;
using StarlightRiver.Core.Systems;

namespace StarlightRiver.Content.Tiles.Vitric
{
	internal class VitricLargeFountain : QuickFountain
	{
		public VitricLargeFountain() : base("VitricLargeFountainItem", AssetDirectory.VitricTile, 4, ModContent.DustType<Dusts.Air>(), new Color(140, 97, 86), 4, 6) { }
	}

	[SLRDebug]
	internal class VitricLargeFountainItem : QuickTileItem
	{
		public VitricLargeFountainItem() : base("VitricLargeFountainItem", "Vitric Large Fountain", "Debug Item", "VitricLargeFountain", texturePath: AssetDirectory.VitricTile) { }
	}
}