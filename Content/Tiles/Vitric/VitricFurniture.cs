using StarlightRiver.Core.Loaders.TileLoading;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric
{
	class VitricFurniture : FurnitureLoader
	{
		public VitricFurniture() : base(
			name: "Vitric",
			path: AssetDirectory.VitricTile + "Decoration/",
			color: new Color(140, 97, 86),
			glowColor: new Color(255, 220, 150),
			dust: DustType<Dusts.Air>(),
			material: ItemType<Content.Items.Vitric.SandstoneChunk>())
		{ }
	}
}