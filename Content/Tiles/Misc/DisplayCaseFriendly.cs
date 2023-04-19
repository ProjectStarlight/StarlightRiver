﻿using Terraria.DataStructures;

namespace StarlightRiver.Content.Tiles.Misc
{
	class DisplayCaseFriendly : DisplayCase
	{
		public override bool RightClick(int i, int j)
		{
			Tile tile = Main.tile[i, j];

			int index = ModContent.GetInstance<DisplayCaseEntity>().Find(i - tile.TileFrameX / 16, j - tile.TileFrameY / 16);

			if (index == -1)
				return true;

			var entity = (DisplayCaseEntity)TileEntity.ByID[index];

			if (entity.containedItem is null)
			{
				entity.containedItem = Main.LocalPlayer.HeldItem.Clone();
				Main.LocalPlayer.HeldItem.TurnToAir();
			}
			else
			{
				Helpers.Helper.NewItemSpecific(Main.LocalPlayer.Center, entity.containedItem.Clone());
				entity.containedItem = null;
			}

			return true;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Tile tile = Main.tile[i, j];

			int index = ModContent.GetInstance<DisplayCaseEntity>().Find(i - tile.TileFrameX / 16, j - tile.TileFrameY / 16);

			if (index == -1)
				return;

			var entity = (DisplayCaseEntity)TileEntity.ByID[index];

			Item.NewItem(new EntitySource_TileBreak(i, j), i, j, 1, 1, ModContent.ItemType<DisplayCaseFriendlyItem>());

			if (entity.containedItem != null && !entity.containedItem.IsAir)
				Helpers.Helper.NewItemSpecific(new Vector2(i, j), entity.containedItem.Clone());

		}
	}

	class DisplayCaseFriendlyItem : QuickTileItem
	{
		public DisplayCaseFriendlyItem() : base("Display Case", "Can hold an Item for glamorous display", "DisplayCaseFriendly", 2, "StarlightRiver/Assets/Tiles/Misc/") { }
	}
}