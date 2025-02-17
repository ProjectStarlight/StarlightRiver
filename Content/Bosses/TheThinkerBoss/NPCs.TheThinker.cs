using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.GUI;
using StarlightRiver.Content.Items.Crimson;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Content.PersistentData;
using StarlightRiver.Content.Tiles.Crimson;
using StarlightRiver.Core.Systems.BarrierSystem;
using StarlightRiver.Core.Systems.MusicFilterSystem;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Bosses.TheThinkerBoss
{
	[AutoloadBossHead]
	internal partial class TheThinker : ModNPC
	{
		/// <summary>
		/// The brain this thinker is tied to, if any
		/// </summary>
		public NPC brain;

		/// <summary>
		/// List of entities to render by the rendering hook, since it needs to feed into the graymatter screen map
		/// </summary>
		public static readonly List<TheThinker> toRender = [];
		public static Effect bodyShader;
		public static Effect petalShader;

		public int bloomProgress;
		public float flowerRotationOnDeath;

		public bool active = false;
		public Vector2 home;

		// Fields related to the platforms movement
		public float platformRadius = 550;
		public float platformRotation = 0;

		public float platformRadiusTarget = 550;
		public float platformRotationTarget = 0;

		public int platformRadiusTransitionTime = 60;
		public int platformRotationTransitionTime = 60;

		private float lastPlatformRadius = -1;
		private float lastPlatformRotation = -1;

		private int platformRadiusTimer;
		private int platformRotationTimer;

		public List<NPC> platforms = [];

		public ref float ExtraGrayAuraRadius => ref NPC.ai[0];
		public ref float Timer => ref NPC.ai[1];
		public ref float AttackTimer => ref NPC.ai[2];
		public ref float AttackState => ref NPC.ai[3];

		public bool Open => StarlightWorld.HasFlag(WorldFlags.ThinkerBossOpen);
		public bool FightActive => ThisBrain != null;

		public bool ShouldBeAttacking => ThisBrain != null && ThisBrain.Phase == DeadBrain.Phases.TempDead;

		public int ArenaRadius => Main.masterMode ? 700 : 750;

		public override string Texture => AssetDirectory.BrainRedux + Name;

		/// <summary>
		/// Helper function to get the dead brain that this thinker is linked to
		/// </summary>
		private DeadBrain ThisBrain => brain?.ModNPC as DeadBrain;

		public override void Load()
		{
			GoreLoader.AddGoreFromTexture<SimpleModGore>(StarlightRiver.Instance, AssetDirectory.BrainRedux + "PetalBig");
			GoreLoader.AddGoreFromTexture<SimpleModGore>(StarlightRiver.Instance, AssetDirectory.BrainRedux + "PetalSmall");
			GoreLoader.AddGoreFromTexture<SimpleModGore>(StarlightRiver.Instance, AssetDirectory.BrainRedux + "Frond");

			GraymatterBiome.onDrawHallucinationMap += DrawGrayAura;
			GraymatterBiome.onDrawOverHallucinationMap += DrawShadedBody;
		}

		public override void SetDefaults()
		{
			NPC.noGravity = true;
			NPC.aiStyle = -1;
			NPC.width = 128;
			NPC.height = 128;
			NPC.damage = 10;
			NPC.lifeMax = 5500;
			NPC.knockBackResist = 0f;
			NPC.friendly = false;
			NPC.noTileCollide = true;
			NPC.hide = true;

			NPC.boss = true;

			toRender.Add(this);
		}

		public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
		{
			if (Main.expertMode)
				NPC.lifeMax = 7000;

			if (Main.masterMode)
				NPC.lifeMax = 9500;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCrimson,
				new FlavorTextBestiaryInfoElement("The strongest thoughts and wills of the enigmatic Brain of Cthulhu have escaped after it's death, growing and forming it's own physical form. It desperately attempts to restore it's progenitor, while wielding it's progress against those that attempt to interfere.")
			});
		}

		public override void BossHeadSlot(ref int index)
		{
			index = active ? index : -1;
		}

		/// <summary>
		/// Attempts to switch the boss bar to the specified entity, for when we need to swap between this and the brain
		/// </summary>
		/// <param name="npc">The entity to track</param>
		public void SwitchBossBar(NPC npc)
		{
			Main.BigBossProgressBar.TryTracking(npc.whoAmI);
			BossBarOverlay.SetTracked(npc);
		}

		public override void AI()
		{
			if (home == default)
				home = NPC.Center;

			if (Open)
				Lighting.AddLight(NPC.Center, new Vector3(1f, 0.9f, 0.8f));

			if (pulseTime > 0)
				pulseTime--;

			if (!FightActive)
			{
				NPC.boss = false;
				Music = default;
				NPC.Center += (home - NPC.Center) * 0.02f;

				// Dust towards assembling brain
				if (Main.rand.NextBool(7))
				{
					SplineGlow.Spawn(NPC.Center, NPC.Center + new Vector2(Main.rand.NextFloat(-150, 150), Main.rand.Next(100, 150)), NPC.Center + new Vector2(0, 250) + Main.rand.NextVector2Circular(100, 100), Main.rand.Next(120, 240), Main.rand.NextFloat(0.1f, 0.25f), new Color(155, Main.rand.Next(20, 50), 50));
				}

				if (ExtraGrayAuraRadius > -140)
					ExtraGrayAuraRadius = -140;

				return;
			}
			else
			{
				NPC.boss = true;
				Music = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/TheThinker");

				if (ThisBrain.Phase >= DeadBrain.Phases.SecondPhase)
				{
					Music = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/TheThinker");
				}

				if (ThisBrain.Phase == DeadBrain.Phases.FirstPhase && NPC.life > NPC.lifeMax / 2)
				{
					BossBarOverlay.forceInvulnerabilityVisuals = false;
				}
				else
				{
					BossBarOverlay.forceInvulnerabilityVisuals = null;
				}

				if (ThisBrain.Phase == DeadBrain.Phases.SecondPhase)
				{
					SwitchBossBar(ThisBrain.NPC);
				}
				else
				{
					SwitchBossBar(NPC);
				}
			}

			GraymatterBiome.forceGrayMatter = true;

			if (ArenaOpacity > 0.1f)
			{
				for (int k = 0; k < 200; k++)
				{
					Lighting.AddLight(home + Vector2.UnitX.RotatedBy(k / 200f * 6.28f) * ArenaRadius, new Vector3(0.4f, 0.1f, 0.12f) * ArenaOpacity);
				}
			}

			for (int k = 0; k < Main.maxPlayers; k++)
			{
				Player player = Main.player[k];

				if (Vector2.DistanceSquared(player.Center, NPC.Center) <= Math.Pow(140 + ExtraGrayAuraRadius, 2))
					player.AddBuff(ModContent.BuffType<CrimsonHallucination>(), 10);
			}

			if (active && (ThisBrain is null || !ThisBrain.NPC.active))
			{
				ResetArena();
			}

			if (platforms.Count > 0)
			{
				// Radius ease
				if (lastPlatformRadius == -1)
				{
					if (platformRadius != platformRadiusTarget)
					{
						lastPlatformRadius = platformRadius;
						platformRadiusTimer = 0;
					}
				}
				else if (platformRadiusTimer <= platformRadiusTransitionTime)
				{
					platformRadiusTimer++;
					platformRadius = lastPlatformRadius + (platformRadiusTarget - lastPlatformRadius) * Helpers.Eases.BezierEase(platformRadiusTimer / (float)platformRadiusTransitionTime);
				}
				else
				{
					platformRadiusTarget = platformRadius;
					lastPlatformRadius = -1;
				}

				// Rotation ease
				if (lastPlatformRotation == -1)
				{
					if (platformRotation != platformRotationTarget)
					{
						lastPlatformRotation = platformRotation;
						platformRotationTimer = 0;
					}
				}
				else if (platformRotationTimer <= platformRotationTransitionTime)
				{
					platformRotationTimer++;
					platformRotation = lastPlatformRotation + (platformRotationTarget - lastPlatformRotation) * Helpers.Eases.BezierEase(platformRotationTimer / (float)platformRotationTransitionTime);
				}
				else
				{
					platformRotationTarget = platformRotation;
					lastPlatformRotation = -1;
				}

				// Set final positions for this frame
				for (int k = 0; k < platforms.Count; k++)
				{
					float prog = k / (float)platforms.Count;

					if (platforms[k].active && platforms[k].type == ModContent.NPCType<BrainPlatform>())
					{
						float rot = prog * 6.28f + platformRotation;
						float targetX = (float)Math.Cos(rot) * platformRadius * 0.95f;
						float targetY = (float)Math.Sin(rot) * platformRadius;
						Vector2 target = home + new Vector2(targetX, targetY);

						platforms[k].velocity = target - platforms[k].Center;

						if (platforms[k].ModNPC is BrainPlatform bp)
						{
							float rotAim = prog * 6.28f + platformRotationTarget;
							float aimX = (float)Math.Cos(rotAim) * platformRadiusTarget * 0.95f;
							float aimY = (float)Math.Sin(rotAim) * platformRadiusTarget;
							bp.targetPos = home + new Vector2(aimX, aimY);

							if (platformRotationTimer <= platformRotationTransitionTime)
							{
								float rotProg = platformRotationTimer / (float)platformRotationTransitionTime;
								bp.glow = MathF.Min(1, MathF.Sin(rotProg * 3.14f) * 2);
							}
							else
							{
								bp.glow = 0;
							}
						}
					}
					else
					{/*TODO: Restore platforms logic*/ }
				}
			}

			// Grow radius when first phase
			if (ThisBrain != null && ThisBrain.Phase == DeadBrain.Phases.FirstPhase)
			{
				if (ExtraGrayAuraRadius < 0)
					ExtraGrayAuraRadius++;
			}

			// Spike logic
			if (ThisBrain != null && ThisBrain.Phase >= DeadBrain.Phases.FirstPhase)
			{
				foreach (Player player in Main.ActivePlayers)
				{
					float dist = Vector2.Distance(player.Center, home);

					if (dist > ArenaRadius && dist < (ArenaRadius + 200) && !player.immune)
					{
						player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " was calcified"), 50, 0);
						player.velocity += Vector2.Normalize(home - player.Center) * 28 * new Vector2(0.5f, 1f);
					}
				}
			}

			// Attacks
			if (ShouldBeAttacking)
			{

				Timer++;
				AttackTimer++;

				if (bloomProgress < 150)
					bloomProgress++;

				NPC.Center += (home - NPC.Center) * 0.02f;

				if (Timer == 1)
				{
					platformRadiusTarget = 1000;
					platformRadiusTransitionTime = 240;

					NPC.GetGlobalNPC<BarrierNPC>().maxBarrier = Main.masterMode ? 750 : Main.expertMode ? 600 : 500;
				}

				if (Timer < 60)
				{
					int maxBarrier = Main.masterMode ? 750 : Main.expertMode ? 600 : 500;
					NPC.GetGlobalNPC<BarrierNPC>().barrier = (int)(Timer / 60f * maxBarrier);
				}

				if (Timer == 60)
				{
					NPC.immortal = false;
				}

				if (Timer < 60)
				{
					MusicFilterSystem.globalPitchModifier = Timer / 60f * -0.5f;
				}
				else if (Timer >= 60 && Timer < 1100)
				{
					MusicFilterSystem.globalPitchModifier = -0.5f;
				}
				else if (Timer >= 1100)
				{
					MusicFilterSystem.globalPitchModifier = -0.5f + (Timer - 1100) / 100f * 0.5f;
				}

				if (ExtraGrayAuraRadius < 600 && Timer <= 1140)
					ExtraGrayAuraRadius += 4f;

				if (Timer == 1)
				{
					SpawnBlock(3, -2);
					SpawnBlock(3, 2);
				}

				if (Timer % (194 / 3) == 0)
				{
					SpawnBlock();
				}

				if (Timer > 100 && Timer < 900 && Timer % 60 == 0)
					SpawnProjectile();

				if (Main.expertMode && Timer > 100 && Timer < 1100 && Timer % 300 == 0)
				{
					for (int k = 0; k < 8; k++)
					{
						float rot = k / 8f * 6.28f;
						rot += Timer / 200f;
						Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.One.RotatedBy(rot) * 3, ModContent.ProjectileType<BrainBolt>(), ThisBrain.BrainBoltDamage, 1, Main.myPlayer, 240, 0, 10);
					}
				}

				if (Timer > 1140)
				{
					ExtraGrayAuraRadius -= 10;
					NPC.GetGlobalNPC<BarrierNPC>().maxBarrier = 0;
				}

				if (Timer == 1200)
				{
					platformRadiusTarget = 400;
					platformRadiusTransitionTime = 60;

					foreach (NPC npc in Main.ActiveNPCs)
					{
						if (npc.ModNPC is HallucinationBlock hb)
						{
							hb.Timer = 500;
						}
					}
				}

				if (Timer >= 1200)
				{
					ThisBrain.Phase = DeadBrain.Phases.SecondPhase;
					ThisBrain.AttackState = -1;
					ThisBrain.AttackTimer = 1;
					ThisBrain.NPC.life = ThisBrain.NPC.lifeMax / 3;
					ThisBrain.NPC.noGravity = true;
					ThisBrain.NPC.noTileCollide = true;
					ThisBrain.NPC.dontTakeDamage = false;
				}
			}
			else
			{
				if (bloomProgress > 0)
					bloomProgress--;

				NPC.immortal = true;
			}
		}

		/// <summary>
		/// Spawns a cube traveling in the given direction and at the given offset (in cube sizes) from the center.
		/// </summary>
		/// <param name="direction">0 = left to right, 1 = right to left, 2 = top to bottom, 3 = bottom to top</param>
		/// <param name="offset">offset in cube sizes from the center, 0 is at the center</param>
		public void SpawnBlock(int direction = -1, int offset = -99)
		{
			int n;

			if (direction == -1 || direction > 3)
				direction = Main.rand.Next(4);

			if (offset == -99 || offset < -3 || offset > 3)
				offset = Main.rand.Next(-3, 4);

			while (Math.Abs(offset) <= 1)
			{
				offset = Main.rand.Next(-4, 5);
			}

			switch (direction)
			{
				case 0:
					n = NPC.NewNPC(NPC.GetSource_FromThis(), (int)home.X - 776, (int)home.Y + offset * 194, ModContent.NPCType<HallucinationBlock>());
					Main.npc[n].velocity.X = 3;
					break;
				case 1:
					n = NPC.NewNPC(NPC.GetSource_FromThis(), (int)home.X + 776, (int)home.Y + offset * 194, ModContent.NPCType<HallucinationBlock>());
					Main.npc[n].velocity.X = -3;
					break;
				case 2:
					n = NPC.NewNPC(NPC.GetSource_FromThis(), (int)home.X + offset * 194, (int)home.Y - 776, ModContent.NPCType<HallucinationBlock>());
					Main.npc[n].velocity.Y = 3;
					break;
				case 3:
					n = NPC.NewNPC(NPC.GetSource_FromThis(), (int)home.X + offset * 194, (int)home.Y + 776, ModContent.NPCType<HallucinationBlock>());
					Main.npc[n].velocity.Y = -3;
					break;
			}
		}

		public void SpawnProjectile(int direction = -1, int offset = -99)
		{
			int n;

			if (direction == -1 || direction > 3)
				direction = Main.rand.Next(4);

			if (offset == -99 || offset < -3 || offset > 3)
				offset = Main.rand.Next(-3, 4);

			switch (direction)
			{
				case 0:
					Projectile.NewProjectile(NPC.GetSource_FromThis(), (int)home.X - 800, (int)home.Y + offset * 194, 5, 0, ModContent.ProjectileType<HallucinationHazard>(), 30, 1, Main.myPlayer);
					break;
				case 1:
					Projectile.NewProjectile(NPC.GetSource_FromThis(), (int)home.X + 800, (int)home.Y + offset * 194, -5, 0, ModContent.ProjectileType<HallucinationHazard>(), 30, 1, Main.myPlayer);
					break;
				case 2:
					Projectile.NewProjectile(NPC.GetSource_FromThis(), (int)home.X + offset * 194, (int)home.Y - 800, 0, 5, ModContent.ProjectileType<HallucinationHazard>(), 30, 1, Main.myPlayer);
					break;
				case 3:
					Projectile.NewProjectile(NPC.GetSource_FromThis(), (int)home.X + offset * 194, (int)home.Y + 800, 0, -5, ModContent.ProjectileType<HallucinationHazard>(), 30, 1, Main.myPlayer);
					break;
			}
		}

		public override bool? CanBeHitByItem(Player player, Item item)
		{
			return active ? null : false;
		}

		public override bool? CanBeHitByProjectile(Projectile projectile)
		{
			if (Open && !active && projectile.Hitbox.Intersects(NPC.Hitbox) && projectile.ModProjectile is BearPokerProjectile)
			{
				DeadBrain.SpawnReduxedBrain(NPC.Center + new Vector2(0, 200));
				return true;
			}

			return active ? null : false;
		}

		public override bool CanBeHitByNPC(NPC attacker)
		{
			return active;
		}

		public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
		{
			if (ThisBrain != null && ThisBrain.Phase == DeadBrain.Phases.TempDead)
				return;

			modifiers.HideCombatText();
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			if (ThisBrain != null && ThisBrain.Phase == DeadBrain.Phases.TempDead)
				return;

			CombatText.NewText(NPC.Hitbox, Color.Gray, 0);
		}

		public override bool CheckDead()
		{
			if (ThisBrain != null && ThisBrain.Phase != DeadBrain.Phases.ReallyDead)
			{
				NPC.Center = home;

				ThisBrain.Phase = DeadBrain.Phases.ReallyDead;
				ThisBrain.Timer = 0;

				NPC.immortal = true;
				NPC.dontTakeDamage = true;
				NPC.life = 1;
				return false;
			}
			else
			{
				BossRushDataStore.DefeatBoss(BossrushUnlockFlag.Thinker);
				ResetArena();
				return true;
			}
		}

		public override bool CheckActive()
		{
			return false;
		}

		/// <summary>
		/// Create the arena by toggling tiles as appropriate and add the arena record to the arena handler
		/// </summary>
		public void CreateArena()
		{
			List<Point16> tilesChanged = [];

			for (int x = -54; x <= 54; x++)
			{
				for (int y = -54; y <= 54; y++)
				{
					var off = new Vector2(x, y);
					float dist = off.LengthSquared();

					if (dist <= Math.Pow(50, 2))
					{
						Tile tile = Main.tile[(int)home.X / 16 + x, (int)home.Y / 16 + y];

						tile.LiquidAmount = 0;

						if (tile.HasTile && !tile.IsActuated)
						{
							tile.IsActuated = true;
							tilesChanged.Add(new Point16(x, y));
						}
					}

					if (dist > Math.Pow(50, 2) && dist <= Math.Pow(54, 2))
					{
						Tile tile = Main.tile[(int)home.X / 16 + x, (int)home.Y / 16 + y];

						if (!tile.HasTile)
						{
							tile.HasTile = true;
							tile.TileType = (ushort)ModContent.TileType<BrainBlocker>();
							tile.Slope = Terraria.ID.SlopeType.Solid;
							WorldGen.TileFrame((int)home.X / 16 + x, (int)home.Y / 16 + y);
							tilesChanged.Add(new Point16(x, y));
						}
					}
				}
			}

			for (int k = 0; k < 12; k++)
			{
				Vector2 pos = home + Vector2.UnitX.RotatedBy(k / 12f * 6.28f) * 550;
				int i = NPC.NewNPC(null, (int)pos.X, (int)pos.Y, ModContent.NPCType<BrainPlatform>());

				NPC newPlat = Main.npc[i];

				newPlat.Center = pos;
				(newPlat.ModNPC as BrainPlatform).thinker = NPC;

				platforms.Add(Main.npc[i]);
			}

			active = true;

			ModContent.GetInstance<ThinkerArenaSafetySystem>().records.Add(new(NPC, home, tilesChanged));
		}

		/// <summary>
		/// Tell the arena handler to reset the arena and remove its record from the save/load safety
		/// </summary>
		public void ResetArena()
		{
			ModContent.GetInstance<ThinkerArenaSafetySystem>().ResetArena(NPC);
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			return Helpers.CollisionHelper.CheckCircularCollision(NPC.Center, 45, target.Hitbox);
		}

		public override void SaveData(TagCompound tag)
		{
			tag.Add("active", active);
			tag.Add("home", home);
		}

		public override void LoadData(TagCompound tag)
		{
			active = tag.GetBool("active");
			home = tag.Get<Vector2>("home");
		}

		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
		{
			return ThisBrain != null && ThisBrain.Phase == DeadBrain.Phases.TempDead;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<ThinkerBossBag>()));

			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Tiles.Trophies.ThinkerTrophyItem>(), 10, 1, 1));

			npcLoot.Add(ItemDropRule.MasterModeCommonDrop(Mod.Find<ModItem>("ThinkerRelicItem").Type));

			LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());

			notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<ImaginaryTissue>(), 1, 30, 40));
			notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<DendriteItem>(), 1, 80, 120));
		}

		public override void OnKill()
		{
			for (int k = 0; k < 10; k++)
			{
				Main.BestiaryTracker.Kills.RegisterKill(brain);
			}
		}
	}
}