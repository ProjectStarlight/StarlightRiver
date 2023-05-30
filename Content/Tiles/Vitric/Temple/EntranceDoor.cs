using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
	class EntranceDoor : ModTile
	{
		public override string Texture => AssetDirectory.VitricTile + Name;

		public override void SetStaticDefaults()
		{
			MinPick = int.MaxValue;
			TileID.Sets.DrawsWalls[Type] = true;
			this.QuickSetFurniture(2, 11, DustType<Content.Dusts.Air>(), SoundID.Tink, false, new Color(200, 150, 80), false, true, "Vitric Temple Door");
		}

		public override bool RightClick(int i, int j)
		{
			if (StarlightRiver.debugMode)
			{
				Tile tile = Framing.GetTileSafely(i, j);
				tile.TileFrameX += 16;
			}

			if (Helpers.Helper.TryTakeItem(Main.LocalPlayer, ModContent.ItemType<Items.Vitric.TempleEntranceKey>(), 1))
			{
				WorldGen.KillTile(i, j);
				return true;
			}

			return false;
		}

		public override void MouseOver(int i, int j)
		{
			Player Player = Main.LocalPlayer;
			Player.cursorItemIconID = ItemType<Items.Vitric.TempleEntranceKey>();
			Player.noThrow = 2;
			Player.cursorItemIconEnabled = true;
		}
	}

	class EntranceDoorItem : QuickTileItem
	{
		public override string Texture => AssetDirectory.Debug;

		public EntranceDoorItem() : base("EntranceDoor", "Debug Item", "EntranceDoor", 1, AssetDirectory.VitricTile) { }
	}
}