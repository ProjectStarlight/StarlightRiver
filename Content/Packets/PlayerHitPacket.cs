using NetEasy;
using System;
using System.Linq;
using Terraria.ID;

namespace StarlightRiver.Content.Packets
{

	/// <summary>
	/// Packet for projectiles hitting the player for syncing custom effects
	/// Extend this out if more functionality is needed.
	/// 
	/// is sent from a multiplayer client -> server -> and forwarded to other clients. make sure that runLocally is set to false
	/// </summary>
	[Serializable]
	public class PlayerHitPacket : Module
	{
		readonly int projIdentity;
		readonly int playerId;
		readonly int damage;
		readonly int projType;

		public PlayerHitPacket(int projIdentity, int playerId, int damage, int projType)
		{
			this.projIdentity = projIdentity;
			this.playerId = playerId;
			this.damage = damage;
			this.projType = projType;
		}

		protected override void Receive()
		{
			Player hitPlayer = Main.player[playerId];

			Projectile proj = Main.projectile.FirstOrDefault(n => projIdentity == n.identity && n.type == projType); // Explictly ignores active check since projectile might be killed before this packet reaches the server and other players

			if (proj is null)
				return;

			var hurtInfo = new Player.HurtInfo
			{
				Damage = damage
			};

			if (proj is not null && proj.ModProjectile is not null)
				proj.ModProjectile.OnHitPlayer(hitPlayer, hurtInfo);

			if (Main.netMode == NetmodeID.Server)
				Send(-1, Sender, false);
		}
	}
}