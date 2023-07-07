using NetEasy;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Bosses.GlassMiniboss;
using StarlightRiver.Content.Bosses.VitricBoss;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core.Systems.CameraSystem;
using System;
using Terraria.ID;
using static StarlightRiver.Content.Bosses.VitricBoss.VitricBoss;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Packets
{
	/// <summary>
	/// A packet for clients to drive the glassweaver's status used in single player as well
	/// This packet should not need to be created by the server -- only singleplayer or multiplayer clients
	/// 
	/// When an NPC sets netupdate to true that only applies to the server telling clients about updates. 
	/// This packet lets clients tell the server the new glassweaver state and then the server will notify other clients
	/// </summary>
	[Serializable]
	public class GlassweaverWaitingPacket : Module
	{
		private readonly float newState;
		private readonly float newTimer;
		private readonly int npcWhoAmI;
		public GlassweaverWaitingPacket(float newState, float newTimer, int npcWhoAmI)
		{
			this.newState = newState;
			this.newTimer = newTimer;
			this.npcWhoAmI = npcWhoAmI;
		}

		protected override void Receive()
		{
			NPC glassweaverNpc = Main.npc[npcWhoAmI];

			GlassweaverWaiting glassweaverWaiting = glassweaverNpc.ModNPC as GlassweaverWaiting;

			glassweaverWaiting.Timer = newTimer;
			glassweaverWaiting.State = newState;

			if (Main.netMode == NetmodeID.Server)
			{
				glassweaverNpc.netUpdate = true; //instead of forwarding this packet we'll just netupdate the npc cause it's simpler
			}
		}
	}
}