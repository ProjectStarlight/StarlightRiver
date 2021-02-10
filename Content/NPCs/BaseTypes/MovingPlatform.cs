using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.NPCs.BaseTypes
{
    internal abstract class MovingPlatform : ModNPC
    {
        public virtual void SafeSetDefaults() { }

        public virtual void SafeAI() { }

        public override bool? CanBeHitByProjectile(Projectile projectile) => false;

        public override bool? CanBeHitByItem(Player player, Item item) => false;

        public override bool CheckActive() => false;

        public override void SetStaticDefaults() => DisplayName.SetDefault("");

        public sealed override void SetDefaults()
        {
            SafeSetDefaults();

            npc.lifeMax = 1;
            npc.immortal = true;
            npc.dontTakeDamage = true;
            npc.noGravity = true;
            npc.knockBackResist = 0; //very very important!!
            npc.aiStyle = -1;
        }

        public sealed override void AI()
        {
            SafeAI();

            foreach (Player player in Main.player)
                if (new Rectangle((int)player.position.X, (int)player.position.Y + (player.height - 2), player.width, 4).Intersects
                (new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, 4)) && player.position.Y <= npc.position.Y)
                    player.position += npc.velocity;

            foreach (Projectile proj in Main.projectile.Where(n => n.active && n.aiStyle == 7 && n.ai[0] != 1 && n.timeLeft < 36000 - 3 && n.Hitbox.Intersects(npc.Hitbox)))
            {
                proj.ai[0] = 2;
                proj.netUpdate = true;
            }
        }
    }
}