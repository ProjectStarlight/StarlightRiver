using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric
{
	class UndergroundTileLoader : TileLoader
    {
        public override string AssetRoot => AssetDirectory.Assets + "Tiles/Underground/";

		public override void Load()
		{
			LoadTile(
				"Springstone",
				"Springstone",
				new TileLoadData(
					minPick: 0,
					dustType: DustID.Stone,
					soundType: SoundID.Tink,
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
					soundType: SoundID.Tink,
					tallBottom: false,
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
					soundType: SoundID.Tink,
					tallBottom: false,
					mapColor: new Color(50, 50, 50)
					)
				);
		}
    }
}
