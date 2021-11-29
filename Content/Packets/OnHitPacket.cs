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
        private readonly byte npcId;
        private int damage;
        private float knockback;
        private bool crit;


        public OnHitPacket(Player player, Projectile proj, NPC target, int damage, float knockback, bool crit)
        {
            this.fromWho = (short)player.whoAmI;

            if (proj != null)
                this.projIdentity = proj.identity;
            else
                this.projIdentity = -1;

            this.npcId = (byte)target.whoAmI;
            this.damage = damage;
            this.knockback = knockback;
            this.crit = crit;
        }

        protected override void Receive()
        {
            Player player = Main.player[fromWho];
            StarlightPlayer modPlayer = player.GetModPlayer<StarlightPlayer>();
            if (projIdentity == -1)
            {
                modPlayer.ModifyHitNPC(player.HeldItem, Main.npc[npcId], ref damage, ref knockback, ref crit);
                modPlayer.OnHitNPC(player.HeldItem, Main.npc[npcId], damage, knockback, crit);

            } else
            {
                //projectile arrays aren't guarenteed to align so we need to use projectile identity to match
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
                    int hitDirection = 1; //we don't seem to use hitDirection at all for our modifyhitnpc custom code so its not being sent. potential TODO if we ever use hitDirection for some reason.
                    modPlayer.ModifyHitNPCWithProj(proj, Main.npc[npcId], ref damage, ref knockback, ref crit, ref hitDirection);
                    modPlayer.OnHitNPCWithProj(proj, Main.npc[npcId], damage, knockback, crit);
                }
            }
            

            if (Main.netMode == Terraria.ID.NetmodeID.Server && fromWho != -1)
                Send(-1, fromWho, false);
        }
    }
}