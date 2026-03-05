using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core.Loaders.TileLoading;
using StarlightRiver.Core.Systems;

namespace StarlightRiver.Content.Tiles.Vitric
{
	internal class VitricFountain : QuickFountain
	{
		public VitricFountain() : base("VitricFountainItem", AssetDirectory.VitricTile) { }
	}

	[SLRDebug]
	internal class VitricFountainItem : BaseTileItem
	{
		public VitricFountainItem() : base("VitricFountainItem", "Vitric Fountain", "{{Debug}} Item", "VitricFountain", texturePath: AssetDirectory.VitricTile) { }
	}
}