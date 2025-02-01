using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.GUI;
using StarlightRiver.Core.Loaders.UILoading;
using StarlightRiver.Core.Systems.CameraSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	internal partial class DeadBrain
	{
		public void Intro()
		{
			// Magnetize the players into the fight here
			foreach (Player player in Main.player.Where(n => n.active && Vector2.Distance(n.Center, thinker.Center) < 1500))
			{
				player.immuneTime = 30;
				player.immune = true;

				player.position += (thinker.Center - player.Center) * 0.1f * (Vector2.Distance(player.Center, thinker.Center) / 1500f);
			}

			// Lighting at chain end
			if (Timer <= 480)
				Lighting.AddLight(attachedChainEndpoint, new Vector3(0.8f, 0.5f, 0.2f));

			if (Timer == 1)
			{
				NPC.Center = thinker.Center + Vector2.UnitY.RotatedBy(-0.5f) * ThisThinker.ArenaRadius;
				savedPos = NPC.Center;
				attachedChainEndpoint = thinker.Center;

				if (IsInArena(Main.LocalPlayer))
				{
					CameraSystem.MoveCameraOut(90, thinker.Center);
					ZoomHandler.SetZoomAnimation(1.5f, 90);
				}

				extraChunkRadius = 2;
				staggeredExtraChunkRadius = 1;
			}

			if (Timer == 100)
			{
				if (IsInArena(Main.LocalPlayer))
				{
					CameraSystem.MoveCameraOut(30, NPC.Center);
					ZoomHandler.SetZoomAnimation(1.2f, 30);
				}
			}

			if (Timer > 100 && Timer < 120)
			{
				attachedChainEndpoint = Vector2.Lerp(thinker.Center, NPC.Center + Vector2.UnitY * 90, (Timer - 100) / 20f);
			}

			if (Timer == 120)
			{
				Helpers.Helper.PlayPitched("Impacts/StabFleshy", 1f, -0.8f, NPC.Center);
			}

			// Chain needs to be linked after this point
			if (Timer >= 120)
				attachedChainEndpoint = NPC.Center + Vector2.UnitY * 90;

			// Move brain into position
			if (Timer > 160 && Timer < 360)
			{
				float prog = (Timer - 160) / 200f;
				prog = Helpers.Helper.BezierEase(prog);
				NPC.Center = Vector2.Lerp(savedPos, thinker.Center + new Vector2(200, -200), Helpers.Helper.SwoopEase(prog));
			}

			if (Timer > 160 && Timer <= 240)
			{
				float prog = 1f - (Timer - 160) / 80f;
				extraChunkRadius = prog * 2;
				staggeredExtraChunkRadius = prog;
			}

			// Move camera to follow
			if (Timer == 160)
			{
				if (IsInArena(Main.LocalPlayer))
				{
					CameraSystem.MoveCameraOut(200, thinker.Center, (a, b, c) => Vector2.Lerp(a, b, Helpers.Helper.SwoopEase(c)));
					ZoomHandler.SetZoomAnimation(Main.GameZoomTarget, 200);
				}
			}

			if (Timer > 430 && Timer < 480)
			{
				shieldOpacity = Helpers.Helper.SwoopEase((Timer - 430) / 50f) * 0.4f;
			}

			// Neurysms
			if (Timer > 280 && Timer <= 440)
			{
				for (int k = 0; k < neurisms.Count; k++)
				{
					float lerp = k / (neurisms.Count - 1f);
					float rot = 6.28f * lerp;

					if (Timer == 280 + k * 5)
					{
						neurisms[k].Center = thinker.Center + Vector2.UnitX.RotatedBy(rot) * 200;
						(neurisms[k].ModNPC as Neurysm).State = 2;
						(neurisms[k].ModNPC as Neurysm).Timer = 0;
					}

					if (Timer >= 360 && Timer <= 420)
					{
						float prog = Helpers.Helper.BezierEase((Timer - 360) / 60f);
						var center = Vector2.Lerp(thinker.Center, NPC.Center, prog);
						neurisms[k].Center = center + Vector2.UnitX.RotatedBy(rot + prog * 1.57f) * (200 - 70f * prog);
						(neurisms[k].ModNPC as Neurysm).State = 0;
						(neurisms[k].ModNPC as Neurysm).Timer = 0;
					}

					if (Timer == 420)
					{
						neurisms[k].velocity *= 0;
						(neurisms[k].ModNPC as Neurysm).State = 1;
						(neurisms[k].ModNPC as Neurysm).Timer = 0;
					}
				}
			}

			if (Timer == 270)
			{
				if (Main.netMode != NetmodeID.Server)
					TextCard.Display(thinker.FullName, Main.rand.NextBool(10000) ? "Thinking of you <3" : "& the Dead Brain", 270, 1.25f); //intro text
			}

			if (Timer == 200)
			{
				(thinker.ModNPC as TheThinker)?.CreateArena();
			}

			if (Timer >= 220)
			{
				if (ThisThinker.arenaFade < 120)
					ThisThinker.arenaFade++;
			}

			if (Timer == 500)
				CameraSystem.ReturnCamera(40);

			if (Timer == 540)
			{
				Phase = Phases.FirstPhase;
				Timer = 0;
				AttackTimer = 0;
			}
		}

		public void FirstPhaseTransition()
		{
			if (Timer == 1)
			{
				savedPos = NPC.Center;
				CameraSystem.MoveCameraOut(30, savedPos);
			}

			// Explode the weakpoint
			if (Timer == 60)
			{
				for (int k = 0; k < 120; k++)
				{
					Dust.NewDust(weakpoint.position, weakpoint.width, weakpoint.height, DustID.Blood);

					if (Main.rand.NextBool(3))
						Dust.NewDust(weakpoint.position, weakpoint.width, weakpoint.height, DustID.FireworksRGB, Main.rand.NextFloat(-5, 5), Main.rand.NextFloat(-5, 5), 0, new Color(1f, 0.5f, 0.6f));
				}

				Helpers.Helper.PlayPitched("Impacts/GoreHeavy", 1f, -0.25f, weakpoint.Center);

				weakpoint.active = false;
				weakpoint = null;

				chainsSplit = true;
			}

			// Shield sound
			if (Timer == 120)
				Helpers.Helper.PlayPitched("Magic/Shadow1", 0.7f, -0.25f, NPC.Center);

			// Dust for shield
			if (Timer > 100 && Timer < 140)
			{
				for (int k = 0; k < 5; k++)
				{
					var color = new Color(Main.rand.NextFloat(0.5f, 1f), Main.rand.NextFloat(0.5f, 1f), Main.rand.NextFloat(0.5f, 1f));
					float rot = Main.rand.NextFloat(6.28f);

					Dust.NewDustPerfect(NPC.Center + Vector2.One.RotatedBy(rot) * 90, ModContent.DustType<Dusts.GlowLineFast>(), Vector2.One.RotatedBy(rot) * Main.rand.NextFloat(8), 0, color, Main.rand.NextFloat(0.5f, 1f));
				}
			}

			// Shield opacity decrease
			if (Timer > 120 && Timer <= 150)
				shieldOpacity = 1 - (Timer - 120) / 30f;

			if (Timer == 150)
			{
				CameraSystem.ReturnCamera(30);
			}

			if (Timer == 200)
			{
				Phase = Phases.SecondPhase;
				Timer = 0;
				AttackTimer = 0;
			}
		}

		public void Death()
		{
			foreach (Player player in Main.ActivePlayers)
			{
				if (IsInArena(player))
				{
					player.immuneTime = 300;
					player.immune = true;

					player.position += (thinker.Center - player.Center) * 0.1f * (Vector2.Distance(player.Center, thinker.Center) / 1500f);
				}
			}

			if (Timer == 1)
			{
				if (IsInArena(Main.LocalPlayer))
				{
					CameraSystem.MoveCameraOut(90, thinker.Center);
					ZoomHandler.SetZoomAnimation(1.5f, 90);
				}

				ThisThinker.ExtraGrayAuraRadius = 0;
			}

			if (Timer == 30)
			{
				foreach (NPC npc in Main.ActiveNPCs)
				{
					if (npc.type == ModContent.NPCType<HallucinationBlock>())
						npc.active = false;
				}

				foreach (Projectile proj in Main.ActiveProjectiles)
				{
					if (proj.type == ModContent.ProjectileType<HallucinationHazard>())
						proj.active = false;
				}
			}

			if (Timer == 300)
			{
				foreach (Player player in Main.ActivePlayers)
				{
					if (IsInArena(player))
						player.Center = thinker.Center;
				}

				CameraSystem.ReturnCamera(180);
				ZoomHandler.SetZoomAnimation(1f, 180);

				for (int k = 0; k < 50; k++)
				{
					Dust.NewDustPerfect(thinker.Center, ModContent.DustType<GraymatterDust>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(9), 0, default, Main.rand.NextFloat(1f, 4f));
				}

				Helpers.Helper.PlayPitched("Impacts/AirstrikeImpact", 1, 0, thinker.Center);

				ThisThinker.platforms.ForEach(n => n.active = false);

				thinker.immortal = false;
				thinker.dontTakeDamage = false;
				thinker.SimpleStrikeNPC(999999, 0);

				neurisms.ForEach(n => n.active = false);

				foreach (Player Player in Main.player.Where(n => n.active && Vector2.Distance(n.Center, ThisThinker.home) <= ThisThinker.ArenaRadius))
				{
					Player.GetModPlayer<MedalPlayer>().ProbeMedal("TheThinker");
				}
			}

			if (Timer == 330)
				NPC.active = false;
		}
	}
}