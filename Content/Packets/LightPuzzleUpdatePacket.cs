using NetEasy;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Bosses.VitricBoss;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Tiles.Vitric.Temple.GearPuzzle;
using StarlightRiver.Content.Tiles.Vitric.Temple.LightPuzzle;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using Terraria.DataStructures;
using Terraria.ID;
using static StarlightRiver.Content.Bosses.VitricBoss.VitricBoss;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Packets
{
	/// <summary>
	/// Sends the rotation of a particular reflector to all other clients and server and then activates the beams
	/// flow is always client -> server -> other clients
	/// </summary>
	[Serializable]
	public class LightPuzzleUpdatePacket : Module
	{
		private readonly int x;
		private readonly int y;
		private readonly int dummyType;
		private readonly float rotation;

		public LightPuzzleUpdatePacket(int x, int y, int dummyType, float rotation)
		{
			this.x = x;
			this.y = y;
			this.dummyType = dummyType;
			this.rotation = rotation;
		}

		protected override void Receive()
		{
			ReflectorDummy dummy = DummyTile.GetDummy(x, y, dummyType).ModProjectile as ReflectorDummy;

			if (Main.netMode != NetmodeID.SinglePlayer)
				dummy.DeactivateDownstream(); //clear beam for everyone before creating the new beams

			dummy.Rotation = rotation;

			dummy.Parent.TileFrameX = (short)(dummy.Rotation / 6.28f * 3600);
			dummy.Rotation = dummy.Parent.TileFrameX / 3600f * 6.28f;

			dummy.FindEndpoint();

			if (Main.netMode == NetmodeID.Server)
				Send(-1, Sender, false);
		}
	}
}