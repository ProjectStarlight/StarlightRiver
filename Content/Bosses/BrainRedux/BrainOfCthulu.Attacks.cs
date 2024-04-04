using StarlightRiver.Core.Systems.BarrierSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	internal partial class BrainOfCthulu
	{
		#region Phase 1
		public void ShrinkingCircle()
		{
			if (AttackTimer == 1)
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
					float rot = k / (float)neurisms.Count * 6.28f + timer * 3.14f;

					neurisms[k].Center = thinker.Center + Vector2.UnitX.RotatedBy(rot) * (750 - timer * 675f);
					(neurisms[k].ModNPC as Neurysm).State = 0;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}
			}

			if (AttackTimer == 240)
			{
				AttackTimer = 0;
			}
		}

		public void LineThrow()
		{
			if (AttackTimer == 1)
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

			for (int k = 0; k < neurisms.Count; k++)
			{
				float lerp = k / (neurisms.Count - 1f);

				if (AttackTimer == 30 + k * 20)
				{
					float rot = 0;
					float direction = k % 2 == 0 ? 1 : -1;

					float a = (1 - lerp * 2) * 750f;
					float h = 750f;

					float offset = (float)Math.Sqrt(Math.Pow(h, 2) - Math.Pow(a, 2));

					neurisms[k].Center = thinker.Center + Vector2.UnitX.RotatedBy(rot) * (-750 + 1500 * lerp) + Vector2.UnitY.RotatedBy(rot) * direction * offset;
					(neurisms[k].ModNPC as Neurysm).State = 2;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}

				if (AttackTimer >= 60 + k * 20 && AttackTimer <= 180 + k * 20)
				{
					float rot = 0;
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

			if (AttackTimer >= 120 + neurisms.Count * 20)
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
				float prog = Helpers.Helper.SwoopEase((AttackTimer - 120) / 60f);
				npc.Center = Vector2.Lerp(savedPos2, savedPos, prog);
			}

			if (AttackTimer > 180)
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

			if (AttackTimer >= 1 && AttackTimer < 480)
			{
				npc.Center += (targetPos - npc.Center) * 0.05f;
			}

			if (AttackTimer % 60 == 0 && Main.npc.Count(n => n.active && n.type == Terraria.ID.NPCID.Creeper) < 10)
			{
				int i = NPC.NewNPC(npc.GetSource_FromThis(), (int)npc.Center.X, (int)npc.Center.Y, Terraria.ID.NPCID.Creeper);

				//TODO: Multiplayer compat
				Main.npc[i].lifeMax = 30;
				Main.npc[i].SpawnedFromStatue = true;
				Main.npc[i].velocity += npc.Center.DirectionTo(Main.player[npc.target].Center) * 30;
			}

			if (AttackTimer > 480)
			{
				AttackTimer = 0;
			}
		}
		#endregion
	}
}
