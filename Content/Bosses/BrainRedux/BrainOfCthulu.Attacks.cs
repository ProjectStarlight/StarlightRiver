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
				npc.Center += (thinker.Center + new Vector2(0, -200) - npc.Center) * 0.08f;
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

				for (int k = 0; k < neurisms.Count; k++)
				{
					float rot = k / (float)neurisms.Count * 6.28f + timer * 2.5f;

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
				npc.Center += (thinker.Center + new Vector2(0, -200) - npc.Center) * 0.08f;
			}

			if (AttackTimer == 1)
			{
				savedRot = Main.rand.NextFloat(6.28f);
				npc.netUpdate = true;
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
			}

			if (AttackTimer > 120)
			{
				float prog = Helpers.Helper.SwoopEase((AttackTimer - 120) / 90f);
				npc.Center = Vector2.Lerp(savedPos2, savedPos, prog);

				var d = Dust.NewDustPerfect(npc.Center, ModContent.DustType<BloodMetaballDust>(), Vector2.UnitY.RotatedByRandom(1) * Main.rand.NextFloat(-5, -3));
				d.customData = 1f;

				Dust.NewDustPerfect(npc.Center, DustID.Blood, Vector2.UnitY.RotatedByRandom(6.28f) * Main.rand.NextFloat(1, 5));
			}

			if (AttackTimer > 210)
			{
				AttackTimer = 0;
			}
		}

		public void Spawn()
		{
			Vector2 relativePos = Main.player[npc.target].Center - thinker.Center;
			Vector2 targetPos = thinker.Center + relativePos.RotatedBy(3.14f);

			if (AttackTimer == 1)
			{
				npc.TargetClosest();
			}

			if (AttackTimer >= 1 && AttackTimer < 200)
			{
				npc.Center += (targetPos - npc.Center) * 0.05f;
			}

			if (AttackTimer % 20 == 0 && Main.npc.Count(n => n.active && n.type == Terraria.ID.NPCID.Creeper) < 10)
			{
				int i = NPC.NewNPC(npc.GetSource_FromThis(), (int)npc.Center.X, (int)npc.Center.Y, Terraria.ID.NPCID.Creeper);

				//TODO: Multiplayer compat
				Main.npc[i].lifeMax = 30;
				Main.npc[i].life = 30;
				Main.npc[i].SpawnedFromStatue = true;
				Main.npc[i].velocity += npc.Center.DirectionTo(Main.player[npc.target].Center) * 30;

				SoundEngine.PlaySound(SoundID.Zombie10, npc.Center);
			}

			if (AttackTimer > 200)
			{
				AttackTimer = 0;
			}
		}
		#endregion
	}
}
