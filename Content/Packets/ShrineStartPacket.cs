using NetEasy;
using StarlightRiver.Content.Tiles.Underground;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Packets
{
	/// <summary>
	/// generalized packet for starting a shrine trial. should only be processed by a server or singleplayer client.
	/// </summary>
	[Serializable]
	public class ShrineStartPacket : Module
	{
		readonly int tileI;
		readonly int tileJ;

		public ShrineStartPacket(int tileI, int tileJ)
		{
			this.tileI = tileI;
			this.tileJ = tileJ;
		}

		protected override void Receive()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return; //this is not for execution by multiplayer clients

			Tile tile = Framing.GetTileSafely(tileI, tileJ);

			ModTile mTile = ModContent.GetModTile(tile.TileType);

			if (mTile == null)
			{
				StarlightRiver.Instance.Logger.Error("at (" + tileI + ", " + tileJ + ") failed to find shrine tile");
				return; //failed to find shrine tile (this should never happen)
			}

			ShrineTile shrineTile = mTile as ShrineTile;

			int x = tileI - tile.TileFrameX / 18;
			int y = tileJ - tile.TileFrameY / 18;

			shrineTile.SetActiveFrame(x, y);

			ShrineDummy shrineDummy = shrineTile.Dummy(x, y) as ShrineDummy;

			shrineDummy.timer = 0;
			shrineDummy.state = ShrineDummy.SHRINE_STATE_ACTIVE;
			shrineTile.AdditionalSetup(shrineDummy);

			shrineDummy.netUpdate = true;
		}
	}
}