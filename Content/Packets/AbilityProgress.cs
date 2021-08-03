using NetEasy;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.Faeflame;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Content.Abilities.GaiasFist;
using StarlightRiver.Content.Abilities.Purify;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using Terraria;

namespace StarlightRiver.Packets
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

			if (handler.Unlocked<Dash>()) unlocks |= 0b10000000;
			if (handler.Unlocked<Wisp>()) unlocks |= 0b01000000;
			if (handler.Unlocked<Pure>()) unlocks |= 0b00100000;
			if (handler.Unlocked<Smash>()) unlocks |= 0b00010000;
        }

        protected override void Receive()
        {
            if (Main.netMode == Terraria.ID.NetmodeID.Server)
            {
                //Send(-1, fromWho, false);
                return;
            }

            Player player = Main.player[fromWho];
            AbilityHandler handler = player.GetHandler();

            for (int k = 0; k < shards.Length; k++)
                if(!handler.Shards.Has(shards[k]))
                    handler.Shards.Add(shards[k]);

            //Part of me really wants to change this to some sort of string matching but that would make the packet like 11x larger
            if((unlocks & 0b10000000) == 0b10000000) handler.Unlock<Dash>();
            if ((unlocks & 0b01000000) == 0b01000000) handler.Unlock<Wisp>();
            if ((unlocks & 0b00100000) == 0b00100000) handler.Unlock<Pure>();
            if ((unlocks & 0b00010000) == 0b00010000) handler.Unlock<Smash>();
        }
    }
}