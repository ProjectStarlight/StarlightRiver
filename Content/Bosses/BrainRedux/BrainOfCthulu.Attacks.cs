using StarlightRiver.Content.Dusts;
using StarlightRiver.Core.Systems.BarrierSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
				npc.Center += (thinker.Center + new Vector2(0, -250) - npc.Center) * 0.08f;
			}

			if (AttackTimer == 31)
			{
				for(int k = 0; k < neurisms.Count; k++)
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
				float totalRot = Main.masterMode ? 3.5f : Main.expertMode ? 3f : 2.5f;

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
				npc.Center += (thinker.Center + new Vector2(0, -250) - npc.Center) * 0.08f;
			}

			if (AttackTimer == 1)
			{
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

					offset *= (1 - prog * 2);

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
				float prog = Helpers.Helper.SwoopEase((AttackTimer - 120) / chargeTime);
				npc.Center = Vector2.Lerp(savedPos2, savedPos, prog);

				var d = Dust.NewDustPerfect(npc.Center, ModContent.DustType<BloodMetaballDust>(), Vector2.UnitY.RotatedByRandom(1) * Main.rand.NextFloat(-5, -3));
				d.customData = 1f;

				Dust.NewDustPerfect(npc.Center, DustID.Blood, Vector2.UnitY.RotatedByRandom(6.28f) * Main.rand.NextFloat(1, 5));
			}

			if (AttackTimer > 120 + chargeTime)
			{
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

				SoundEngine.PlaySound(SoundID.NPCDeath13, npc.Center);
			}

			if (AttackTimer > 600)
			{
				AttackTimer = 0;
			}
		}
		#endregion

		#region Phase 2
		public void DoubleSpin()
		{
			Vector2 targetPos = (thinker.ModNPC as TheThinker).home + Vector2.UnitX.RotatedBy(AttackTimer / 400f * 6.28f) * 600;
			Vector2 targetPos2 = (thinker.ModNPC as TheThinker).home + Vector2.UnitX.RotatedBy(AttackTimer / 400f * 6.28f) * -600;

			if (AttackTimer == 1)
			{
				for (int k = 0; k < neurisms.Count; k++)
				{
					(neurisms[k].ModNPC as Neurysm).State = 2;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}
			}

			if (AttackTimer >= 1 && AttackTimer < 400)
			{
				npc.Center += (targetPos - npc.Center) * 0.05f;
				thinker.Center += (targetPos2 - thinker.Center) * 0.05f;

				float rad = 300 + (float)Math.Cos(AttackTimer / 200f * 3.14f + 3.14f) * 200;

				for (int k = 0; k < neurisms.Count; k++)
				{
					float rot = k * 2 / (float)neurisms.Count * 6.28f + AttackTimer * -0.02f;

					if (k % 2 == 0)
						neurisms[k].Center = thinker.Center + Vector2.UnitX.RotatedBy(rot) * rad;
					else
						neurisms[k].Center = npc.Center + Vector2.UnitX.RotatedBy(rot) * rad;
				}
			}

			if (AttackTimer == 60)
			{
				for (int k = 0; k < neurisms.Count; k++)
				{
					(neurisms[k].ModNPC as Neurysm).State = 0;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}
			}

			if (AttackTimer == 340)
			{
				for (int k = 0; k < neurisms.Count; k++)
				{
					(neurisms[k].ModNPC as Neurysm).State = 1;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}
			}

			if (AttackTimer >= 400)
			{
				AttackTimer = 0;
			}
		}

		public void Clones()
		{
			if (AttackTimer == 1)
			{
				int random = Main.rand.Next(10);
				savedPos = (thinker.ModNPC as TheThinker).home + Vector2.UnitX.RotatedBy(random / 10f * 6.28f) * 750;

				for(int k = 0; k < 10; k++)
				{
					if (k != random)
						Projectile.NewProjectile(null, (thinker.ModNPC as TheThinker).home + Vector2.UnitX.RotatedBy(k / 10f * 6.28f) * 750, Vector2.Zero, ModContent.ProjectileType<HorrifyingVisage>(), 25, 0, Main.myPlayer);
				}
			}

			if (AttackTimer <= 90)
			{
				npc.Center += (savedPos - npc.Center) * 0.05f;
				thinker.Center += ((thinker.ModNPC as TheThinker).home - thinker.Center) * 0.05f;
			}

			if (AttackTimer == 240)
			{
				AttackTimer = 0;
			}

		}
		#endregion
	}
}
