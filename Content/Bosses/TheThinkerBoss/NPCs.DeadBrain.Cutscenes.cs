using StarlightRiver.Content.GUI;
using StarlightRiver.Core.Systems.CameraSystem;
using System;
using System.Linq;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.TheThinkerBoss
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
				NPC.Center = thinker.Center + new Vector2(0, 200);
				attachedChainEndpoint = thinker.Center;

				if (IsInArena(Main.LocalPlayer))
				{
					CameraSystem.MoveCameraOut(60, thinker.Center);
					ZoomHandler.SetZoomAnimation(1.5f, 60);
				}

				ThisThinker.shellFrame = 1;

				Gore.NewGorePerfect(NPC.GetSource_FromThis(), thinker.Center, Vector2.UnitX.RotatedBy(0.5f).RotatedByRandom(1f) * 8, StarlightRiver.Instance.Find<ModGore>("ThinkerShellGore1").Type);
				Gore.NewGorePerfect(NPC.GetSource_FromThis(), thinker.Center, Vector2.UnitX.RotatedBy(0.5f).RotatedByRandom(1f) * 8, StarlightRiver.Instance.Find<ModGore>("ThinkerShellGore2").Type);

				Gore.NewGorePerfect(NPC.GetSource_FromThis(), thinker.Center, Vector2.UnitX.RotatedBy(-2.5f).RotatedByRandom(0.6f) * 8, StarlightRiver.Instance.Find<ModGore>("ThinkerShellGore3").Type);
				Gore.NewGorePerfect(NPC.GetSource_FromThis(), thinker.Center, Vector2.UnitX.RotatedBy(-2.5f).RotatedByRandom(0.6f) * 8, StarlightRiver.Instance.Find<ModGore>("ThinkerShellGore4").Type);

				for (int k = 0; k < 30; k++)
				{
					Dust.NewDustPerfect(ThisThinker.NPC.Center, DustID.Stone, Vector2.UnitX.RotatedBy(0.5f).RotatedByRandom(1f) * Main.rand.NextFloat(10), 0, default, 1);
					Dust.NewDustPerfect(ThisThinker.NPC.Center, DustID.Stone, Vector2.UnitX.RotatedBy(-2.5f).RotatedByRandom(0.6f) * Main.rand.NextFloat(10), 0, default, 0.8f);

					Dust.NewDustPerfect(ThisThinker.NPC.Center, ModContent.DustType<Dusts.GraymatterDust>(), Vector2.UnitX.RotatedByRandom(6.28f) * Main.rand.NextFloat(10), 0, default, Main.rand.NextFloat(1.5f, 2f));
				}

				Helpers.SoundHelper.PlayPitched("Impacts/StoneStrike", 1, 2f, ThisThinker.NPC.Center);

				extraChunkRadius = ThisThinker.FakeBrainRadius;
			}

			if (Timer < 60)
			{
				extraChunkRadius += (0.8f - extraChunkRadius) * 0.002f;
			}

			if (Timer < 100)
			{
				ThisThinker.ExtraGrayAuraRadius = -140 + 140 * (Timer / 100f);
			}

			if (Timer == 100)
			{
				if (IsInArena(Main.LocalPlayer))
				{
					CameraSystem.MoveCameraOut(30, NPC.Center);
					ZoomHandler.SetZoomAnimation(1.2f, 30);
				}
			}

			if (Timer < 120)
			{
				NPC.position += new Vector2(0.25f, 0.04f * Timer);

				if (Timer > 60)
					extraChunkRadius = 0.8f + 0.2f * ((Timer - 60) / 60f);
			}

			if (Timer > 120 && Timer < 160)
			{
				NPC.position += new Vector2(0.25f, 20f * MathF.Sin((Timer - 120) / 40f * 6.28f)) * (1f - (Timer - 120) / 40f);
				savedPos = NPC.Center;
			}

			if (Timer > 100 && Timer < 120)
			{
				attachedChainEndpoint = Vector2.Lerp(thinker.Center, NPC.Center + Vector2.UnitY * 90, (Timer - 100) / 20f);
			}

			if (Timer == 120)
			{
				Helpers.SoundHelper.PlayPitched("Impacts/StabFleshy", 1f, -0.8f, NPC.Center);
			}

			// Chain needs to be linked after this point
			if (Timer >= 120)
				attachedChainEndpoint = NPC.Center + Vector2.UnitY * 90;

			// Move brain into position
			if (Timer > 160 && Timer < 360)
			{
				float prog = (Timer - 160) / 200f;
				prog = Helpers.Eases.BezierEase(prog);
				NPC.Center = Vector2.Lerp(savedPos, thinker.Center + new Vector2(200, -200), Helpers.Eases.SwoopEase(prog));
			}

			if (Timer > 160 && Timer <= 240)
			{
				float prog = 1f - (Timer - 160) / 80f;
				extraChunkRadius = prog;
			}

			// Move camera to follow
			if (Timer == 160)
			{
				if (IsInArena(Main.LocalPlayer))
				{
					CameraSystem.MoveCameraOut(200, thinker.Center, (a, b, c) => Vector2.Lerp(a, b, Helpers.Eases.SwoopEase(c)));
					ZoomHandler.SetZoomAnimation(Main.GameZoomTarget, 200);
				}
			}

			if (Timer < 430)
			{
				Lighting.AddLight(NPC.Center, new Vector3(0.5f, 0.35f, 0.35f));
			}

			if (Timer > 430 && Timer < 480)
			{
				shieldOpacity = Helpers.Eases.SwoopEase((Timer - 430) / 50f) * 0.4f;
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
						float prog = Helpers.Eases.BezierEase((Timer - 360) / 60f);
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
				contactDamage = false;
				chargeAnimation = 0;
				NPC.velocity *= 0;
				CameraSystem.MoveCameraOut(30, savedPos);
			}

			NPC.Center = savedPos;

			// Explode the weakpoint
			if (Timer == 60)
			{
				for (int k = 0; k < 120; k++)
				{
					Dust.NewDust(weakpoint.position, weakpoint.width, weakpoint.height, DustID.Blood);

					if (Main.rand.NextBool(3))
						Dust.NewDust(weakpoint.position, weakpoint.width, weakpoint.height, DustID.FireworksRGB, Main.rand.NextFloat(-5, 5), Main.rand.NextFloat(-5, 5), 0, new Color(1f, 0.5f, 0.6f));
				}

				Helpers.SoundHelper.PlayPitched("Impacts/GoreHeavy", 1f, -0.25f, weakpoint.Center);

				weakpoint.active = false;
				weakpoint = null;

				chainsSplit = true;
			}

			// Shield sound
			if (Timer == 120)
				Helpers.SoundHelper.PlayPitched("Magic/Shadow1", 0.7f, -0.25f, NPC.Center);

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
				ThisThinker.shellFrame = 2;

				for (int k = 5; k < 13; k++)
				{
					Gore.NewGorePerfect(NPC.GetSource_FromThis(), thinker.Center, Vector2.UnitX.RotatedByRandom(6.28f) * Main.rand.NextFloat(5, 7), StarlightRiver.Instance.Find<ModGore>("ThinkerShellGore" + k).Type);
				}

				for (int k = 0; k < 30; k++)
				{
					Dust.NewDustPerfect(ThisThinker.NPC.Center, DustID.Stone, Vector2.UnitX.RotatedByRandom(6.28f) * Main.rand.NextFloat(10), 0, default, 2.2f);
					Dust.NewDustPerfect(ThisThinker.NPC.Center, ModContent.DustType<Dusts.GraymatterDust>(), Vector2.UnitX.RotatedByRandom(6.28f) * Main.rand.NextFloat(6), 0, default, Main.rand.NextFloat(0.8f, 1f));
				}

				Helpers.SoundHelper.PlayPitched("Impacts/StoneStrike", 1, 2f, ThisThinker.NPC.Center);
			}

			if (Timer == 260)
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

			if (Timer > 120 && Timer < 240)
				ThisThinker.arenaFade = 120 - (Timer - 120);

			if (Timer == 1 || Timer == 90 || Timer == 120 || Timer == 150 || Timer == 180 || Timer == 210)
			{
				SoundHelper.PlayPitched("MagicAttack", 1, 1f - Timer / 300f, thinker.Center);
				CameraSystem.shake += 20;

				for (int k = 0; k < 30; k++)
				{
					Dust.NewDustPerfect(thinker.Center, ModContent.DustType<Dusts.PixelatedEmber>(), Main.rand.NextVector2Circular(10, 10));
					Dust.NewDustPerfect(thinker.Center, ModContent.DustType<Dusts.PixelatedImpactLineDust>(), Main.rand.NextVector2Circular(30, 30));
				}
			}

			if (Timer < 300)
			{
				ThisThinker.ExtraGrayAuraRadius = 600 - Timer / 300f * 750;
			}

			if (Timer == 300)
			{
				SoundHelper.PlayPitched("MagicAttack", 1, -0.5f, thinker.Center);
				CameraSystem.shake += 30;

				for (int k = 0; k < 30; k++)
				{
					var color = new Color(Main.rand.NextFloat(0.5f, 1f), Main.rand.NextFloat(0.5f, 1f), Main.rand.NextFloat(0.5f, 1f), 0);

					Dust.NewDustPerfect(thinker.Center, ModContent.DustType<Dusts.PixelatedEmber>(), Main.rand.NextVector2Circular(20, 20) * Main.rand.NextFloat(), 0, color, Main.rand.NextFloat(1, 1.25f));
					Dust.NewDustPerfect(thinker.Center, ModContent.DustType<Dusts.PixelatedImpactLineDust>(), Main.rand.NextVector2Circular(30, 30), 0, color);
				}
			}

			if (Timer == 360)
			{
				foreach (Player player in Main.ActivePlayers)
				{
					if (IsInArena(player))
						player.Center = thinker.Center;
				}

				CameraSystem.ReturnCamera(180);
				ZoomHandler.ReturnZoom(180);

				for (int k = 1; k < 13; k++)
				{
					Gore.NewGorePerfect(NPC.GetSource_FromThis(), thinker.Center, Vector2.UnitX.RotatedByRandom(6.28f) * Main.rand.NextFloat(5, 7), StarlightRiver.Instance.Find<ModGore>("ThinkerShellGore" + k).Type);
				}

				for (int k = 0; k < 50; k++)
				{
					Dust.NewDustPerfect(ThisThinker.NPC.Center, DustID.Stone, Vector2.UnitX.RotatedByRandom(6.28f) * Main.rand.NextFloat(14), 0, default, Main.rand.NextFloat(2, 2.2f));
				}

				SoundHelper.PlayPitched("Effects/Splat", 0.7f, 0.5f, thinker.Center);
				Helpers.SoundHelper.PlayPitched("Impacts/StoneStrike", 1, 2f, ThisThinker.NPC.Center);

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

			if (Timer == 360)
				NPC.active = false;
		}
	}
}