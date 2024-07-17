using StarlightRiver.Content.Dusts;
using System;
using System.Linq;
using Terraria.Audio;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	internal partial class BrainOfCthulu
	{
		#region Phase 1
		public void ShrinkingCircle()
		{
			if (AttackTimer < 60)
			{
				npc.Center += (thinker.Center + new Vector2(100, -250) - npc.Center) * 0.08f;
			}

			if (AttackTimer == 31)
			{
				ThisThinker.platformRadiusTarget = 400;
				ThisThinker.platformRotationTarget = 0.2f;

				for (int k = 0; k < neurisms.Count; k++)
				{
					float rot = k / (float)neurisms.Count * 6.28f;

					neurisms[k].Center = thinker.Center + Vector2.UnitX.RotatedBy(rot) * 750;
					(neurisms[k].ModNPC as Neurysm).State = 2;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}
			}

			if (AttackTimer >= 61)
			{
				float timer = Helpers.Helper.BezierEase((AttackTimer - 60) / 180f);
				float totalRot = Main.masterMode ? 3f : Main.expertMode ? 2.4f : 2f;

				for (int k = 0; k < neurisms.Count; k++)
				{
					float rot = k / (float)neurisms.Count * 6.28f + timer * totalRot;

					neurisms[k].Center = thinker.Center + Vector2.UnitX.RotatedBy(rot) * (750 - timer * 675f);
					(neurisms[k].ModNPC as Neurysm).State = 0;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}
			}

			if (AttackTimer == 240)
			{
				neurisms.ForEach(n =>
				{
					n.velocity *= 0;

					if ((n.ModNPC as Neurysm).State != 1)
					{
						(n.ModNPC as Neurysm).State = 1;
						(n.ModNPC as Neurysm).Timer = 0;
					}
				});

				AttackTimer = 0;
			}
		}

		public void LineThrow()
		{
			if (AttackTimer < 60)
			{
				npc.Center += (thinker.Center + new Vector2(-100, -250) - npc.Center) * 0.08f;
			}

			if (AttackTimer == 1)
			{
				ThisThinker.platformRadiusTarget = 550;
				ThisThinker.platformRotationTarget = 0f;

				savedRot = Main.rand.NextFloat(6.28f);
				npc.netUpdate = true;
			}

			if (AttackTimer == 60)
			{
				if (Main.masterMode)
				{
					for (int k = 0; k < 9; k++)
					{
						Projectile.NewProjectile(null, npc.Center, Vector2.UnitX.RotatedBy(savedRot + k / 9f * 6.28f) * 0.5f, ModContent.ProjectileType<VeinSpear>(), 25, 0, Main.myPlayer);
					}
				}
				else if (Main.expertMode)
				{
					for (int k = 0; k < 5; k++)
					{
						Projectile.NewProjectile(null, npc.Center, Vector2.UnitX.RotatedBy(savedRot + k / 5f * 6.28f) * 0.5f, ModContent.ProjectileType<VeinSpear>(), 25, 0, Main.myPlayer);
					}
				}
			}

			for (int k = 0; k < neurisms.Count; k++)
			{
				float lerp = k / (neurisms.Count - 1f);

				if (AttackTimer == 30 + k * 20)
				{
					float rot = savedRot;
					float direction = k % 2 == 0 ? 1 : -1;

					float a = (1 - lerp * 2) * 750f;
					float h = 750f;

					float offset = (float)Math.Sqrt(Math.Pow(h, 2) - Math.Pow(a, 2));

					neurisms[k].Center = thinker.Center + Vector2.UnitX.RotatedBy(rot) * (-750 + 1500 * lerp) + Vector2.UnitY.RotatedBy(rot) * direction * offset;
					(neurisms[k].ModNPC as Neurysm).State = 2;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
					(neurisms[k].ModNPC as Neurysm).TellTime = 30;
					(neurisms[k].ModNPC as Neurysm).tellDirection = rot + 1.57f * direction;
					(neurisms[k].ModNPC as Neurysm).tellLen = offset * 2;
				}

				if (AttackTimer >= 60 + k * 20 && AttackTimer <= 180 + k * 20)
				{
					float rot = savedRot;
					float direction = k % 2 == 0 ? 1 : -1;

					float a = (1 - lerp * 2) * 750f;
					float h = 750f;

					float offset = (float)Math.Sqrt(Math.Pow(h, 2) - Math.Pow(a, 2));

					float prog = Helpers.Helper.BezierEase((AttackTimer - (60 + k * 20)) / 120f);

					offset *= 1 - prog * 2;

					neurisms[k].Center = thinker.Center + Vector2.UnitX.RotatedBy(rot) * (-750 + 1500 * lerp) + Vector2.UnitY.RotatedBy(rot) * direction * offset;
					(neurisms[k].ModNPC as Neurysm).State = 0;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}

				if (AttackTimer == 180 + k * 20)
				{
					neurisms[k].velocity *= 0;
					(neurisms[k].ModNPC as Neurysm).State = 1;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}
			}

			if (AttackTimer >= 180 + neurisms.Count * 20)
			{
				AttackTimer = 0;
			}
		}

		public void Ram()
		{
			Vector2 relativePos = Main.player[npc.target].Center - thinker.Center;
			Vector2 targetPos = thinker.Center + relativePos.RotatedBy(3.14f);

			float chargeTime = Main.masterMode ? 60f : Main.expertMode ? 75f : 90f;

			if (AttackTimer == 1)
			{
				ThisThinker.platformRadiusTarget = 400;
				ThisThinker.platformRotationTarget = 0.4f;

				npc.TargetClosest();
			}

			if (AttackTimer >= 1 && AttackTimer < 120)
			{
				npc.Center += (targetPos - npc.Center) * 0.05f;
			}

			if (AttackTimer == 120)
			{
				savedPos = thinker.Center + relativePos;
				savedPos2 = npc.Center;

				SoundEngine.PlaySound(SoundID.NPCDeath10.WithPitchOffset(0.5f), npc.Center);
			}

			if (AttackTimer > 120)
			{
				contactDamage = true;
				float prog = Helpers.Helper.SwoopEase((AttackTimer - 120) / chargeTime);
				npc.Center = Vector2.Lerp(savedPos2, savedPos, prog);

				var d = Dust.NewDustPerfect(npc.Center, ModContent.DustType<BloodMetaballDust>(), Vector2.UnitY.RotatedByRandom(1) * Main.rand.NextFloat(-5, -3));
				d.customData = 1f;

				Dust.NewDustPerfect(npc.Center, DustID.Blood, Vector2.UnitY.RotatedByRandom(6.28f) * Main.rand.NextFloat(1, 5));
			}

			if (AttackTimer > 120 + chargeTime)
			{
				contactDamage = false;
				AttackTimer = 0;
			}
		}

		public void Spawn()
		{
			Vector2 relativePos = Main.player[npc.target].Center - thinker.Center;
			Vector2 targetPos = thinker.Center + relativePos.RotatedBy(3.14f);

			float radiusMax = Main.masterMode ? 700 : Main.expertMode ? 200 : 100;
			float speed = Main.masterMode ? 9.46f : 6.28f;

			if (AttackTimer == 1)
			{
				ThisThinker.platformRadiusTarget = 600;
				ThisThinker.platformRotationTarget = -0.2f;

				npc.TargetClosest();
			}

			if (AttackTimer >= 1 && AttackTimer < 600)
			{
				npc.Center += (targetPos - npc.Center) * 0.05f;
			}

			if (AttackTimer == 1)
			{
				for (int k = 0; k < neurisms.Count; k++)
				{
					float radius = k / (float)neurisms.Count * radiusMax;
					float rot = k * 2 / (float)neurisms.Count * 6.28f;

					neurisms[k].Center = thinker.Center + Vector2.UnitX.RotatedBy(rot) * (750 - radius);
					(neurisms[k].ModNPC as Neurysm).State = 2;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}
			}

			if (AttackTimer >= 31)
			{
				for (int k = 0; k < neurisms.Count; k++)
				{
					float radius = k / (float)neurisms.Count * radiusMax;
					float rot = k * 2 / (float)neurisms.Count * 6.28f + Helpers.Helper.BezierEase((AttackTimer - 30) / 540f) * speed * (k % 2 == 0 ? 1 : -1);

					neurisms[k].Center = thinker.Center + Vector2.UnitX.RotatedBy(rot) * (750 - radius);
					(neurisms[k].ModNPC as Neurysm).State = 0;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}
			}

			if (AttackTimer == 570)
			{
				neurisms.ForEach(n =>
				{
					n.velocity *= 0;

					if ((n.ModNPC as Neurysm).State != 1)
					{
						(n.ModNPC as Neurysm).State = 1;
						(n.ModNPC as Neurysm).Timer = 0;
					}
				});

				AttackTimer = 0;
			}

			if (AttackTimer % 40 == 0 && Main.npc.Count(n => n.active && n.type == Terraria.ID.NPCID.Creeper) < 10)
			{
				int i = NPC.NewNPC(npc.GetSource_FromThis(), (int)npc.Center.X, (int)npc.Center.Y, Terraria.ID.NPCID.Creeper);

				//TODO: Multiplayer compat
				Main.npc[i].lifeMax = 30;
				Main.npc[i].life = 30;
				Main.npc[i].SpawnedFromStatue = true;
				Main.npc[i].velocity += npc.Center.DirectionTo(Main.player[npc.target].Center) * 30;
				Main.npc[i].GetGlobalNPC<Creeper>().reworked = true;

				SoundEngine.PlaySound(SoundID.NPCDeath13, npc.Center);
			}

			if (AttackTimer > 600)
			{
				AttackTimer = 0;
			}
		}
		#endregion

		#region Phase 2
		public void Recover()
		{
			if (AttackTimer < 120)
			{
				if (npc.rotation > 0)
					npc.rotation -= 0.02f;

				npc.position.Y -= 1.8f;
			}

			if (AttackTimer > 90 && AttackTimer < 120)
				opacity = 1f - (AttackTimer - 90) / 30f;

			if (AttackTimer == 120)
				npc.Center = thinker.Center + new Vector2(0, -200);

			if (AttackTimer > 120 && AttackTimer < 150)
				opacity = (AttackTimer - 120) / 30f;

			if (AttackTimer == 150)
				AttackTimer = 0;
		}

		public void DoubleSpin()
		{
			if (AttackTimer == 1)
			{
				savedRot = Main.rand.Next(4) * (6.28f / 4f);
				npc.netUpdate = true;
			}

			Vector2 targetPos = (thinker.ModNPC as TheThinker).home + Vector2.UnitX.RotatedBy(savedRot + AttackTimer / 400f * 6.28f) * 550;
			Vector2 targetPos2 = (thinker.ModNPC as TheThinker).home + Vector2.UnitX.RotatedBy(savedRot + AttackTimer / 400f * 6.28f) * -550;

			if (AttackTimer < 30)
				(thinker.ModNPC as TheThinker).ExtraRadius = -60 * AttackTimer / 30f;

			if (AttackTimer >= 1 && AttackTimer < 400)
			{
				float speed = Math.Min(0.05f, AttackTimer / 100f * 0.05f);

				npc.Center += (targetPos - npc.Center) * speed;
				thinker.Center += (targetPos2 - thinker.Center) * speed;

				if (AttackTimer >= 60)
				{
					float rad = 280 + (float)Math.Cos((AttackTimer - 60) / 170f * 3.14f + 3.14f) * 200;

					for (int k = 0; k < neurisms.Count; k++)
					{
						float rot = k * 2 / (float)neurisms.Count * 6.28f + AttackTimer * -0.015f;

						if (k % 2 == 0)
							neurisms[k].Center = thinker.Center + Vector2.UnitX.RotatedBy(rot) * rad;
						else
							neurisms[k].Center = npc.Center + Vector2.UnitX.RotatedBy(rot) * rad;
					}
				}
			}

			if (AttackTimer == 60)
			{
				contactDamage = true;

				for (int k = 0; k < neurisms.Count; k++)
				{
					(neurisms[k].ModNPC as Neurysm).State = 2;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}
			}

			if (AttackTimer == 90)
			{
				for (int k = 0; k < neurisms.Count; k++)
				{
					(neurisms[k].ModNPC as Neurysm).State = 0;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}
			}

			if (AttackTimer == 370)
			{
				for (int k = 0; k < neurisms.Count; k++)
				{
					(neurisms[k].ModNPC as Neurysm).State = 1;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}
			}

			if (AttackTimer >= 370)
				(thinker.ModNPC as TheThinker).ExtraRadius = -60 + 60 * (AttackTimer - 370) / 30f;

			if (AttackTimer >= 400)
			{
				contactDamage = false;

				AttackTimer = 0;
			}
		}

		public void Clones()
		{
			if (AttackTimer <= 30)
				opacity = 1f - AttackTimer / 30f;

			if (AttackTimer > 30 && AttackTimer < 60)
				(thinker.ModNPC as TheThinker).ExtraRadius = 100 * (AttackTimer - 30) / 30f;

			if (AttackTimer == 60)
			{
				int random = Main.rand.Next(10);
				savedPos = (thinker.ModNPC as TheThinker).home + Vector2.UnitX.RotatedBy(random / 10f * 6.28f) * 590;

				for (int k = 0; k < 10; k++)
				{
					if (k != random)
					{
						Vector2 pos = (thinker.ModNPC as TheThinker).home + Vector2.UnitX.RotatedBy(k / 10f * 6.28f) * 590;
						int i = NPC.NewNPC(null, (int)pos.X, (int)pos.Y, ModContent.NPCType<HorrifyingVisage>());
						Main.npc[i].Center = pos;
					}
				}

				npc.chaseable = false;
				npc.Center = savedPos;
			}

			if (AttackTimer > 60 && AttackTimer <= 90)
				opacity = (AttackTimer - 60) / 30f;

			if (AttackTimer <= 120)
			{
				thinker.Center += ((thinker.ModNPC as TheThinker).home - thinker.Center) * 0.03f;
			}

			if (AttackTimer > 90 && AttackTimer < 510)
			{
				if (hurtLastFrame)
					AttackTimer = 510;
			}

			// Timeout
			if (AttackTimer == 509)
			{
				SoundEngine.PlaySound(SoundID.NPCDeath10.WithPitchOffset(0.5f), npc.Center);

				foreach (NPC npc in Main.npc.Where(n => n.active && n.ModNPC is HorrifyingVisage))
					npc.ai[1] = 1;
			}

			if (AttackTimer == 540)
			{
				foreach (NPC npc in Main.npc.Where(n => n.active && n.ModNPC is HorrifyingVisage))
					npc.ai[0] = 540;
			}

			if (AttackTimer > 540 && AttackTimer < 570)
				(thinker.ModNPC as TheThinker).ExtraRadius = 100 - 100 * (AttackTimer - 540) / 30f;

			if (AttackTimer == 600)
			{
				npc.chaseable = true;
				AttackTimer = 0;
			}
		}

		public void TeleportHunt()
		{
			if (AttackTimer <= 120)
			{
				thinker.Center += ((thinker.ModNPC as TheThinker).home - thinker.Center) * 0.03f;
			}

			// 5 Charges
			float motionTime = AttackTimer % 150;

			if (motionTime <= 30)
			{
				contactDamage = false;
				opacity = 1f - motionTime / 30f;
			}

			if (motionTime == 30)
			{
				npc.TargetClosest();

				npc.Center = Main.player[npc.target].Center + Vector2.UnitX.RotatedByRandom(6.28f) * 150;

				savedPos = npc.Center;
				savedPos2 = savedPos + savedPos.DirectionTo(Main.player[npc.target].Center) * 500;
			}

			if (motionTime > 30 && motionTime <= 60)
			{
				opacity = (motionTime - 30) / 30f;
			}

			if (motionTime > 30 && motionTime <= 90)
			{
				npc.Center = Vector2.SmoothStep(savedPos, savedPos + savedPos.DirectionTo(savedPos2) * -100f, (motionTime - 30) / 60f);
			}

			if (motionTime == 90)
			{
				savedPos = npc.Center;
				SoundEngine.PlaySound(SoundID.NPCDeath10.WithPitchOffset(0.5f), npc.Center);
			}

			if (motionTime >= 90)
			{
				contactDamage = true;
				npc.Center = Vector2.Lerp(savedPos, savedPos2, Helpers.Helper.SwoopEase((motionTime - 90) / 60f));
			}

			// Similar neirusm pattern to spawning in phase 1

			float radiusMax = Main.masterMode ? 700 : Main.expertMode ? 400 : 200;
			if (AttackTimer == 1)
			{
				for (int k = 0; k < neurisms.Count; k++)
				{
					float rot = k / (float)neurisms.Count * 6.28f;

					neurisms[k].Center = (thinker.ModNPC as TheThinker).home + Vector2.UnitX.RotatedBy(rot) * 750;
					(neurisms[k].ModNPC as Neurysm).State = 2;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}
			}

			if (AttackTimer >= 31)
			{
				for (int k = 0; k < neurisms.Count; k++)
				{
					float radius = Helpers.Helper.BezierEase((AttackTimer - 30) / (150 * 5 - 30)) * radiusMax;
					float rot = k / (float)neurisms.Count * 6.28f + Helpers.Helper.BezierEase((AttackTimer - 30) / (150 * 5 - 30)) * 6;

					neurisms[k].Center = (thinker.ModNPC as TheThinker).home + Vector2.UnitX.RotatedBy(rot) * (750 - radius);
					(neurisms[k].ModNPC as Neurysm).State = 0;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}
			}

			// End
			if (AttackTimer >= 150 * 5)
			{
				neurisms.ForEach(n =>
				{
					n.velocity *= 0;

					if ((n.ModNPC as Neurysm).State != 1)
					{
						(n.ModNPC as Neurysm).State = 1;
						(n.ModNPC as Neurysm).Timer = 0;
					}
				});

				contactDamage = false;
				AttackTimer = 0;
			}
		}

		public void Clones2()
		{
			// Clones
			if (AttackTimer <= 30)
				opacity = 1f - AttackTimer / 30f;

			if (AttackTimer > 30 && AttackTimer < 60)
				(thinker.ModNPC as TheThinker).ExtraRadius = 100 * (AttackTimer - 30) / 30f;

			if (AttackTimer == 60)
			{
				int random = Main.rand.Next(4);
				savedPos = (thinker.ModNPC as TheThinker).home + Vector2.UnitX.RotatedBy(random / 4f * 6.28f) * 590;

				for (int k = 0; k < 4; k++)
				{
					if (k != random)
					{
						Vector2 pos = (thinker.ModNPC as TheThinker).home + Vector2.UnitX.RotatedBy(k / 4f * 6.28f) * 590;
						int i = NPC.NewNPC(null, (int)pos.X, (int)pos.Y, ModContent.NPCType<HorrifyingVisage>());
						Main.npc[i].Center = pos;
					}
				}

				npc.chaseable = false;
				npc.Center = savedPos;
			}

			if (AttackTimer > 60 && AttackTimer <= 90)
				opacity = (AttackTimer - 60) / 30f;

			if (AttackTimer <= 120)
			{
				thinker.Center += ((thinker.ModNPC as TheThinker).home - thinker.Center) * 0.03f;
			}

			// Exapanding circle
			if (AttackTimer == 31)
			{
				for (int k = 0; k < neurisms.Count; k++)
				{
					float rot = k / (float)neurisms.Count * 6.28f;

					neurisms[k].Center = (thinker.ModNPC as TheThinker).home + Vector2.UnitX.RotatedBy(rot) * 80;
					(neurisms[k].ModNPC as Neurysm).State = 2;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}
			}

			if (AttackTimer >= 61 && AttackTimer < 240)
			{
				float timer = Helpers.Helper.BezierEase((AttackTimer - 60) / 180f);
				float totalRot = Main.masterMode ? 2f : Main.expertMode ? 1.5f : 1f;

				for (int k = 0; k < neurisms.Count; k++)
				{
					float rot = k / (float)neurisms.Count * 6.28f + timer * totalRot;

					neurisms[k].Center = (thinker.ModNPC as TheThinker).home + Vector2.UnitX.RotatedBy(rot) * (80 + timer * 620f);
					(neurisms[k].ModNPC as Neurysm).State = 0;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}
			}

			if (AttackTimer >= 240)
			{
				neurisms.ForEach(n =>
				{
					n.velocity *= 0;

					if ((n.ModNPC as Neurysm).State != 1)
					{
						(n.ModNPC as Neurysm).State = 1;
						(n.ModNPC as Neurysm).Timer = 0;
					}
				});
			}

			// End conditions
			if (AttackTimer > 240 && AttackTimer < 510)
			{
				if (hurtLastFrame)
					AttackTimer = 510;
			}

			// Timeout
			if (AttackTimer == 509)
			{
				SoundEngine.PlaySound(SoundID.NPCDeath10.WithPitchOffset(0.5f), npc.Center);

				foreach (NPC npc in Main.npc.Where(n => n.active && n.ModNPC is HorrifyingVisage))
					npc.ai[1] = 1;
			}

			if (AttackTimer == 540 || hurtLastFrame)
			{
				foreach (NPC npc in Main.npc.Where(n => n.active && n.ModNPC is HorrifyingVisage))
				{
					if (npc.ai[0] < 540)
						npc.ai[0] = 540;
				}
			}

			if (AttackTimer > 540 && AttackTimer < 570)
				(thinker.ModNPC as TheThinker).ExtraRadius = 100 - 100 * (AttackTimer - 540) / 30f;

			if (AttackTimer == 600)
			{
				neurisms.ForEach(n =>
				{
					n.velocity *= 0;

					if ((n.ModNPC as Neurysm).State != 1)
					{
						(n.ModNPC as Neurysm).State = 1;
						(n.ModNPC as Neurysm).Timer = 0;
					}
				});

				npc.chaseable = true;
				AttackTimer = 0;
			}
		}
		#endregion
	}
}
