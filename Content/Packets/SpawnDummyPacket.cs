using Microsoft.Xna.Framework;
using NetEasy;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;

namespace StarlightRiver.Packets
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
            if (Main.netMode == Terraria.ID.NetmodeID.Server)
            {
                if (Core.DummyTile.DummyExists(x, y, type))
                    return;

                //Send(-1, fromWho, false);

                Projectile p = new Projectile();
                p.SetDefaults(type);

                var spawnPos = new Vector2(x, y) * 16 + p.Size / 2;

                int n = Projectile.NewProjectile(spawnPos, Vector2.Zero, type, 0, 0, fromWho);
                NetMessage.SendData(Terraria.ID.MessageID.SyncProjectile, -1, -1, null, n);

                Point16 key = new Point16(x, y);
                DummyTile.dummies[key] = Main.projectile[n];
            }
        }
    }
}