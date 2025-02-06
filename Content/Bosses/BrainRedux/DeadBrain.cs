using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Physics;
using StarlightRiver.Core.DrawingRigs;
using StarlightRiver.Core.Systems.PixelationSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	[AutoloadBossHead]
	internal partial class DeadBrain : ModNPC
	{
		/// <summary>
		/// The thinker NPC this brain is tied to
		/// </summary>
		public NPC thinker;

		/// <summary>
		/// List of the atttacking minions
		/// </summary>
		public List<NPC> neurisms = [];

		/// <summary>
		/// The weakpoint NPC to be damaged during the first phase
		/// </summary>
		public NPC weakpoint;

		public Vector2 savedPos;
		public Vector2 savedPos2;
		public Vector2 lastPos;
		public float savedRot;
		public int[] safeMineIndicides = new int[4];
		public bool contactDamage = false;
		public float contactDamageOpacity;
		public float chargeAnimation;

		public bool hurtLastFrame;

		public float opacity;

		public float arenaFade;

		public List<int> attackQueue = [];

		public VerletChain attachedChain;
		public Vector2 attachedChainEndpoint;
		private List<Vector2> attachedChainCache;
		private Trail attachedChainTrail;

		// The two seperate chains to be drawn after teh main one is snapped
		// there isnt really a good way to "split" a chain in two so we just disable
		// that one and enable these two.
		public VerletChain chainSplitBrainAttached;
		private List<Vector2> chainSplitBrainAttachedCache;
		private Trail chainSplitBrainAttachedTrail;

		public VerletChain chainSplitThinkerAttached;
		private List<Vector2> chainSplitThinkerAttachedCache;
		private Trail chainSplitThinkerAttachedTrail;

		private bool chainsSplit = false;

		// chunk animation inputs
		private static StaticRig rig;
		public float extraChunkRadius;
		public float staggeredExtraChunkRadius;

		// shield shader inputs
		public float shieldOpacity = 0f;

		public int BrainBoltDamage => Helper.GetProjectileDamage(80, 60, 40);
		public int VeinSpearDamage => Helper.GetProjectileDamage(200, 120, 80);

		// Phase enum and state variables
		public enum Phases : int
		{
			Fleeing = -1,
			Setup = 0,
			SpawnAnim = 1,
			FirstPhase = 2,
			FirstToSecond = 3,
			SecondPhase = 4,
			TempDead = 5,
			ReallyDead = 6
		}

		public ref float Timer => ref NPC.ai[0];

		public Phases Phase
		{
			get => (Phases)NPC.ai[1];
			set => NPC.ai[1] = (float)value;
		}

		public ref float AttackTimer => ref NPC.ai[2];
		public ref float AttackState => ref NPC.ai[3];

		/// <summary>
		/// Helper property to obtain the specific ModNPC instance of the linked thinker
		/// </summary>
		private TheThinker ThisThinker => thinker?.ModNPC as TheThinker;

		public override string Texture => AssetDirectory.BrainRedux + "DeadBrain";

		public override void Load()
		{
			GraymatterBiome.onDrawHallucinationMap += DrawGraymatterLink;
			GraymatterBiome.onDrawOverHallucinationMap += DrawOverGraymatter;

			Stream stream = StarlightRiver.Instance.GetFileStream("Assets/Bosses/BrainRedux/DeadBrainRig.json");
			rig = JsonSerializer.Deserialize<StaticRig>(stream);
			stream.Close();
		}

		/// <summary>
		/// Returns if the given player is able to be targeted by the boss, being inside of its arena
		/// </summary>
		/// <param name="player">The player to check for</param>
		/// <returns>If the player is a valid target by virtue of being inside of the arena</returns>
		private bool IsInArena(Player player)
		{
			return Vector2.Distance(player.Center, thinker.Center) < ThisThinker.ArenaRadius + 20;
		}

		private void DrawOverGraymatter(SpriteBatch obj)
		{
			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (npc.ModNPC is DeadBrain brain)
				{
					if (brain != null)
					{
						//TODO: The weak point entity really sohuld make its own hook and draw this there...
						brain.weakpoint?.ModNPC?.PreDraw(obj, Main.screenPosition, Color.White);

						// If doing a clones attack, highlight the real one
						if (brain.Phase == Phases.SecondPhase && (brain.AttackState == 1 || brain.AttackState == 3))
							brain.DrawBrain(obj, Lighting.GetColor((brain.NPC.Center / 16).ToPoint()), true);
					}
				}
			}
		}

		/// <summary>
		/// Initialize the chains for this instance of the boss, to be called on SetDefaults
		/// </summary>
		public void InitChains()
		{
			attachedChain = new VerletChain(100, true, NPC.Center, 4);
			chainSplitBrainAttached = new VerletChain(33, true, NPC.Center, 4);

			// This wants to start at the thinker center but we handle that in update, since if we set it here
			// we get issues on mod load when the prototype calls SetDefaults
			chainSplitThinkerAttached = new VerletChain(66, true, NPC.Center, 4);
		}

		/// <summary>
		/// This handles all the logic needed to properly update the visual verlet chains representing the teather
		/// between the brain and the thinker. It handles the "linking" chain and the two child chains that become
		/// visible one that one "splits"
		/// </summary>
		private void UpdateChains()
		{
			// Update the attached chain, this always happens since its always visible in either the form
			// of the flesh teather or the graymatter link
			if (attachedChain != null)
			{
				attachedChain.startPoint = attachedChainEndpoint;
				attachedChain.endPoint = thinker.Center;
				attachedChain.useEndPoint = true;
				attachedChain.drag = 1.1f;
				attachedChain.forceGravity = Vector2.UnitY * 1f;
				attachedChain.constraintRepetitions = 30;
				attachedChain.UpdateChain();
			}

			if (!chainsSplit) // During the first phase, set these to match the attached chain so it wont look strange when they swap out
			{
				if (attachedChain != null && chainSplitBrainAttached != null)
				{
					chainSplitBrainAttached.UpdateChain();
					for (int k = 0; k < chainSplitBrainAttached.ropeSegments.Count; k++)
					{
						// This one copies from the back since its start is the end of the full chain
						chainSplitBrainAttached.ropeSegments[k].posOld = attachedChain.ropeSegments[^(k + 1)].posNow;
						chainSplitBrainAttached.ropeSegments[k].posNow = attachedChain.ropeSegments[^(k + 1)].posNow;
					}

					chainSplitBrainAttached.startPoint = NPC.Center;
				}

				if (attachedChain != null && chainSplitThinkerAttached != null)
				{
					chainSplitThinkerAttached.UpdateChain();
					for (int k = 0; k < chainSplitThinkerAttached.ropeSegments.Count; k++)
					{
						chainSplitThinkerAttached.ropeSegments[k].posOld = attachedChain.ropeSegments[k].posNow;
						chainSplitThinkerAttached.ropeSegments[k].posNow = attachedChain.ropeSegments[k].posNow;
					}

					chainSplitThinkerAttached.startPoint = thinker?.Center ?? Vector2.Zero;
				}
			}
			else // For everything after the first phase, update the seperated chains on their own
			{
				if (chainSplitBrainAttached != null)
				{
					chainSplitBrainAttached.startPoint = NPC.Center + Vector2.UnitY * 90;
					chainSplitBrainAttached.useEndPoint = false;
					chainSplitBrainAttached.drag = 1.1f;
					chainSplitBrainAttached.forceGravity = Vector2.UnitY * 1f;
					chainSplitBrainAttached.constraintRepetitions = 30;
					chainSplitBrainAttached.UpdateChain();

					chainSplitBrainAttached.IterateRope(a => chainSplitBrainAttached.ropeSegments[a].posNow.X += (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f);

					Lighting.AddLight(chainSplitBrainAttached.ropeSegments.Last().posNow, Color.Red.ToVector3() * 0.45f);
					for (int k = 0; k < 5; k++)
					{
						Dust.NewDust(chainSplitBrainAttached.ropeSegments.Last().posNow, 8, 8, DustID.Blood);
					}
				}

				if (chainSplitThinkerAttached != null)
				{
					chainSplitThinkerAttached.startPoint = ThisThinker.home;
					chainSplitThinkerAttached.useEndPoint = false;
					chainSplitThinkerAttached.drag = 1.1f;
					chainSplitThinkerAttached.forceGravity = Vector2.UnitY * 1f;
					chainSplitThinkerAttached.constraintRepetitions = 30;

					chainSplitThinkerAttached.IterateRope(a => chainSplitThinkerAttached.ropeSegments[a].posNow.X += (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.3f);

					chainSplitThinkerAttached.UpdateChain();

					Lighting.AddLight(chainSplitThinkerAttached.ropeSegments.Last().posNow, Color.Red.ToVector3() * 0.45f);
					for (int k = 0; k < 5; k++)
					{
						Dust.NewDust(chainSplitThinkerAttached.ropeSegments.Last().posNow, 8, 8, DustID.Blood);
					}
				}
			}
		}

		public override void SetStaticDefaults()
		{
			NPCID.Sets.TrailCacheLength[Type] = 30;
			NPCID.Sets.TrailingMode[Type] = 1;
		}

		public override void SetDefaults()
		{
			NPC.CloneDefaults(NPCID.BrainofCthulhu);
			NPC.boss = false;
			NPC.aiStyle = -1;

			InitChains();
		}

		public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
		{
			int life = Main.masterMode ? 1600 : Main.expertMode ? 1250 : 1000;

			NPC.lifeMax = life;
			NPC.life = life;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCrimson,
				new FlavorTextBestiaryInfoElement("The corpse of the Brain of Cthulhu, re-animated by it's strongest thoughts and wills.")
			});
		}

		public override void AI()
		{
			lastPos = NPC.Center;

			NPC.knockBackResist = 0f;

			Timer++;
			AttackTimer++;

			if (contactDamage && contactDamageOpacity < 1)
				contactDamageOpacity += 0.05f;

			if (!contactDamage && contactDamageOpacity > 0)
				contactDamageOpacity -= 0.05f;

			// Emit light if not in the dead state
			if (Phase != Phases.TempDead)
				Lighting.AddLight(NPC.Center, Phase > Phases.FirstPhase ? new Vector3(0.5f, 0.4f, 0.2f) : new Vector3(0.5f, 0.5f, 0.5f) * (shieldOpacity / 0.4f));

			if (Phase == Phases.SecondPhase)
				NPC.boss = true;

			// If we dont have a thinker, try to find one
			if (thinker is null)
			{
				float dist = float.PositiveInfinity;

				foreach (NPC n in Main.ActiveNPCs)
				{
					if (n.active && n.type == ModContent.NPCType<TheThinker>())
					{
						float thisDist = Vector2.DistanceSquared(NPC.Center, n.Center);
						if (thisDist < dist)
						{
							thinker = n;
							dist = thisDist;
						}
					}
				}

				// If the nearest thinker is too far away, flee.
				if (thinker is null || dist > Math.Pow(2000, 2))
				{
					Phase = Phases.Fleeing;
					Timer = 0;
				}
			}

			// If we found a thinker, link it
			if (thinker != null)
				ThisThinker.brain = NPC;

			// Reset the endpoint for the attached chain to the thinker if its the default
			if (attachedChainEndpoint == default)
				attachedChainEndpoint = thinker.Center;

			UpdateChains();

			if (!Main.dedServ)
			{
				ManageCaches();
				ManageTrail();
			}

			switch (Phase)
			{
				case Phases.Fleeing:
					NPC.position.Y += 10;

					if (Timer > 60)
						NPC.active = false;

					break;

				case Phases.Setup:
					opacity = 1;

					for (int k = 0; k < 10; k++)
					{
						// Spawn minion
						int i = NPC.NewNPC(null, (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<Neurysm>(), 0, 1, 60);
						neurisms.Add(Main.npc[i]);

						// Link minion
						var neurism = Main.npc[i].ModNPC as Neurysm;

						// Safety check, if its not a minion go back and try again
						if (neurism is null)
						{
							Main.npc[i].active = false;
							k--;
							continue;
						}
						else
						{
							// Link
							neurism.brain = NPC;
						}
					}

					// Spawn the weakpoint NPC
					int weakpointIndex = NPC.NewNPC(null, (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<WeakPoint>(), 0);
					weakpoint = Main.npc[weakpointIndex];

					if (weakpoint.ModNPC is WeakPoint wp)
					{
						// Link the weakpoint to the appropriate thinker
						wp.thinker = thinker;
					}

					// populate the attack queue
					attackQueue.Add(Main.rand.Next(5));

					for (int k = 0; k < 3; k++)
					{
						int next = Main.rand.Next(5);

						while (next == attackQueue.Last())
							next = Main.rand.Next(5);

						attackQueue.Add(next);
					}

					// qualify players for the medal
					foreach (Player Player in Main.player.Where(n => n.active && Vector2.Distance(n.Center, ThisThinker.home) <= ThisThinker.ArenaRadius))
					{
						Player.GetModPlayer<MedalPlayer>().QualifyForMedal("TheThinker", 2);
					}

					Timer = 0;
					Phase = Phases.SpawnAnim;

					break;

				case Phases.SpawnAnim:

					Intro();
					weakpoint.Center = attachedChain.ropeSegments[attachedChain.ropeSegments.Count / 3].posNow;

					break;

				case Phases.FirstPhase:

					attachedChainEndpoint = NPC.Center + Vector2.UnitY * 90;
					weakpoint.Center = attachedChain.ropeSegments[attachedChain.ropeSegments.Count / 3].posNow;

					if (thinker.life <= thinker.lifeMax / 2f)
						thinker.life = (int)(thinker.lifeMax / 2f);

					if (AttackTimer == 1)
					{
						AttackState = attackQueue[0];
						attackQueue.RemoveAt(0);

						int next = Main.rand.Next(6);

						while (next == attackQueue.Last())
							next = Main.rand.Next(6);

						attackQueue.Add(next);

						// Transition check
						if (thinker.life <= thinker.lifeMax / 2f)
						{
							Phase = Phases.FirstToSecond;
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

						NPC.netUpdate = true;
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
						case 4:
							Choice();
							break;
						case 5:
							ChoiceAlt();
							break;
					}

					break;

				case Phases.FirstToSecond:
					FirstPhaseTransition();
					break;

				case Phases.SecondPhase:

					NPC.dontTakeDamage = false;
					NPC.immortal = false;

					if (AttackTimer == 1)
					{
						AttackState = attackQueue[0];
						attackQueue.RemoveAt(0);

						int next = Main.rand.Next(5);

						while (next == attackQueue.Last())
							next = Main.rand.Next(5);

						attackQueue.Add(next);

						NPC.netUpdate = true;
					}

					switch (AttackState)
					{
						case -1:
							Recover();
							break;
						case 0:
							DoubleSpin();
							break;
						case 1:
							Clones();
							break;
						case 2:
							TeleportHunt();
							break;
						case 3:
							Clones2();
							break;
						case 4:
							MindMines();
							break;
					}

					break;

				case Phases.TempDead:

					NPC.noGravity = false;
					NPC.noTileCollide = false;

					if (NPC.velocity.Y > 0)
						NPC.rotation += 0.01f;
					else
						NPC.velocity.X *= 0;

					Dust.NewDust(NPC.position, NPC.width, NPC.height, ModContent.DustType<Dusts.BloodMetaballDust>());

					if (opacity < 1)
						opacity += 0.01f;

					break;

				case Phases.ReallyDead:
					Death();
					break;
			}

			hurtLastFrame = false;
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			return contactDamage && opacity > 0.9f;
		}

		public override bool? CanBeHitByItem(Player player, Item item)
		{
			if (opacity < 0.5f)
				return false;

			return base.CanBeHitByItem(player, item);
		}

		public override bool? CanBeHitByProjectile(Projectile projectile)
		{
			if (opacity < 0.5f)
				return false;

			return base.CanBeHitByProjectile(projectile);
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			hurtLastFrame = true;
		}

		public override bool CheckActive()
		{
			return false;
		}

		public override bool CheckDead()
		{
			NPC.life = 1;
			Phase = Phases.TempDead;

			(thinker.ModNPC as TheThinker).Timer = 0;
			(thinker.ModNPC as TheThinker).AttackTimer = 0;

			for (int k = 0; k < neurisms.Count; k++)
			{
				(neurisms[k].ModNPC as Neurysm).State = 1;
				(neurisms[k].ModNPC as Neurysm).Timer = 0;
			}

			NPC.dontTakeDamage = true;

			return false;
		}

		public override void FindFrame(int frameHeight)
		{
			if (Phase >= Phases.SecondPhase)
				NPC.frame = new Rectangle(0, 182 * 4 + 182 * (int)(Timer / 10f % 4), 200, 182);
			else
				NPC.frame = new Rectangle(0, 182 * (int)(Timer / 6f % 4), 200, 182);
		}

		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
		{
			// Prevent the health bar from being drawn in the clone phases
			if (Phase == Phases.SecondPhase && (AttackState == 1 || AttackState == 3))
				return false;

			return base.DrawHealthBar(hbPosition, ref scale, ref position);
		}

		public static void DrawBrainSegments(SpriteBatch spriteBatch, NPC npc, Vector2 center, Color color, float rotation, float scale, float opacity, Vector2 oldCenter = default)
		{
			Texture2D tex = Assets.Bosses.BrainRedux.BrainChunk.Value;
			Texture2D texGlow = Assets.Bosses.BrainRedux.BrainChunkGlow.Value;

			foreach (StaticRigPoint point in rig.Points)
			{
				float magnitude = 0.1f;
				float speed = 100f + (8 - point.Frame) * 15;
				float offset = 1f + MathF.Sin((Main.GameUpdateCount + point.Frame * 10) * (1f / speed) * 6.28f) * magnitude;

				float chunkOpacity = 1f;
				if (npc.ModNPC is DeadBrain deadBrain)
				{
					float extra = deadBrain.extraChunkRadius + deadBrain.staggeredExtraChunkRadius * point.Frame;
					offset += extra;
					chunkOpacity *= 1f - extra / 2f;
				}

				float rotOffset = MathF.Sin((Main.GameUpdateCount + point.Frame * 16) * (0.4f / speed) * 6.28f) * 0.1f;

				Vector2 velOffset;
				if (oldCenter != default)
					velOffset = (center - (oldCenter - Main.screenPosition)) * -0.35f * point.Frame;
				else
					velOffset = Vector2.Zero;

				if (velOffset.Length() > 32)
					velOffset = Vector2.Normalize(velOffset) * 32;

				var frame = new Rectangle(0, 76 * point.Frame, 80, 76);
				spriteBatch.Draw(tex, center + (point.Pos - new Vector2(44, 40)) * offset + velOffset, frame, color * opacity * chunkOpacity, rotation + rotOffset, new Vector2(40, 38), scale, 0, 0);

				var glowColor = new Color(
					0.5f + 0.2f * MathF.Sin((Main.GameUpdateCount + point.Frame * 10) * (1f / speed) * 6.28f),
					0.5f + 0.2f * MathF.Sin((Main.GameUpdateCount + point.Frame * 10 + 30) * (1f / speed) * 6.28f),
					0.5f + 0.2f * MathF.Sin((Main.GameUpdateCount + point.Frame * 10 + 60) * (1f / speed) * 6.28f),
					0);

				if (npc.ModNPC is DeadBrain deadBrain2)
					glowColor *= deadBrain2.shieldOpacity / 0.4f;
				else
					glowColor *= 0;

				spriteBatch.Draw(texGlow, center + (point.Pos - new Vector2(44, 40)) * offset + velOffset, frame, glowColor * opacity * chunkOpacity, rotation + rotOffset, new Vector2(40, 38), scale, 0, 0);
			}
		}

		private void DrawBrain(SpriteBatch spriteBatch, Color drawColor, bool isOverlay)
		{
			if (attachedChain != null && chainSplitBrainAttached != null && chainSplitThinkerAttached != null)
				DrawFleshyChainTrails(isOverlay);

			// Draws a trail while the brain has contact damage, and a red glow around it
			if (contactDamageOpacity > 0)
			{
				for (int k = 0; k < 10; k++)
				{
					Vector2 pos = NPC.oldPos[k] + NPC.Size / 2f;
					DrawBrainSegments(spriteBatch, NPC, pos - Main.screenPosition, new Color(255, 100, 100), NPC.rotation, NPC.scale, (1f - k / 10f) * contactDamageOpacity * 0.5f * opacity, lastPos);
				}

				for(int k = 0; k < 4; k++)
				{
					Vector2 pos = NPC.Center + Vector2.UnitX.RotatedBy(k / 4f * 6.28f) * 8;
					DrawBrainSegments(spriteBatch, NPC, pos - Main.screenPosition, new Color(255, 150, 150), NPC.rotation, NPC.scale, contactDamageOpacity * opacity, lastPos);
				}
			}

			if (chargeAnimation > 0)
			{
				for (int k = 0; k < 6; k++)
				{
					Vector2 pos = NPC.Center + Vector2.UnitX.RotatedBy(k / 6f * 6.28f + chargeAnimation * 3.14f) * (1f - chargeAnimation) * 128;
					DrawBrainSegments(spriteBatch, NPC, pos - Main.screenPosition, new Color(255, 100, 100), NPC.rotation, NPC.scale, 0.35f * chargeAnimation * opacity, lastPos);
				}
			}

			if (Phase == Phases.FirstPhase && AttackState == 2)
				DrawRamGraphics(spriteBatch);

			if (Phase == Phases.SecondPhase && AttackState == 2)
				DrawHuntGraphics(spriteBatch);

			if (opacity >= 1)
			{
				DrawBrainSegments(spriteBatch, NPC, NPC.Center - Main.screenPosition, drawColor, NPC.rotation, NPC.scale, opacity, lastPos);
			}
			else
			{
				for (int k = 0; k < 6; k++)
				{
					float rot = k / 6f * 6.28f + opacity * 3.14f;
					Vector2 offset = Vector2.UnitX.RotatedBy(rot) * (1 - opacity) * 64;

					DrawBrainSegments(spriteBatch, NPC, NPC.Center + offset - Main.screenPosition, drawColor, NPC.rotation, NPC.scale, opacity * 0.2f, lastPos);
				}
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (NPC.IsABestiaryIconDummy)
			{
				DrawBrainSegments(spriteBatch, NPC, NPC.Center - screenPos, Color.White, 0, 1, 1);
				return false;
			}

			DrawBrain(spriteBatch, drawColor, false);

			return false;
		}

		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (Phase <= Phases.FirstToSecond)
			{
				Texture2D tex = Assets.Bosses.BrainRedux.ShieldMap.Value;

				Effect effect = Terraria.Graphics.Effects.Filters.Scene["BrainShield"].GetShader().Shader;

				effect.Parameters["time"]?.SetValue(Main.GameUpdateCount * 0.02f);
				effect.Parameters["size"]?.SetValue(tex.Size() * 0.6f);
				effect.Parameters["opacity"]?.SetValue(shieldOpacity);
				effect.Parameters["pixelRes"]?.SetValue(2f);

				effect.Parameters["drawTexture"]?.SetValue(tex);
				effect.Parameters["noiseTexture"]?.SetValue(Assets.Noise.SwirlyNoiseLooping.Value);
				effect.Parameters["pulseTexture"]?.SetValue(Assets.Noise.PerlinNoise.Value);
				effect.Parameters["edgeTexture"]?.SetValue(Assets.Bosses.BrainRedux.ShieldEdge.Value);
				effect.Parameters["outTexture"]?.SetValue(Assets.Bosses.BrainRedux.ShieldMapOut.Value);
				effect.Parameters["color"].SetValue(Vector3.Lerp(Vector3.One, new Vector3(1, 0.5f, 0.5f), contactDamageOpacity));

				spriteBatch.End();
				spriteBatch.Begin(default, BlendState.Additive, default, default, default, effect, Main.Transform);

				spriteBatch.Draw(tex, NPC.Center - Main.screenPosition, null, Color.White, 0, tex.Size() / 2f, 0.58f, SpriteEffects.FlipVertically, 0);

				spriteBatch.End();
				spriteBatch.Begin(default, default, default, default, default, default, Main.Transform);
			}
		}

		private void DrawGraymatterLink(SpriteBatch batch)
		{
			if (Phase == Phases.SecondPhase && attachedChain != null)
			{
				DrawGraymatterChainTrails();
			}
		}

		/// <summary>
		/// Builds all appropriate caches based on the phase
		/// </summary>
		protected void ManageCaches()
		{
			attachedChain.UpdateCacheFromChain(ref attachedChainCache);

			if (chainsSplit)
			{
				chainSplitBrainAttached.UpdateCacheFromChain(ref chainSplitBrainAttachedCache);
				chainSplitThinkerAttached.UpdateCacheFromChain(ref chainSplitThinkerAttachedCache);
			}
		}

		/// <summary>
		/// Initialize the trail for the attached chain, this may be called more than once if the trail is ever disposed early
		/// to reclaim resources due to being off-screen for a time.
		/// </summary>
		protected void InitAttachedChainTrail()
		{
			attachedChainTrail = new Trail(Main.instance.GraphicsDevice, attachedChain.segmentCount, new NoTip(), factor =>
			{
				float sin = (float)Math.Sin((factor * 3f + Main.GameUpdateCount / 30f) * 3.14f) - 0.4f;

				if (factor > 0.30f && factor < 0.36f)
					sin *= 1.5f;

				float floored = Math.Max(0, sin);

				return 32 + floored * 24;
			},
			factor =>
			{
				float sin = (float)Math.Sin((factor.X * 3f + Main.GameUpdateCount / 30f) * 3.14f) - 0.4f;
				float floored = Math.Max(0, sin);

				int index = (int)(factor.X * attachedChain.segmentCount);
				index = Math.Clamp(index, 0, attachedChain.segmentCount - 1);

				var glowColor = new Color(
					0.5f + 0.2f * MathF.Sin((Main.GameUpdateCount + factor.X) * 6.28f),
					0.5f + 0.2f * MathF.Sin((Main.GameUpdateCount + factor.X + 0.3f) * 6.28f),
					0.5f + 0.2f * MathF.Sin((Main.GameUpdateCount + factor.X + 0.6f) * 6.28f),
					0);

				Color lightColor = Lighting.GetColor((attachedChain.ropeSegments[index].posNow / 16).ToPoint());
				Color color = Color.Lerp(lightColor, glowColor, floored * 0.95f) * (1 + floored);
				return color;
			});
		}

		/// <summary>
		/// Initializes one of the dangling split chains' trail
		/// </summary>
		/// <param name="chain">The chain to create a trail for</param>
		/// <param name="toInit">The trail to populate</param>
		protected void InitSplitChainTrail(VerletChain chain, ref Trail toInit)
		{
			toInit = new Trail(Main.instance.GraphicsDevice, chain.segmentCount, new NoTip(), factor => 32,
			factor =>
			{
				int index = (int)(factor.X * chain.segmentCount);
				index = Math.Clamp(index, 0, chain.segmentCount - 1);

				return Lighting.GetColor((chain.ropeSegments[index].posNow / 16).ToPoint()) * opacity;
			});
		}

		/// <summary>
		/// Performs all trail updates to prepare for rendering based on phase
		/// </summary>
		protected void ManageTrail()
		{
			if (attachedChainTrail is null || attachedChainTrail.IsDisposed)
				InitAttachedChainTrail();

			attachedChainTrail.Positions = attachedChainCache.ToArray();
			attachedChainTrail.NextPosition = NPC.Center;

			if (chainsSplit)
			{
				if (chainSplitBrainAttachedTrail is null || chainSplitBrainAttachedTrail.IsDisposed)
					InitSplitChainTrail(chainSplitBrainAttached, ref chainSplitBrainAttachedTrail);

				if (chainSplitThinkerAttachedTrail is null || chainSplitThinkerAttachedTrail.IsDisposed)
					InitSplitChainTrail(chainSplitThinkerAttached, ref chainSplitThinkerAttachedTrail);

				chainSplitBrainAttachedTrail.Positions = chainSplitBrainAttachedCache.ToArray();
				chainSplitThinkerAttachedTrail.Positions = chainSplitThinkerAttachedCache.ToArray();
			}
		}

		/// <summary>
		/// Renders a trail using the repeating chain shader with the fleshy teather texture
		/// </summary>
		/// <param name="trail">The trail to render</param>
		/// /// <param name="repeats">The amount of times the flesh chain should repeat over the course of the trail</param>
		private void DrawFleshyTrail(Trail trail, float repeats)
		{
			Effect effect = Terraria.Graphics.Effects.Filters.Scene["RepeatingChain"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["alpha"].SetValue(1f);
			effect.Parameters["repeats"].SetValue(repeats);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);

			effect.Parameters["sampleTexture"].SetValue(Assets.Bosses.BrainRedux.DeadTeather.Value);
			trail?.Render(effect);
		}

		/// <summary>
		/// Renders all of the "fleshy" trails
		/// </summary>
		public void DrawFleshyChainTrails(bool isOverlay)
		{
			if (!chainsSplit)
			{
				DrawFleshyTrail(attachedChainTrail, 10f);
			}
			else
			{
				DrawFleshyTrail(chainSplitBrainAttachedTrail, 3.3f);

				if (!isOverlay)
					DrawFleshyTrail(chainSplitThinkerAttachedTrail, 6.6f);
			}
		}

		/// <summary>
		/// Renders all of the graymatter trails
		/// </summary>
		public void DrawGraymatterChainTrails()
		{
			Effect effect = Terraria.Graphics.Effects.Filters.Scene["LightningTrail"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"]?.SetValue(Main.GameUpdateCount * 0.025f);
			effect.Parameters["repeats"]?.SetValue(2f);
			effect.Parameters["transformMatrix"]?.SetValue(world * view * projection);

			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/WavyTrail").Value);
			attachedChainTrail?.Render(effect);
		}

		public override void SendExtraAI(BinaryWriter binaryWriter)
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

		public override void ReceiveExtraAI(BinaryReader binaryReader)
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
			int i = NPC.NewNPC(null, (int)position.X, (int)position.Y, ModContent.NPCType<DeadBrain>());
		}
	}
}