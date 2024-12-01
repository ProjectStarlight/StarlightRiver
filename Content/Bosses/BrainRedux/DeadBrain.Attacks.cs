using Microsoft.CodeAnalysis.CSharp.Syntax;
using StarlightRiver.Content.Bosses.SquidBoss;
using StarlightRiver.Content.Dusts;
using System;
using System.Linq;
using Terraria.Audio;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	internal partial class DeadBrain
	{
		#region Phase 1
		public void ShrinkingCircle()
		{
			if (AttackTimer < 60)
			{
				NPC.Center += (thinker.Center + new Vector2(250, -250) - NPC.Center) * 0.08f;
			}

			if (AttackTimer == 31)
			{
				ThisThinker.platformRadiusTarget = 400;
				ThisThinker.platformRotationTarget += 0.2f;

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
				float totalRot = Main.masterMode ? 2.4f : Main.expertMode ? 2f : 1.6f;

				for (int k = 0; k < neurisms.Count; k++)
				{
					float rot = k / (float)neurisms.Count * 6.28f + timer * totalRot;

					neurisms[k].Center = thinker.Center + Vector2.UnitX.RotatedBy(rot) * (750 - timer * 675f);
					(neurisms[k].ModNPC as Neurysm).State = 0;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}
			}

			if (AttackTimer == 160 && Main.masterMode)
			{
				for(int k = 0; k < 12; k++)
				{
					Projectile.NewProjectile(null, thinker.Center, Vector2.UnitX.RotatedBy(k / 12f * 6.28f) * 4, ModContent.ProjectileType<BrainBolt>(), BrainBoltDamage, 0, Main.myPlayer, 200);
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
				NPC.Center += (thinker.Center + new Vector2(-250, -250) - NPC.Center) * 0.08f;
			}

			if (AttackTimer == 1)
			{
				ThisThinker.platformRadiusTarget = 550;
				ThisThinker.platformRotationTarget += 0.2f;

				savedRot = Main.rand.NextFloat(6.28f);
				NPC.netUpdate = true;
			}

			if (AttackTimer == 60)
			{
				if (Main.masterMode)
				{
					for (int k = 0; k < 9; k++)
					{
						Projectile.NewProjectile(null, thinker.Center, Vector2.UnitX.RotatedBy(savedRot + k / 9f * 6.28f) * 0.5f, ModContent.ProjectileType<VeinSpear>(), VeinSpearDamage, 0, Main.myPlayer, thinker.whoAmI);
					}
				}
				else if (Main.expertMode)
				{
					for (int k = 0; k < 5; k++)
					{
						Projectile.NewProjectile(null, thinker.Center, Vector2.UnitX.RotatedBy(savedRot + k / 5f * 6.28f) * 0.5f, ModContent.ProjectileType<VeinSpear>(), VeinSpearDamage, 0, Main.myPlayer, thinker.whoAmI);
					}
				}
			}

			if (AttackTimer == 75 && Main.masterMode)
			{
				for (int k = 0; k < 9; k++)
				{
					Projectile.NewProjectile(null, thinker.Center, Vector2.UnitX.RotatedBy(savedRot + (k + 0.5f) / 9f * 6.28f) * 4, ModContent.ProjectileType<BrainBolt>(), BrainBoltDamage, 0, Main.myPlayer, 200);
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
			Vector2 relativePos = Main.player[NPC.target].Center - thinker.Center;
			Vector2 targetPos = thinker.Center + relativePos.RotatedBy(3.14f);

			float chargeTime = Main.masterMode ? 60f : Main.expertMode ? 75f : 90f;

			if (AttackTimer == 1)
			{
				ThisThinker.platformRadiusTarget = 400;
				ThisThinker.platformRotationTarget += 0.2f;

				NPC.TargetClosest();
			}

			if (AttackTimer > 1 && AttackTimer < 140)
			{
				float speed = 0.05f;

				if (AttackTimer > 110)
					speed *= (1f - (AttackTimer - 110) / 30f);

				NPC.Center += (targetPos - NPC.Center) * speed;
			}

			if (AttackTimer == 130)
			{
				savedPos = thinker.Center + relativePos;
			}

			if (AttackTimer > 140 && AttackTimer < 160)
			{
				NPC.Center += Vector2.Normalize(NPC.Center - savedPos) * 20 * (1f - (AttackTimer - 140) / 20f);
			}

			if (AttackTimer == 160)
			{
				savedPos2 = NPC.Center;
				SoundEngine.PlaySound(SoundID.NPCDeath10.WithPitchOffset(0.5f), NPC.Center);
			}

			if (AttackTimer > 160)
			{
				contactDamage = true;
				float prog = Helpers.Helper.SwoopEase((AttackTimer - 160) / chargeTime);
				NPC.Center = Vector2.Lerp(savedPos2, savedPos, prog);
			}

			if (AttackTimer > 160 + chargeTime)
			{
				contactDamage = false;
				AttackTimer = 0;
			}
		}

		public void DrawRamGraphics(SpriteBatch spriteBatch)
		{
			float chargeTime = Main.masterMode ? 60f : Main.expertMode ? 75f : 90f;

			if (AttackTimer > 100 && AttackTimer < 140)
			{
				float prog = (AttackTimer - 100) / 40f;

				for(int k = 0; k < 6; k++)
				{
					Vector2 pos = NPC.Center + Vector2.UnitX.RotatedBy(k / 6f * 6.28f + prog * 3.14f) * (1f - prog) * 128;
					DrawBrainSegments(spriteBatch, NPC, pos - Main.screenPosition, new Color(255, 100, 100), NPC.rotation, NPC.scale, 0.35f * prog, lastPos);
				}
			}

			if (AttackTimer > 130 && AttackTimer < 160)
			{
				float rot = NPC.Center.DirectionTo(savedPos).ToRotation();
				var tellTex = Assets.Misc.DirectionalBeam.Value;
				var pos = Vector2.Lerp(NPC.Center, savedPos, 0.2f);
				var target = new Rectangle((int)pos.X, (int)pos.Y, 900, 50);
				target.Offset((-Main.screenPosition).ToPoint());

				spriteBatch.Draw(tellTex, target, null, new Color(160, 30, 30, 0) * (float)Math.Sin((AttackTimer - 140) / 30f * 3.14f), rot, new Vector2(0, tellTex.Height / 2), 0, 0);

				target = new Rectangle((int)pos.X, (int)pos.Y, 900, 250);
				target.Offset((-Main.screenPosition).ToPoint());

				spriteBatch.Draw(tellTex, target, null, new Color(60, 10, 10, 0) * (float)Math.Sin((AttackTimer - 140) / 30f * 3.14f), rot, new Vector2(0, tellTex.Height / 2), 0, 0);

			}

			if (AttackTimer > 160)
			{
				for (int k = 0; k < 10; k++)
				{
					Vector2 pos = NPC.oldPos[k] + NPC.Size / 2f;
					DrawBrainSegments(spriteBatch, NPC, pos - Main.screenPosition, new Color(255, 100, 100), NPC.rotation, NPC.scale, (k / 30f) * (1f - (AttackTimer - 160) / chargeTime), lastPos);
				}
			}
		}

		public void Spawn()
		{
			Vector2 relativePos = Main.player[NPC.target].Center - thinker.Center;
			Vector2 targetPos = thinker.Center + relativePos.RotatedBy(3.14f);

			float radiusMax = Main.masterMode ? 700 : Main.expertMode ? 200 : 100;
			float speed = Main.masterMode ? 9.46f : 6.28f;

			if (AttackTimer == 1)
			{
				ThisThinker.platformRadiusTarget = 600;
				ThisThinker.platformRotationTarget += 0.2f;

				NPC.TargetClosest();
			}

			if (AttackTimer > 1 && AttackTimer < 600)
			{
				NPC.Center += (targetPos - NPC.Center) * 0.05f;
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

			if (AttackTimer == 560 && Main.masterMode)
			{
				neurisms.ForEach(n =>
				{
					Projectile.NewProjectile(null, n.Center, Vector2.Normalize(thinker.Center - n.Center) * Vector2.Distance(n.Center, thinker.Center) / 45f, ModContent.ProjectileType<BrainBolt>(), BrainBoltDamage, 0, Main.myPlayer, 45);
				});
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
				int i = NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y, Terraria.ID.NPCID.Creeper);

				//TODO: Multiplayer compat
				Main.npc[i].lifeMax = 30;
				Main.npc[i].life = 30;
				Main.npc[i].SpawnedFromStatue = true;
				Main.npc[i].velocity += NPC.Center.DirectionTo(Main.player[NPC.target].Center) * 30;
				Main.npc[i].GetGlobalNPC<Creeper>().reworked = true;

				SoundEngine.PlaySound(SoundID.NPCDeath13, NPC.Center);
			}

			if (AttackTimer > 600)
			{
				AttackTimer = 0;
			}
		}

		public void Choice()
		{
			if (AttackTimer == 1)
			{
				savedPos = Main.rand.NextBool() ? thinker.Center + new Vector2(-325, 0) : thinker.Center + new Vector2(325, 0);
				ThisThinker.platformRadiusTarget = 400;
				ThisThinker.platformRotationTarget += 0.2f;
			}

			if (AttackTimer < 60)
			{
				NPC.Center += (savedPos - NPC.Center) * 0.08f;
			}

			if (AttackTimer == 60)
			{
				Projectile.NewProjectile(null, thinker.Center, Vector2.UnitY * 0.5f, ModContent.ProjectileType<VeinSpear>(), VeinSpearDamage, 0, Main.myPlayer, thinker.whoAmI);
				Projectile.NewProjectile(null, thinker.Center, Vector2.UnitY * -0.5f, ModContent.ProjectileType<VeinSpear>(), VeinSpearDamage, 0, Main.myPlayer, thinker.whoAmI);
			}

			Vector2 relativePos = savedPos - thinker.Center;
			Vector2 oppPos = thinker.Center + relativePos.RotatedBy(3.14f);

			for (int k = 0; k < neurisms.Count; k++)
			{
				float lerp = k / (neurisms.Count - 1f);

				if (AttackTimer == 60 + k * 20)
				{
					neurisms[k].Center = oppPos + Vector2.UnitX * (-300 + lerp * 600) + Vector2.UnitY * 750;
					(neurisms[k].ModNPC as Neurysm).State = 2;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
					(neurisms[k].ModNPC as Neurysm).TellTime = 30;
					(neurisms[k].ModNPC as Neurysm).tellDirection = 1.57f;
					(neurisms[k].ModNPC as Neurysm).tellLen = 1500;
				}

				if (AttackTimer >= 90 + k * 20 && AttackTimer <= 210 + k * 20)
				{
					Vector2 start = oppPos + Vector2.UnitX * (-300 + lerp * 600) + Vector2.UnitY * 750;
					Vector2 target = oppPos + Vector2.UnitX * (-300 + lerp * 600) + Vector2.UnitY * -750;

					float prog = Helpers.Helper.BezierEase((AttackTimer - (90 + k * 20)) / 120f);

					neurisms[k].Center = Vector2.Lerp(start, target, prog);
					(neurisms[k].ModNPC as Neurysm).State = 0;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}

				if (AttackTimer == 210 + k * 20)
				{
					neurisms[k].velocity *= 0;
					(neurisms[k].ModNPC as Neurysm).State = 1;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}
			}

			for(int k = 1; k < 6; k++)
			{
				if (AttackTimer == 120 + k * 10)
					Projectile.NewProjectile(null, thinker.Center, Vector2.Normalize(NPC.Center - thinker.Center).RotatedBy((-0.5f + k / 6f) * 3.14f) * 0.5f, ModContent.ProjectileType<VeinSpear>(), VeinSpearDamage, 0, Main.myPlayer, thinker.whoAmI);
			}

			int secondPartBase = 210 + neurisms.Count * 20;

			if (AttackTimer >= secondPartBase)
			{

				if (AttackTimer == secondPartBase)
				{
					savedPos = Main.rand.NextBool() ? thinker.Center + new Vector2(0, -325) : thinker.Center + new Vector2(0, 325);
				}

				if (AttackTimer < secondPartBase + 60)
				{
					// Extra offset here since being straight above/below looks weird
					NPC.Center += ((savedPos + Vector2.UnitX * 200) - NPC.Center) * 0.08f;
				}

				if (AttackTimer == secondPartBase + 60)
				{
					Projectile.NewProjectile(null, thinker.Center, Vector2.UnitX * 0.5f, ModContent.ProjectileType<VeinSpear>(), VeinSpearDamage, 0, Main.myPlayer, thinker.whoAmI);
					Projectile.NewProjectile(null, thinker.Center, Vector2.UnitX * -0.5f, ModContent.ProjectileType<VeinSpear>(), VeinSpearDamage, 0, Main.myPlayer, thinker.whoAmI);
				}

				for (int k = 0; k < neurisms.Count; k += 2)
				{
					float lerp = k / (neurisms.Count - 1f);

					if (AttackTimer == secondPartBase + 60 + k * 20)
					{
						neurisms[k].Center = oppPos + Vector2.UnitY * (-300 + lerp * 600) + Vector2.UnitX * 750;
						(neurisms[k].ModNPC as Neurysm).State = 2;
						(neurisms[k].ModNPC as Neurysm).Timer = 0;
						(neurisms[k].ModNPC as Neurysm).TellTime = 30;
						(neurisms[k].ModNPC as Neurysm).tellDirection = 0f;
						(neurisms[k].ModNPC as Neurysm).tellLen = 1500;
					}

					if (AttackTimer >= secondPartBase + 90 + k * 20 && AttackTimer <= secondPartBase + 210 + k * 20)
					{
						Vector2 start = oppPos + Vector2.UnitY * (-300 + lerp * 600) + Vector2.UnitX * 750;
						Vector2 target = oppPos + Vector2.UnitY * (-300 + lerp * 600) + Vector2.UnitX * -750;

						float prog = Helpers.Helper.BezierEase((AttackTimer - (secondPartBase + 90 + k * 20)) / 120f);

						neurisms[k].Center = Vector2.Lerp(start, target, prog);
						(neurisms[k].ModNPC as Neurysm).State = 0;
						(neurisms[k].ModNPC as Neurysm).Timer = 0;
					}

					if (AttackTimer == secondPartBase + 210 + k * 20)
					{
						neurisms[k].velocity *= 0;
						(neurisms[k].ModNPC as Neurysm).State = 1;
						(neurisms[k].ModNPC as Neurysm).Timer = 0;
					}
				}

				for (int k = 1; k < 6; k++)
				{
					if (AttackTimer == secondPartBase + 120 + k * 10)
						Projectile.NewProjectile(null, thinker.Center, Vector2.Normalize(savedPos - thinker.Center).RotatedBy((-0.5f + k / 6f) * 3.14f) * 0.5f, ModContent.ProjectileType<VeinSpear>(), 25, 0, Main.myPlayer, thinker.whoAmI);
				}
			}

			if (AttackTimer >= secondPartBase + 450)
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
				if (NPC.rotation > 0)
					NPC.rotation -= 0.02f;

				NPC.position.Y -= 1.8f;
			}

			if (AttackTimer > 90 && AttackTimer < 120)
				opacity = 1f - (AttackTimer - 90) / 30f;

			if (AttackTimer == 120)
				NPC.Center = thinker.Center + new Vector2(0, -200);

			if (AttackTimer > 120 && AttackTimer < 150)
				opacity = (AttackTimer - 120) / 30f;

			if (AttackTimer == 150)
				AttackTimer = 0;
		}

		public void DoubleSpin()
		{
			if (AttackTimer == 1)
			{
				ThisThinker.platformRadiusTarget = 600;
				ThisThinker.platformRotationTarget -= 0.2f;

				savedRot = Main.rand.Next(4) * (6.28f / 4f);
				NPC.netUpdate = true;
			}

			Vector2 targetPos = (thinker.ModNPC as TheThinker).home + Vector2.UnitX.RotatedBy(savedRot + AttackTimer / 400f * 6.28f) * 550;
			Vector2 targetPos2 = (thinker.ModNPC as TheThinker).home + Vector2.UnitX.RotatedBy(savedRot + AttackTimer / 400f * 6.28f) * -550;

			if (AttackTimer < 30)
				(thinker.ModNPC as TheThinker).ExtraRadius = -60 * AttackTimer / 30f;

			if (AttackTimer >= 1 && AttackTimer < 400)
			{
				float speed = Math.Min(0.05f, AttackTimer / 100f * 0.05f);

				NPC.Center += (targetPos - NPC.Center) * speed;
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
							neurisms[k].Center = NPC.Center + Vector2.UnitX.RotatedBy(rot) * rad;
					}
				}
			}

			if (Main.expertMode && AttackTimer > 60 && AttackTimer % 60 == 0)
			{
				Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, NPC.Center.DirectionTo(ThisThinker.home) * 8, ModContent.ProjectileType<BrainBolt>(), BrainBoltDamage, 1, Main.myPlayer, 160, 0, 30);

				if (Main.masterMode)
				{
					Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, NPC.Center.DirectionTo(ThisThinker.home).RotatedBy(0.5f) * 8, ModContent.ProjectileType<BrainBolt>(), BrainBoltDamage, 1, Main.myPlayer, 160, 0, 30);
					Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, NPC.Center.DirectionTo(ThisThinker.home).RotatedBy(-0.5f) * 8, ModContent.ProjectileType<BrainBolt>(), BrainBoltDamage, 1, Main.myPlayer, 160, 0, 30);

					Projectile.NewProjectile(NPC.GetSource_FromThis(), thinker.Center, thinker.Center.DirectionTo(ThisThinker.home) * 8, ModContent.ProjectileType<BrainBolt>(), BrainBoltDamage, 1, Main.myPlayer, 160, 0, 30);
					Projectile.NewProjectile(NPC.GetSource_FromThis(), thinker.Center, thinker.Center.DirectionTo(ThisThinker.home).RotatedBy(0.5f) * 8, ModContent.ProjectileType<BrainBolt>(), BrainBoltDamage, 1, Main.myPlayer, 160, 0, 30);
					Projectile.NewProjectile(NPC.GetSource_FromThis(), thinker.Center, thinker.Center.DirectionTo(ThisThinker.home).RotatedBy(-0.5f) * 8, ModContent.ProjectileType<BrainBolt>(), BrainBoltDamage, 1, Main.myPlayer, 160, 0, 30);
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
				ThisThinker.platformRadiusTarget = 400;
				ThisThinker.platformRotationTarget -= 0.2f;
			}

			if (AttackTimer == 90)
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

				NPC.chaseable = false;
				NPC.Center = savedPos;
			}

			if (AttackTimer > 90 && AttackTimer <= 120)
				opacity = (AttackTimer - 90) / 30f;

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
				SoundEngine.PlaySound(SoundID.NPCDeath10.WithPitchOffset(0.5f), NPC.Center);

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
				NPC.chaseable = true;
				AttackTimer = 0;
			}
		}

		public void TeleportHunt()
		{
			if (AttackTimer == 1)
			{
				ThisThinker.platformRadiusTarget = 550;
				ThisThinker.platformRotationTarget -= 0.2f;
			}

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

			if (motionTime == 30 && AttackTimer < 150 * 5)
			{
				// Cant do this on the last one or else the platforms may try to move again too early
				if (AttackTimer < 150 * 4)
				{
					ThisThinker.platformRadiusTarget -= 40;
					ThisThinker.platformRotationTarget -= 0.1f;
				}

				NPC.TargetClosest();

				NPC.Center = Main.player[NPC.target].Center + Vector2.UnitX.RotatedByRandom(6.28f) * 150;

				savedPos = NPC.Center;
				savedPos2 = savedPos + savedPos.DirectionTo(Main.player[NPC.target].Center) * 500;
			}

			if (motionTime > 30 && motionTime <= 60)
			{
				opacity = (motionTime - 30) / 30f;
			}

			if (motionTime > 60 && motionTime <= 90)
			{
				NPC.Center += Vector2.Normalize(NPC.Center - savedPos2) * 7 * (1f - (motionTime - 60) / 30f); 
				//Vector2.Normalize( * -100f, (motionTime - 30) / 60f);
			}

			if (motionTime == 90)
			{
				savedPos = NPC.Center;
				SoundEngine.PlaySound(SoundID.NPCDeath10.WithPitchOffset(0.5f), NPC.Center);
			}

			if (motionTime >= 90)
			{
				contactDamage = true;
				NPC.Center = Vector2.Lerp(savedPos, savedPos2, Helpers.Helper.SwoopEase((motionTime - 90) / 60f));
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

		public void DrawHuntGraphics(SpriteBatch spriteBatch)
		{
			float motionTime = AttackTimer % 150;

			if (motionTime > 60 && motionTime < 90)
			{
				float prog = (motionTime - 60) / 30f;

				for (int k = 0; k < 6; k++)
				{
					Vector2 pos = NPC.Center + Vector2.UnitX.RotatedBy(k / 6f * 6.28f + prog * 3.14f) * (1f - prog) * 128;
					DrawBrainSegments(spriteBatch, NPC, pos - Main.screenPosition, new Color(255, 50, 70), NPC.rotation, NPC.scale, 0.35f * prog, lastPos);
				}
			}

			if (motionTime > 50 && motionTime < 80)
			{
				float rot = NPC.Center.DirectionTo(savedPos2).ToRotation();
				var tellTex = Assets.Misc.DirectionalBeam.Value;
				var pos = Vector2.Lerp(NPC.Center, savedPos2, 0.2f);
				var target = new Rectangle((int)pos.X, (int)pos.Y, 900, 50);
				target.Offset((-Main.screenPosition).ToPoint());

				spriteBatch.Draw(tellTex, target, null, new Color(160, 30, 30, 0) * (float)Math.Sin((motionTime - 50) / 30f * 3.14f), rot, new Vector2(0, tellTex.Height / 2), 0, 0);

				target = new Rectangle((int)pos.X, (int)pos.Y, 900, 250);
				target.Offset((-Main.screenPosition).ToPoint());

				spriteBatch.Draw(tellTex, target, null, new Color(60, 10, 10, 0) * (float)Math.Sin((motionTime - 50) / 30f * 3.14f), rot, new Vector2(0, tellTex.Height / 2), 0, 0);
			}

			if (motionTime > 90)
			{
				for (int k = 0; k < 10; k++)
				{
					Vector2 pos = NPC.oldPos[k] + NPC.Size / 2f;
					DrawBrainSegments(spriteBatch, NPC, pos - Main.screenPosition, new Color(255, 50, 70), NPC.rotation, NPC.scale, (k / 30f) * (1f - (motionTime - 90) / 60f), lastPos);
				}
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
				ThisThinker.platformRadiusTarget = 400;
				ThisThinker.platformRotationTarget -= 0.2f;

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

				NPC.chaseable = false;
				NPC.Center = savedPos;
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
				SoundEngine.PlaySound(SoundID.NPCDeath10.WithPitchOffset(0.5f), NPC.Center);

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

				NPC.chaseable = true;
				AttackTimer = 0;
			}
		}
		#endregion
	}
}
