using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using rail;
using StarlightRiver.Content.Bosses.SquidBoss;
using StarlightRiver.Content.Dusts;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.TheThinkerBoss
{
	internal partial class DeadBrain
	{
		#region Phase 1
		public void ShrinkingCircle()
		{
			if (AttackTimer == 1)
				NPC.TargetClosest();

			if (AttackTimer < 60 && !Main.expertMode)
			{
				NPC.Center += (thinker.Center + new Vector2(250, -250) - NPC.Center) * 0.08f;
			}

			// Do an additional slam on expert or higher
			if (Main.expertMode)
			{
				if (AttackTimer < 180)
				{
					Vector2 target = Main.player[NPC.target].Center + new Vector2(0, -500);

					if (target.Y > ThisThinker.home.Y)
						target.Y = ThisThinker.home.Y;

					float speed = AttackTimer < 140 ? 1 : (AttackTimer - 140) / 40f;

					NPC.Center += (target - NPC.Center) * 0.1f * speed;
				}

				if (AttackTimer == 120)
				{
					SoundEngine.PlaySound(SoundID.Zombie62.WithPitchOffset(-0.25f), NPC.Center);
				}

				if (AttackTimer > 150 && AttackTimer < 180)
				{
					chargeAnimation = (AttackTimer - 150) / 30f;
				}

				if (AttackTimer == 180)
				{
					savedPos = NPC.Center;
					chargeAnimation = 0;
				}

				if (AttackTimer > 180 && AttackTimer < 220)
				{
					contactDamage = true;
					NPC.Center = Vector2.Lerp(savedPos, savedPos + new Vector2(0, -80), Helpers.Eases.BezierEase((AttackTimer - 180) / 40f));
				}

				if (AttackTimer == 220)
				{
					SoundEngine.PlaySound(SoundID.DeerclopsScream.WithPitchOffset(-0.1f), NPC.Center);
					savedPos = NPC.Center;
				}

				if (AttackTimer > 220 && AttackTimer < 260)
				{
					contactDamage = true;
					NPC.Center = Vector2.Lerp(savedPos, savedPos + new Vector2(0, 900), Helpers.Eases.BezierEase((AttackTimer - 220) / 40f));
				}

				if (AttackTimer == 260)
				{
					contactDamage = false;
				}
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

			if (AttackTimer >= 61 && AttackTimer < 230)
			{
				float timer = Helpers.Eases.BezierEase((AttackTimer - 60) / 180f);
				float totalRot = Main.masterMode ? 2.4f : Main.expertMode ? 2f : 1.6f;

				for (int k = 0; k < neurisms.Count; k++)
				{
					float rot = k / (float)neurisms.Count * 6.28f + timer * totalRot;

					neurisms[k].Center = thinker.Center + Vector2.UnitX.RotatedBy(rot) * (750 - timer * 675f);
					(neurisms[k].ModNPC as Neurysm).State = 0;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}
			}

			if (AttackTimer == 160 && Main.masterMode && Main.netMode != NetmodeID.MultiplayerClient)
			{
				for (int k = 0; k < 12; k++)
				{
					Projectile.NewProjectile(null, thinker.Center, Vector2.UnitX.RotatedBy(k / 12f * 6.28f) * 4, ModContent.ProjectileType<BrainBolt>(), BrainBoltDamage, 0, Main.myPlayer, 200, k % 2 == 0 ? 1 : 0);
				}
			}

			if (AttackTimer == 230)
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

			if (AttackTimer == 260)
			{
				AttackTimer = 0;
			}
		}

		public void LineThrow()
		{
			if (!Main.expertMode)
			{
				if (AttackTimer < 60)
				{
					NPC.Center += (thinker.Center + new Vector2(-250, -250) - NPC.Center) * 0.08f;
				}
			}
			else // Spin on expert or higher
			{
				if (AttackTimer < 60)
				{
					NPC.Center += (thinker.Center + new Vector2(-470, 0) - NPC.Center) * 0.08f;
				}

				if (AttackTimer > 140 && AttackTimer < 180)
					chargeAnimation = (AttackTimer - 140) / 40f;

				if (AttackTimer == 180)
				{
					SoundEngine.PlaySound(SoundID.Zombie1.WithPitchOffset(-1f), NPC.Center);
					contactDamage = true;
				}

				if (AttackTimer == 270)
					SoundEngine.PlaySound(SoundID.Zombie1.WithPitchOffset(-1f), NPC.Center);

				if (AttackTimer > 180)
				{
					float rot = Helpers.Eases.BezierEase((AttackTimer - 180) / 180f) * 6.28f;
					NPC.Center = thinker.Center + new Vector2(-470, 0).RotatedBy(rot);
				}

				if (AttackTimer == 360)
					contactDamage = false;
			}

			if (AttackTimer == 1)
			{
				ThisThinker.platformRadiusTarget = 550;
				ThisThinker.platformRotationTarget += 0.2f;

				savedRot = Main.rand.NextFloat(6.28f);
				NPC.netUpdate = true;
			}

			if (AttackTimer == 60 && Main.netMode != NetmodeID.MultiplayerClient)
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

			if (AttackTimer == 75 && Main.masterMode && Main.netMode != NetmodeID.MultiplayerClient)
			{
				for (int k = 0; k < 9; k++)
				{
					Projectile.NewProjectile(null, thinker.Center, Vector2.UnitX.RotatedBy(savedRot + (k + 0.5f) / 9f * 6.28f) * 4, ModContent.ProjectileType<BrainBolt>(), BrainBoltDamage, 0, Main.myPlayer, 200, 1);
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
					(neurisms[k].ModNPC as Neurysm).DoTell(offset * 2, rot + 1.57f * direction);
				}

				if (AttackTimer >= 60 + k * 20 && AttackTimer <= 180 + k * 20)
				{
					float rot = savedRot;
					float direction = k % 2 == 0 ? 1 : -1;

					float a = (1 - lerp * 2) * 750f;
					float h = 750f;

					float offset = (float)Math.Sqrt(Math.Pow(h, 2) - Math.Pow(a, 2));

					float prog = Helpers.Eases.BezierEase((AttackTimer - (60 + k * 20)) / 120f);

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
					speed *= 1f - (AttackTimer - 110) / 30f;

				NPC.Center += (targetPos - NPC.Center) * speed;
			}

			if (AttackTimer == 70)
			{
				SoundEngine.PlaySound(SoundID.Zombie62.WithPitchOffset(-0.5f), NPC.Center);
			}

			if (AttackTimer == 130)
			{
				savedPos = thinker.Center + relativePos;
			}

			// Charge animation
			if (AttackTimer > 100 && AttackTimer < 140)
			{
				chargeAnimation = (AttackTimer - 100) / 40f;
			}

			if (AttackTimer == 140)
			{
				contactDamage = true;
				chargeAnimation = 0;
			}

			// Move back
			if (AttackTimer > 140 && AttackTimer < 160)
			{
				NPC.Center += Vector2.Normalize(NPC.Center - savedPos) * 20 * (1f - (AttackTimer - 140) / 20f);
			}

			// Play attack noise
			if (AttackTimer == 160)
			{
				savedPos2 = NPC.Center;
				SoundEngine.PlaySound(SoundID.NPCDeath10.WithPitchOffset(0.5f), NPC.Center);
			}

			// Charge
			if (AttackTimer > 160)
			{
				float prog = Helpers.Eases.SwoopEase((AttackTimer - 160) / chargeTime);
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

			if (AttackTimer > 130 && AttackTimer < 160)
			{
				float rot = NPC.Center.DirectionTo(savedPos).ToRotation();
				Texture2D tellTex = Assets.Misc.DirectionalBeam.Value;
				var pos = Vector2.Lerp(NPC.Center, savedPos, 0.2f);
				var target = new Rectangle((int)pos.X, (int)pos.Y, 900, 50);
				target.Offset((-Main.screenPosition).ToPoint());

				spriteBatch.Draw(tellTex, target, null, new Color(160, 30, 30, 0) * (float)Math.Sin((AttackTimer - 140) / 30f * 3.14f), rot, new Vector2(0, tellTex.Height / 2), 0, 0);

				target = new Rectangle((int)pos.X, (int)pos.Y, 900, 250);
				target.Offset((-Main.screenPosition).ToPoint());

				spriteBatch.Draw(tellTex, target, null, new Color(60, 10, 10, 0) * (float)Math.Sin((AttackTimer - 140) / 30f * 3.14f), rot, new Vector2(0, tellTex.Height / 2), 0, 0);

			}
		}

		public void Spawn()
		{
			Vector2 relativePos = Main.player[NPC.target].Center - thinker.Center;
			Vector2 targetPos = thinker.Center + relativePos.RotatedBy(3.14f);

			float radiusMax = Main.masterMode ? 700 : Main.expertMode ? 400 : 200;
			float speed = Main.masterMode ? 9.42f : 6.28f;

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
					float rot = k * 2 / (float)neurisms.Count * 6.28f + Helpers.Eases.BezierEase((AttackTimer - 30) / 540f) * speed * (k % 2 == 0 ? 1 : -1);

					neurisms[k].Center = thinker.Center + Vector2.UnitX.RotatedBy(rot) * (750 - radius);
					(neurisms[k].ModNPC as Neurysm).State = 0;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}
			}

			if (AttackTimer == 560 && Main.masterMode && Main.netMode != NetmodeID.MultiplayerClient)
			{
				neurisms.ForEach(n => Projectile.NewProjectile(null, n.Center, Vector2.Normalize(thinker.Center - n.Center) * Vector2.Distance(n.Center, thinker.Center) / 45f, ModContent.ProjectileType<BrainBolt>(), BrainBoltDamage, 0, Main.myPlayer, 45));
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
				new CreeperSpawnPacket(NPC.whoAmI, NPC.target).Send(-1, -1, Main.netMode == NetmodeID.SinglePlayer);
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
				savedPos2 = savedPos;
				ThisThinker.platformRadiusTarget = 400;
				ThisThinker.platformRotationTarget += 0.2f;
			}

			if (AttackTimer > 1)
			{
				NPC.Center += (savedPos - NPC.Center) * 0.03f;
				savedPos.Y = Main.player[NPC.target].Center.Y;
			}

			if (AttackTimer == 60 && Main.netMode != NetmodeID.MultiplayerClient)
			{
				Projectile.NewProjectile(null, thinker.Center, Vector2.UnitY * 0.5f, ModContent.ProjectileType<VeinSpear>(), VeinSpearDamage, 0, Main.myPlayer, thinker.whoAmI, 260);
				Projectile.NewProjectile(null, thinker.Center, Vector2.UnitY * -0.5f, ModContent.ProjectileType<VeinSpear>(), VeinSpearDamage, 0, Main.myPlayer, thinker.whoAmI, 260);
			}

			Vector2 relativePos = savedPos2 - thinker.Center;
			Vector2 oppPos = thinker.Center + relativePos.RotatedBy(3.14f);

			for (int k = 0; k < neurisms.Count; k++)
			{
				float lerp = k / (neurisms.Count - 1f);

				if (AttackTimer == 60 + k * 15)
				{
					neurisms[k].Center = ThisThinker.home + Vector2.UnitX * (-600 + lerp * 1200) + Vector2.UnitY * 750;
					(neurisms[k].ModNPC as Neurysm).State = 2;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
					//(neurisms[k].ModNPC as Neurysm).DoTell(1500, 1.57f);
				}

				if (AttackTimer >= 90 + k * 15 && AttackTimer <= 240 + k * 15)
				{
					Vector2 start = ThisThinker.home + Vector2.UnitX * (-600 + lerp * 1200) + Vector2.UnitY * 750;
					Vector2 target = ThisThinker.home + Vector2.UnitX * (-600 + lerp * 1200) + Vector2.UnitY * -750;

					float prog = Helpers.Eases.BezierEase((AttackTimer - (90 + k * 15)) / 150f);

					neurisms[k].Center = Vector2.Lerp(start, target, prog);
					neurisms[k].position.X += MathF.Sin(prog * 3.14f) * (start.X - ThisThinker.home.X) * 0.5f;
					(neurisms[k].ModNPC as Neurysm).State = 0;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}

				if (AttackTimer == 240 + k * 15)
				{
					neurisms[k].velocity *= 0;
					(neurisms[k].ModNPC as Neurysm).State = 1;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}
			}

			for (int k = 1; k < 6; k++)
			{
				if (AttackTimer == 120 + k * 10 && Main.netMode != NetmodeID.MultiplayerClient)
					Projectile.NewProjectile(null, thinker.Center, Vector2.Normalize(oppPos - thinker.Center).RotatedBy((-0.5f + k / 6f) * 3.14f) * 0.5f, ModContent.ProjectileType<VeinSpear>(), VeinSpearDamage, 0, Main.myPlayer, thinker.whoAmI, 200 - k * 10);
			}

			if (AttackTimer >= 240 + neurisms.Count * 15)
			{
				AttackTimer = 0;
			}
		}

		public void ChoiceAlt()
		{
			if (AttackTimer == 1)
			{
				savedPos = Main.rand.NextBool() ? thinker.Center + new Vector2(0, -325) : thinker.Center + new Vector2(0, 325);
				savedPos2 = savedPos;
			}

			if (AttackTimer > 1)
			{
				// Extra offset here since being straight above/below looks weird
				NPC.Center += (savedPos + Vector2.UnitX * 200 - NPC.Center) * 0.03f;
				savedPos.X = Main.player[NPC.target].Center.X;
			}

			if (AttackTimer == 60 && Main.netMode != NetmodeID.MultiplayerClient)
			{
				Projectile.NewProjectile(null, thinker.Center, Vector2.UnitX * 0.5f, ModContent.ProjectileType<VeinSpear>(), VeinSpearDamage, 0, Main.myPlayer, thinker.whoAmI, 260);
				Projectile.NewProjectile(null, thinker.Center, Vector2.UnitX * -0.5f, ModContent.ProjectileType<VeinSpear>(), VeinSpearDamage, 0, Main.myPlayer, thinker.whoAmI, 260);
			}

			for (int k = 0; k < neurisms.Count; k += 2)
			{
				float lerp = k / (neurisms.Count - 1f);

				if (AttackTimer == 60 + k * 15)
				{
					neurisms[k].Center = savedPos2 + Vector2.UnitY * (-300 + lerp * 600) + Vector2.UnitX * 750;
					(neurisms[k].ModNPC as Neurysm).State = 2;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
					(neurisms[k].ModNPC as Neurysm).DoTell(1500, 0f);
				}

				if (AttackTimer >= 90 + k * 15 && AttackTimer <= 210 + k * 15)
				{
					Vector2 start = savedPos2 + Vector2.UnitY * (-300 + lerp * 600) + Vector2.UnitX * 750;
					Vector2 target = savedPos2 + Vector2.UnitY * (-300 + lerp * 600) + Vector2.UnitX * -750;

					float prog = Helpers.Eases.BezierEase((AttackTimer - (90 + k * 15)) / 120f);

					neurisms[k].Center = Vector2.Lerp(start, target, prog);
					(neurisms[k].ModNPC as Neurysm).State = 0;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}

				if (AttackTimer == 210 + k * 15)
				{
					neurisms[k].velocity *= 0;
					(neurisms[k].ModNPC as Neurysm).State = 1;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}
			}

			for (int k = 1; k < 6; k++)
			{
				if (AttackTimer == 120 + k * 10 && Main.netMode != NetmodeID.MultiplayerClient)
					Projectile.NewProjectile(null, thinker.Center, Vector2.Normalize(savedPos2 - thinker.Center).RotatedBy((-0.5f + k / 6f) * 3.14f) * 0.5f, ModContent.ProjectileType<VeinSpear>(), 25, 0, Main.myPlayer, thinker.whoAmI, 200 - k * 10);
			}

			if (AttackTimer == 60)
			{
				ThisThinker.pulseTime = 30;

				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					for (int k = 0; k <= 7; k++)
						Projectile.NewProjectile(null, thinker.Center, Vector2.Normalize(savedPos2 - thinker.Center).RotatedBy(1.57f + k / 7f * 3.14f) * 4.5f, ModContent.ProjectileType<BrainBolt>(), BrainBoltDamage, 1, Main.myPlayer, 210, 0, 20);
				}
			}

			if (AttackTimer == 120)
			{
				ThisThinker.pulseTime = 30;

				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					for (int k = 0; k <= 4; k++)
						Projectile.NewProjectile(null, thinker.Center, Vector2.Normalize(savedPos2 - thinker.Center).RotatedBy(1.57f + k / 4f * 3.14f) * 4.5f, ModContent.ProjectileType<BrainBolt>(), BrainBoltDamage, 1, Main.myPlayer, 210, 1, 20);
				}
			}

			if (AttackTimer == 180)
			{
				ThisThinker.pulseTime = 30;

				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					for (int k = 0; k <= 7; k++)
						Projectile.NewProjectile(null, thinker.Center, Vector2.Normalize(savedPos2 - thinker.Center).RotatedBy(1.57f + k / 7f * 3.14f) * 4.5f, ModContent.ProjectileType<BrainBolt>(), BrainBoltDamage, 1, Main.myPlayer, 210, 0, 20);
				}
			}

			if (AttackTimer >= 210 + neurisms.Count * 15)
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

				savedRot = 0;
				NPC.netUpdate = true;
			}

			if (AttackTimer < 30)
				ThisThinker.ExtraGrayAuraRadius = -60 * AttackTimer / 30f;

			if (AttackTimer < 90)
			{
				Vector2 targetPos = ThisThinker.home + Vector2.UnitX * 550;
				Vector2 targetPos2 = ThisThinker.home + Vector2.UnitX * -550;

				float speed = Math.Min(0.05f, MathF.Sin(AttackTimer / 90f * 3.14f) * 0.05f);

				NPC.Center += (targetPos - NPC.Center) * speed;
				thinker.Center += (targetPos2 - thinker.Center) * speed;
			}

			if ((AttackTimer - 90) >= 1 && (AttackTimer - 90) < 400)
			{
				Vector2 targetPos = ThisThinker.home + Vector2.UnitX.RotatedBy(savedRot + (AttackTimer - 90) / 400f * 6.28f) * 550;
				Vector2 targetPos2 = ThisThinker.home + Vector2.UnitX.RotatedBy(savedRot + (AttackTimer - 90) / 400f * 6.28f) * -550;

				float speed = Math.Min(0.05f, (AttackTimer - 90) / 100f * 0.05f);

				NPC.Center += (targetPos - NPC.Center) * speed;
				thinker.Center += (targetPos2 - thinker.Center) * speed;

				if ((AttackTimer - 90) >= 60)
				{
					float maxRad = Main.masterMode ? 500 : Main.expertMode ? 480 : 360;
					float period = Main.masterMode ? 85 : 170;
					float rad = 280 + (float)Math.Cos((AttackTimer - 90 - 60) / period * 3.14f + 3.14f) * (maxRad - 280);

					if (Main.masterMode && ((AttackTimer - 90) == 60 + 170 || (AttackTimer - 90) == 60) && Main.netMode != NetmodeID.MultiplayerClient)
					{
						for (int k = 0; k < 9; k++)
						{
							float rot = k / 9f * 6.28f;
							Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.UnitX.RotatedBy(rot) * 6, ModContent.ProjectileType<BrainBolt>(), BrainBoltDamage, 1, Main.myPlayer, 160, 0, 20);
							Projectile.NewProjectile(NPC.GetSource_FromThis(), thinker.Center, Vector2.UnitX.RotatedBy(rot) * 6, ModContent.ProjectileType<BrainBolt>(), BrainBoltDamage, 1, Main.myPlayer, 160, 1, 20);
						}
					}

					for (int k = 0; k < neurisms.Count; k++)
					{
						float rot = k * 2 / (float)neurisms.Count * 6.28f + (AttackTimer - 90) * -0.015f;

						if (k % 2 == 0)
							neurisms[k].Center = thinker.Center + Vector2.UnitX.RotatedBy(rot) * rad;
						else
							neurisms[k].Center = NPC.Center + Vector2.UnitX.RotatedBy(rot) * rad;
					}
				}
			}

			if ((AttackTimer - 90) == 60)
			{
				contactDamage = true;

				for (int k = 0; k < neurisms.Count; k++)
				{
					(neurisms[k].ModNPC as Neurysm).State = 2;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}
			}

			if ((AttackTimer - 90) == 90)
			{
				for (int k = 0; k < neurisms.Count; k++)
				{
					(neurisms[k].ModNPC as Neurysm).State = 0;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}
			}

			if ((AttackTimer - 90) == 370)
			{
				for (int k = 0; k < neurisms.Count; k++)
				{
					(neurisms[k].ModNPC as Neurysm).State = 1;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}
			}

			if ((AttackTimer - 90) >= 370)
				ThisThinker.ExtraGrayAuraRadius = -60 + 60 * (AttackTimer - 90 - 370) / 30f;

			if ((AttackTimer - 90) >= 400)
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
			{
				ThisThinker.ExtraGrayAuraRadius = 180 * (AttackTimer - 30) / 30f;
				ThisThinker.heartPetalProgress = (AttackTimer - 30) / 30f;
			}

			if (AttackTimer > 60 && AttackTimer < 300)
				ThisThinker.ExtraGrayAuraRadius = 180 - 60 * ((AttackTimer - 60) / 240f);

			if (AttackTimer == 60)
			{
				ThisThinker.platformRadiusTarget = 400;
				ThisThinker.platformRotationTarget -= 0.2f;
			}

			if (AttackTimer == 90)
			{
				int random = Main.rand.Next(10);
				savedPos = ThisThinker.home + Vector2.UnitX.RotatedBy(random / 10f * 6.28f) * 590;
				NPC.netUpdate = true;

				for (int k = 0; k < 10; k++)
				{
					if (k != random)
					{
						Vector2 pos = ThisThinker.home + Vector2.UnitX.RotatedBy(k / 10f * 6.28f) * 590;
						new VisageSpawnPacket(NPC.whoAmI, (int)pos.X, (int)pos.Y).Send(-1, -1, Main.netMode == NetmodeID.SinglePlayer);
					}
				}

				NPC.chaseable = false;
				TeleportWithChain(savedPos);
			}

			if (AttackTimer > 90 && AttackTimer <= 120)
				opacity = (AttackTimer - 90) / 30f;

			if (AttackTimer <= 120)
			{
				thinker.Center += (ThisThinker.home - thinker.Center) * 0.03f;
			}

			if (AttackTimer > 90 && AttackTimer < 410)
			{
				if (hurtLastFrame)
				{
					AttackTimer = 410;
					NPC.netUpdate = true;
				}
			}

			// Bullet hell for expert and above
			if (Main.expertMode)
			{
				if (AttackTimer == 140)
				{
					ThisThinker.pulseTime = 30;

					if (Main.netMode != NetmodeID.MultiplayerClient)
					{
						for (int k = 0; k < 12; k++)
						{
							Projectile.NewProjectile(null, thinker.Center, Vector2.UnitX.RotatedBy(k / 12f * 6.28f) * 4, ModContent.ProjectileType<BrainBolt>(), BrainBoltDamage, 0, Main.myPlayer, 200, 0, 20);
						}
					}
				}

				if (AttackTimer == 200)
				{
					ThisThinker.pulseTime = 30;

					if (Main.netMode != NetmodeID.MultiplayerClient)
					{
						for (int k = 0; k < 12; k++)
						{
							Projectile.NewProjectile(null, thinker.Center, Vector2.UnitX.RotatedBy((k + 0.5f) / 12f * 6.28f) * 4, ModContent.ProjectileType<BrainBolt>(), BrainBoltDamage, 0, Main.myPlayer, 200, 0, 20);
						}
					}
				}
			}

			// Timeout
			if (AttackTimer == 409)
			{
				SoundEngine.PlaySound(SoundID.NPCDeath10.WithPitchOffset(0.5f), NPC.Center);

				foreach (NPC npc in Main.npc.Where(n => n.active && n.ModNPC is HorrifyingVisage))
					npc.ai[1] = 1;
			}

			if (AttackTimer == 440)
			{
				foreach (NPC npc in Main.npc.Where(n => n.active && n.ModNPC is HorrifyingVisage))
					npc.ai[0] = 440;
			}

			if (AttackTimer > 440 && AttackTimer < 470)
			{
				ThisThinker.ExtraGrayAuraRadius = 120 - 120 * (AttackTimer - 440) / 30f;
				ThisThinker.heartPetalProgress = 1f - (AttackTimer - 440) / 30f;
			}

			if (AttackTimer == 500)
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

			// thinker follows then teleports back
			if (AttackTimer <= 30)
			{
				ThisThinker.ExtraGrayAuraRadius = -80 * (AttackTimer / 30f);
			}

			if (AttackTimer <= 710)
			{
				Vector2 targetPos = Main.player[NPC.target].Center + new Vector2(0, 175);
				targetPos += Vector2.UnitX.RotatedBy(AttackTimer / 650 * 6.28f * 4) * 30;

				float speed = AttackTimer < 650 ? 0.045f : 0.045f * (1f - (AttackTimer - 650) / 60f);

				if (AttackTimer < 120)
					speed *= AttackTimer / 120f;

				thinker.Center += (targetPos - thinker.Center) * speed;
			}

			if (AttackTimer > 680 && AttackTimer <= 710)
			{
				float teleTimer = AttackTimer - 680;
				ThisThinker.ExtraGrayAuraRadius = -80 + -100 * (teleTimer / 30f);
			}

			if (AttackTimer == 710)
			{
				Helpers.SoundHelper.PlayPitched("Magic/FireCast", 1, -0f, NPC.Center);
				thinker.Center = ThisThinker.home;
			}

			if (AttackTimer > 710 && AttackTimer <= 730)
			{
				float teleTimer = AttackTimer - 710;
				ThisThinker.ExtraGrayAuraRadius = -180 + 180 * (teleTimer / 20f);
			}

			if (AttackTimer == 730)
			{
				ThisThinker.ExtraGrayAuraRadius = 0;

				for (int k = 0; k < 30; k++)
				{
					float rot = Main.rand.NextFloat(6.28f);
					Dust.NewDustPerfect(thinker.Center + Vector2.One.RotatedBy(rot) * 50f, ModContent.DustType<GraymatterDust>(), Vector2.One.RotatedBy(rot) * Main.rand.NextFloat(5), 0, Color.White, Main.rand.NextFloat(1.5f, 3f));
				}
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

				TeleportWithChain(Main.player[NPC.target].Center + Vector2.UnitX.RotatedByRandom(6.28f) * 150);

				savedPos = NPC.Center;
				savedPos2 = savedPos + savedPos.DirectionTo(Main.player[NPC.target].Center) * 500;

				SoundEngine.PlaySound(SoundID.Zombie62.WithPitchOffset(-0.5f), NPC.Center);
			}

			if (motionTime > 30 && motionTime <= 50)
			{
				opacity = (motionTime - 30) / 20f;
			}

			if (motionTime > 50 && motionTime <= 70)
			{
				chargeAnimation = (motionTime - 50) / 20f;
			}

			if (motionTime == 70)
			{
				contactDamage = true;
				chargeAnimation = 0;
			}

			if (motionTime > 50 && motionTime <= 90)
			{
				NPC.Center += Vector2.Normalize(NPC.Center - savedPos2) * 16 * Eases.EaseCircularIn(1f - (motionTime - 50) / 40f);
				//Vector2.Normalize( * -100f, (motionTime - 30) / 60f);
			}

			if (motionTime == 90)
			{
				savedPos = NPC.Center;
				SoundEngine.PlaySound(SoundID.NPCDeath10.WithPitchOffset(0.5f), NPC.Center);
			}

			if (motionTime >= 90)
			{
				NPC.Center = Vector2.Lerp(savedPos, savedPos2, Helpers.Eases.SwoopEase((motionTime - 90) / 60f));
			}

			// Similar neirusm pattern to spawning in phase 1

			float radiusMax = Main.masterMode ? 700 : Main.expertMode ? 400 : 200;
			if (AttackTimer == 1)
			{
				for (int k = 0; k < neurisms.Count; k++)
				{
					float rot = k / (float)neurisms.Count * 6.28f;

					neurisms[k].Center = ThisThinker.home + Vector2.UnitX.RotatedBy(rot) * 750;
					(neurisms[k].ModNPC as Neurysm).State = 2;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}
			}

			if (AttackTimer >= 31)
			{
				for (int k = 0; k < neurisms.Count; k++)
				{
					float radius = Helpers.Eases.BezierEase((AttackTimer - 30) / (150 * 5 - 30)) * radiusMax;
					float rot = k / (float)neurisms.Count * 6.28f + Helpers.Eases.BezierEase((AttackTimer - 30) / (150 * 5 - 30)) * 6;

					neurisms[k].Center = ThisThinker.home + Vector2.UnitX.RotatedBy(rot) * (750 - radius);
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

			if (motionTime > 50 && motionTime < 80)
			{
				float rot = NPC.Center.DirectionTo(savedPos2).ToRotation();
				Texture2D tellTex = Assets.Misc.DirectionalBeam.Value;
				var pos = Vector2.Lerp(NPC.Center, savedPos2, 0.2f);
				var target = new Rectangle((int)pos.X, (int)pos.Y, 900, 50);
				target.Offset((-Main.screenPosition).ToPoint());

				spriteBatch.Draw(tellTex, target, null, new Color(160, 30, 30, 0) * (float)Math.Sin((motionTime - 50) / 30f * 3.14f), rot, new Vector2(0, tellTex.Height / 2), 0, 0);

				target = new Rectangle((int)pos.X, (int)pos.Y, 900, 250);
				target.Offset((-Main.screenPosition).ToPoint());

				spriteBatch.Draw(tellTex, target, null, new Color(60, 10, 10, 0) * (float)Math.Sin((motionTime - 50) / 30f * 3.14f), rot, new Vector2(0, tellTex.Height / 2), 0, 0);
			}
		}

		public void Clones2()
		{
			// Clones
			if (AttackTimer <= 30)
				opacity = 1f - AttackTimer / 30f;

			if (AttackTimer > 30 && AttackTimer < 60)
			{
				ThisThinker.ExtraGrayAuraRadius = 100 * (AttackTimer - 30) / 30f;
				ThisThinker.heartPetalProgress = (AttackTimer - 30) / 30f;
			}

			if (AttackTimer == 60)
			{
				ThisThinker.platformRadiusTarget = 400;
				ThisThinker.platformRotationTarget -= 0.2f;

				int random = Main.rand.Next(4);
				savedPos = ThisThinker.home + Vector2.UnitX.RotatedBy(random / 4f * 6.28f) * 590;

				NPC.netUpdate = true;

				for (int k = 0; k < 4; k++)
				{
					if (k != random)
					{
						Vector2 pos = ThisThinker.home + Vector2.UnitX.RotatedBy(k / 4f * 6.28f) * 590;
						new VisageSpawnPacket(NPC.whoAmI, (int)pos.X, (int)pos.Y).Send(-1, -1, Main.netMode == NetmodeID.SinglePlayer);
					}
				}

				NPC.chaseable = false;
				TeleportWithChain(savedPos);
			}

			if (AttackTimer > 60 && AttackTimer <= 90)
				opacity = (AttackTimer - 60) / 30f;

			if (AttackTimer <= 120)
			{
				thinker.Center += (ThisThinker.home - thinker.Center) * 0.03f;
			}

			// Exapanding circle
			if (AttackTimer == 31)
			{
				for (int k = 0; k < neurisms.Count; k++)
				{
					float rot = k / (float)neurisms.Count * 6.28f;

					neurisms[k].Center = ThisThinker.home + Vector2.UnitX.RotatedBy(rot) * 80;
					(neurisms[k].ModNPC as Neurysm).State = 2;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}
			}

			if (AttackTimer >= 61 && AttackTimer < 240)
			{
				float timer = Helpers.Eases.BezierEase((AttackTimer - 60) / 180f);
				float totalRot = Main.masterMode ? 2f : Main.expertMode ? 1.5f : 1f;

				for (int k = 0; k < neurisms.Count; k++)
				{
					float rot = k / (float)neurisms.Count * 6.28f + timer * totalRot;

					neurisms[k].Center = ThisThinker.home + Vector2.UnitX.RotatedBy(rot) * (80 + timer * 620f);
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
			if (AttackTimer > 240 && AttackTimer < 410)
			{
				if (hurtLastFrame)
				{
					AttackTimer = 410;
					NPC.netUpdate = true;
				}
			}

			// Timeout
			if (AttackTimer == 409)
			{
				SoundEngine.PlaySound(SoundID.NPCDeath10.WithPitchOffset(0.5f), NPC.Center);

				foreach (NPC npc in Main.npc.Where(n => n.active && n.ModNPC is HorrifyingVisage))
					npc.ai[1] = 1;
			}

			if (AttackTimer == 440 || hurtLastFrame)
			{
				foreach (NPC npc in Main.npc.Where(n => n.active && n.ModNPC is HorrifyingVisage))
				{
					if (npc.ai[0] < 440)
						npc.ai[0] = 440;
				}
			}

			if (AttackTimer > 440 && AttackTimer <= 470)
			{
				ThisThinker.ExtraGrayAuraRadius = 100 - 100 * (AttackTimer - 440) / 30f;
				ThisThinker.heartPetalProgress = 1f - (AttackTimer - 440) / 30f;
			}

			if (AttackTimer == 500)
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

		public void MindMines()
		{
			// Determines the points based on the sunflower algorithm for the mine distribution
			static Vector2 sunflower(int n, int index, Vector2 center)
			{
				float phi = (1 + MathF.Sqrt(5)) / 2;//golden ratio
				float angle_stride = 360 * phi;

				static float radius(float k, float n, float b)
				{
					return k > n - b ? 1 : MathF.Sqrt(k - 0.5f) / MathF.Sqrt(n - (b + 1) / 2);
				}

				int b = (int)(1 * MathF.Sqrt(n));

				float r = radius(index, n, b) * 700;
				float theta = index * angle_stride;
				float x = !float.IsNaN(r * MathF.Cos(theta)) ? r * MathF.Cos(theta) : 0;
				float y = !float.IsNaN(r * MathF.Sin(theta)) ? r * MathF.Sin(theta) : 0;

				return center + new Vector2(x, y);
			}

			if (AttackTimer > 30 && AttackTimer < 60)
				ThisThinker.ExtraGrayAuraRadius = 100 * (AttackTimer - 30) / 30f;

			if (AttackTimer == 60)
			{
				ThisThinker.platformRadiusTarget = 450;
				ThisThinker.platformRotationTarget -= 0.2f;

				safeMineIndicides = new int[4];

				for (int k = 0; k < safeMineIndicides.Length; k++)
				{
					safeMineIndicides[k] = Main.rand.Next(10, 30);
				}

				NPC.netUpdate = true;
			}

			if (AttackTimer > 90 && AttackTimer < 400)
			{
				Vector2 targetPos;

				if (AttackTimer <= 300)
					targetPos = ThisThinker.home + -Vector2.UnitY.RotatedBy(Eases.EaseQuadIn((AttackTimer - 90) / 210f) * 6.28f) * 300;
				else
					targetPos = ThisThinker.home + -Vector2.UnitY * 300;

				ThisThinker.NPC.Center += (targetPos - ThisThinker.NPC.Center) * 0.05f;
			}

			if (AttackTimer > 200 && AttackTimer <= 230)
			{
				opacity = 1f - (AttackTimer - 200) / 30f;
			}

			if (AttackTimer == 231)
				NPC.Center = ThisThinker.home + new Vector2(0, -200);

			if (AttackTimer > 90 && AttackTimer % 2 == 0 && AttackTimer <= 180 && Main.netMode != NetmodeID.MultiplayerClient)
			{
				float thisTimer = AttackTimer - 90;

				float index = thisTimer / 90f * 45;

				Vector2 pos = sunflower(45, (int)index, ThisThinker.home);

				if (safeMineIndicides.Contains((int)index))
					Projectile.NewProjectile(NPC.GetSource_FromThis(), pos, Vector2.Zero, ModContent.ProjectileType<FakeMindMine>(), Helpers.StarlightMathHelper.GetProjectileDamage(100, 150, 200), 1, Main.myPlayer, thinker.whoAmI);
				else
					Projectile.NewProjectile(NPC.GetSource_FromThis(), pos, Vector2.Zero, ModContent.ProjectileType<ThinkerMindMine>(), Helpers.StarlightMathHelper.GetProjectileDamage(100, 150, 200), 1, Main.myPlayer, thinker.whoAmI);
			}

			if (AttackTimer > 300 && AttackTimer <= 400)
			{
				ThisThinker.ExtraGrayAuraRadius = 100 * (1 - (AttackTimer - 300) / 100f);
			}

			if (AttackTimer > 400)
			{
				ThisThinker.NPC.Center += (ThisThinker.home - ThisThinker.NPC.Center) * 0.035f;
			}

			if (AttackTimer > 430 && AttackTimer <= 460)
				opacity = (AttackTimer - 430) / 30f;

			if (AttackTimer >= 500)
				AttackTimer = 0;
		}

		public void TeleportBlooms()
		{
			// thinker follows then teleports back
			if (AttackTimer <= 30)
			{
				ThisThinker.ExtraGrayAuraRadius = -80 * (AttackTimer / 30f);
			}

			if (AttackTimer <= 545)
			{
				Vector2 targetPos = Main.player[NPC.target].Center + new Vector2(0, -225).RotatedBy(AttackTimer / 545f * 6.28f * 2f);
				targetPos += Vector2.UnitX.RotatedBy(AttackTimer / 650 * 6.28f * 4) * 30;

				float speed = AttackTimer < 485 ? 0.045f : 0.045f * (1f - (AttackTimer - 485) / 60f);

				if (AttackTimer < 120)
					speed *= AttackTimer / 120f;

				thinker.Center += (targetPos - thinker.Center) * speed;
			}

			if (AttackTimer > 515 && AttackTimer <= 545)
			{
				float teleTimer = AttackTimer - 515;
				ThisThinker.ExtraGrayAuraRadius = -80 + -100 * (teleTimer / 30f);
			}

			if (AttackTimer == 545)
			{
				Helpers.SoundHelper.PlayPitched("Magic/FireCast", 1, -0f, NPC.Center);
				thinker.Center = ThisThinker.home;
			}

			if (AttackTimer > 545 && AttackTimer <= 565)
			{
				float teleTimer = AttackTimer - 545;
				ThisThinker.ExtraGrayAuraRadius = -180 + 180 * (teleTimer / 20f);
			}

			if (AttackTimer == 565)
			{
				ThisThinker.ExtraGrayAuraRadius = 0;

				for (int k = 0; k < 30; k++)
				{
					float rot = Main.rand.NextFloat(6.28f);
					Dust.NewDustPerfect(thinker.Center + Vector2.One.RotatedBy(rot) * 50f, ModContent.DustType<GraymatterDust>(), Vector2.One.RotatedBy(rot) * Main.rand.NextFloat(5), 0, Color.White, Main.rand.NextFloat(1.5f, 3f));
				}
			}

			// Main attack
			if (AttackTimer == 30)
			{
				ThisThinker.platformRadiusTarget = 400;
				ThisThinker.platformRotationTarget -= 0.2f;
				savedPos = ThisThinker.home + Vector2.One.RotatedByRandom(6.28f) * 370; // First random pos
			}

			if (AttackTimer <= 120)
			{
				thinker.Center += (ThisThinker.home - thinker.Center) * 0.03f;
			}

			for (int k = 0; k < 7; k++)
			{
				int relTime = (int)AttackTimer - 60 - k * 75;

				if (relTime < 0 || relTime > 75)
					continue;

				if (relTime <= 15)
					opacity = 1f - relTime / 15f;

				if (relTime == 15)
				{
					TeleportWithChain(savedPos);
					float currentRotation = (savedPos - ThisThinker.home).ToRotation();
					savedPos = ThisThinker.home + Vector2.One.RotatedBy(currentRotation + Main.rand.NextFloat(1f, 5f)) * 370;
				}

				if (relTime > 15 && relTime <= 45)
					opacity = (relTime - 15) / 30f;

				if (relTime > 45 && relTime <= 65)
				{
					extraChunkRadius = Eases.BezierEase((relTime - 45) / 20f) * -0.5f;
				}

				if (relTime == 65 && Main.netMode != NetmodeID.MultiplayerClient)
				{
					int projCount = Main.expertMode ? 9 : 6;
					for (int i = 0; i < projCount; i++)
					{
						float rot = i / (float)projCount * 6.28f;
						Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.UnitX.RotatedBy(rot) * (6 + k / 4f), ModContent.ProjectileType<BrainBolt>(), BrainBoltDamage, 1, Main.myPlayer, 190, 0, 20);

						if (Main.masterMode)
							Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.UnitX.RotatedBy(rot + 6.28f / 18f) * (4f + k / 4f), ModContent.ProjectileType<BrainBolt>(), BrainBoltDamage, 1, Main.myPlayer, 190, 1, 20);
					}
				}

				if (relTime > 65 && relTime <= 75)
					extraChunkRadius = -0.5f + Eases.SwoopEase((relTime - 65) / 10f) * 0.5f;
			}

			if (AttackTimer >= 60 + 75 * 7)
			{
				float relTime = AttackTimer - (60 + 75 * 7);
				ThisThinker.ExtraGrayAuraRadius = -70 + relTime / 30f * 70;
			}

			if (AttackTimer >= 60 + 75 * 7 + 30)
				AttackTimer = 0;
		}
		#endregion
	}
}