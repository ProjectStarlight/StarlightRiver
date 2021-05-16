using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ObjectData;

namespace StarlightRiver.Content.Items.Forest
{
	class Trowel : ModItem
	{
		public int maxRange = 30;

		public override string Texture => AssetDirectory.ForestItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Cumlayers Trowel");
			Tooltip.SetDefault("Extends blocks in a straight line\nDirection is based on your position\nHold SHIFT for reverse direction\n30 block range");
		}

		public override void SetDefaults()
		{
			item.width = 16;
			item.height = 16;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.useTime = 20;
			item.useAnimation = 20;
		}

		private Point16 FindNextTile(Player player)
		{
			if (Math.Abs(Player.tileTargetX - (player.Center.X / 16)) > Player.tileRangeX ||
				Math.Abs(Player.tileTargetY - (player.Center.Y / 16)) > Player.tileRangeY)
				return default;

			Tile tile = Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY);
			int direction = Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) ? -1 : 1;

			for (int k = 0; k < maxRange + Player.tileRangeX - 6; k++)
			{
				int nextX = Player.tileTargetX;
				int nextY = Player.tileTargetY;

				float angle = (new Vector2(Player.tileTargetX, Player.tileTargetY) * 16 + Vector2.One * 8 - player.Center).ToRotation();
				angle = Helpers.Helper.ConvertAngle(angle);

				if (angle < Math.PI / 4 || angle > (Math.PI / 4) * 7)
					nextX -= k * direction;
				else if (angle >= Math.PI / 4 && angle <= (Math.PI / 4) * 3)
					nextY += k * direction;
				else if (angle > (Math.PI / 4) * 3 && angle < (Math.PI / 4) * 5)
					nextX += k * direction;
				else
					nextY -= k * direction;

				Tile nextTile = Framing.GetTileSafely(nextX, nextY);

				if (!nextTile.active())
					return new Point16(nextX, nextY);

				else if (nextTile.type != tile.type)
					return default;
			}

			return default;
		}

		public override bool UseItem(Player player)
		{
			Tile tile = Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY);
			Item item = null;

			if (!tile.active() || Main.tileFrameImportant[tile.type])
				return true;

			for (int k = 0; k < player.inventory.Length; k++)  //find the item to place the tile
			{
				var thisItem = player.inventory[k];

				if (!thisItem.IsAir && thisItem.createTile == tile.type)
					item = player.inventory[k];
			}

			if (item is null) //dont bother calculating tile position if we cant place it
				return true;

			Point16 next = FindNextTile(player);

			if (next != default)
			{
				WorldGen.PlaceTile(next.X, next.Y, tile.type);
				item.stack--;
				if (item.stack <= 0)
					item.TurnToAir();
			}

			return true;
		}

		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			if (Main.LocalPlayer.HeldItem != item)
				return;

			Tile tile = Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY);

			if (!tile.active() || Main.tileFrameImportant[tile.type])
				return;

			var pos = FindNextTile(Main.LocalPlayer).ToVector2() * 16 - Main.screenPosition;

			spriteBatch.Draw(Main.tileTexture[tile.type], pos, new Rectangle(162, 54, 16, 16), Helpers.Helper.IndicatorColor * 0.5f);
		}
	}

	class Trowel2 : Trowel
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Automatic Cumlayers Trowel");
			Tooltip.SetDefault("Extends blocks in a straight line\nDirection is based on your position\nHold SHIFT for reverse direction\n40 block range");
		}

		public override void SetDefaults()
		{
			item.width = 16;
			item.height = 16;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.useTime = 10;
			item.useAnimation = 10;
			item.autoReuse = true;
			maxRange = 40;
		}
	}
}
