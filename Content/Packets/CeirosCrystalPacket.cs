using Microsoft.Xna.Framework;
using NetEasy;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Bosses.VitricBoss;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using static StarlightRiver.Content.Bosses.VitricBoss.VitricBoss;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Packets
{
    [Serializable]
    public class CeirosCrystal : Module
    {
        private readonly int fromWho;
        private readonly int whosBreaking;
        private readonly int whosTheBoss;
        private readonly Vector2 newPlayerVelocity; //we send velocity instead of multiplying since we don't want a race condition with a player update packet to end up wildly changing player velocity

        public CeirosCrystal(int fromWho, int whosBreaking, int whosTheBoss, Vector2 newPlayerVelocity)
        {
            this.fromWho = fromWho;
            this.whosBreaking = whosBreaking;
            this.whosTheBoss = whosTheBoss;
            this.newPlayerVelocity = newPlayerVelocity;
        }

        protected override void Receive()
        {
            //other clients only need to perform visuals and the player movement, npc updates will come from different packets

            NPC crystal = Main.npc[whosBreaking];
            VitricBoss Parent = Main.npc[whosTheBoss].modNPC as VitricBoss;
            Player player = Main.player[fromWho];

            //turn off the ability of the player who collided and deactivate it
            player.GetModPlayer<AbilityHandler>().ActiveAbility?.Deactivate();
            player.velocity = newPlayerVelocity;

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                //visuals and sounds for other players

                if (Parent.arena.Contains(Main.LocalPlayer.Center.ToPoint()))
                    Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 10;

                Main.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, (int)crystal.Center.X, (int)crystal.Center.Y);
                Main.PlaySound(SoundID.Item70.SoundId, (int)crystal.Center.X, (int)crystal.Center.Y, SoundID.Item70.Style, 2, -0.5f);

                for (int k = 0; k < 20; k++)
                {
                    Dust.NewDustPerfect(crystal.Center, DustType<GlassGravity>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(8), 0, default, 2.2f); //Crystal
                    Dust.NewDustPerfect(crystal.Center, DustType<Glow>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(4), 0, new Color(150, 230, 255), 0.8f); //Crystal
                }

                for (int k = 0; k < 40; k++)
                    Dust.NewDustPerfect(Parent.npc.Center, DustType<GlassGravity>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(6), 0, default, 2.6f); //Boss

                for (int k = 0; k < 5; k++)
                    Gore.NewGore(Parent.npc.Center, Vector2.One.RotatedBy(k / 4f * 6.28f) * 4, mod.GetGoreSlot("Gores/ShieldGore"));

            }
            else if (Main.netMode == NetmodeID.Server)
            {
                //logic for phase updates
                var crystalMod = crystal.modNPC as VitricBossCrystal;

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

                Send(-1, fromWho, false);
            }
        }
    }
}