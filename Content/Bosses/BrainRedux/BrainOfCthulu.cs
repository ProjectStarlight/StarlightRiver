using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	internal partial class BrainOfCthulu : GlobalNPC
	{
		/// <summary>
		/// If this instance of the boss should be our rework or just the vanilla fight
		/// </summary>
		public bool reworked;

		public NPC npc;

		public NPC thinker;

		public List<NPC> neurisms = new();
		public Vector2 savedPos;

		public ref float Timer => ref npc.ai[0];
		public ref float State => ref npc.ai[1];
		public ref float AttackTimer => ref npc.ai[2];
		public ref float AttackState => ref npc.ai[3];

		public override bool InstancePerEntity => true;

		public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
		{
			return entity.type == NPCID.BrainofCthulhu;
		}

		public override void SetDefaults(NPC entity)
		{
			npc = entity;
		}

		public override bool PreAI(NPC npc)
		{
			if (!reworked)
				return true;

			Timer++;
			AttackTimer++;

			// If we dont have a thinker, try to find one
			if (thinker is null)
			{
				float dist = float.PositiveInfinity;

				foreach (NPC n in Main.npc)
				{
					if (n.active && n.type == ModContent.NPCType<TheThinker>())
					{
						float thisDist = Vector2.DistanceSquared(npc.Center, n.Center);
						if (thisDist < dist)
						{
							thinker = n;
							dist = thisDist;
						}
					}
				}

				// If the nearest thinker is too far away, flee.
				if (dist > Math.Pow(2000, 2))
				{
					State = -1;
					Timer = 0;
				}
			}

			switch(State)
			{
				// Fleeing
				case -1:
					npc.position.Y += 10;

					if (Timer > 60)
						npc.active = false;

					break;

				// Setup
				case 0:
					for(int k = 0; k < 10; k++)
					{
						int i = NPC.NewNPC(null, (int)npc.Center.X, (int)npc.Center.Y, ModContent.NPCType<Neurysm>());
						neurisms.Add(Main.npc[i]);
					}

					Timer = 0;
					State = 1;

					break;

				// Intro
				case 1:

					if (Timer == 1)
						savedPos = npc.Center;

					if (Timer < 120)
						npc.Center = Vector2.SmoothStep(savedPos, thinker.Center + new Vector2(0, -180), Timer / 120f);

					if (Timer == 240)
					{
						State = 2;
						Timer = 0;
						AttackTimer = 0;
					}

					break;

				// First phase
				case 2:

					if (Timer == 1)
						AttackState = Main.rand.Next(3);

					switch(AttackState)
					{
						case 0:
							ShrinkingCircle();
							break;
						case 1:
							ShrinkingCircle();
							break;
						case 2:
							ShrinkingCircle();
							break;
						case 3:
							ShrinkingCircle();
							break;
					}

					break;
			}

			return false;
		}

		/// <summary>
		/// Spawns the reworked brain at the desired location
		/// </summary>
		/// <param name="position">Where to spawn the reworked brain</param>
		public static void SpawnReduxedBrain(Vector2 position)
		{
			int i = NPC.NewNPC(null, (int)position.X, (int)position.Y, NPCID.BrainofCthulhu);

			if(Main.npc[i].TryGetGlobalNPC(out BrainOfCthulu brain))
				brain.reworked = true;
		}
	}
}
