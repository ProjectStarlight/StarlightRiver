using StarlightRiver.Content.DropRules;
using StarlightRiver.Content.GUI;
using StarlightRiver.Content.Items.Permafrost;
using StarlightRiver.Content.NPCs.BaseTypes;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
	[AutoloadBossHead]
	public partial class SquidBoss : ModNPC, IUnderwater
	{
		public enum AIStates
		{
			SpawnEffects = 0,
			SpawnAnimation = 1,
			FirstPhase = 2,
			FirstPhaseTwo = 3,
			SecondPhase = 4,
			ThirdPhase = 5,
			DeathAnimation = 6,
			Fleeing = 7
		}

		public List<NPC> tentacles = new(); //the tentacle NPCs which this boss controls
		public List<NPC> platforms = new(); //the big platforms the boss' arena has
		public bool variantAttack;
		public int baseLife;
		public NPC arenaActor;
		private NPC arenaBlocker;
		Vector2 spawnPoint;
		Vector2 savedPoint;

		public float Opacity = 1;
		public bool OpaqueJelly = false;

		internal ref float Phase => ref NPC.ai[0];
		internal ref float GlobalTimer => ref NPC.ai[1];
		internal ref float AttackPhase => ref NPC.ai[2];
		internal ref float AttackTimer => ref NPC.ai[3];

		internal ArenaActor Arena => arenaActor.ModNPC as ArenaActor;

		public override string Texture => AssetDirectory.Invisible;

		public override string BossHeadTexture => AssetDirectory.SquidBoss + "SquidBoss_Head_Boss";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Auroracle");
		}

		public override bool CheckActive()
		{
			return false;
		}

		public void DrawBestiary(SpriteBatch spriteBatch, Vector2 screenPos)
		{
			Animate(12, 0, 8);
			NPC.Center += Main.screenPosition - screenPos;
			NPC.ai[1]++;

			Texture2D body = Request<Texture2D>(AssetDirectory.SquidBoss + "BodyUnder").Value;
			spriteBatch.Draw(body, NPC.Center - Main.screenPosition, NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() / 2, 1, 0, 0);

			DrawHeadBlobs(spriteBatch);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (NPC.IsABestiaryIconDummy)
				DrawBestiary(spriteBatch, screenPos);

			return false;
		}

		public override bool CheckDead()
		{
			if (Phase != (int)AIStates.DeathAnimation)
			{
				Phase = (int)AIStates.DeathAnimation;
				NPC.life = 1;
				NPC.dontTakeDamage = true;
				GlobalTimer = 0;

				foreach (NPC tentacle in tentacles.Where(n => n.active))
					tentacle.Kill();

				return false;
			}

			else
			{
				Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCDeath1 with { Pitch = -0.8f }, NPC.Center);

				/*for (int k = 0; k < 10; k++)
                    Gore.NewGore(NPC.Center, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(6), Mod.Find<ModGore>("SquidGore").Type);*/
				return true;
			}
		}

		public override void SetDefaults()
		{
			NPC.lifeMax = 4000;
			NPC.width = 80;
			NPC.height = 80;
			NPC.boss = true;
			NPC.damage = 1;
			NPC.noGravity = true;
			NPC.aiStyle = -1;
			Music = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/SquidBoss");
			NPC.noTileCollide = true;
			NPC.knockBackResist = 0;
			NPC.dontTakeDamage = true;

			baseLife = 2000;
		}

		public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
		{
			NPC.lifeMax = Main.masterMode ? (int)(8000 * bossLifeScale) : (int)(6000 * bossLifeScale);
			baseLife = Main.masterMode ? (int)(4000 * bossLifeScale) : (int)(3000 * bossLifeScale);
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				Bestiary.SLRSpawnConditions.AuroraSquid,
				new FlavorTextBestiaryInfoElement("An aquatic titan that stalks the impossibly cold waters behind its temple, channeling the light of the aurora into powerful magic to hunt its prey.")
			});
		}

		public override ModNPC Clone(NPC npc)
		{
			var newNpc = base.Clone(npc) as SquidBoss;
			newNpc.tentacles = new List<NPC>();
			newNpc.platforms = new List<NPC>();
			newNpc.arenaBlocker = null;

			return newNpc;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			var normalMode = new LeadingConditionRule(new Conditions.NotExpert());
			normalMode.ConditionalOneFromOptions(new int[]
			{
				ItemType<OverflowingUrn>(),
				ItemType<AuroraBell>(),
				ItemType<AuroraThroneMountItem>(),
				ItemType<Tentalance>(),
				ItemType<Octogun>(),
			});

			npcLoot.Add(normalMode);
			npcLoot.Add(CustomDropRules.onlyInNormalMode(ItemType<SquidFins>(), 4));
			npcLoot.Add(ItemDropRule.Common(ItemType<Tiles.Trophies.AuroracleTrophyItem>(), 10, 1, 1));
			npcLoot.Add(ItemDropRule.BossBag(ItemType<SquidBossBag>()));
		}

		public override void BossLoot(ref string name, ref int potionType)
		{
			for (int k = 0; k < Main.maxPlayers; k++)
			{
				Player Player = Main.player[k];

				if (Player.active && StarlightWorld.squidBossArena.Contains((Player.Center / 16).ToPoint()))
					Player.GetModPlayer<MedalPlayer>().ProbeMedal("Auroracle");
			}

			StarlightWorld.Flag(WorldFlags.SquidBossDowned);
		}

		public void DrawUnderWater(SpriteBatch spriteBatch, int NPCLayer)
		{
			Texture2D ring = Request<Texture2D>(AssetDirectory.SquidBoss + "BodyRing").Value;
			Texture2D ringGlow = Request<Texture2D>(AssetDirectory.SquidBoss + "BodyRingGlow").Value;
			Texture2D ringSpecular = Request<Texture2D>(AssetDirectory.SquidBoss + "BodyRingSpecular").Value;
			Texture2D ringBlur = Request<Texture2D>(AssetDirectory.SquidBoss + "BodyRingBlur").Value;

			Texture2D body = Request<Texture2D>(AssetDirectory.SquidBoss + "BodyUnder").Value;
			Texture2D bodyGlow = Request<Texture2D>(AssetDirectory.SquidBoss + "BodyGlow").Value;

			for (int k = 3; k > 0; k--) //handles the drawing of the jelly rings under the boss.
			{
				Vector2 pos = NPC.Center + new Vector2(0, 70 + k * 35).RotatedBy(NPC.rotation) - Main.screenPosition;

				int squish = k * 10 + (int)(Math.Sin(GlobalTimer / 10f - k / 4f * 6.28f) * 20);
				var rect = new Rectangle((int)pos.X, (int)pos.Y, ring.Width + (3 - k) * 20 - squish, ring.Height + (int)(squish * 0.4f) + (3 - k) * 5);

				int squish2 = k * 10 + (int)(Math.Sin(GlobalTimer / 10f - k / 4f * 6.28f + 1.5f) * 24);
				var rect2 = new Rectangle((int)pos.X, (int)pos.Y, ring.Width + (3 - k) * 20 - squish2, ring.Height + (int)(squish2 * 0.4f) + (3 - k) * 5);

				float sin = 1 + (float)Math.Sin(GlobalTimer / 10f - k);
				float cos = 1 + (float)Math.Cos(GlobalTimer / 10f + k);
				Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f) * Opacity;

				if (Phase == (int)AIStates.ThirdPhase || Phase == (int)AIStates.DeathAnimation)
					color = new Color(1.2f + sin * 0.1f, 0.7f + sin * -0.25f, 0.25f) * 0.7f * Opacity;

				if (OpaqueJelly)
					color.A = 255;

				spriteBatch.Draw(ring, rect, ring.Frame(), color * 0.8f, NPC.rotation, ring.Size() / 2, 0, 0);

				float opacity = Math.Min(Opacity, 1);

				color.A = 0;
				Rectangle rect3 = rect;
				rect3.Inflate(10, 6);
				rect3.Offset(new Point(10, 6));
				spriteBatch.Draw(ringBlur, rect3, null, color * (0.2f * ((opacity - 0.5f) / 0.5f)), NPC.rotation, ringBlur.Size() / 2, 0, 0);
				rect3.Inflate(10, 6);
				rect3.Offset(new Point(10, 6));
				spriteBatch.Draw(ringBlur, rect3, null, color * (0.075f * ((opacity - 0.5f) / 0.5f)), NPC.rotation, ringBlur.Size() / 2, 0, 0);

				if (!OpaqueJelly)
				{
					spriteBatch.Draw(ringGlow, rect, ring.Frame(), color * 0.6f, NPC.rotation, ring.Size() / 2, 0, 0);
					spriteBatch.Draw(ringSpecular, rect2, ring.Frame(), Color.White * Opacity, NPC.rotation, ring.Size() / 2, 0, 0);
				}
			}

			Color lightColor = Lighting.GetColor((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16);
			Color bodyColor = lightColor * 1.2f * Opacity;
			bodyColor.A = 255;
			spriteBatch.Draw(body, NPC.Center - Main.screenPosition, NPC.frame, bodyColor, NPC.rotation, NPC.frame.Size() / 2, 1, 0, 0);
			spriteBatch.Draw(bodyGlow, NPC.Center - Main.screenPosition, NPC.frame, Color.White * (0.5f + Opacity * 0.5f), NPC.rotation, NPC.frame.Size() / 2, 1, 0, 0);

			DrawHeadBlobs(spriteBatch);

			if (Phase >= (int)AIStates.SecondPhase)
			{
				Texture2D sore = Request<Texture2D>(Texture).Value;
				spriteBatch.Draw(sore, NPC.Center - Main.screenPosition, sore.Frame(), lightColor * 1.2f, NPC.rotation, sore.Size() / 2, 1, 0, 0);
			}
		}

		private void DrawHeadBlobs(SpriteBatch spriteBatch)
		{
			Texture2D headBlob = Request<Texture2D>(AssetDirectory.SquidBoss + "BodyOver").Value;
			Texture2D headBlobGlow = Request<Texture2D>(AssetDirectory.SquidBoss + "BodyOverGlow").Value;
			Texture2D headBlobSpecular = Request<Texture2D>(AssetDirectory.SquidBoss + "BodyOverSpecular").Value;
			Texture2D headBlobBlur = Request<Texture2D>(AssetDirectory.SquidBoss + "BodyBlur").Value;

			for (int k = 0; k < 5; k++) //draws the head blobs
			{
				Vector2 off = Vector2.Zero;
				var frame = new Rectangle();

				switch (k)
				{
					case 0:
						off = new Vector2(42, 12);
						frame = new Rectangle(248, 0, 76, 64);
						break;

					case 1:
						off = new Vector2(-43, 12);
						frame = new Rectangle(64, 0, 76, 64);
						break;

					case 2:
						off = new Vector2(-41, -20);
						frame = new Rectangle(0, 0, 64, 64);
						break;

					case 3:
						off = new Vector2(40, -20);
						frame = new Rectangle(324, 0, 64, 64);
						break;

					case 4:
						off = new Vector2(-1, -58);
						frame = new Rectangle(140, 0, 108, 64);
						break;
				}

				off = off.RotatedBy(NPC.rotation);

				float sin = 1 + (float)Math.Sin(GlobalTimer / 10f - k * 0.5f);
				float sin2 = 1 + (float)Math.Sin(GlobalTimer / 10f - k * 0.5f + 1.5f);
				float cos = 1 + (float)Math.Cos(GlobalTimer / 10f + k * 0.5f);
				float scale = 1 + sin * 0.04f;
				float scale2 = 1 + sin2 * 0.06f;

				Color color = Color.White;

				Color auroraColor = new Color(0.5f + cos * 0.25f, 0.8f, 0.5f + sin * 0.25f) * Opacity;
				Color redColor = new Color(1.2f + sin * 0.1f, 0.7f + sin * -0.25f, 0.25f) * 0.8f * Opacity;

				color = auroraColor;

				if (Phase == (int)AIStates.ThirdPhase) //Red jelly in last phases
					color = Color.Lerp(auroraColor, redColor, GlobalTimer / 60f);

				if (Phase == (int)AIStates.DeathAnimation)
					color = redColor;

				if (Phase == (int)AIStates.DeathAnimation) //Unique drawing for death animation
				{
					sin = 1 + (float)Math.Sin(GlobalTimer / 5f - k * 0.5f); //faster pulsing
					scale = 1 + sin * 0.08f; //bigger pulsing

					if (GlobalTimer >= (k + 1) * 20)
						continue; //"destroy" the blobs
				}

				if (OpaqueJelly)
					color.A = 255;

				spriteBatch.Draw(headBlob, NPC.Center + off - Main.screenPosition, frame, color * 0.8f, NPC.rotation,
					new Vector2(frame.Width / 2, frame.Height), scale, 0, 0);

				if (k == 4)
				{
					float opacity = Math.Min(Opacity, 1);

					color.A = 0;
					spriteBatch.Draw(headBlobBlur, NPC.Center - Main.screenPosition, null, color * (0.2f * ((opacity - 0.5f) / 0.5f)), NPC.rotation, headBlobBlur.Size() / 2, 0.26f, 0, 0);
					spriteBatch.Draw(headBlobBlur, NPC.Center - Main.screenPosition, null, color * (0.075f * ((opacity - 0.5f) / 0.5f)), NPC.rotation, headBlobBlur.Size() / 2, 0.28f, 0, 0);
				}

				if (!OpaqueJelly)
				{
					color.A = 0;

					spriteBatch.Draw(headBlobGlow, NPC.Center + off - Main.screenPosition, frame, color * 0.6f, NPC.rotation,
						new Vector2(frame.Width / 2, frame.Height), scale, 0, 0);

					spriteBatch.Draw(headBlobSpecular, NPC.Center + off - Main.screenPosition, frame, Color.White * Opacity, NPC.rotation,
						new Vector2(frame.Width / 2, frame.Height), scale2, 0, 0);
				}
			}
		}

		private void DoLighting()
		{
			for (int k = 0; k < 5; k++) //lighting for the head blobs
			{
				Vector2 off = Vector2.Zero;

				switch (k)
				{
					case 0:
						off = new Vector2(42, 12);
						break;

					case 1:
						off = new Vector2(-43, 12);
						break;

					case 2:
						off = new Vector2(-41, -20);
						break;

					case 3:
						off = new Vector2(40, -20);
						break;

					case 4:
						off = new Vector2(-1, -58);
						break;
				}

				off = off.RotatedBy(NPC.rotation);

				float sin = 1 + (float)Math.Sin(GlobalTimer / 10f - k * 0.5f);
				float cos = 1 + (float)Math.Cos(GlobalTimer / 10f + k * 0.5f);

				var color = new Color(0.5f + cos * 0.25f, 0.8f, 0.5f + sin * 0.25f);

				if (Phase == (int)AIStates.ThirdPhase || Phase == (int)AIStates.DeathAnimation) //Red in last phases
					color = new Color(1.2f + sin * 0.1f, 0.7f + sin * -0.25f, 0.25f) * 0.8f;

				Lighting.AddLight(NPC.Center + off, color.ToVector3() * 0.5f);
			}
		}

		internal void Animate(int fps, int animation, int frames)
		{
			NPC.frame.Width = 188;
			NPC.frame.Height = 200;
			NPC.Frame(NPC.frame.Width * animation, NPC.frame.Height * ((int)(Main.GameUpdateCount / 60f * fps) % frames));
		}

		internal void ManualAnimate(int frameX, int frameY)
		{
			NPC.frame.Width = 188;
			NPC.frame.Height = 200;
			NPC.Frame(NPC.frame.Width * frameX, NPC.frame.Height * frameY);
		}

		/// <summary>
		/// Only intended for use by the fake boss in the arena actor!
		/// </summary>
		public void QuickSetup()
		{
			for (int k = 0; k < 4; k++) //each tenticle
			{
				int x;
				int y;
				int xb;

				switch (k) //I handle these manually to get them to line up with the window correctly
				{
					case 0: x = -370; y = 0; xb = -50; break;
					case 1: x = -420; y = -100; xb = -20; break;
					case 3: x = 370; y = 0; xb = 50; break;
					case 2: x = 420; y = -100; xb = 20; break;
					default: x = 0; y = 0; xb = 0; break;
				}

				var tent = new NPC();
				tent.SetDefaults(NPCType<Tentacle>());
				tent.Center = new Vector2(NPC.Center.X + x, NPC.Center.Y + 550);
				tent.ai[0] = 1;

				(tent.ModNPC as Tentacle).Parent = this;
				(tent.ModNPC as Tentacle).OffsetFromParentBody = xb;
				(tent.ModNPC as Tentacle).Timer = 60;
				tentacles.Add(tent);
			}
		}

		public override void AI()
		{
			GlobalTimer++;

			DoLighting();

			Animate(12, 0, 8);

			if (arenaActor is null || !arenaActor.active)
				arenaActor = Main.npc.FirstOrDefault(n => n.active && n.ModNPC is ArenaActor);

			//boss health bar glow effects

			float sin = (float)Math.Sin(Main.GameUpdateCount * 0.05f);
			float sin2 = (float)Math.Sin(Main.GameUpdateCount * 0.05f + 1.5f);
			float cos = (float)Math.Cos(Main.GameUpdateCount * 0.05f);
			BossBarOverlay.glowColor = new Color(0.5f + cos * 0.25f, 0.8f, 0.5f + sin * 0.25f) * 0.8f;

			if (Phase == (int)AIStates.SpawnEffects)
			{
				Phase = (int)AIStates.SpawnAnimation;

				NPC.damage = 0;

				foreach (NPC NPC in Main.npc.Where(n => n.active && n.ModNPC is IcePlatform))
				{
					platforms.Add(NPC);
				}

				platforms.RemoveAll(n => Math.Abs(n.Center.X - Main.npc.FirstOrDefault(l => l.active && l.ModNPC is ArenaActor).Center.X) >= 550);

				spawnPoint = NPC.Center;

				int i = NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y - 1050, NPCType<ArenaBlocker>(), 0, 800);
				arenaBlocker = Main.npc[i];

				for (int k = 0; k < Main.maxPlayers; k++)
				{
					Player Player = Main.player[k];

					if (Player.active && StarlightWorld.squidBossArena.Contains((Player.Center / 16).ToPoint()))
						Player.GetModPlayer<MedalPlayer>().QualifyForMedal("Auroracle", 0);
				}

				BossBarOverlay.SetTracked(NPC, ", The Venerated", Request<Texture2D>(AssetDirectory.GUI + "BossBarFrame").Value);
			}

			if (Phase == (int)AIStates.SpawnAnimation)
				SpawnAnimation();

			if (Phase == (int)AIStates.FirstPhase) //first phase, part 1. Tentacle attacks and ink.
			{
				AttackTimer++;

				//passive movement
				NPC.position.X += (float)Math.Sin(GlobalTimer * 0.03f);
				NPC.position.Y += (float)Math.Cos(GlobalTimer * 0.08f);

				int tentacleLife = 0;

				foreach (NPC tentacle in tentacles)
				{
					tentacleLife += tentacle.life;
				}

				NPC.life = baseLife + tentacleLife;

				if (AttackTimer == 1)
				{
					int tentacleCount = tentacles.Count(n => n.ai[0] == 2);

					if (tentacleCount <= 2 && tentacleCount > 1) //phasing logic
					{
						Phase = (int)AIStates.FirstPhaseTwo;
						GlobalTimer = 0;
						ResetAttack();
						return;
					}
					else //else advance the attack pattern
					{
						AttackPhase++;
						variantAttack = Main.rand.NextBool();

						if (AttackPhase > (Main.expertMode ? 5 : 4))
							AttackPhase = 1;
					}
				}

				switch (AttackPhase)
				{
					case 1:
						TentacleSpike();
						break;

					case 2:
						if (variantAttack)
							InkBurstAlt();
						else
							InkBurst();
						break;

					case 3:
						if (variantAttack)
							TentacleSpike();
						else
							PlatformSweep();
						break;

					case 4:
						if (variantAttack)
							SpawnAdds();
						else
							InkBurst();
						break;

					case 5:
						ArenaSweep();
						break;
				}
			}

			if (Phase == (int)AIStates.FirstPhaseTwo) //first phase, part 2. Tentacle attacks and ink. Raise water first.
			{
				int tentacleLife = 0;

				foreach (NPC tentacle in tentacles)
				{
					tentacleLife += tentacle.life;
				}

				NPC.life = baseLife + tentacleLife;

				if (GlobalTimer == 1)
					savedPoint = NPC.Center;

				if (GlobalTimer < 50)
					Arena.WaterfallWidth = (int)GlobalTimer;

				if (GlobalTimer < 325) //water rising up
				{
					float x = GlobalTimer + 1;
					float fun = (float)(-937.5f * Math.Pow(x / 325f, 3) + 1406.25f * Math.Pow(x / 325f, 2) - 143.75 * x / 325f);
					float dif = fun - (float)(-937.5f * Math.Pow(GlobalTimer / 325f, 3) + 1406.25f * Math.Pow(GlobalTimer / 325f, 2) - 143.75 * GlobalTimer / 325f);
					Arena.NPC.ai[0] += dif;

					NPC.Center = Vector2.SmoothStep(savedPoint, spawnPoint + new Vector2(0, -750), GlobalTimer / 325f);

					if (GlobalTimer % 45 == 0 && GlobalTimer < 200)
						Helper.PlayPitched("SquidBoss/UnderwaterSwoosh", 0.5f, 0f, NPC.Center);

					if (GlobalTimer % 180 == 0 || GlobalTimer == 1)
						Helper.PlayPitched("SquidBoss/WaterLoop", 2, 0.0f, NPC.Center);
				}

				if (GlobalTimer > 275 && GlobalTimer <= 325)
					Arena.WaterfallWidth = 50 - ((int)GlobalTimer - 275);

				if (GlobalTimer == 325) //make the remaining tentacles vulnerable
				{
					foreach (NPC tentacle in tentacles.Where(n => n.ai[0] == 1))
						tentacle.ai[0] = 0;
				}

				if (GlobalTimer > 325) //continue attacking otherwise
				{
					AttackTimer++;

					//passive movement
					NPC.position.X += (float)Math.Sin(GlobalTimer * 0.03f);
					NPC.position.Y += (float)Math.Cos(GlobalTimer * 0.08f);

					if (AttackTimer == 1)
					{
						if (tentacles.Count(n => n.ai[0] == 2) == 4) //phasing logic
						{
							Phase = (int)AIStates.SecondPhase;
							GlobalTimer = 0;
							ResetAttack();
							return;
						}
						else //else advance the attack pattern
						{
							AttackPhase++;

							if (AttackPhase > (Main.expertMode ? 6 : 5))
								AttackPhase = 1;
						}
					}

					switch (AttackPhase)
					{
						case 1:
							TentacleSpike();
							break;

						case 2:
							if (variantAttack)
								InkBurstAlt();
							else
								InkBurst();
							break;

						case 3:
							PlatformSweep();
							break;

						case 4:
							if (variantAttack)
								TentacleSpike();
							else
								InkBurstAlt();
							break;

						case 5:
							InkBurst();
							break;

						case 6:
							ArenaSweep();
							break;
					}
				}
			}

			if (Phase == (int)AIStates.SecondPhase) //second phase
			{
				if (GlobalTimer < 50)
					Arena.WaterfallWidth = (int)GlobalTimer;

				if (GlobalTimer < 300) //water rising
				{
					float x = GlobalTimer + 1;
					float fun = (float)(-833.33f * Math.Pow(x / 325f, 3) + 1250f * Math.Pow(x / 325f, 2) - 116.66 * x / 325f);
					float dif = fun - (float)(-833.33f * Math.Pow(GlobalTimer / 325f, 3) + 1250f * Math.Pow(GlobalTimer / 325f, 2) - 116.66 * GlobalTimer / 325f);
					Arena.NPC.ai[0] += dif;

					if (GlobalTimer % 45 == 0 && GlobalTimer < 200)
						Helper.PlayPitched("SquidBoss/UnderwaterSwoosh", 1, 0f, NPC.Center);

					if (GlobalTimer % 180 == 0 || GlobalTimer == 1)
						Helper.PlayPitched("SquidBoss/WaterLoop", 2, 0.0f, NPC.Center);

					arenaBlocker.position.Y -= 1f;
				}

				if (GlobalTimer > 250 && GlobalTimer <= 300)
					Arena.WaterfallWidth = 50 - ((int)GlobalTimer - 250);

				if (GlobalTimer == 300) //reset
				{
					NPC.dontTakeDamage = false;
					ResetAttack();
					AttackPhase = 0;
				}

				if (GlobalTimer > 300)
				{
					if (NPC.life < NPC.lifeMax / 4)
						NPC.dontTakeDamage = true; //health gate

					AttackTimer++;

					if (AttackTimer == 1)
					{
						if (NPC.life < NPC.lifeMax / 4) //phasing logic
						{
							Phase = (int)AIStates.ThirdPhase;
							GlobalTimer = 0;
							AttackPhase = 0;
							ResetAttack();
							arenaBlocker.ai[1] = 1;
							return;
						}

						AttackPhase++;

						variantAttack = Main.rand.NextBool();

						if (AttackPhase > 4)
							AttackPhase = 1;

						NPC.netUpdate = true;
					}

					switch (AttackPhase)
					{
						case 1:
							if (variantAttack)
								SpewAlternate();
							else
								Spew();
							break;

						case 2:
							Laser();
							break;

						case 3:
							if (variantAttack)
								SpewAlternate();
							else
								Spew();
							break;

						case 4:
							Leap();
							break;
					}
				}
			}

			if (Phase == (int)AIStates.ThirdPhase)
			{
				if (GlobalTimer == 1) //reset velocity + set movement points
				{
					NPC.velocity *= 0;
					NPC.rotation = 0;
					savedPoint = NPC.Center;
				}

				if (GlobalTimer < 240)
					NPC.Center = Vector2.SmoothStep(savedPoint, spawnPoint + new Vector2(0, -1400), GlobalTimer / 240f); //move to the top of the arena

				if (GlobalTimer == 240) //roar and activate
				{
					NPC.dontTakeDamage = false;
					CameraSystem.shake += 40;
					Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
				}

				if (GlobalTimer > 240 && GlobalTimer <= 290)
					Arena.WaterfallWidth = (int)GlobalTimer - 240;

				if (GlobalTimer > 240) //following unless using ink attack
				{
					if (AttackPhase != 3)
					{
						NPC.velocity += Vector2.Normalize(NPC.Center - (Main.player[NPC.target].Center + new Vector2(0, -300))) * -0.3f;

						if (NPC.velocity.LengthSquared() > 36)
							NPC.velocity = Vector2.Normalize(NPC.velocity) * 6;

						NPC.rotation = NPC.velocity.X * 0.05f;
					}

					GlobalTimer++;

					if (GlobalTimer % 6 == 0)
						Main.npc.FirstOrDefault(n => n.active && n.ModNPC is ArenaActor).ai[0]++; //rising water

					AttackTimer++;

					if (AttackTimer == 1)
					{
						AttackPhase++;

						if (AttackPhase > 3)
							AttackPhase = 1;
					}

					switch (AttackPhase)
					{
						case 1: TentacleSpike2(); break;
						case 2: StealPlatform(); break;
						case 3: InkBurst2(); break;
					}
				}
			}

			if (Phase == (int)AIStates.DeathAnimation)
				DeathAnimation();

			if (Phase == (int)AIStates.Fleeing)
			{
				if (GlobalTimer < 50)
					Arena.WaterfallWidth = 50 - (int)GlobalTimer;

				if (GlobalTimer > 50)
					NPC.active = false;
			}
		}

		public override void SendExtraAI(System.IO.BinaryWriter writer)
		{
			writer.Write(variantAttack);
		}

		public override void ReceiveExtraAI(System.IO.BinaryReader reader)
		{
			variantAttack = reader.ReadBoolean();
		}
	}
}