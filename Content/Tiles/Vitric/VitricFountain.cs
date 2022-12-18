namespace StarlightRiver.Content.Tiles.Vitric
{
	internal class VitricFountain : QuickFountain
	{
		public VitricFountain() : base("VitricFountainItem", AssetDirectory.VitricTile) { }
	}

	internal class VitricFountainItem : QuickTileItem
	{
		public VitricFountainItem() : base("VitricFountainItem", "Vitric Fountain", "Debug Item", "VitricFountain", texturePath: AssetDirectory.VitricTile) { }
	}
}