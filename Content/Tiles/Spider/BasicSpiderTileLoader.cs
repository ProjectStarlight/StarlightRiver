using StarlightRiver.Core.Loaders.TileLoading;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Spider
{
	internal class BasicSpiderTileLoader : SimpleTileLoader
	{
		public override string AssetRoot => AssetDirectory.SpiderTile;

		public override float Priority => 2.03f;

		public override void Load()
		{
			LoadTile(
				"SpiderCave",
				"Spider Nest Block",
				new TileLoadData(
					minPick: 200,
					dustType: DustID.Web,
					hitSound: SoundID.Tink,
					mapColor: new Color(115, 115, 115)
					)
				);
		}
	}
}
