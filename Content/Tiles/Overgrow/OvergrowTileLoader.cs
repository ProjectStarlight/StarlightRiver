using StarlightRiver.Core.Loaders;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Overgrow
{
	class OvergrowTileLoader : SimpleTileLoader
	{
		public override string AssetRoot => AssetDirectory.OvergrowTile;

		public override float Priority => 2.01f;

		public override void Load()
		{
			LoadTile(
				"LeafOvergrow",
				"Faerie Leaves",
				new TileLoadData(
					minPick: 210,
					dustType: DustType<Dusts.Leaf>(),
					hitSound: SoundID.Grass,
					mapColor: new Color(215, 180, 67),
					dirtMerge: true,
					stone: true
					)
				);

			LoadTile(
				"BrickOvergrow",
				"Runic Bricks",
				new TileLoadData(
					minPick: 210,
					dustType: DustID.Stone,
					hitSound: SoundID.Tink,
					mapColor: new Color(79, 76, 71)
					)
				);

			LoadTile(
				"StoneOvergrow",
				"Runic Stone",
				new TileLoadData(
					minPick: 210,
					dustType: DustID.Stone,
					hitSound: SoundID.Tink,
					mapColor: new Color(71, 68, 64)
					)
				);
		}

		public override void PostLoad()
		{
			int typeLeafOvergrow = Mod.Find<ModTile>("LeafOvergrow").Type;
			int typeBrickOvergrow = Mod.Find<ModTile>("BrickOvergrow").Type;
			int typeStoneOvergrow = Mod.Find<ModTile>("StoneOvergrow").Type;

			AddMerge(typeLeafOvergrow, typeBrickOvergrow);
			AddMerge(typeLeafOvergrow, typeStoneOvergrow);
			AddMerge(typeBrickOvergrow, typeStoneOvergrow);

			AddMerge(typeBrickOvergrow, TileID.BlueDungeonBrick);
			AddMerge(typeBrickOvergrow, TileID.GreenDungeonBrick);
			AddMerge(typeBrickOvergrow, TileID.PinkDungeonBrick);
		}
	}
}