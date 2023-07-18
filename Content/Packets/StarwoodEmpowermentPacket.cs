using NetEasy;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items;
using System;
using Terraria.DataStructures;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Packets
{
	[Serializable]
	public class StarwoodEmpowermentPacket : Module
	{
		private readonly byte fromWho;

		public StarwoodEmpowermentPacket(int fromWho)
		{
			this.fromWho = (byte)fromWho;
		}

		protected override void Receive()
		{
			Player player = Main.player[fromWho];
			player.TryGetModPlayer(out StarlightPlayer starlightPlayer);

			if (!starlightPlayer.empowered)
			{
				for (int k = 0; k < 80; k++)//pickup sfx
					Dust.NewDustPerfect(player.Center, DustType<BlueStamina>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(0.8f, 1.2f) * new Vector2(1f, 1.5f), 0, default, 1.5f);
			}
			else
			{
				for (int k = 0; k < 40; k++)//reduced pickup sfx if its already active
					Dust.NewDustPerfect(player.Center, DustType<BlueStamina>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(0.5f, 0.8f) * new Vector2(1f, 1.5f), 0, default, 1.5f);
			}

			starlightPlayer.empowered = true;
			starlightPlayer.empowermentTimer = 600;//resets timer

			if (Main.netMode == NetmodeID.Server)
				Send(-1, Sender, false);
		}
	}
}