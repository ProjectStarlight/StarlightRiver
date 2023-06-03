using StarlightRiver.Core.Systems;
using StarlightRiver.Content.Abilities;
using Terraria.ID;
using Terraria.ObjectData;
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
			TileObjectData.newTile.RandomStyleRange = 2;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.StyleLineSkip = 2;
			this.QuickSetFurniture(2, 11, DustType<Dusts.Air>(), SoundID.Tink, false, new Color(200, 150, 80), false, true, "Vitric Temple Door");
		}

		public override bool CanDrop(int i, int j)
		{
			return false;
		}

		public override bool RightClick(int i, int j)
		{
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
		public string GetHint()
		{
			return "The monolithic door does not budge. You need a key...";
		}
	}

	[SLRDebug]
	class EntranceDoorItem : QuickTileItem
	{
		public override string Texture => AssetDirectory.Debug;

		public EntranceDoorItem() : base("EntranceDoor", "Debug Item", "EntranceDoor", 1, AssetDirectory.VitricTile) { }
	}
}