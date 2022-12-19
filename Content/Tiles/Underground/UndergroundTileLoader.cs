using StarlightRiver.Core.Loaders.TileLoading;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Underground
{
	class UndergroundTileLoader : SimpleTileLoader
	{
		public override string AssetRoot => AssetDirectory.Assets + "Tiles/Underground/";

		public override float Priority => 2.02f;

		public override void Load()
		{
			LoadTile(
				"Springstone",
				"Springstone",
				new TileLoadData(
					minPick: 0,
					dustType: DustID.Stone,
					hitSound: SoundID.Tink,
					mapColor: new Color(86, 88, 91),
					stone: true
					)
				);

			LoadFurniture(
				"ShrineBrazier",
				"Mysterious Brazier",
				new FurnitureLoadData(
					width: 2,
					height: 2,
					dustType: DustID.Stone,
					hitSound: SoundID.Tink,
					tallBottom: true,
					mapColor: new Color(100, 100, 100)
					)
				);

			LoadFurniture(
				"WitTile",
				"Tile of Wit",
				new FurnitureLoadData(
					width: 2,
					height: 2,
					dustType: DustID.Stone,
					hitSound: SoundID.Tink,
					tallBottom: false,
					mapColor: new Color(50, 50, 50)
					)
				);
		}
	}
}
