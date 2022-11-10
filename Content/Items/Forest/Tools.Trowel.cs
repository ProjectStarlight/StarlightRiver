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
			Tooltip.SetDefault("Extends blocks in a straight line\nDirection is based on your position\nHold SHIFT for reverse direction\n30 block range");
		}

		public override void SetDefaults()
		{
			Item.width = 16;
			Item.height = 16;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 10;
			Item.useAnimation = 20;
			Item.autoReuse = true;
		}

		private Point16 FindNextTile(Player Player)
		{
			if (Math.Abs(Player.tileTargetX - Player.Center.X / 16) > Player.tileRangeX || Math.Abs(Player.tileTargetY - Player.Center.Y / 16) > Player.tileRangeY)
				return default;

			Tile tile = Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY);
			int direction = Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) ? -1 : 1;

			for (int k = 0; k < maxRange + Player.tileRangeX - 6; k++)
			{
				int nextX = Player.tileTargetX;
				int nextY = Player.tileTargetY;

				float angle = (new Vector2(Player.tileTargetX, Player.tileTargetY) * 16 + Vector2.One * 8 - Player.Center).ToRotation();
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

		public override bool? UseItem(Player Player)
		{
			Tile tile = Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY);
			Item Item = null;

			if (!tile.HasTile || Main.tileFrameImportant[tile.TileType])
				return true;

			for (int k = 0; k < Player.inventory.Length; k++)  //find the Item to place the tile
			{
				Item thisItem = Player.inventory[k];

				if (!thisItem.IsAir && thisItem.createTile == tile.TileType)
					Item = Player.inventory[k];
			}

			if (Item is null) //dont bother calculating tile position if we cant place it
				return true;

			Point16 next = FindNextTile(Player);

			if (next != default)
			{
				WorldGen.PlaceTile(next.X, next.Y, tile.TileType);
				Item.stack--;
				if (Item.stack <= 0)
					Item.TurnToAir();
			}

			return true;
		}

		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
		{
			if (Main.LocalPlayer.HeldItem != Item)
				return;

			Tile tile = Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY);

			if (!tile.HasTile || Main.tileFrameImportant[tile.TileType])
				return;

			Vector2 pos = FindNextTile(Main.LocalPlayer).ToVector2() * 16 - Main.screenPosition;

			spriteBatch.Draw(TextureAssets.Tile[tile.TileType].Value, pos, new Rectangle(162, 54, 16, 16), Helpers.Helper.IndicatorColor * 0.5f);
		}
	}

	class Trowel2 : Trowel
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Autotrowel 9000");
			Tooltip.SetDefault("Extends blocks in a straight line extremely quickly\nDirection is based on your position\nHold SHIFT for reverse direction\n40 block range\n'The perfect tool for every esteemed bridgebuilder!'");
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
		}
	}
}
