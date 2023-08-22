﻿using NetEasy;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Bosses.VitricBoss;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core.Systems.CameraSystem;
using System;
using Terraria.ID;
using static StarlightRiver.Content.Bosses.VitricBoss.VitricBoss;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Packets
{
	[Serializable]
	public class CeirosCrystal : Module
	{
		private readonly int fromWho;
		private readonly int whosBreaking;
		private readonly int whosTheBoss;
		private readonly Vector2 newPlayerVelocity; //we send velocity instead of multiplying since we don't want a race condition with a Player update packet to end up wildly changing Player velocity

		public CeirosCrystal(int fromWho, int whosBreaking, int whosTheBoss, Vector2 newPlayerVelocity)
		{
			this.fromWho = fromWho;
			this.whosBreaking = whosBreaking;
			this.whosTheBoss = whosTheBoss;
			this.newPlayerVelocity = newPlayerVelocity;
		}

		protected override void Receive()
		{
			//other clients only need to perform visuals and the Player movement, NPC updates will come from different packets

			NPC crystal = Main.npc[whosBreaking];
			var Parent = Main.npc[whosTheBoss].ModNPC as VitricBoss;
			Player Player = Main.player[fromWho];

			//turn off the ability of the Player who collided and deactivate it
			Player.GetModPlayer<AbilityHandler>().ActiveAbility?.Deactivate();
			Player.velocity = newPlayerVelocity;

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				//visuals and sounds for other Players

				if (Parent.arena.Contains(Main.LocalPlayer.Center.ToPoint()))
					CameraSystem.shake += 10;

				Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, crystal.Center);
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item70 with { Volume = 1f, Pitch = -0.5f }, crystal.Center);

				for (int k = 0; k < 20; k++)
				{
					Dust.NewDustPerfect(crystal.Center, DustType<GlassGravity>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(8), 0, default, 2.2f); //Crystal
					Dust.NewDustPerfect(crystal.Center, DustType<Glow>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(4), 0, new Color(150, 230, 255), 0.8f); //Crystal
				}

				for (int k = 0; k < 40; k++)
					Dust.NewDustPerfect(Parent.NPC.Center, DustType<GlassGravity>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(6), 0, default, 2.6f); //Boss
			}
			else if (Main.netMode == NetmodeID.Server)
			{
				//logic for phase updates
				var crystalMod = crystal.ModNPC as VitricBossCrystal;

				crystalMod.state = 1; //It's all broken and on the floor!
				crystalMod.phase = 0; //go back to doing nothing
				crystalMod.timer = 0; //reset timer

				Parent.NPC.ai[1] = (int)AIStates.Anger; //boss should go into it's angery phase
				Parent.NPC.dontTakeDamage = false;
				Parent.ResetAttack();

				crystal.netUpdate = true;

				foreach (NPC NPC in (Parent.NPC.ModNPC as VitricBoss).crystals) //reset all our crystals to idle mode
				{
					crystalMod.phase = 0;
					NPC.friendly = false; //damaging again
					NPC.netUpdate = true;
				}

				Send(-1, fromWho, false);
			}
		}
	}
}