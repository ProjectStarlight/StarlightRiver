using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Packets;
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
		public abstract int ShrineTileWidth { get; }
		public abstract int ShrineTileHeight { get; }

		public override bool CanExplode(int i, int j)
		{
			return false;
		}

		/// <summary>
		/// Called for the start packet. used to setup initial variables outside of the State and Timer. Anything included here most likely needs to be added to a SafeSendExtraAI override on the dummy too
		/// </summary>
		public virtual void AdditionalSetup(ShrineDummy shrineDummy) { }

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, ShrineTileWidth, ShrineTileHeight, DustID.Stone, SoundID.Tink, false, new Color(100, 100, 100), false, false, "Mysterious Shrine");
			MinPick = int.MaxValue;
		}

		public override bool SpawnConditions(int i, int j)//ensures the dummy can spawn if the shrine gets stuck in its second frame
		{
			Tile tile = Main.tile[i, j];
			return (tile.TileFrameX == 0 || tile.TileFrameX == ShrineTileWidth * 18) && tile.TileFrameY == 0;
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

			if (IsShrineActive(tile))//shrine is active
			{
				return false;
			}
			else if (IsShrineDormant(tile))//shrine is dormant
			{
				Main.NewText("The shrine has gone dormant...", Color.DarkSlateGray);
				return false;
			}

			int x = i - tile.TileFrameX / 18;
			int y = j - tile.TileFrameY / 18;

			Dummy dummy = Dummy(x, y);

			if (dummy is null)
				return false;

			if ((dummy as ShrineDummy).state == ShrineDummy.SHRINE_STATE_IDLE)
			{
				ShrineStartPacket packet = new ShrineStartPacket(i, j);
				packet.Send();
				return true;
			}

			dummy.netUpdate = true;

			return false;
		}

		public virtual bool IsShrineDormant(Tile tile)
		{
			return tile.TileFrameX >= ShrineTileWidth * 2 * 18;
		}

		public virtual bool IsShrineActive(Tile tile)
		{
			return tile.TileFrameX == ShrineTileWidth * 18;
		}
	}
}