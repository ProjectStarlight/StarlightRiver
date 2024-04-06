using StarlightRiver.Core.Systems.BarrierSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader.IO;

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
		public Vector2 savedPos2;
		public float savedRot;

		public List<int> attackQueue = new List<int>();

		public ref float Timer => ref npc.ai[0];
		public ref float State => ref npc.ai[1];
		public ref float AttackTimer => ref npc.ai[2];
		public ref float AttackState => ref npc.ai[3];

		public override bool InstancePerEntity => true;

		public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
		{
			return entity.type == NPCID.BrainofCthulhu;
		}

		public override void Load()
		{
			On_WorldGen.CheckOrb += SpecialSpawn;
		}

		private void SpecialSpawn(On_WorldGen.orig_CheckOrb orig, int i, int j, int type)
		{
			var tile = Framing.GetTileSafely(i, j);

			orig(i, j, type);

			if (WorldGen.crimson && WorldGen.shadowOrbCount >= 2 && WorldGen.destroyObject)
			{
				Vector2 pos = new Vector2(i, j) * 16;

				if (Main.npc.Any(n => n.active && n.type == ModContent.NPCType<TheThinker>() && Vector2.Distance(n.Center, pos) < 1000))
				{
					for (int k = 0; k < Main.maxNPCs; k++)
					{
						var npc = Main.npc[k];

						if (npc.active && npc.type == NPCID.BrainofCthulhu)
							npc.active = false;

						if (npc.active && npc.type == NPCID.Creeper)
							npc.active = false;
					}

					SpawnReduxedBrain(pos + new Vector2(0, 200));
				}
			}		
		}

		public override void SetDefaults(NPC entity)
		{
			npc = entity;
		}

		public override bool PreAI(NPC npc)
		{
			if (!reworked)
				return true;

			GUI.BossBarOverlay.SetTracked(npc);

			Timer++;
			AttackTimer++;

			NPC.crimsonBoss = npc.whoAmI;

			Lighting.AddLight(npc.Center, new Vector3(0.5f, 0.4f, 0.2f));

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

					attackQueue.Add(Main.rand.Next(4));

					for (int k = 0; k < 3; k++)
					{
						int next = Main.rand.Next(4);

						while (next == attackQueue.Last())
							next = Main.rand.Next(4);

						attackQueue.Add(next);
					}

					npc.lifeMax = 2000;
					npc.life = 2000;

					npc.GetGlobalNPC<BarrierNPC>().maxBarrier = 800;
					npc.GetGlobalNPC<BarrierNPC>().barrier = 800;

					Timer = 0;
					State = 1;

					break;

				// Intro
				case 1:

					if (Timer == 1)
						savedPos = npc.Center;

					if (Timer < 120)
						npc.Center = Vector2.SmoothStep(savedPos, thinker.Center + new Vector2(0, -180), Timer / 120f);

					foreach(Player player in Main.player.Where(n => n.active && Vector2.Distance(n.Center, thinker.Center) < 1500))
					{
						player.position += (thinker.Center - player.Center) * 0.1f * (Vector2.Distance(player.Center, thinker.Center) / 1500f);
					}

					if (Timer == 240)
					{
						State = 2;
						Timer = 0;
						AttackTimer = 0;
						(thinker.ModNPC as TheThinker)?.CreateArena();
					}

					break;

				// First phase
				case 2:

					if (npc.life <= npc.lifeMax / 2f)
						npc.life = (int)(npc.lifeMax / 2f);

					if (AttackTimer == 1)
					{
						AttackState = attackQueue[0];
						attackQueue.RemoveAt(0);

						int next = Main.rand.Next(4);

						while (next == attackQueue.Last())
							next = Main.rand.Next(4);

						attackQueue.Add(next);

						// Transition check
						if (npc.life <= npc.lifeMax / 2f)
						{
							State = 3;
							Timer = 0;
							AttackState = 0;
						}

						npc.netUpdate = true;
					}

					switch(AttackState)
					{
						case 0:
							ShrinkingCircle();
							break;
						case 1:
							LineThrow();
							break;
						case 2:
							Ram();
							break;
						case 3:
							Spawn();
							break;
					}

					break;

				// Second phase
				case 3:
					npc.dontTakeDamage = false;
					npc.immortal = false;
					break;
			}

			return false;
		}

		public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
		{
			binaryWriter.WriteVector2(savedPos);
			binaryWriter.WriteVector2(savedPos2);
			binaryWriter.Write(savedRot);

			binaryWriter.Write(attackQueue.Count);
			for(int k = 0; k < attackQueue.Count; k++)
			{
				binaryWriter.Write(attackQueue[k]);
			}
		}

		public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
		{
			savedPos = binaryReader.ReadVector2();
			savedPos2 = binaryReader.ReadVector2();
			savedRot = binaryReader.ReadSingle();

			int amount = binaryReader.ReadInt32();
			attackQueue.Clear();
			for(int k = 0; k < amount; k++)
			{
				attackQueue.Add(binaryReader.ReadInt32());
			}
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
