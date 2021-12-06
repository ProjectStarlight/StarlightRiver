using Microsoft.Xna.Framework;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

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

        public virtual void SafeSendExtraAI() { }

        public virtual void SafeReceiveExtraAI() { }

        public sealed override void SetDefaults()
        {
            SafeSetDefaults();

            npc.lifeMax = 10;
            npc.immortal = true;
            npc.dontTakeDamage = true;
            npc.noGravity = true;
            npc.knockBackResist = 0; //very very important!!
            npc.aiStyle = -1;
            npc.damage = 0;
            npc.netAlways = true;
        }

        public override sealed void SendExtraAI(BinaryWriter writer)
        {
            SafeSendExtraAI();
        }

        public override sealed void ReceiveExtraAI(BinaryReader reader)
        {
            SafeReceiveExtraAI();
        }

        public sealed override void AI()
        {
            SafeAI();

            for (int k = 0; k < Main.maxProjectiles; k++)
			{
                var proj = Main.projectile[k];

                if (!proj.active || proj.aiStyle != 7)
                    continue;

                if(proj.ai[0] != 1 && proj.timeLeft < 36000 - 3 && proj.Hitbox.Intersects(npc.Hitbox))
				{
                    proj.ai[0] = 2;
                    proj.netUpdate = true;
                }
			}
        }
    }
}