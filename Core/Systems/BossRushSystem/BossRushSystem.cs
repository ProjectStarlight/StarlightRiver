using StarlightRiver.Content.Bosses.GlassMiniboss;
using StarlightRiver.Content.Bosses.SquidBoss;
using StarlightRiver.Content.Bosses.VitricBoss;
using StarlightRiver.Content.Items.Permafrost;
using StarlightRiver.Content.Tiles.Vitric;
using StarlightRiver.Core.Systems.ScreenTargetSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.Generation;
using Terraria.Graphics.Effects;
using Terraria.IO;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;

namespace StarlightRiver.Core.Systems.BossRushSystem
{
	internal class BossRushSystem : ModSystem
	{
		public static bool isBossRush = true;

		public static int currentStage = -1;
		public static int trackedBossType = 0;

		public static int scoreTimer;
		public static int score;

		public static int transitionTimer = 0;
		public static Rectangle visibleArea = new(0, 0, 0, 0);

		public static List<BossRushStage> stages;

		public static ScreenTarget starsTarget = new(DrawStars, () => isBossRush, 1f);
		public static ScreenTarget starsMap = new(DrawMap, () => isBossRush, 1f);

		public static ParticleSystem stars = new("StarlightRiver/Assets/Misc/DotTell", updateStars);

		public static BossRushStage CurrentStage => (currentStage >= 0 && currentStage < stages.Count) ? stages[currentStage] : null;

		public override void Load()
		{
			On.Terraria.Main.DrawInterface += DrawOverlay;
		}

		public void Reset()
		{
			currentStage = 0;
			scoreTimer = 0;
			score = 0;

			transitionTimer = 0;
		}

		public override void PostAddRecipes()
		{
			stages = new List<BossRushStage>()
			{
				new BossRushStage(
					"Structures/SquidBossArena",
					ModContent.NPCType<SquidBoss>(),
					new Vector2(500, 1600),
					a =>
					{
						Item.NewItem(null, a + new Vector2(800, 2700), ModContent.ItemType<SquidBossSpawn>());

						visibleArea = new Rectangle((int)a.X, (int)a.Y + 120, 1764, 2800);
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

						visibleArea = new Rectangle((int)a.X, (int)a.Y, 1200 - 16, 800 - 48);
						HushArmorSystem.DPSTarget = 65;
					},
					a => StarlightWorld.vitricBiome = new Rectangle(a.X + 37, a.Y - 68, 400, 140)),

				new BossRushStage(
					"Structures/VitricTempleNew",
					ModContent.NPCType<VitricBoss>(),
					new Vector2(1300, 400),
					a =>
					{
						StarlightWorld.vitricBiome = new Rectangle((int)(a.X - 200 * 16) / 16, (int)(a.Y - 6 * 16) / 16, 400, 140);

						var dummy = Main.projectile.FirstOrDefault(n => n.active && n.ModProjectile is VitricBossAltarDummy)?.ModProjectile as VitricBossAltarDummy;
						ModContent.GetInstance<VitricBossAltar>().SpawnBoss(dummy.ParentX - 2, dummy.ParentY - 3, Main.LocalPlayer);

						visibleArea = new Rectangle((int)a.X + 1040, (int)a.Y + 60, 1520, 1064);
						HushArmorSystem.DPSTarget = 80;
					},
					a => _ = a),
			};
		}

		public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight)
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

		public override void PostWorldGen()
		{
			if (!isBossRush)
				return;

			Main.spawnTileX = 150;
			Main.spawnTileY = 106;

			Main.dungeonX = 1000;
			Main.dungeonY = 500;

			StarlightWorld.Flag(WorldFlags.SquidBossOpen);
		}

		private void GenerateArenas(GenerationProgress progress, GameConfiguration configuration)
		{
			var pos = new Vector2(100, 592);
			stages.ForEach(n => n.Generate(ref pos));
		}

		public override void PostUpdateEverything()
		{
			if (Main.LocalPlayer.controlHook)
			{
				foreach (NPC npc in Main.npc)
					npc.active = false;

				currentStage = 0;
			}

			if (!isBossRush || currentStage > stages.Count)
				return;

			scoreTimer++;

			if (transitionTimer > 0)
				transitionTimer--;

			if (transitionTimer <= 0 && (!NPC.AnyNPCs(trackedBossType) || trackedBossType == 0))
			{
				currentStage++;

				transitionTimer = 240;

				if (currentStage > stages.Count)
				{
					// go to jade room
				}
			}

			// transition animation
			if (transitionTimer > 0)
			{
				if (transitionTimer == 130)
					CurrentStage?.EnterArena(Main.LocalPlayer);

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

		private static void updateStars(Particle particle)
		{
			particle.Timer--;
			particle.Position += particle.Velocity;
			particle.Position.Y += (float)Math.Sin(particle.Timer * 0.015f) * particle.StoredPosition.X;

			if (particle.Timer < 30)
				particle.Alpha = particle.Timer / 30f;
		}

		private void DrawOverlay(On.Terraria.Main.orig_DrawInterface orig, Main self, GameTime gameTime)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;

			Effect mapEffect = Filters.Scene["StarMap"].GetShader().Shader;
			mapEffect.Parameters["map"].SetValue(starsMap.RenderTarget);
			mapEffect.Parameters["background"].SetValue(starsTarget.RenderTarget);

			//spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, mapEffect, Main.GameViewMatrix.TransformationMatrix);

			spriteBatch.Draw(starsMap.RenderTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

			spriteBatch.End();
			//spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);

			orig(self, gameTime);
		}

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

		public static void DrawMap(SpriteBatch sb)
		{
			Texture2D tex = Terraria.GameContent.TextureAssets.MagicPixel.Value;
			Texture2D gradV = ModContent.Request<Texture2D>("StarlightRiver/Assets/GradientV").Value;
			Texture2D gradH = ModContent.Request<Texture2D>("StarlightRiver/Assets/GradientH").Value;

			Color color = Color.White;
			color.A = 0;

			sb.Draw(tex, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White * (float)Math.Sin(transitionTimer / 240f * 3.14f));

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

		public override void SaveWorldData(TagCompound tag)
		{
			if (isBossRush)
			{
				for (int k = 0; k < stages.Count; k++)
				{
					var newTag = new TagCompound();
					stages[k].Save(newTag);
					tag["stage" + k] = newTag;
				}
			}
		}

		public override void LoadWorldData(TagCompound tag)
		{
			if (isBossRush)
			{
				for (int k = 0; k < stages.Count; k++)
				{
					TagCompound newTag = tag.Get<TagCompound>("stage" + k);

					if (newTag != null)
						stages[k].Load(newTag);
				}
			}
		}
	}
}
