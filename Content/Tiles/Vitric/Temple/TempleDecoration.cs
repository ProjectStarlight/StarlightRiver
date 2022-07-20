using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.Enums;
using StarlightRiver.Core.Loaders;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.ObjectData;
using Terraria;
using static StarlightRiver.Helpers.Helper;

namespace StarlightRiver.Content.Tiles.Vitric.TempleDecoration
{
	public class TempleDecoration : SimpleTileLoader
	{
		public override string AssetRoot => "StarlightRiver/Assets/Tiles/Vitric/TempleDecoration/";

		public override void Load()
		{
			//Todo dust

			LoadDecoTile("TempleChair", "Temple Chair",
				new FurnitureLoadData(1, 2, 0, SoundID.Dig, false, new Color(140, 97, 86), false, false, "Chair", AnchorFloor(1), faceDirection: true));


			LoadDecoTile("TempleLargeVase", "Temple Large Vase",
				new FurnitureLoadData(2, 3, 0, SoundID.Dig, false, new Color(140, 97, 86), false, false, "Large Vase", AnchorFloor(2)));

			LoadDecoTile("TempleLargeVaseCrystal", "Temple Large Crystal Vase",
				new FurnitureLoadData(2, 3, 0, SoundID.Dig, false, new Color(140, 97, 86), false, false, "Large Vase", AnchorFloor(2)));

			LoadDecoTile("TempleWorkbench", "Temple Workbench",
				new FurnitureLoadData(2, 1, 0, SoundID.Dig, false, new Color(140, 97, 86), true, false, "Workbench", AnchorFloor(2)));

			LoadDecoTile("TempleThrone", "Temple Throne",
				new FurnitureLoadData(2, 3, 0, SoundID.Dig, false, new Color(140, 97, 86), false, false, "Throne", AnchorFloor(2)));


			LoadDecoTile("TempleFloorWeaponRack", "Temple Floor Weapon Rack",
				new FurnitureLoadData(3, 4, 0, SoundID.Dig, false, new Color(140, 97, 86), false, false, "Weapon Rack", AnchorFloor(3)));

			LoadDecoTile("TempleTable", "Temple Table",
				new FurnitureLoadData(3, 2, 0, SoundID.Dig, false, new Color(140, 97, 86), true, false, "Table", AnchorFloor(3)));

			LoadDecoTile("TempleTableCrystal", "Temple Crystal Table",
				new FurnitureLoadData(3, 2, 0, SoundID.Dig, false, new Color(80, 131, 142), true, false, "Table", AnchorFloor(3)));


			FurnitureLoadData weaponRackData = new FurnitureLoadData(3, 3, 0, SoundID.Dig, false, new Color(140, 97, 86), true, false, "Weapon Rack", wallAnchor: true);
			LoadDecoTile("TempleWeaponRack0", "Temple Weapon Rack A", weaponRackData);
			LoadDecoTile("TempleWeaponRack1", "Temple Weapon Rack B", weaponRackData);
			LoadDecoTile("TempleWeaponRack2", "Temple Weapon Rack C", weaponRackData);
			LoadDecoTile("TempleWeaponRack3", "Temple Weapon Rack D", weaponRackData);


			LoadDecoTile("TempleThingamajig", "Temple Large Weapon Rack",
				new FurnitureLoadData(4, 3, 0, SoundID.Dig, false, new Color(140, 97, 86), false, false, "Weapon Rack", AnchorFloor(4), faceDirection: true));
		}

		private void LoadDecoTile(string name, string displayName, FurnitureLoadData boxData)
		{
			LoadFurniture(name, displayName, boxData);
		}
	}
}
