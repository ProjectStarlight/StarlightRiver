using StarlightRiver.Core.Loaders.TileLoading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ObjectData;

namespace StarlightRiver.Content.Tiles.Starlight
{
	internal class BasicStarlightTileLoader : SimpleTileLoader
	{
		public override string AssetRoot => AssetDirectory.StarlightTile;

		public override float Priority => 2.04f;

		public override void Load()
		{
			LoadTile(
			"Blinkbrick",
			"Blinkbrick",
			new TileLoadData(
				minPick: 100,
				dustType: DustID.Stone,
				hitSound: SoundID.Tink,
				mapColor: new Color(92, 103, 113),
				dirtMerge: true,
				stone: true
				)
			);

			LoadGemsparkCompositeTile(
				"ReinforcedBlinkbrick",
				"Reinforced Blinkbrick",
				6,
				1,
				new TileLoadData(
					minPick: 100,
					dustType: DustID.Gold,
					hitSound: SoundID.Tink,
					mapColor: new Color(92, 103, 113)
				)
			);

			LoadGemsparkCompositeTile(
				"ChartGrate",
				"Chart Grate",
				6,
				6,
				new TileLoadData(
					minPick: 100,
					dustType: DustID.Gold,
					hitSound: SoundID.Tink,
					mapColor: new Color(255, 210, 150)
				)
			);

			LoadGemsparkCompositeTile(
				"BrassPlate",
				"Brass Plate",
				3,
				3,
				new TileLoadData(
					minPick: 100,
					dustType: DustID.Gold,
					hitSound: SoundID.Tink,
					mapColor: new Color(255, 210, 150)
				)
			);

			LoadGemsparkCompositeTile(
				"SmoothBrassPlate",
				"Smooth Brass Plate",
				3,
				3,
				new TileLoadData(
					minPick: 100,
					dustType: DustID.Gold,
					hitSound: SoundID.Tink,
					mapColor: new Color(255, 210, 150)
				)
			);

			LoadGemsparkCompositeTile(
				"RawOnyx",
				"Raw Onyx",
				6,
				6,
				new TileLoadData(
					minPick: 100,
					dustType: DustID.Granite,
					hitSound: SoundID.DD2_SkeletonHurt,
					mapColor: new Color(79, 99, 122)
				)
			);

			LoadGemsparkCompositeTile(
				"LockedOnyx",
				"Locked Onyx",
				6,
				6,
				new TileLoadData(
					minPick: 100,
					dustType: DustID.Gold,
					hitSound: SoundID.DD2_SkeletonHurt,
					mapColor: new Color(79, 99, 122)
				)
			);

			LoadGemsparkCompositeTile(
				"DreamingOnyx",
				"Dreaming Onyx",
				6,
				6,
				new TileLoadData(
					minPick: 100,
					dustType: DustID.Granite,
					hitSound: SoundID.DD2_SkeletonHurt,
					mapColor: new Color(79, 99, 162)
				)
			);
		}

		public override void PostLoad()
		{
			Main.tileBlockLight[Mod.Find<ModTile>("ChartGrate").Type] = false;
			AddMerge("Blinkbrick", "ReinforcedBlinkbrick");
		}
	}
}