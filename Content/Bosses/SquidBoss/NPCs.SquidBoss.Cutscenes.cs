﻿using StarlightRiver.Content.GUI;
using StarlightRiver.Core.Loaders.UILoading;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
	public partial class SquidBoss : ModNPC
	{
		public void SpawnAnimation()
		{
			if (GlobalTimer == 1)
				savedPoint = Arena?.fakeBoss?.Center ?? Arena.NPC.Center;

			if (GlobalTimer > 1 && GlobalTimer < 100)
			{
				NPC.Center = spawnPoint + new Vector2(0, 20);

				if (Arena?.fakeBoss != null)
				{
					float progress = Helper.BezierEase(GlobalTimer / 100f);
					Arena.fakeBoss.Center = Vector2.Lerp(savedPoint, new Vector2(savedPoint.X, spawnPoint.Y), progress);
				}
			}

			if (GlobalTimer == 300)
			{
				CameraSystem.shake += 25;
				Helper.PlayPitched("ArenaHit", 1f, 0.5f, NPC.Center);
			}

			if (GlobalTimer > 300 && GlobalTimer < 400)
			{
				float progress = Helper.SwoopEase((GlobalTimer - 300) / 100f);
				NPC.Center = Vector2.Lerp(spawnPoint + new Vector2(0, 20), spawnPoint + new Vector2(0, -600), progress); //rise up from the ground

				if (GlobalTimer == 306)
				{
					for (int k = 0; k < 30; k++)
					{
						float rand = Main.rand.NextFloat(6.28f);
						float sin2 = (float)Math.Sin(Main.GameUpdateCount * 0.01f * 0.2f + rand);
						float cos = (float)Math.Cos(Main.GameUpdateCount * 0.01f + rand);
						Color color = new Color(10 * (1 + sin2), 14 * (1 + cos), 18) * 0.14f;

						Dust.NewDustPerfect(NPC.Center + new Vector2(Main.rand.Next(-30, 30), 0), DustType<Dusts.AuroraWater>(), -Vector2.UnitY.RotatedByRandom(1.57f) * Main.rand.NextFloat(5, 8), 0, color, Main.rand.NextFloat(1, 2));
						Dust.NewDustPerfect(NPC.Center + new Vector2(Main.rand.Next(-30, 30), 0), DustType<Dusts.AuroraWater>(), -Vector2.UnitY.RotatedByRandom(0.5f) * Main.rand.NextFloat(7, 15), 0, color, Main.rand.NextFloat(1, 2));
					}
				}
			}

			if (GlobalTimer == 100 && !Main.dedServ)
			{
				string title = Main.rand.NextBool(10000) ? "Jammed Mod" : "The Venerated";
				UILoader.GetUIState<TextCard>().Display("Auroracle", title, null, 440);
				CameraSystem.DoPanAnimation(440, NPC.Center + new Vector2(0, -600));
			}

			for (int k = 0; k < 4; k++) //each tenticle
			{
				if (GlobalTimer == 100)
				{
					int x;
					int y;
					int xb;

					switch (k) //I handle these manually to get them to line up with the window correctly
					{
						case 0: x = -270; y = 0; xb = -50; break;
						case 1: x = -420; y = -100; xb = -20; break;
						case 3: x = 270; y = 0; xb = 50; break;
						case 2: x = 420; y = -100; xb = 20; break;
						default: x = 0; y = 0; xb = 0; break;
					}

					int i = NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X + x, (int)NPC.Center.Y - 50, NPCType<Tentacle>(), 0, 1, 0, k);
					(Main.npc[i].ModNPC as Tentacle).Parent = this;
					(Main.npc[i].ModNPC as Tentacle).movementTarget = new Vector2((int)NPC.Center.X + x, (int)NPC.Center.Y - 500 - y);
					(Main.npc[i].ModNPC as Tentacle).offsetFromParentBody = xb;
					(Main.npc[i].ModNPC as Tentacle).basePoint = Main.npc[i].Center + Vector2.UnitY * 10;
					(Main.npc[i].ModNPC as Tentacle).Timer = 120 + k * 20;
					(Main.npc[i].ModNPC as Tentacle).stalkWaviness = 0;
					tentacles.Add(Main.npc[i]);

					Main.npc[i].netUpdate = true;

					Mod.Logger.Info("Auroracle spawned tentacle " + i);
				}

				if (GlobalTimer == 100 + k * 30)
				{
					CameraSystem.shake += 5;
					Helper.PlayPitched("ArenaHit", 0.5f, 1f, tentacles[k].Center);
				}

				if (GlobalTimer > 100 + k * 30 && GlobalTimer <= 160 + k * 30)
				{
					var tentacle = tentacles[k].ModNPC as Tentacle;
					float progress = Helper.SwoopEase((GlobalTimer - (100 + k * 30)) / 60f);

					tentacle.NPC.Center = Vector2.Lerp(tentacle.basePoint, tentacle.movementTarget, progress);
					tentacle.downwardDrawDistance = 50;
					tentacle.stalkWaviness = progress * 0.5f;
				}
			}

			if (GlobalTimer > 500 && GlobalTimer <= 550) //tentacles returning back underwater
			{
				foreach (NPC tentacle in tentacles)
				{
					var mt = tentacle.ModNPC as Tentacle;
					tentacle.Center = Vector2.SmoothStep(mt.movementTarget, mt.basePoint, (GlobalTimer - 500) / 50f);
				}
			}

			if (GlobalTimer > 550 && GlobalTimer < 600)
			{
				foreach (NPC tentacle in tentacles)
				{
					var mt = tentacle.ModNPC as Tentacle;
					mt.downwardDrawDistance = 28 + (int)(22 * (1 - (GlobalTimer - 550) / 50f));
				}
			}

			if (GlobalTimer > 700)
			{
				foreach (NPC tentacle in tentacles)
				{
					var mt = tentacle.ModNPC as Tentacle;
					mt.downwardDrawDistance = 28;
				}

				Phase = (int)AIStates.FirstPhase;
			}
		}

		public void DeathAnimation()
		{
			if (GlobalTimer == 1)
			{
				for (int k = 0; k < tentacles.Count; k++)
				{
					var tentacle = tentacles[k].ModNPC as Tentacle;
					tentacle.movementTarget = tentacle.NPC.Center;
				}
			}

			if (GlobalTimer < 50)
				Arena.waterfallWidth = 50 - (int)GlobalTimer;

			if (GlobalTimer < 60)
			{
				NPC.velocity *= 0.9f;
				NPC.rotation *= 0.9f;
				CameraSystem.DoPanAnimation(240, NPC.Center);

				for (int k = 0; k < tentacles.Count; k++)
				{
					var tentacle = tentacles[k].ModNPC as Tentacle;

					for (int i = 0; i < 4; i++)
					{
						if (tentacle.downwardDrawDistance > 28)
							tentacle.downwardDrawDistance--;

						tentacle.NPC.Center = Vector2.SmoothStep(tentacle.movementTarget, tentacle.basePoint, GlobalTimer / 60f);
					}
				}
			}

			if (GlobalTimer % 20 == 0 && GlobalTimer <= 100)
				Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCDeath1, NPC.Center);

			for (int k = 0; k < 5; k++)
			{
				Vector2 off = Vector2.Zero;

				switch (k)
				{
					case 0: off = new Vector2(42, 12); break;
					case 1: off = new Vector2(-43, 12); break;
					case 2: off = new Vector2(-41, -20); break;
					case 3: off = new Vector2(40, -20); break;
					case 4: off = new Vector2(-1, -58); break;
				}

				float time = GlobalTimer - (k + 1) * 20;

				if (time > 0 && time < 20) //dust explosion
				{
					if (time == 1)
					{
						for (int n = 0; n < 40; n++)
						{
							Vector2 vel = Vector2.Normalize(NPC.Center + off - (NPC.Center + Vector2.UnitY * 100)).RotatedByRandom(0.3f) * Main.rand.NextFloat(5, 10);
							Dust.NewDustPerfect(NPC.Center + off, DustType<Dusts.AuroraWater>(), vel, 0, Color.Lerp(Color.Orange, Color.White, 0.5f), Main.rand.NextFloat(0.5f, 1f));
						}
					}

					for (int n = 0; n < 2; n++)
					{
						Vector2 vel = Vector2.Normalize(NPC.Center + off - (NPC.Center + Vector2.UnitY * 100)).RotatedByRandom(0.4f) * Main.rand.NextFloat(5, 20);
						var d = Dust.NewDustPerfect(NPC.Center + off + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(30), DustType<Dusts.AuroraFast>(), vel, 0, Color.Lerp(Color.Red, Color.Yellow, Main.rand.NextFloat(0.3f, 0.8f)));
						d.customData = Main.rand.NextFloat(1, 2);
					}
				}
			}

			if (GlobalTimer >= 200)
			{
				NPC.Kill();

				for (int n = 0; n < 100; n++)
				{
					var off = new Vector2(Main.rand.Next(-50, 50), Main.rand.Next(80, 120));
					Dust.NewDustPerfect(NPC.Center + off, DustType<Dusts.Stamina>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(6) + Vector2.UnitY * -8, 0, Color.White, 2);
				}

				for (int n = 0; n < 100; n++)
				{
					var off = new Vector2(Main.rand.Next(-50, 50), Main.rand.Next(80, 120));
					Vector2 vel = Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(6);
					var color = Color.Lerp(new Color(255, 100, 0) * 0.5f, Color.White, Main.rand.NextFloat(0.7f));
					Dust.NewDustPerfect(NPC.Center + off + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(30), DustType<Dusts.Ink>(), vel, 0, color, Main.rand.NextFloat(1, 2.4f));
				}

				for (int k = 0; k <= 5; k++)
				{
					Gore.NewGore(NPC.GetSource_Death(), NPC.Center + Vector2.UnitX.RotatedByRandom(6.28f) * Main.rand.NextFloat(60), Vector2.One.RotatedByRandom(6.28f) * 6, StarlightRiver.Instance.Find<ModGore>("SquidGore" + k).Type);
				}

				for (int k = 0; k < 10; k++)
				{
					Gore.NewGore(NPC.GetSource_Death(), NPC.Center + Vector2.UnitX.RotatedByRandom(6.28f) * Main.rand.NextFloat(60), Vector2.One.RotatedByRandom(6.28f) * 6, StarlightRiver.Instance.Find<ModGore>("SquidGoreTentacle").Type);
				}
			}
		}
	}
}