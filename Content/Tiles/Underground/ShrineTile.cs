using StarlightRiver.Content.Abilities;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Underground
{
	public abstract class ShrineTile : DummyTile, IHintable
	{
		public abstract string GetHint();

		public abstract bool isShrineDormant(Tile tile);

		public abstract bool isShrineActive(Tile tile);

		public abstract int ShrineTileWidth { get; }
		public abstract int ShrineTileHeight { get; }

		public override bool CanExplode(int i, int j)
		{
			return false;
		}

		public void SetActiveFrame(int startX, int startY)
		{
			for (int relX = 0; relX < ShrineTileWidth; relX++)
			{
				for (int relY = 0; relY < ShrineTileHeight; relY++)
				{
					int realX = relX + startX;
					int realY = relY + startY;

					Framing.GetTileSafely(realX, realY).TileFrameX = (short)((ShrineTileWidth + relX) * 18);
				}
			}

			if (Main.netMode == NetmodeID.Server)
				NetMessage.SendTileSquare(-1, startX, startY, ShrineTileWidth, ShrineTileHeight, TileChangeType.None);
		}

		public override void MouseOver(int i, int j)
		{
			Player Player = Main.LocalPlayer;
			Player.cursorItemIconID = ModContent.ItemType<Items.Hovers.GenericHover>();
			Player.noThrow = 2;
			Player.cursorItemIconEnabled = true;
		}

		public override bool RightClick(int i, int j)
		{
			Tile tile = Framing.GetTileSafely(i, j);

			if (isShrineActive(tile))//shrine is active
			{
				return false;
			}
			else if (isShrineDormant(tile))//shrine is dormant
			{
				Main.NewText("The shrine has gone dormant...", Color.DarkSlateGray);
				return false;
			}

			int x = i - tile.TileFrameX / 18;
			int y = j - tile.TileFrameY / 18;

			Projectile dummy = Dummy(x, y);
			Main.NewText(dummy is null);
			if (dummy is null)
				return false;
			
			if ((dummy.ModProjectile as ShrineDummy).State == ShrineDummy.ShrineState_Idle)
			{
				ShineStartPacket packet = new ShineStartPacket(i, j);
				packet.Send();
				return true;
			}

			return false;
		}
	}
}
