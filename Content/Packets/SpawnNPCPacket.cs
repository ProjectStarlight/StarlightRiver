using Microsoft.Xna.Framework;
using NetEasy;
using StarlightRiver.Content.Abilities;
using System;
using System.Linq;
using Terraria;

namespace StarlightRiver.Packets
{
    [Serializable]
    public class SpawnNPC : Module
    {
        private readonly int fromWho;
        private readonly int type;
        private readonly int x;
        private readonly int y;

        public SpawnNPC(int fromWho, int x, int y, int type)
        {
            this.fromWho = fromWho;
            this.type = type;
            this.x = x;
            this.y = y;
        }

        protected override void Receive()
        {
            if (Main.netMode == Terraria.ID.NetmodeID.Server)
            {
                int n = NPC.NewNPC(x, y, type);
                NetMessage.SendData(Terraria.ID.MessageID.SyncNPC, -1, -1, null, n);
            }

            if (Main.netMode == Terraria.ID.NetmodeID.SinglePlayer)
                NPC.NewNPC(x, y, type);
        }
    }
}