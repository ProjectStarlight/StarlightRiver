using System;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Forest
{
	class Trowel : ModItem
	{
		public int maxRange = 30;

		public override string Texture => AssetDirectory.ForestItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Bricklayer's Trowel");
			Tooltip.SetDefault("Extends blocks in a straight line\nDirection is based on your position\n<right> for reverse direction\n30 block range");
		}

		public override void SetDefaults()
		{
			Item.width = 16;
			Item.height = 16;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 10;
			Item.useAnimation = 20;
			Item.autoReuse = true;

			Item.value = Item.sellPrice(silver: 25);
		}

		private Point16 FindNextTile(Player player, int direction)
		{
			if (Math.Abs(Player.tileTargetX - player.Center.X / 16) > Player.tileRangeX || Math.Abs(Player.tileTargetY - player.Center.Y / 16) > Player.tileRangeY)
				return default;

			Tile tile = Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY);

			for (int k = 0; k < maxRange + Player.tileRangeX - 6; k++)
			{
				int nextX = Player.tileTargetX;
				int nextY = Player.tileTargetY;

				float angle = (new Vector2(Player.tileTargetX, Player.tileTargetY) * 16 + Vector2.One * 8 - player.Center).ToRotation();
				angle = Helpers.Helper.ConvertAngle(angle);

				if (angle < Math.PI / 4 || angle > Math.PI / 4 * 7)
					nextX -= k * direction;
				else if (angle >= Math.PI / 4 && angle <= Math.PI / 4 * 3)
					nextY += k * direction;
				else if (angle > Math.PI / 4 * 3 && angle < Math.PI / 4 * 5)
					nextX += k * direction;
				else
					nextY -= k * direction;

				Tile nextTile = Framing.GetTileSafely(nextX, nextY);

				if (!nextTile.HasTile)
					return new Point16(nextX, nextY);
				else if (nextTile.TileType != tile.TileType)
					return default;
			}

			return default;
		}

		//returns the item ID that is consumed
		private static int BlockWandSubstitutions(int createTile)
		{//this could check item id instead if needed
			switch (createTile)
			{
				case TileID.LivingWood:
				case TileID.LeafBlock:
					return ItemID.Wood;

				case TileID.LivingMahogany:
				case TileID.LivingMahoganyLeaves:
					return ItemID.RichMahogany;

				case TileID.BoneBlock:
					return ItemID.Bone;

				case TileID.Hive:
					return ItemID.Hive;

				default:
					break;
			}

			return -1;
		}

		public override bool? UseItem(Player player)
		{
			Tile tile = Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY);
			Item Item = null;

			if (!tile.HasTile || Main.tileFrameImportant[tile.TileType])
				return true;

			int itemSubstitution = BlockWandSubstitutions(tile.TileType);//if this block should look for a different item than the one used to place it

			for (int k = 0; k < player.inventory.Length; k++)  //find the Item to place the tile
			{
				Item thisItem = player.inventory[k];

				if (!thisItem.IsAir && (thisItem.type == itemSubstitution || itemSubstitution == -1 && thisItem.createTile == tile.TileType))
					Item = player.inventory[k];
			}

			if (Item is null) //dont bother calculating tile position if we cant place it
				return true;

			Point16 next = FindNextTile(player, player.altFunctionUse == 2 ? -1 : 1);

			if (next != default)
			{
				WorldGen.PlaceTile(next.X, next.Y, tile.TileType);
				if (Item.consumable)//so that infinite items do not get used up
				{
					Item.stack--;
					if (Item.stack <= 0)
						Item.TurnToAir();
				}
			}

			return true;
		}

		public override bool AltFunctionUse(Player player)
		{
			return true;
		}

		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
		{
			if (Main.LocalPlayer.HeldItem != Item)
				return;

			Tile tile = Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY);

			if (!tile.HasTile || Main.tileFrameImportant[tile.TileType])
				return;

			Vector2 pos = FindNextTile(Main.LocalPlayer, 1).ToVector2() * 16 - Main.screenPosition;

			spriteBatch.Draw(TextureAssets.Tile[tile.TileType].Value, pos, new Rectangle(162, 54, 16, 16), Helpers.Helper.IndicatorColor * 0.5f);
		}
	}

	class Trowel2 : Trowel
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Autotrowel 9000");
			Tooltip.SetDefault("Extends blocks in a straight line extremely quickly\nDirection is based on your position\n<right> for reverse direction\n40 block range\n'The perfect tool for every esteemed bridgebuilder!'");
		}

		public override void SetDefaults()
		{
			Item.width = 16;
			Item.height = 16;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 2;
			Item.useAnimation = 10;
			Item.autoReuse = true;
			maxRange = 40;

			Item.value = Item.sellPrice(gold: 2);
		}
	}
}