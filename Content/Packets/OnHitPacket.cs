using NetEasy;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core;
using System;
using Terraria;

namespace StarlightRiver.Packets
{
    [Serializable]
    public class OnHitPacket : Module
    {
        private readonly short fromWho;
        private readonly int projIdentity;
        private readonly byte NPCId;
        private int damage;
        private float knockback;
        private bool crit;


        public OnHitPacket(Player Player, Projectile proj, NPC target, int damage, float knockback, bool crit)
        {
            this.fromWho = (short)Player.whoAmI;

            if (proj != null)
                this.projIdentity = proj.identity;
            else
                this.projIdentity = -1;

            this.NPCId = (byte)target.whoAmI;
            this.damage = damage;
            this.knockback = knockback;
            this.crit = crit;
        }

        protected override void Receive()
        {
            Player Player = Main.player[fromWho];
            StarlightPlayer modPlayer = Player.GetModPlayer<StarlightPlayer>();
            if (projIdentity == -1)
            {
                modPlayer.ModifyHitNPC(Player.HeldItem, Main.npc[NPCId], ref damage, ref knockback, ref crit);
                modPlayer.OnHitNPC(Player.HeldItem, Main.npc[NPCId], damage, knockback, crit);

            } else
            {
                //Projectile arrays aren't guarenteed to align so we need to use Projectile identity to match
                Projectile proj = null;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].identity == projIdentity)
                    {
                        proj = Main.projectile[i];
                        break;
                    }

                }
                if (proj != null)
                {
                    int hitDirection = 1; //we don't seem to use hitDirection at all for our modifyhitNPC custom code so its not being sent. potential TODO if we ever use hitDirection for some reason.
                    modPlayer.ModifyHitNPCWithProj(proj, Main.npc[NPCId], ref damage, ref knockback, ref crit, ref hitDirection);
                    modPlayer.OnHitNPCWithProj(proj, Main.npc[NPCId], damage, knockback, crit);
                }
            }
            

            if (Main.netMode == Terraria.ID.NetmodeID.Server && fromWho != -1)
                Send(-1, fromWho, false);
        }
    }
}