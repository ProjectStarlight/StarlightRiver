using NetEasy;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using Terraria.DataStructures;

namespace StarlightRiver.Content.Packets
{
	[Serializable]
	public class SpawnDummy : Module
	{
		private readonly int fromWho;
		private readonly int type;
		private readonly int x;
		private readonly int y;

		public SpawnDummy(int fromWho, int type, int x, int y)
		{
			this.fromWho = fromWho;
			this.type = type;
			this.x = x;
			this.y = y;
		}

		protected override void Receive()
		{
			if (DummyTile.DummyExists(x, y, type))
			{
				//this case meant that a Player went up to a tile dummy that did not exist for them, but did on the server and we want to make sure they receive it
				DummyTile.GetDummy(x, y, type).netUpdate = true;
				return;
			}

			Vector2 spawnPos = new Vector2(x, y) * 16 + DummySystem.prototypes[type].Size / 2;
			Dummy newDummy = DummySystem.NewDummy(type, spawnPos);

			var key = new Point16(x, y);
			DummyTile.dummiesByPosition[key] = newDummy;

			if (Main.netMode == Terraria.ID.NetmodeID.Server)
				Send(-1, -1, false);
		}
	}
}