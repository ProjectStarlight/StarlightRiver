using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;
using StarlightRiver.Content.Dusts;

namespace StarlightRiver.NPCs
{
    public class BleedStack
    {
        public int timeLeft;

        public BleedStack(int timeleft)
        {
            timeLeft = timeleft;
        }
        public void Update(List<BleedStack> stack, int thisisme)
        {
            timeLeft -= 1;
            if (timeLeft < 1)
                stack.RemoveAt(thisisme);
        }
        public static void RefreshStacks(NPC npc, int time)
        {
            DebuffHandler dbh = npc.GetGlobalNPC<DebuffHandler>();

            for (int i = 0; i < dbh.BarbedBleeds.Count; i += 1)
            {
                int stacktime = dbh.BarbedBleeds[i].timeLeft;
                dbh.BarbedBleeds[i].timeLeft = Math.Max(time, stacktime);
            }
        }

        public static bool ApplyBleedStack(NPC npc, int time, bool refresh = true)
        {
            DebuffHandler dbh = npc.GetGlobalNPC<DebuffHandler>();

            if (dbh.BarbedBleeds.Count < 5)
            {
                dbh.BarbedBleeds.Add(new BleedStack(time));
                return true;
            }

            if (refresh)
                RefreshStacks(npc, time);

            return false;
        }
    }
    public class DebuffHandler : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public int frozenTime = 0;
        internal int impaled = 0;
        public List<BleedStack> BarbedBleeds = new List<BleedStack>();

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            for (int i = 0; i < BarbedBleeds.Count; i += 1)
            {
                impaled += 15;
                BarbedBleeds[i].Update(BarbedBleeds, i);
            }
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
                damage = Math.Max(impaled / 4, damage);
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
            if (BarbedBleeds.Count > 0)
            {
                int count = BarbedBleeds.Count;
                for (int i = 0; i < count; i += 1)
                {
                    Vector2 location = npc.position + new Vector2((npc.width / 2) + ((i * 16) - ((count - 1) * 8)), -8);
                    Dust dust2 = Dust.NewDustPerfect(location, Terraria.ID.DustID.Blood, npc.velocity, 200, Color.Red, 1f);
                    dust2.noGravity = true;
                    if (Main.rand.Next(0, 10) == 0)
                    {
                        Dust dust = Dust.NewDustPerfect(location, ModContent.DustType<BloodyJungleSplash>(), new Vector2(Main.rand.NextFloat(-1, 1), 0), 50, Color.Red, 1f);
                        dust.noGravity = true;
                    }
                }
            }
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