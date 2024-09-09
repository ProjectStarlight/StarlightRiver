using StarlightRiver.Core.Loaders.TileLoading;
using Terraria.ID;
using static StarlightRiver.Helpers.Helper;

namespace StarlightRiver.Content.Tiles.Crimson
{
	public class GraymatterDeco : SimpleTileLoader
	{
		public override string AssetRoot => "StarlightRiver/Assets/Tiles/Crimson/";

		public override void Load()
		{
			LoadFurniture("GraymatterDeco1x4", "Gray Nerve (1x4)",
				new FurnitureLoadData(1, 4, 0, SoundID.Dig, false, new Color(167, 180, 191), false, false, "", AnchorFloor(1), anchorTiles: [ModContent.TileType<GrayMatter>()]));

			LoadFurniture("GraymatterDeco2x3", "Gray Nerve (2x3)",
				new FurnitureLoadData(2, 3, 0, SoundID.Dig, false, new Color(167, 180, 191), false, false, "", AnchorFloor(2), anchorTiles: [ModContent.TileType<GrayMatter>()]));

			LoadFurniture("GraymatterDeco2x2", "Gray Nerve (2x2)",
				new FurnitureLoadData(2, 2, 0, SoundID.Dig, false, new Color(167, 180, 191), false, false, "", AnchorFloor(2), anchorTiles: [ModContent.TileType<GrayMatter>()], variants: 2));

			LoadFurniture("GraymatterDeco1x2", "Gray Nerve (1x2)",
				new FurnitureLoadData(1, 2, 0, SoundID.Dig, false, new Color(167, 180, 191), false, false, "", AnchorFloor(1), anchorTiles: [ModContent.TileType<GrayMatter>()], variants: 2));

			LoadFurniture("GraymatterDeco3x3", "Gray Nerve (3x3)",
				new FurnitureLoadData(3, 3, 0, SoundID.Dig, false, new Color(167, 180, 191), false, false, "", AnchorFloor(3), anchorTiles: [ModContent.TileType<GrayMatter>()]));

			LoadFurniture("GraymatterDeco1x1", "Gray Nerve (1x1)",
				new FurnitureLoadData(1, 1, 0, SoundID.Dig, false, new Color(167, 180, 191), false, false, "", AnchorFloor(1), anchorTiles: [ModContent.TileType<GrayMatter>()], variants: 2));
		}

		public override void PostLoad()
		{
			GrayMatter.grayEmissionTypes.Add(StarlightRiver.Instance.Find<ModTile>("GraymatterDeco1x4").Type);
			GrayMatter.grayEmissionTypes.Add(StarlightRiver.Instance.Find<ModTile>("GraymatterDeco2x3").Type);
			GrayMatter.grayEmissionTypes.Add(StarlightRiver.Instance.Find<ModTile>("GraymatterDeco2x2").Type);
			GrayMatter.grayEmissionTypes.Add(StarlightRiver.Instance.Find<ModTile>("GraymatterDeco1x2").Type);
			GrayMatter.grayEmissionTypes.Add(StarlightRiver.Instance.Find<ModTile>("GraymatterDeco3x3").Type);
			GrayMatter.grayEmissionTypes.Add(StarlightRiver.Instance.Find<ModTile>("GraymatterDeco1x1").Type);
		}
	}
}