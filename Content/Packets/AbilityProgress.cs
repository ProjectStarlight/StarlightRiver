using NetEasy;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.Faewhip;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using System;

namespace StarlightRiver.Content.Packets
{
	[Serializable]
	public class AbilityProgress : Module
	{
		private readonly int fromWho;

		private readonly int[] shards;
		private readonly byte unlocks;

		public AbilityProgress(int fromWho, AbilityHandler handler)
		{
			this.fromWho = fromWho;

			shards = handler.Shards.ToList().ToArray();

			if (handler.Unlocked<Dash>())
				unlocks |= 0b10000000;

			if (handler.Unlocked<Whip>())
				unlocks |= 0b01000000;
		}

		protected override void Receive()
		{
			Player Player = Main.player[fromWho];
			AbilityHandler handler = Player.GetHandler();

			for (int k = 0; k < shards.Length; k++)
			{
				if (!handler.Shards.Has(shards[k]))
					handler.Shards.Add(shards[k]);
			}

			//Part of me really wants to change this to some sort of string matching but that would make the packet like 11x larger
			if ((unlocks & 0b10000000) == 0b10000000)
				handler.Unlock<Dash>();

			if ((unlocks & 0b01000000) == 0b01000000)
				handler.Unlock<Whip>();

			if (Main.netMode == Terraria.ID.NetmodeID.Server && fromWho != -1)
				Send(-1, fromWho, false);
		}
	}
}