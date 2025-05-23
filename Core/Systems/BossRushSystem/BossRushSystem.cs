using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Backgrounds;
using StarlightRiver.Content.Bosses.GlassMiniboss;
using StarlightRiver.Content.Bosses.SquidBoss;
using StarlightRiver.Content.Bosses.TheThinkerBoss;
using StarlightRiver.Content.Bosses.VitricBoss;
using StarlightRiver.Content.GUI;
using StarlightRiver.Content.Items.Permafrost;
using StarlightRiver.Content.NPCs.BossRush;
using StarlightRiver.Content.PersistentData;
using StarlightRiver.Content.Tiles.Vitric;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Loaders.UILoading;
using StarlightRiver.Core.Systems.DummyTileSystem;
using StarlightRiver.Core.Systems.PersistentDataSystem;
using StarlightRiver.Core.Systems.ScreenTargetSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;

namespace StarlightRiver.Core.Systems.BossRushSystem
{
	internal enum VisibleShape
	{
		rectangle = 0,
		circle = 1
	}

	internal class BossRushSystem : ModSystem
	{
		public const float MAX_DEATH_FADEOUT = 330f;

		public static bool isBossRush = false;

		public static int bossRushDifficulty;

		public static int currentStage = -1;
		public static int trackedBossType = 0;

		public static int scoreTimer;

		public static int killScore;
		public static int damageScore;
		public static int hitsTaken;
		public static int timeScore;
		public static int scoreMult;

		public static int transitionTimer = 0;
		public static int deathFadeoutTimer = 0;
		public static Rectangle visibleArea = new(0, 0, 0, 0);
		public static VisibleShape visibleShape = VisibleShape.rectangle;

		public static List<BossRushStage> stages;

		public static int HurtScore => hitsTaken > 0 ? hitsTaken * -100 : 10000;

		public static int Score => (killScore + damageScore + HurtScore + timeScore) * scoreMult;

		public static BossRushStage CurrentStage => (currentStage >= 0 && currentStage < stages.Count) ? stages[currentStage] : null;

		public override void Load()
		{
			StarlightRiverBackground.DrawMapEvent += DrawMap;
			StarlightRiverBackground.DrawOverlayEvent += DrawOverlay;
			StarlightRiverBackground.CheckIsActiveEvent += () => isBossRush;

			On_NPC.UpdateNPC += DisableWhenDead;

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

			killScore = 0;
			damageScore = 0;
			hitsTaken = 0;
			timeScore = 10000;
			scoreMult = bossRushDifficulty + 1;

			transitionTimer = 0;
			deathFadeoutTimer = 0;

			HushArmorSystem.resistance = 0;
			HushArmorSystem.highestDPS = 1;

			MasterDeathTicker.animationTimer = 480;
			UILoader.GetUIState<MessageBox>().Visible = false;
		}

		/// <summary>
		/// Forces a failure of the boss rush if a boss flees for example. To be called by the boss.
		/// </summary>
		public static void ForceFail()
		{
			if (isBossRush)
			{
				foreach (Player player in Main.player)
				{
					if (player.active)
					{
						player.KillMe(PlayerDeathReason.ByCustomReason($"{player.name} was swept away by the starlight river"), 9999999, 1, false);
						player.Center = Vector2.One * 32;
					}
				}
			}
		}

		/// <summary>
		/// pauses the boss rush and submits your final score, waiting for player to exit out or click retry
		/// </summary>
		public static void DeadLogic()
		{
			if (Main.GameMode == 0)
				PersistentDataStoreSystem.GetDataStore<BossRushDataStore>().normalScore = Score;
			if (Main.GameMode == 1)
				PersistentDataStoreSystem.GetDataStore<BossRushDataStore>().expertScore = Score;
			if (Main.GameMode == 2)
				PersistentDataStoreSystem.GetDataStore<BossRushDataStore>().masterScore = Score;

			PersistentDataStoreSystem.GetDataStore<BossRushDataStore>().ForceSave();

			if (deathFadeoutTimer < MAX_DEATH_FADEOUT)
			{
				deathFadeoutTimer++;
			}
			else
			{
				foreach (NPC npc in Main.npc)
				{
					npc.active = false;
				}
			}

			Main.LocalPlayer.respawnTimer = 2;
			BossRushGUIHack.inScoreScreen = true; //just means if they exit out without the button dumps them into the score screen from dead
		}

		/// <summary>
		/// Ends the boss rush and submits your final score. Returning to the main screen
		/// </summary>
		public static void End()
		{
			if (Main.GameMode == 0)
				PersistentDataStoreSystem.GetDataStore<BossRushDataStore>().normalScore = Score;
			if (Main.GameMode == 1)
				PersistentDataStoreSystem.GetDataStore<BossRushDataStore>().expertScore = Score;
			if (Main.GameMode == 2)
				PersistentDataStoreSystem.GetDataStore<BossRushDataStore>().masterScore = Score;

			PersistentDataStoreSystem.GetDataStore<BossRushDataStore>().ForceSave();

			WorldGen.SaveAndQuit();

			BossRushScore.Reset();
			BossRushGUIHack.inScoreScreen = true;
		}

		public override void PreSaveAndQuit()
		{
			UILoader.GetUIState<BossRushDeathScreen>().Visible = false;

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
		/// The actual stages are populated here. The first and last stages are bumpers for the DPS evaluation and ending item respectively.
		/// </summary>
		public override void PostAddRecipes()
		{
			stages = new List<BossRushStage>()
			{
				new(
					"Structures/ArmillarySphereRoom",
					false,
					ModContent.NPCType<BossRushOrb>(),
					new Vector2(952, 720),
					a =>
					{
						StarlightWorld.vitricBiome = new Rectangle(2000, 2000, 40, 40);
						CutawaySystem.CutawayHandler.CreateCutaways();

						NPC.NewNPC(null, (int)a.X + 952, (int)a.Y + 720, ModContent.NPCType<BossRushOrb>());
						Main.LocalPlayer.GetModPlayer<MedalPlayer>().QualifyForMedal("BossRush", 999);

						visibleArea = new Rectangle((int)a.X, (int)a.Y, 500, 360);
						visibleShape = VisibleShape.rectangle;
						HushArmorSystem.DPSTarget = 75;
					},
					a => _ = a,
					100
					),

				new(
					"Structures/SquidBossArena",
					false,
					ModContent.NPCType<SquidBoss>(),
					new Vector2(500, 1600),
					a =>
					{
						StarlightWorld.squidBossArena = new Rectangle((int)a.X / 16, (int)a.Y / 16, 109, 180);
						NPC.NewNPC(new EntitySource_WorldEvent(), StarlightWorld.squidBossArena.Center.X * 16 + 8, StarlightWorld.squidBossArena.Center.Y * 16 + 56 * 16, ModContent.NPCType<ArenaActor>());

						Item.NewItem(null, a + new Vector2(800, 2700), ModContent.ItemType<SquidBossSpawn>());

						visibleArea = new Rectangle((int)a.X, (int)a.Y + 120, 1748, 2800);
						visibleShape = VisibleShape.rectangle;
						HushArmorSystem.DPSTarget = 120;
					},
					a => StarlightWorld.squidBossArena = new Rectangle(a.X, a.Y, 109, 180)),

				new(
					"Structures/VitricForge",
					true,
					ModContent.NPCType<Glassweaver>(),
					new Vector2(600, 24 * 16),
					a =>
					{
						StructureHelper.API.MultiStructureGenerator.GenerateMultistructureSpecific("Structures/VitricForge", Main.masterMode ? 2 : Main.expertMode ? 1 : 0, (a / 16).ToPoint16(), StarlightRiver.Instance);

						StarlightWorld.vitricBiome = new Rectangle((int)(a.X + 37 * 16) / 16, (int)(a.Y - 68 * 16) / 16, 400, 140);
						CutawaySystem.CutawayHandler.CreateCutaways();
						CutawaySystem.CutawayHandler.forgeOverlay.pos = StarlightWorld.GlassweaverArena.TopLeft() + new Vector2(-2, 2) * 16;

						NPC.NewNPC(null, (int)a.X + 600, (int)a.Y + 24 * 16, ModContent.NPCType<Glassweaver>());

						visibleArea = new Rectangle((int)a.X, (int)a.Y + 32, 1200 - 16, 532);
						visibleShape = VisibleShape.rectangle;
						HushArmorSystem.DPSTarget = 175;
					},
					a => StarlightWorld.vitricBiome = new Rectangle(a.X + 37, a.Y - 68, 400, 140)),

				new(
					"Structures/VitricTempleNew",
					false,
					ModContent.NPCType<VitricBoss>(),
					new Vector2(1600, 1000),
					a =>
					{
						StarlightWorld.vitricBiome = new Rectangle((int)(a.X - 200 * 16) / 16, (int)(a.Y - 6 * 16) / 16, 400, 640);
						CutawaySystem.CutawayHandler.CreateCutaways();

						var dummy = DummySystem.dummies.FirstOrDefault(n => n.active && n is VitricBossAltarDummy) as VitricBossAltarDummy;

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
						visibleShape = VisibleShape.rectangle;
						HushArmorSystem.DPSTarget = 215;
					},
					a => _ = a),

				new(
					"Structures/ThinkerBossRushArena",
					false,
					ModContent.NPCType<TheThinker>(),
					new Vector2(16 * 55, 16 * 70),
					a =>
					{
						NPC.NewNPC(null, (int)a.X + 16 * 60 - 8, (int)a.Y + 16 * 60 - 8, ModContent.NPCType<TheThinker>());
						NPC.NewNPC(null, (int)a.X + 16 * 60, (int)a.Y + 16 * 45, ModContent.NPCType<DeadBrain>());

						visibleArea = new Rectangle((int)a.X, (int)a.Y, 1920, 1920);
						visibleShape = VisibleShape.circle;
						HushArmorSystem.DPSTarget = 300;

						StarlightWorld.Flag(WorldFlags.ThinkerBossOpen);
					},
					a => _ = a),

				new(
					"Structures/ArmillarySphereRoom",
					false,
					ModContent.NPCType<BossRushGoal>(),
					new Vector2(252, 720),
					a =>
					{
						NPC.NewNPC(null, (int)a.X + 952, (int)a.Y + 720, ModContent.NPCType<BossRushGoal>());

						visibleArea = new Rectangle((int)a.X, (int)a.Y, 2000, 1064);
						visibleShape = VisibleShape.rectangle;
						HushArmorSystem.DPSTarget = 2;
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

			tasks.Clear();
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

			Main.spawnTileX = 159;
			Main.spawnTileY = 645;

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
			{
				DeadLogic();
				return;
			}

			// decrement the score as time goes on
			scoreTimer++;

			if (scoreTimer % 20 == 0)
				timeScore--;

			// advance the animation
			if (transitionTimer > 0)
				transitionTimer--;

			// advance the stage if the boss isnt there anymore
			if (transitionTimer <= 0 && (!NPC.AnyNPCs(trackedBossType) || trackedBossType == 0))
			{
				Heal();
				transitionTimer = 240;

				if (currentStage == 0)
					transitionTimer = 180;

				if (currentStage == -1)
					transitionTimer = 0;

				currentStage++;
			}

			if (currentStage == 0 && !NPC.AnyNPCs(trackedBossType))
			{
				CurrentStage?.EnterArena(Main.LocalPlayer);
				CurrentStage?.BeginFight();
			}

			// transition animation
			if (currentStage > 0 && transitionTimer > 0)
			{
				Main.LocalPlayer.immune = true; //so we dont die during transitions
				Main.LocalPlayer.immuneTime = 2;

				Main.LocalPlayer.fallStart = (int)Main.LocalPlayer.position.Y; //prevent fall damage

				float opacity = 1;
				if (transitionTimer > 210)
					opacity = 1 - (transitionTimer - 210) / 30f;
				else if (transitionTimer > 120 && transitionTimer <= 210)
					opacity = 1;
				else if (transitionTimer - 90 <= 30)
					opacity = (transitionTimer - 90) / 30f;

				StarlightRiverBackground.starOpacity = Math.Min(1f, 1f - opacity);

				if (transitionTimer == 130)
				{
					if (currentStage >= stages.Count)
					{
						killScore += 10000; //completion bonus
						End();
						return;
					}

					CurrentStage?.EnterArena(Main.LocalPlayer);
					killScore += 2000;
				}

				if (transitionTimer == 120)
					CurrentStage?.BeginFight();
			}
		}

		private void DrawOverlay(GameTime gameTime, ScreenTarget starsMap, ScreenTarget starsTarget)
		{
			if (!isBossRush)
				return;

			SpriteBatch spriteBatch = Main.spriteBatch;

			if (currentStage != 0)
			{
				Effect mapEffect = ShaderLoader.GetShader("StarMap").Value;

				if (mapEffect != null)
				{
					mapEffect.Parameters["map"].SetValue(starsMap.RenderTarget);
					mapEffect.Parameters["background"].SetValue(starsTarget.RenderTarget);

					spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, Main.Rasterizer, mapEffect, Main.GameViewMatrix.TransformationMatrix);

					spriteBatch.Draw(starsMap.RenderTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

					spriteBatch.End();
				}
			}
		}

		/// <summary>
		/// Draws the score
		/// </summary>
		/// <param name="spriteBatch"></param>
		public override void PostDrawInterface(SpriteBatch spriteBatch)
		{
			if (!isBossRush)
				return;

			Utils.DrawBorderString(spriteBatch, "Score: " + Score, new Vector2(32, 200), Color.White);
		}

		/// <summary>
		/// Draws the map for where the background should appear. Has fade around the visible rectangle
		/// </summary>
		/// <param name="sb"></param>
		public static void DrawMap(SpriteBatch sb)
		{
			if (!isBossRush)
				return;

			Texture2D tex = Terraria.GameContent.TextureAssets.MagicPixel.Value;
			Texture2D gradV = Assets.GradientV.Value;
			Texture2D gradH = Assets.GradientH.Value;

			Color color = Color.White;
			color.A = 0;

			float opacity = 0;

			if (transitionTimer > 210)
				opacity = 1 - (transitionTimer - 210) / 30f;
			else if (transitionTimer > 120 && transitionTimer <= 210)
				opacity = 1;
			else if (transitionTimer - 90 <= 30)
				opacity = (transitionTimer - 90) / 30f;

			if (deathFadeoutTimer > 0)
				opacity = deathFadeoutTimer / MAX_DEATH_FADEOUT;

			sb.Draw(tex, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White * opacity);

			Vector2 pos = visibleArea.TopLeft() - Main.screenPosition;

			if (currentStage != 0)
			{
				switch (visibleShape)
				{
					case VisibleShape.rectangle:
						sb.Draw(tex, new Rectangle(0, 0, (int)pos.X, Main.screenHeight), Color.White);
						sb.Draw(gradV, new Rectangle((int)pos.X, 0, 80, Main.screenHeight), color);

						sb.Draw(tex, new Rectangle((int)(pos.X + visibleArea.Width), 0, Main.screenWidth - (int)(pos.X + visibleArea.Width), Main.screenHeight), Color.White);
						sb.Draw(gradV, new Rectangle((int)(pos.X + visibleArea.Width - 80), 0, 80, Main.screenHeight), null, color, default, default, SpriteEffects.FlipHorizontally, 0);

						sb.Draw(tex, new Rectangle(0, 0, Main.screenWidth, (int)pos.Y), Color.White);
						sb.Draw(gradH, new Rectangle(0, (int)pos.Y, Main.screenWidth, 80), null, color, default, default, SpriteEffects.FlipVertically, 0);

						sb.Draw(tex, new Rectangle(0, (int)(pos.Y + visibleArea.Height), Main.screenWidth, Main.screenHeight - (int)(pos.Y + visibleArea.Height)), Color.White);
						sb.Draw(gradH, new Rectangle(0, (int)(pos.Y + visibleArea.Height - 80), Main.screenWidth, 80), color);
						break;

					case VisibleShape.circle:
						Texture2D circle = Assets.Masks.GlowLarge.Value;
						Rectangle target = visibleArea;
						target.Offset((-Main.screenPosition).ToPoint());
						sb.Draw(tex, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
						sb.Draw(circle, target, Color.Black * (1f - opacity));
						break;
				}
			}
		}

		private void DisableWhenDead(On_NPC.orig_UpdateNPC orig, NPC self, int i)
		{
			if (isBossRush && Main.LocalPlayer.dead && self.boss)
				return;

			orig.Invoke(self, i);
		}

		/// <summary>
		/// Reset boss rush when the world is left
		/// </summary>
		public override void OnWorldUnload()
		{
			if (isBossRush)
				isBossRush = false;
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