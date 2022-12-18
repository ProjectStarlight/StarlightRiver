using StarlightRiver.Core.Loaders.TileLoading;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric
{
	internal class VitricVine : ModVine
	{
		public VitricVine() : base(new string[] { "VitricSand" }, DustType<Dusts.Air>(), new Color(199, 224, 190), path: AssetDirectory.VitricTile) { }
	}
}