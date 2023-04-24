using StarlightRiver.Content.Biomes;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
	public class TempleWallCandle : ModTile
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Vitric/TempleDecoration/" + Name;

		public override void SetStaticDefaults()
		{

			Main.tileLighted[Type] = true;
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

			LocalizedText name = CreateMapEntryName();
			name.SetDefault("Candle");
			AddMapEntry(new Color(140, 97, 86), name);
			DustType = 0;
			HitSound = SoundID.Dig;
			ItemDrop = ModContent.ItemType<TempleWallCandleItem>();
			AnimationFrameHeight = 36;
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			if (Main.LocalPlayer.InModBiome<VitricTempleBiome>())
			{
				r = 1f;
				g = 0.5f;
				b = 0.25f;
			}
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

	public class TempleWallCandleItem : QuickTileItem
	{
		public TempleWallCandleItem() : base("Temple Wall Candle", "", "TempleWallCandle", ItemRarityID.White, "StarlightRiver/Assets/Tiles/Vitric/TempleDecoration/") { }
	}
}