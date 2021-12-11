using Microsoft.Xna.Framework;
using NetEasy;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Bosses.VitricBoss;
using System;
using System.Linq;
using Terraria;
using static StarlightRiver.Content.Bosses.VitricBoss.VitricBoss;

namespace StarlightRiver.Packets
{
    [Serializable]
    public class CeirosCrystal : Module
    {
        private readonly int fromWho;
        private readonly int whosBreaking;
        private readonly int whosTheBoss;

        public CeirosCrystal(int fromWho, int whosBreaking, int whosTheBoss)
        {
            this.fromWho = fromWho;
            this.whosBreaking = whosBreaking;
            this.whosTheBoss = whosTheBoss;
        }

        protected override void Receive()
        {
            if (Main.netMode == Terraria.ID.NetmodeID.Server)
            {
                var crystal = Main.npc[whosBreaking];
                var crystalMod = crystal.modNPC as VitricBossCrystal;

                var Parent = Main.npc[whosTheBoss].modNPC as VitricBoss;

                crystalMod.state = 1; //It's all broken and on the floor!
                crystalMod.phase = 0; //go back to doing nothing
                crystalMod.timer = 0; //reset timer

                Parent.npc.ai[1] = (int)AIStates.Anger; //boss should go into it's angery phase
                Parent.ResetAttack();

                crystal.netUpdate = true;

                foreach (NPC npc in (Parent.npc.modNPC as VitricBoss).crystals) //reset all our crystals to idle mode
                {
                    crystalMod.phase = 0;
                    npc.friendly = false; //damaging again
                    npc.netUpdate = true;
                }
            }
        }
    }
}