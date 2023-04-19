﻿using StarlightRiver.Content.Bosses.GlassMiniboss;
using StarlightRiver.Content.Bosses.SquidBoss;
using StarlightRiver.Content.Bosses.VitricBoss;
using StarlightRiver.Content.Items.Permafrost;
using StarlightRiver.Content.NPCs.BossRush;
using StarlightRiver.Content.Tiles.Vitric;
using StarlightRiver.Core.Systems.ScreenTargetSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.GameContent.Generation;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;

namespace StarlightRiver.Core.Systems.BossRushSystem
{
	internal class BossRushSystem : ModSystem
	{
		public static bool isBossRush = false;

		public static int bossRushDifficulty;

		public static int currentStage = -1;
		public static int trackedBossType = 0;

		public static int scoreTimer;
		public static int score;

		public static int savedNormalScore;
		public static int savedExpertScore;
		public static int savedMasterScore;

		public static int speedupTimer;

		public static int transitionTimer = 0;
		public static Rectangle visibleArea = new(0, 0, 0, 0);

		public static List<BossRushStage> stages;

		public static ScreenTarget starsTarget = new(DrawStars, () => isBossRush, 1f);
		public static ScreenTarget starsMap = new(DrawMap, () => isBossRush, 1f);

		public static ParticleSystem stars = new("StarlightRiver/Assets/Misc/DotTell", updateStars);

		public static BossRushStage CurrentStage => (currentStage >= 0 && currentStage < stages.Count) ? stages[currentStage] : null;

		public override void Load()
		{
			On_Main.DrawInterface += DrawOverlay;
			On_Main.DoUpdate += Speedup;

			File.WriteAllBytes(Path.Combine(ModLoader.ModPath, "BossRushWorld.wld"), Mod.GetFileBytes("Worlds/BossRushWorld.wld"));
			File.WriteAllBytes(Path.Combine(ModLoader.ModPath, "BossRushWorld.twld"), Mod.GetFileBytes("Worlds/BossRushWorld.twld"));
		}

		/// <summary>
		/// Resets the boss rush to the default starting state
		/// </summary>
		public static void Reset()
		{
			trackedBossType = 0;
			currentStage = -1;
			scoreTimer = 0;
			score = 8000;

			transitionTimer = 0;
		}

		/// <summary>
		/// Ends the boss rush and submits your final score
		/// </summary>
		public void End()
		{
			if (Main.GameMode == 0)
				savedNormalScore = score;
			if (Main.GameMode == 1)
				savedExpertScore = score * 2;
			if (Main.GameMode == 2)
				savedMasterScore = score * 3;

			WorldGen.SaveAndQuit();
		}

		/// <summary>
		/// This handles the per-stage healing rules.
		/// </summary>
		public static void Heal()
		{
			if (Main.masterMode) // No heal
				return;

			if (Main.expertMode) // Partial heal, clear only non-potion-sickness
			{
				Main.LocalPlayer.Heal(200);

				for (int k = 0; k < Main.LocalPlayer.buffType.Length; k++)
				{
					int type = Main.LocalPlayer.buffType[k];

					if (type != BuffID.PotionSickness && Main.debuff[type])
						Main.LocalPlayer.buffTime[k] = 0;
				}

				return;
			}

			// Full heal and clear ALL debuffs
			Main.LocalPlayer.statLife = Main.LocalPlayer.statLifeMax2;

			for (int k = 0; k < Main.LocalPlayer.buffType.Length; k++)
			{
				if (Main.debuff[Main.LocalPlayer.buffType[k]])
					Main.LocalPlayer.buffTime[k] = 0;
			}
		}

		/// <summary>
		/// This handles the speedup rules of the boss rush. IE that blitz should be 1.25x gamespeed and showdown 1.5x
		/// </summary>
		/// <param name="orig"></param>
		/// <param name="self"></param>
		/// <param name="gameTime"></param>
		private void Speedup(On_Main.orig_DoUpdate orig, Main self, ref GameTime gameTime)
		{
			orig(self, ref gameTime);

			if (!isBossRush || Main.gameMenu) //dont do anything outside of bossrush but the normal update
				return;

			speedupTimer++; //track this seperately since gameTime would get sped up

			if (Main.expertMode) //1.25x on expert
			{
				if (speedupTimer % 4 == 0)
					orig(self, ref gameTime);

				return;
			}

			if (Main.masterMode) //1.5x on master
			{
				if (speedupTimer % 2 == 0)
					orig(self, ref gameTime);

				return;
			}
		}

		/// <summary>
		/// The actual stages are populated here. The first and last stages are bumpers for the DPS evaluation and ending item respectively.
		/// </summary>
		public override void PostAddRecipes()
		{
			stages = new List<BossRushStage>()
			{
				new BossRushStage(
					"Structures/BossRushStart",
					ModContent.NPCType<BossRushLock>(),
					new Vector2(250, 200),
					a =>
					{
						StarlightWorld.vitricBiome = new Rectangle(0, 2000, 40, 40);

						NPC.NewNPC(null, (int)a.X + 250, (int)a.Y + 200, ModContent.NPCType<BossRushLock>());

						visibleArea = new Rectangle((int)a.X, (int)a.Y, 500, 360);
						HushArmorSystem.DPSTarget = 50;
					},
					a => _ = a),

				new BossRushStage(
					"Structures/SquidBossArena",
					ModContent.NPCType<SquidBoss>(),
					new Vector2(500, 1600),
					a =>
					{
						Item.NewItem(null, a + new Vector2(800, 2700), ModContent.ItemType<SquidBossSpawn>());

						visibleArea = new Rectangle((int)a.X, (int)a.Y + 120, 1748, 2800);
						HushArmorSystem.DPSTarget = 50;
					},
					a => StarlightWorld.squidBossArena = new Rectangle(a.X, a.Y, 109, 180)),

				new BossRushStage(
					"Structures/VitricForge",
					ModContent.NPCType<Glassweaver>(),
					new Vector2(600, 24 * 16),
					a =>
					{
						StarlightWorld.vitricBiome = new Rectangle((int)(a.X + 37 * 16) / 16, (int)(a.Y - 68 * 16) / 16, 400, 140);

						NPC.NewNPC(null, (int)a.X + 600, (int)a.Y + 24 * 16, ModContent.NPCType<Glassweaver>());

						visibleArea = new Rectangle((int)a.X, (int)a.Y + 32, 1200 - 16, 532);
						HushArmorSystem.DPSTarget = 65;
					},
					a => StarlightWorld.vitricBiome = new Rectangle(a.X + 37, a.Y - 68, 400, 140)),

				new BossRushStage(
					"Structures/VitricTempleNew",
					ModContent.NPCType<VitricBoss>(),
					new Vector2(1600, 1000),
					a =>
					{
						StarlightWorld.vitricBiome = new Rectangle((int)(a.X - 200 * 16) / 16, (int)(a.Y - 6 * 16) / 16, 400, 140);

						var dummy = Main.projectile.FirstOrDefault(n => n.active && n.ModProjectile is VitricBossAltarDummy)?.ModProjectile as VitricBossAltarDummy;

						if (Framing.GetTileSafely(dummy.ParentX, dummy.ParentY).TileFrameX < 90)
						{
							for (int x = dummy.ParentX - 2; x < dummy.ParentX + 3; x++)
							{
								for (int y = dummy.ParentY - 3; y < dummy.ParentY + 4; y++)
									Framing.GetTileSafely(x, y).TileFrameX += 90;
							}
						}

						ModContent.GetInstance<VitricBossAltar>().SpawnBoss(dummy.ParentX - 2, dummy.ParentY - 3, Main.LocalPlayer);

						visibleArea = new Rectangle((int)a.X + 1040, (int)a.Y + 60, 1520, 1064);
						HushArmorSystem.DPSTarget = 80;
					},
					a => _ = a),

				new BossRushStage(
					"Structures/BossRushEnd",
					ModContent.NPCType<BossRushGoal>(),
					new Vector2(50, 200),
					a =>
					{
						NPC.NewNPC(null, (int)a.X + 250, (int)a.Y + 200, ModContent.NPCType<BossRushGoal>());

						visibleArea = new Rectangle((int)a.X, (int)a.Y, 500, 360);
						HushArmorSystem.DPSTarget = 50;
					},
					a => _ = a),
			};
		}

		/// <summary>
		/// This generates the boss rush world. Only really needed on dev builds.
		/// </summary>
		/// <param name="tasks"></param>
		/// <param name="totalWeight"></param>
		public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
		{
			if (!isBossRush)
				return;

			//tasks.Clear();
			tasks.Add(new PassLegacy("Boss rush arena clear", (a, b) =>
			{
				for (int x = 0; x < Main.maxTilesX; x++)
				{
					for (int y = 0; y < Main.maxTilesY; y++)
					{
						Main.tile[x, y].ClearEverything();
					}
				}
			}));

			tasks.Add(new PassLegacy("Boss rush arenas", GenerateArenas));
		}

		/// <summary>
		/// This sets up the values of the boss rush world to handle things correctly (All bosses available, etc.)
		/// </summary>
		public override void PostWorldGen()
		{
			if (!isBossRush)
				return;

			Main.spawnTileX = 110;
			Main.spawnTileY = 600;

			Main.dungeonX = 1000;
			Main.dungeonY = 500;

			StarlightWorld.Flag(WorldFlags.SquidBossOpen);
			StarlightWorld.Flag(WorldFlags.VitricBossOpen);
		}

		/// <summary>
		/// Generates the actual arenas based on the stages
		/// </summary>
		/// <param name="progress"></param>
		/// <param name="configuration"></param>
		private void GenerateArenas(GenerationProgress progress, GameConfiguration configuration)
		{
			var pos = new Vector2(100, 592);
			stages.ForEach(n => n.Generate(ref pos));
		}

		/// <summary>
		/// Handles the logic of the boss rush, such as stage transitions and scoring
		/// </summary>
		public override void PostUpdateEverything()
		{
			// if were out of boss rush dont do anything!
			if (!isBossRush || currentStage > stages.Count)
				return;

			// set the difficulty
			Main.GameMode = bossRushDifficulty;

			// end the rush if the player died
			if (Main.LocalPlayer.dead)
				End();

			// decrement the score as time goes on
			scoreTimer++;

			if (scoreTimer % 20 == 0)
				score--;

			// advance the animation
			if (transitionTimer > 0)
				transitionTimer--;

			// advance the stage if the boss isnt there anymore
			if (transitionTimer <= 0 && (!NPC.AnyNPCs(trackedBossType) || trackedBossType == 0))
			{
				currentStage++;

				Heal();
				transitionTimer = 240;
			}

			// transition animation
			if (transitionTimer > 0)
			{
				Main.LocalPlayer.immune = true; //so we dont die during transitions
				Main.LocalPlayer.immuneTime = 2;

				Main.LocalPlayer.fallStart = (int)Main.LocalPlayer.position.Y; //prevent fall damage

				if (transitionTimer == 130)
				{
					if (currentStage >= stages.Count)
					{
						score += 10000; //completion bonus
						End();
						return;
					}

					CurrentStage?.EnterArena(Main.LocalPlayer);
					score += 2000;
				}

				if (transitionTimer == 120)
					CurrentStage?.BeginFight();
			}

			// star particles
			var starColor = new Color(150, Main.rand.Next(150, 255), 255)
			{
				A = 0
			};

			if (Main.rand.NextBool(10))
				stars.AddParticle(new Particle(new Vector2(Main.rand.Next(Main.screenWidth), Main.rand.Next(Main.screenHeight)), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(0.5f), 0, Main.rand.NextFloat(0.2f), starColor, 1600, Vector2.One * Main.rand.NextFloat(3f), default, 1));

			if (Main.rand.NextBool(4))
				stars.AddParticle(new Particle(new Vector2(0, Main.screenHeight * 0.2f + Main.rand.Next(-100, 200)), new Vector2(Main.rand.NextFloat(5.9f, 6.2f), 1), 0, Main.rand.NextFloat(0.2f), starColor, 600, Vector2.One * Main.rand.NextFloat(1f, 5f), default, 1));

			stars.AddParticle(new Particle(new Vector2(0, Main.screenHeight * 0.2f + Main.rand.Next(100)), new Vector2(Main.rand.NextFloat(5.9f, 6.2f), 1), 0, Main.rand.NextFloat(0.2f), starColor, 600, Vector2.One * Main.rand.NextFloat(3f, 3.3f), default, 1));
		}

		/// <summary>
		/// The update method for the stardust in the background
		/// </summary>
		/// <param name="particle"></param>
		private static void updateStars(Particle particle)
		{
			particle.Timer--;
			particle.Position += particle.Velocity;
			particle.Position.Y += (float)Math.Sin(particle.Timer * 0.015f) * particle.StoredPosition.X;

			if (particle.Timer < 30)
				particle.Alpha = particle.Timer / 30f;
		}

		/// <summary>
		/// This draws the background -over- the rest of the game
		/// </summary>
		/// <param name="orig"></param>
		/// <param name="self"></param>
		/// <param name="gameTime"></param>
		private void DrawOverlay(On_Main.orig_DrawInterface orig, Main self, GameTime gameTime)
		{
			if (!isBossRush)
			{
				orig(self, gameTime);
				return;
			}

			SpriteBatch spriteBatch = Main.spriteBatch;

			Effect mapEffect = Filters.Scene["StarMap"].GetShader().Shader;
			mapEffect.Parameters["map"].SetValue(starsMap.RenderTarget);
			mapEffect.Parameters["background"].SetValue(starsTarget.RenderTarget);

			spriteBatch.Begin(default, default, default, default, default, mapEffect, Main.GameViewMatrix.TransformationMatrix);

			spriteBatch.Draw(starsMap.RenderTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

			spriteBatch.End();

			orig(self, gameTime);
		}

		/// <summary>
		/// Draws the score
		/// </summary>
		/// <param name="spriteBatch"></param>
		public override void PostDrawInterface(SpriteBatch spriteBatch)
		{
			if (!isBossRush)
				return;

			Utils.DrawBorderString(spriteBatch, "Score: " + score, new Vector2(32, 200), Color.White);
		}

		/// <summary>
		/// Draws the background to a ScreenTarget to be used later
		/// </summary>
		/// <param name="sb"></param>
		public static void DrawStars(SpriteBatch sb)
		{
			Texture2D texB = Terraria.GameContent.TextureAssets.MagicPixel.Value;
			sb.Draw(texB, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black);

			sb.End();
			sb.Begin(default, default, SamplerState.LinearWrap, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Noise/MiscNoise1").Value;
			var color = new Color(100, 230, 220)
			{
				A = 0
			};

			var target = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);

			color.G = 160;
			color.B = 255;
			Vector2 offset = Vector2.One * Main.GameUpdateCount * 0.2f + Vector2.One.RotatedBy(Main.GameUpdateCount * 0.01f) * 20;
			var source = new Rectangle((int)offset.X, (int)offset.Y, tex.Width, tex.Height);
			sb.Draw(tex, target, source, color * 0.05f);

			color.G = 220;
			color.B = 255;
			offset = Vector2.One * Main.GameUpdateCount * 0.4f + Vector2.One.RotatedBy(Main.GameUpdateCount * 0.005f) * 36;
			source = new Rectangle((int)offset.X, (int)offset.Y, tex.Width, tex.Height);
			sb.Draw(tex, target, source, color * 0.05f);

			color.G = 255;
			color.B = 255;
			offset = Vector2.One * Main.GameUpdateCount * 0.6f + Vector2.One.RotatedBy(Main.GameUpdateCount * 0.021f) * 15;
			source = new Rectangle((int)offset.X, (int)offset.Y, tex.Width, tex.Height);
			sb.Draw(tex, target, source, color * 0.05f);

			sb.End();
			sb.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			stars.DrawParticles(sb);
		}

		/// <summary>
		/// Draws the map for where the background should appear. Has fade around the visible rectangle
		/// </summary>
		/// <param name="sb"></param>
		public static void DrawMap(SpriteBatch sb)
		{
			Texture2D tex = Terraria.GameContent.TextureAssets.MagicPixel.Value;
			Texture2D gradV = ModContent.Request<Texture2D>("StarlightRiver/Assets/GradientV").Value;
			Texture2D gradH = ModContent.Request<Texture2D>("StarlightRiver/Assets/GradientH").Value;

			Color color = Color.White;
			color.A = 0;

			float opacity = 0;

			if (transitionTimer > 210)
				opacity = 1 - (transitionTimer - 210) / 30f;
			else if (transitionTimer > 120 && transitionTimer <= 210)
				opacity = 1;
			else if (transitionTimer - 90 <= 30)
				opacity = (transitionTimer - 90) / 30f;

			sb.Draw(tex, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White * opacity);

			Vector2 pos = visibleArea.TopLeft() - Main.screenPosition;

			sb.Draw(tex, new Rectangle(0, 0, (int)pos.X, Main.screenHeight), Color.White);
			sb.Draw(gradV, new Rectangle((int)pos.X, 0, 80, Main.screenHeight), color);

			sb.Draw(tex, new Rectangle((int)(pos.X + visibleArea.Width), 0, Main.screenWidth - (int)(pos.X + visibleArea.Width), Main.screenHeight), Color.White);
			sb.Draw(gradV, new Rectangle((int)(pos.X + visibleArea.Width - 80), 0, 80, Main.screenHeight), null, color, default, default, SpriteEffects.FlipHorizontally, 0);

			sb.Draw(tex, new Rectangle(0, 0, Main.screenWidth, (int)pos.Y), Color.White);
			sb.Draw(gradH, new Rectangle(0, (int)pos.Y, Main.screenWidth, 80), null, color, default, default, SpriteEffects.FlipVertically, 0);

			sb.Draw(tex, new Rectangle(0, (int)(pos.Y + visibleArea.Height), Main.screenWidth, Main.screenHeight - (int)(pos.Y + visibleArea.Height)), Color.White);
			sb.Draw(gradH, new Rectangle(0, (int)(pos.Y + visibleArea.Height - 80), Main.screenWidth, 80), color);
		}

		/// <summary>
		/// Saves if this is a boss rush world, and if so, the arena positions
		/// </summary>
		/// <param name="tag"></param>
		public override void SaveWorldData(TagCompound tag)
		{
			tag["isBossRush"] = isBossRush;

			if (isBossRush)
			{
				for (int k = 0; k < stages.Count; k++)
				{
					var newTag = new TagCompound();
					stages[k].Save(newTag);
					tag["stage" + k] = newTag;
				}
			}

			isBossRush = false;
		}

		/// <summary>
		/// Loads data about the boss rush if applicable
		/// </summary>
		/// <param name="tag"></param>
		public override void LoadWorldData(TagCompound tag)
		{
			isBossRush = tag.GetBool("isBossRush");

			if (isBossRush)
			{
				Reset();

				for (int k = 0; k < stages.Count; k++)
				{
					TagCompound newTag = tag.Get<TagCompound>("stage" + k);

					if (newTag != null)
						stages[k].Load(newTag);
				}

				PostWorldGen(); //Cant hurt to enforce these are set correctly
			}
			else
			{
				Main.mapEnabled = true;
			}
		}
	}
}