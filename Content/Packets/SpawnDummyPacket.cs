using Microsoft.Xna.Framework;
using NetEasy;
using StarlightRiver.Content.Abilities;
using System;
using System.Linq;
using Terraria;

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
            if (!Main.projectile.Any(n => n.active && n.type == type && n.position == new Vector2(i, j) * 16))
            {
                Projectile p = new Projectile();
                p.SetDefaults(type);

                int n = Projectile.NewProjectile(new Vector2(x, y) + p.Size / 2, Vector2.Zero, type, 1, 0);

                p = null;
            }
        }
    }
}