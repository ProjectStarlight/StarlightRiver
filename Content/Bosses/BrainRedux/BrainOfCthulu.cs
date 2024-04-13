using StarlightRiver.Content.Biomes;
using StarlightRiver.Core.Systems.BarrierSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
		public bool contactDamage = false;

		public bool hurtLastFrame;

		public float opacity;

		private float arenaFade;

		public List<int> attackQueue = new();

		public ref float Timer => ref npc.ai[0];
		public ref float State => ref npc.ai[1];
		public ref float AttackTimer => ref npc.ai[2];
		public ref float AttackState => ref npc.ai[3];

		public static float ArenaOpacity => TheBrain?.arenaFade / 120f ?? 0f;

		public static BrainOfCthulu TheBrain
		{
			get
			{
				if (NPC.crimsonBoss > 0 && Main.npc[NPC.crimsonBoss].TryGetGlobalNPC(out BrainOfCthulu brain))
					return brain;

				return null;
			}
		}

		public override bool InstancePerEntity => true;

		public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
		{
			return entity.type == NPCID.BrainofCthulhu;
		}

		public override void Load()
		{
			On_WorldGen.CheckOrb += SpecialSpawn;

			GraymatterBiome.onDrawOverHallucinationMap += DrawPrediction;
		}

		private void DrawPrediction(SpriteBatch obj)
		{
			if (NPC.crimsonBoss > 0)
			{
				if (TheBrain != null)
				{
					TheBrain.PreDraw(TheBrain.npc, obj, Main.screenPosition, Lighting.GetColor((TheBrain.npc.Center / 16).ToPoint()));

					if (TheBrain.State == 2)
					{
						Vector2 pos = TheBrain.npc.Center - Main.screenPosition;

						for (int k = 0; k < TheBrain.attackQueue.Count; k++)
						{
							int attack = TheBrain.attackQueue[k];
							Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.BrainRedux + $"Indicator{attack}").Value;
							obj.Draw(tex, pos + new Vector2(-100 + k * 66, -50), null, Color.White, 0, tex.Size() / 2f, 1, 0, 0);
						}

						Texture2D tex2 = ModContent.Request<Texture2D>(AssetDirectory.BrainRedux + $"Indicator{TheBrain.AttackState}").Value;
						obj.Draw(tex2, pos + new Vector2(0, -100), null, Color.White, 0, tex2.Size() / 2f, 1.5f, 0, 0);
					}
				}
			}
		}

		private void SpecialSpawn(On_WorldGen.orig_CheckOrb orig, int i, int j, int type)
		{
			Tile tile = Framing.GetTileSafely(i, j);

			orig(i, j, type);

			if (WorldGen.crimson && WorldGen.shadowOrbCount >= 2 && WorldGen.destroyObject)
			{
				Vector2 pos = new Vector2(i, j) * 16;

				if (Main.npc.Any(n => n.active && n.type == ModContent.NPCType<TheThinker>() && Vector2.Distance(n.Center, pos) < 1000))
				{
					for (int k = 0; k < Main.maxNPCs; k++)
					{
						NPC npc = Main.npc[k];

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

			switch (State)
			{
				// Fleeing
				case -1:
					npc.position.Y += 10;

					if (Timer > 60)
						npc.active = false;

					break;

				// Setup
				case 0:
					opacity = 1;

					for (int k = 0; k < 10; k++)
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

					int life = Main.masterMode ? 4500 : Main.expertMode ? 3000 : 2000;

					npc.lifeMax = life;
					npc.life = life;

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
						npc.Center = Vector2.SmoothStep(savedPos, thinker.Center + new Vector2(0, -250), Timer / 120f);

					foreach (Player player in Main.player.Where(n => n.active && Vector2.Distance(n.Center, thinker.Center) < 1500))
					{
						player.position += (thinker.Center - player.Center) * 0.1f * (Vector2.Distance(player.Center, thinker.Center) / 1500f);
					}

					if (Timer == 120)
					{
						(thinker.ModNPC as TheThinker)?.CreateArena();
					}

					if (Timer >= 120)
					{
						if (arenaFade < 120)
							arenaFade++;
					}

					if (Timer == 240)
					{
						State = 2;
						Timer = 0;
						AttackTimer = 0;
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

							// Reset attack queue
							attackQueue.Clear();
							attackQueue.Add(Main.rand.Next(2));

							for (int k = 0; k < 3; k++)
							{
								int next2 = Main.rand.Next(2);

								while (next2 == attackQueue.Last())
									next2 = Main.rand.Next(2);

								attackQueue.Add(next2);
							}

							foreach (NPC creeper in Main.npc.Where(n => n.active && n.type == NPCID.Creeper))
							{
								creeper.active = false;
							}
						}

						npc.netUpdate = true;
					}

					switch (AttackState)
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

					if (AttackTimer == 1)
					{
						AttackState = attackQueue[0];
						attackQueue.RemoveAt(0);

						int next = Main.rand.Next(2);

						while (next == attackQueue.Last())
							next = Main.rand.Next(2);

						attackQueue.Add(next);

						npc.netUpdate = true;
					}

					switch (AttackState)
					{
						case 0:
							DoubleSpin();
							break;
						case 1:
							Clones();
							break;
						case 2:
							Ram();
							break;
						case 3:
							Spawn();
							break;
					}

					break;
			}

			hurtLastFrame = false;

			return false;
		}

		public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
		{
			return contactDamage && opacity > 0.9f;
		}

		public override void HitEffect(NPC npc, NPC.HitInfo hit)
		{
			hurtLastFrame = true;
		}

		public override void OnKill(NPC npc)
		{
			if (reworked)
			{
				for (int k = 0; k < 10; k++)
				{
					Main.BestiaryTracker.Kills.RegisterKill(thinker);
				}
			}
		}

		public override void FindFrame(NPC npc, int frameHeight)
		{
			if (reworked && State >= 3)
				npc.frame = new Rectangle(0, 182 * 4 + 182 * (int)(Timer / 10f % 4), 200, 182);
		}

		public override bool? DrawHealthBar(NPC npc, byte hbPosition, ref float scale, ref Vector2 position)
		{
			if (State == 3 && AttackState == 1)
				return false;

			return base.DrawHealthBar(npc, hbPosition, ref scale, ref position);
		}

		public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (!reworked)
				return true;

			Texture2D tex = Terraria.GameContent.TextureAssets.Npc[NPCID.BrainofCthulhu].Value;

			if (opacity >= 1)
			{
				Main.spriteBatch.Draw(tex, npc.Center - Main.screenPosition, npc.frame, drawColor * opacity, 0, npc.frame.Size() / 2f, npc.scale, 0, 0);
			}
			else
			{
				for (int k = 0; k < 6; k++)
				{
					float rot = k / 6f * 6.28f + opacity * 3.14f;
					Vector2 offset = Vector2.UnitX.RotatedBy(rot) * (1 - opacity) * 64;
					spriteBatch.Draw(tex, npc.Center + offset - Main.screenPosition, npc.frame, drawColor * opacity * 0.2f, npc.rotation, npc.frame.Size() / 2f, npc.scale, 0, 0);
				}
			}

			return false;
		}

		public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (reworked)
			{
				if (State == 2)
				{
					Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Misc/GlowRing").Value;
					Color color = Color.Gray * Math.Min(1f, Timer / 60f);
					color.A = 0;

					spriteBatch.Draw(tex, npc.Center - Main.screenPosition, null, color, 0, tex.Size() / 2f, 0.6f, 0, 0);

					spriteBatch.Draw(tex, npc.Center - Main.screenPosition, null, color, 0, tex.Size() / 2f, 0.6f + 0.05f * (float)Math.Sin(Main.GameUpdateCount * 0.1f), 0, 0);
				}
			}
		}

		public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
		{
			binaryWriter.WriteVector2(savedPos);
			binaryWriter.WriteVector2(savedPos2);
			binaryWriter.Write(savedRot);

			binaryWriter.Write(attackQueue.Count);
			for (int k = 0; k < attackQueue.Count; k++)
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
			for (int k = 0; k < amount; k++)
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

			if (Main.npc[i].TryGetGlobalNPC(out BrainOfCthulu brain))
				brain.reworked = true;
		}
	}
}
