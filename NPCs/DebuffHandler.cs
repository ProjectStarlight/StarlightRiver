using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.NPCs
{
    public class DebuffHandler : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public int frozenTime = 0;
        internal int impaled = 0;

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (frozenTime != 0)
            {
                frozenTime -= 1;
                npc.color.B += 180;
                npc.color.G += 90;
                if (npc.color.B >= 255)
                {
                    npc.color.B = 255;
                }
                if (npc.color.G >= 255)
                {
                    npc.color.G = 255;
                }
                npc.velocity *= 0.2f;
            }

            if (impaled > 0)
            {
                if (npc.lifeRegen > 0) npc.lifeRegen = 0;
                npc.lifeRegen -= impaled;
                    damage = Math.Max(impaled/4,damage);
            }
            //ResetEffects seems to be called after projectile AI it seems, but this works, for now
            impaled = 0;
        }

        public override void ResetEffects(NPC npc)
        {
            base.ResetEffects(npc);
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (frozenTime != 0)
            {
                Dust dust = Main.dust[Dust.NewDust(npc.position, npc.width, npc.height, 15, 0f, 0f, 255, default, 1f)];
                dust.noGravity = true;
                dust.scale = 1.1f;
                dust.noLight = true;
            }
        }
    }
}