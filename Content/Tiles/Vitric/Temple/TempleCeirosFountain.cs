﻿using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
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

			LocalizedText name = CreateMapEntryName();
			name.SetDefault("Fountain");
			AddMapEntry(new Color(140, 97, 86), name);
			DustType = 0;
			HitSound = SoundID.Dig;
			RegisterItemDrop(ModContent.ItemType<TempleWallCandleItem>());
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
			{
				frameCounter++;
			}

			if (frame >= 4)
				frame = 0;
		}
	}

	public class TempleCeirosFountainItem : QuickTileItem
	{
		public TempleCeirosFountainItem() : base("Temple Sentinel Fountain", "", "TempleCeirosFountain", ItemRarityID.White, "StarlightRiver/Assets/Tiles/Vitric/TempleDecoration/") { }
	}
}