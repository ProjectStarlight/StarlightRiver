using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Physics;
using StarlightRiver.Core.Systems.BarrierSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.Graphics.Effects;
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

		public VerletChain chain;
		private List<Vector2> cache;
		private Trail trail;

		public TheThinker ThisThinker => thinker?.ModNPC as TheThinker;

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

			GraymatterBiome.onDrawHallucinationMap += DrawTether;
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
			chain = new VerletChain(40, true, npc.Center, 10);
		}

		public override bool PreAI(NPC npc)
		{
			if (!reworked)
				return true;

			npc.knockBackResist = 0f;

			Timer++;
			AttackTimer++;

			NPC.crimsonBoss = npc.whoAmI;

			if (State != 5)
				Lighting.AddLight(npc.Center, State > 2 ? new Vector3(0.5f, 0.4f, 0.2f) : new Vector3(0.5f, 0.5f, 0.5f));

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

			GUI.BossBarOverlay.SetTracked(thinker);
			Main.BigBossProgressBar.TryTracking(thinker.whoAmI);

			if (chain != null)
			{
				chain.startPoint = npc.Center + Vector2.UnitY * 90;
				chain.endPoint = thinker.Center;
				chain.useEndPoint = true;
				chain.drag = 1.1f;
				chain.forceGravity = Vector2.UnitY * 1f;
				chain.UpdateChain();
			}

			if (!Main.dedServ)
			{
				ManageCaches();
				ManageTrail();
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
						int i = NPC.NewNPC(null, (int)npc.Center.X, (int)npc.Center.Y, ModContent.NPCType<Neurysm>(), 0, 1, 60);
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

					int life = Main.masterMode ? 1000 : Main.expertMode ? 800 : 600;

					npc.lifeMax = life;
					npc.life = life;

					int barrier = Main.masterMode ? 400 : Main.expertMode ? 200 : 100;

					npc.GetGlobalNPC<BarrierNPC>().maxBarrier = barrier;
					npc.GetGlobalNPC<BarrierNPC>().barrier = barrier;

					Timer = 0;
					State = 1;

					break;

				// Intro
				case 1:

					Intro();

					break;

				// First phase
				case 2:

					if (thinker.life <= thinker.lifeMax / 2f)
						thinker.life = (int)(thinker.lifeMax / 2f);

					if (AttackTimer == 1)
					{
						AttackState = attackQueue[0];
						attackQueue.RemoveAt(0);

						int next = Main.rand.Next(4);

						while (next == attackQueue.Last())
							next = Main.rand.Next(4);

						attackQueue.Add(next);

						// Transition check
						if (thinker.life <= thinker.lifeMax / 2f)
						{
							State = 3;
							Timer = 0;
							AttackState = 0;

							// Set new trail
							if (!Main.dedServ)
								trail = new Trail(Main.instance.GraphicsDevice, chain.segmentCount, new NoTip(), factor => 30, factor => Color.White * opacity * 0.4f);

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

						int next = Main.rand.Next(4);

						while (next == attackQueue.Last())
							next = Main.rand.Next(4);

						attackQueue.Add(next);

						npc.netUpdate = true;
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
					}

					break;

				// Temporarily dead
				case 5:

					npc.noGravity = false;
					npc.noTileCollide = false;

					if (npc.velocity.Y > 0)
						npc.rotation += 0.01f;
					else
						npc.velocity.X *= 0;

					Dust.NewDust(npc.position, npc.width, npc.height, ModContent.DustType<Dusts.BloodMetaballDust>());

					if (opacity < 1)
						opacity += 0.01f;

					break;
			}

			hurtLastFrame = false;

			return false;
		}

		public override void PostAI(NPC npc)
		{
			if (reworked)
				npc.boss = false;
		}

		public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
		{
			return contactDamage && opacity > 0.9f;
		}

		public override bool? CanBeHitByItem(NPC npc, Player player, Item item)
		{
			if (reworked && opacity < 0.5f)
				return false; 

			return base.CanBeHitByItem(npc, player, item);
		}

		public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
		{
			if (reworked && opacity < 0.5f)
				return false;

			return base.CanBeHitByProjectile(npc, projectile);
		}

		public override void HitEffect(NPC npc, NPC.HitInfo hit)
		{
			hurtLastFrame = true;
		}

		public override bool CheckDead(NPC npc)
		{
			npc.life = 1;
			State = 5;

			(thinker.ModNPC as TheThinker).Timer = 0;
			(thinker.ModNPC as TheThinker).AttackTimer = 0;

			for (int k = 0; k < neurisms.Count; k++)
			{
				(neurisms[k].ModNPC as Neurysm).State = 1;
				(neurisms[k].ModNPC as Neurysm).Timer = 0;
			}

			npc.dontTakeDamage = true;

			return false;
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
			if (State == 3 && (AttackState == 1 || AttackState == 3))
				return false;

			return base.DrawHealthBar(npc, hbPosition, ref scale, ref position);
		}

		public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (!reworked)
				return true;

			Texture2D tex = Terraria.GameContent.TextureAssets.Npc[NPCID.BrainofCthulhu].Value;

			if (State == 2 && chain != null)
			{
				DrawPrimitives();
			}

			if (opacity >= 1)
			{
				Main.spriteBatch.Draw(tex, npc.Center - Main.screenPosition, npc.frame, drawColor * opacity, npc.rotation, npc.frame.Size() / 2f, npc.scale, 0, 0);
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
				if (TheBrain.State == 2)
				{
					Texture2D tex = Assets.Bosses.BrainRedux.ShieldMap.Value;

					var effect = Filters.Scene["BrainShield"].GetShader().Shader;

					effect.Parameters["time"]?.SetValue(Main.GameUpdateCount * 0.02f);
					effect.Parameters["size"]?.SetValue(tex.Size() * 0.6f);
					effect.Parameters["opacity"]?.SetValue(0.4f);
					effect.Parameters["pixelRes"]?.SetValue(2f);

					effect.Parameters["drawTexture"]?.SetValue(tex);
					effect.Parameters["noiseTexture"]?.SetValue(Assets.Noise.SwirlyNoiseLooping.Value);
					effect.Parameters["pulseTexture"]?.SetValue(Assets.Noise.PerlinNoise.Value);
					effect.Parameters["edgeTexture"]?.SetValue(Assets.Bosses.BrainRedux.ShieldEdge.Value);

					spriteBatch.End();
					spriteBatch.Begin(default, BlendState.Additive, default, default, default, effect, Main.Transform);

					spriteBatch.Draw(tex, TheBrain.npc.Center - Main.screenPosition, null, Color.White, 0, tex.Size() / 2f, 0.6f, SpriteEffects.FlipVertically, 0);

					spriteBatch.End();
					spriteBatch.Begin(default, default, default, default, default, default, Main.Transform);
				}
			}
		}

		private void DrawTether(SpriteBatch batch)
		{
			if (TheBrain is null)
				return;

			if (TheBrain.State == 3 && TheBrain.chain != null)
			{
				TheBrain.DrawPrimitivesGray();
			}
		}

		protected void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < chain.segmentCount; i++)
				{
					cache.Add(chain.ropeSegments[i].posNow);
				}
			}

			for (int i = 0; i < chain.segmentCount - 1; i++)
			{
				cache[i] = chain.ropeSegments[i].posNow;
			}

			cache[chain.segmentCount - 1] = chain.endPoint;
		}

		protected void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, chain.segmentCount, new NoTip(), factor => 10, factor =>
			{
				int index = (int)(factor.X * chain.segmentCount);
				index = Math.Clamp(index, 0, chain.segmentCount - 1);
				return Lighting.GetColor((chain.ropeSegments[index].posNow / 16).ToPoint());
				});

			trail.Positions = cache.ToArray();
			trail.NextPosition = npc.Center;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(0f);
			effect.Parameters["repeats"].SetValue(0f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);

			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
			trail?.Render(effect);
		}

		public void DrawPrimitivesGray()
		{
			Effect effect = Filters.Scene["LightningTrail"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"]?.SetValue(Main.GameUpdateCount * 0.025f);
			effect.Parameters["repeats"]?.SetValue(2f);
			effect.Parameters["transformMatrix"]?.SetValue(world * view * projection);

			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/WavyTrail").Value);
			trail?.Render(effect);
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
