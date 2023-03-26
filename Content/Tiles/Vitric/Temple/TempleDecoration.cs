using StarlightRiver.Core.Loaders.TileLoading;
using Terraria.ID;
using static StarlightRiver.Helpers.Helper;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
	public class TempleDecoration : SimpleTileLoader
	{
		public override string AssetRoot => "StarlightRiver/Assets/Tiles/Vitric/TempleDecoration/";

		public override void Load()
		{
			//Todo dust

			LoadFurniture("TempleChair", "Temple Chair",
				new FurnitureLoadData(1, 2, 0, SoundID.Dig, false, new Color(140, 97, 86), false, false, "Chair", AnchorFloor(1), faceDirection: true));

			LoadFurniture("TempleLargeVase", "Temple Large Vase",
				new FurnitureLoadData(2, 3, 0, SoundID.Dig, false, new Color(140, 97, 86), false, false, "Large Vase", AnchorFloor(2)));

			LoadFurniture("TempleLargeVaseCrystal", "Temple Large Crystal Vase",
				new FurnitureLoadData(2, 3, 0, SoundID.Dig, false, new Color(140, 97, 86), false, false, "Large Vase", AnchorFloor(2)));

			LoadFurniture("TempleWorkbench", "Temple Workbench",
				new FurnitureLoadData(2, 1, 0, SoundID.Dig, false, new Color(140, 97, 86), true, false, "Workbench", AnchorFloor(2)));

			LoadFurniture("TempleThrone", "Temple Throne",
				new FurnitureLoadData(2, 3, 0, SoundID.Dig, false, new Color(140, 97, 86), false, false, "Throne", AnchorFloor(2)));

			LoadFurniture("TempleFloorWeaponRack", "Temple Floor Weapon Rack",
				new FurnitureLoadData(3, 4, 0, SoundID.Dig, false, new Color(140, 97, 86), false, false, "Weapon Rack", AnchorFloor(3)));

			LoadFurniture("TempleTable", "Temple Table",
				new FurnitureLoadData(3, 2, 0, SoundID.Dig, false, new Color(140, 97, 86), true, false, "Table", AnchorFloor(3)));

			LoadFurniture("TempleTableCrystal", "Temple Crystal Table",
				new FurnitureLoadData(3, 2, 0, SoundID.Dig, false, new Color(80, 131, 142), true, false, "Table", AnchorFloor(3)));

			var weaponRackData = new FurnitureLoadData(3, 3, 0, SoundID.Dig, false, new Color(140, 97, 86), true, false, "Weapon Rack", wallAnchor: true);
			LoadFurniture("TempleWeaponRack0", "Temple Weapon Rack A", weaponRackData);
			LoadFurniture("TempleWeaponRack1", "Temple Weapon Rack B", weaponRackData);
			LoadFurniture("TempleWeaponRack2", "Temple Weapon Rack C", weaponRackData);
			LoadFurniture("TempleWeaponRack3", "Temple Weapon Rack D", weaponRackData);

			LoadFurniture("TempleThingamajig", "Temple Large Weapon Rack",
				new FurnitureLoadData(4, 3, 0, SoundID.Dig, false, new Color(140, 97, 86), false, false, "Weapon Rack", AnchorFloor(4), faceDirection: true));
		}
	}
}
