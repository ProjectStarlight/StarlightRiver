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

namespace StarlightRiver.Content.Tiles.Vitric.TempleDecoration
{
	public class TempleDecoration : SimpleTileLoader
	{
		//comment out
		//private static AnchorData anchor = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.Table, 2, 0);
		//private static FurnitureLoadData boxData = new FurnitureLoadData(2, 2, 0, SoundID.Dig, true, new Color(255, 200, 100), false, false, "Music Box", anchor);

		public override string AssetRoot => "StarlightRiver/Assets/Tiles/Vitric/TempleDecoration/";

		public override void Load()
		{
			//todo dust
			AnchorData anchorTabletop1X = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.Table, 1, 0);

			LoadDecoTile("TempleCandle", "Temple Candle",
				new FurnitureLoadData(1, 1, 0, SoundID.Dig, false, new Color(140, 97, 86), false, false, "Candle", anchorTabletop1X));

			LoadDecoTile("TempleCandleCrystal", "Temple Crystal Candle",
				new FurnitureLoadData(1, 2, 0, SoundID.Dig, false, new Color(80, 131, 142), false, false, "Candle", anchorTabletop1X));


			AnchorData anchorFloor1X = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, 1, 0);

			LoadDecoTile("TempleChair", "Temple Chair",
				new FurnitureLoadData(1, 2, 0, SoundID.Dig, false, new Color(140, 97, 86), false, false, "Chair", anchorFloor1X, faceDirection: true));


			AnchorData anchorFloor2X = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, 2, 0);

			LoadDecoTile("TempleLargeVase", "Temple Large Vase",
				new FurnitureLoadData(2, 3, 0, SoundID.Dig, false, new Color(140, 97, 86), false, false, "Large Vase", anchorFloor2X));

			LoadDecoTile("TempleLargeVaseCrystal", "Temple Large Crystal Vase",
				new FurnitureLoadData(2, 3, 0, SoundID.Dig, false, new Color(140, 97, 86), false, false, "Large Vase", anchorFloor2X));

			LoadDecoTile("TempleWorkbench", "Temple Workbench",
				new FurnitureLoadData(2, 1, 0, SoundID.Dig, false, new Color(140, 97, 86), true, false, "Workbench", anchorFloor2X));

			LoadDecoTile("TempleThrone", "Temple Throne",
				new FurnitureLoadData(2, 3, 0, SoundID.Dig, false, new Color(140, 97, 86), false, false, "Throne", anchorFloor2X));


			AnchorData anchorFloor3X = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, 3, 0);

			LoadDecoTile("TempleFloorWeaponRack", "Temple Floor Weapon Rack",
				new FurnitureLoadData(3, 4, 0, SoundID.Dig, false, new Color(140, 97, 86), false, false, "Weapon Rack", anchorFloor3X));

			LoadDecoTile("TempleTable", "Temple Table",
				new FurnitureLoadData(3, 2, 0, SoundID.Dig, false, new Color(140, 97, 86), true, false, "Table", anchorFloor3X));

			LoadDecoTile("TempleTableCrystal", "Temple Crystal Table",
				new FurnitureLoadData(3, 2, 0, SoundID.Dig, false, new Color(80, 131, 142), true, false, "Table", anchorFloor3X));


			//AnchorData anchorBackground3X = new AnchorData(AnchorType.EmptyTile, 3, 0);

			LoadDecoTile("TempleWeaponRack0", "Temple Weapon Rack A",
				new FurnitureLoadData(3, 3, 0, SoundID.Dig, false, new Color(140, 97, 86), true, false, "Weapon Rack", wallAnchor: true));

			LoadDecoTile("TempleWeaponRack1", "Temple Weapon Rack B",
				new FurnitureLoadData(3, 3, 0, SoundID.Dig, false, new Color(140, 97, 86), true, false, "Weapon Rack", wallAnchor: true));

			LoadDecoTile("TempleWeaponRack2", "Temple Weapon Rack C",
				new FurnitureLoadData(3, 3, 0, SoundID.Dig, false, new Color(140, 97, 86), true, false, "Weapon Rack", wallAnchor: true));

			LoadDecoTile("TempleWeaponRack3", "Temple Weapon Rack D",
				new FurnitureLoadData(3, 3, 0, SoundID.Dig, false, new Color(140, 97, 86), true, false, "Weapon Rack", wallAnchor: true));


			AnchorData anchorFloor4X = new AnchorData(AnchorType.SolidTile | AnchorType., 3, 0);

			LoadDecoTile("TempleThingamajig", "Temple Large Weapon Rack",
				new FurnitureLoadData(4, 3, 0, SoundID.Dig, false, new Color(140, 97, 86), false, false, "Weapon Rack", anchorFloor4X, faceDirection: true));
		}

		//public override void Unload()
		//{

		//}

		private void LoadDecoTile(string name, string displayName, FurnitureLoadData boxData)
		{
			LoadFurniture(name, displayName, boxData);
		}
	}

	public class TempleWallCandle : ModTile
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Vitric/TempleDecoration/" + Name;

		public override void SetStaticDefaults()
        {

			Main.tileLavaDeath[Type] = false;
			Main.tileFrameImportant[Type] = true;
			TileID.Sets.FramesOnKillWall[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);


			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.AnchorRight = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, 2, 0);
			TileObjectData.newTile.AnchorBottom = AnchorData.Empty;

			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.addAlternate(0);

			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.AnchorRight = AnchorData.Empty;
			TileObjectData.newAlternate.AnchorLeft = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, 2, 0);
			TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
			TileObjectData.addAlternate(1);

			TileObjectData.addTile(Type);

			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Candle");
			AddMapEntry(new Color(140, 97, 86), name);
			DustType = 0;
			HitSound = SoundID.Dig;
			ItemDrop = ModContent.ItemType<TempleWallCandleItem>();
			AnimationFrameHeight = 36;
		}

		public override void AnimateTile(ref int frame, ref int frameCounter)
		{
			if(frameCounter >= 4)
            {
				frame++;
				frameCounter = 0;
			}
			else
				frameCounter++;

			if (frame >= 4)
				frame = 0;
		}
	}

	public class TempleWallCandleItem : QuickTileItem
    {
        public TempleWallCandleItem() : base("Temple Wall Candle", "", "TempleWallCandle", ItemRarityID.White, "StarlightRiver/Assets/Tiles/Vitric/TempleDecoration/") { }
	}

	public class TempleCeirosFountain : ModTile
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Vitric/TempleDecoration/" + Name;

		public override void SetStaticDefaults()
		{

			Main.tileLavaDeath[Type] = false;
			Main.tileFrameImportant[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);

			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, 2, 0);

			TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
			TileObjectData.newTile.StyleHorizontal = true;

			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
			TileObjectData.addAlternate(1);

			TileObjectData.addTile(Type);

			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Fountain");
			AddMapEntry(new Color(140, 97, 86), name);
			DustType = 0;
			HitSound = SoundID.Dig;
			ItemDrop = ModContent.ItemType<TempleWallCandleItem>();
			AnimationFrameHeight = 36;
		}

		public override void AnimateTile(ref int frame, ref int frameCounter)
		{
			if (frameCounter >= 4)
			{
				frame++;
				frameCounter = 0;
			}
			else
				frameCounter++;

			if (frame >= 4)
				frame = 0;
		}
	}

	public class TempleCeirosFountainItem : QuickTileItem
	{
		public TempleCeirosFountainItem() : base("Temple Sentinel Fountain", "", "TempleCeirosFountain", ItemRarityID.White, "StarlightRiver/Assets/Tiles/Vitric/TempleDecoration/") { }
	}
}
