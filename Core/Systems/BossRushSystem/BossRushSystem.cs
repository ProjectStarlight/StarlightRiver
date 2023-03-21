using StarlightRiver.Content.Bosses.GlassMiniboss;
using StarlightRiver.Content.Bosses.SquidBoss;
using StarlightRiver.Content.Bosses.VitricBoss;
using StarlightRiver.Content.Items.Permafrost;
using StarlightRiver.Content.Tiles.Vitric;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;

namespace StarlightRiver.Core.Systems.BossRushSystem
{
	internal class BossRushSystem : ModSystem
	{
		public bool isBossRush = true;

		public int currentStage = -1;
		public int trackedBossType = 0;

		public int scoreTimer;
		public int score;

		public int transitionTimer = 0;

		public List<BossRushStage> stages;

		public BossRushStage CurrentStage => (currentStage >= 0 && currentStage < stages.Count) ? stages[currentStage] : null;

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
					a => Item.NewItem(null, a + new Vector2(800, 2700), ModContent.ItemType<SquidBossSpawn>()),
					a => StarlightWorld.squidBossArena = new Rectangle(a.X, a.Y, 109, 180)),

				new BossRushStage(
					"Structures/VitricForge",
					ModContent.NPCType<Glassweaver>(),
					new Vector2(600, 24 * 16),
					a =>
					{
						StarlightWorld.vitricBiome = new Rectangle((int)(a.X + 37 * 16) / 16, (int)(a.Y - 68 * 16) / 16, 400, 140);

						NPC.NewNPC(null, (int)a.X + 600, (int)a.Y + 24 * 16, ModContent.NPCType<Glassweaver>());
					},
					a => StarlightWorld.vitricBiome = new Rectangle(a.X + 37, a.Y - 68, 400, 140)),

				new BossRushStage(
					"Structures/VitricTempleNew",
					ModContent.NPCType<VitricBoss>(),
					new Vector2(1300, 400),
					a =>
					{
						StarlightWorld.vitricBiome = new Rectangle((int)(a.X - 200 * 16) / 16, (int)(a.Y - 69 * 16) / 16, 400, 140);

						var dummy = Main.projectile.FirstOrDefault(n => n.active && n.ModProjectile is VitricBossAltarDummy)?.ModProjectile as VitricBossAltarDummy;
						ModContent.GetInstance<VitricBossAltar>().SpawnBoss(dummy.ParentX - 2, dummy.ParentY - 3, Main.LocalPlayer);
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
		}

		public override void PostDrawInterface(SpriteBatch spriteBatch)
		{
			Texture2D tex = Terraria.GameContent.TextureAssets.MagicPixel.Value;
			spriteBatch.Draw(tex, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White * (float)Math.Sin(transitionTimer / 240f * 3.14f));
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
