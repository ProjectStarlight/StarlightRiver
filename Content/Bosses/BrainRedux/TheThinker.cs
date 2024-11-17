using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Tiles.Crimson;
using StarlightRiver.Core.Systems.LightingSystem;
using StarlightRiver.Core.Systems.PixelationSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	internal partial class TheThinker : ModNPC
	{
		public static readonly List<TheThinker> toRender = [];
		public static Effect bodyShader;

		public bool active = false;
		public Vector2 home;

		public float platformRadius = 550;
		public float platformRotation = 0;

		public float platformRadiusTarget = 550;
		public float platformRotationTarget = 0;

		private float lastRadius = -1;
		private float lastRotation = -1;

		private int radTimer;
		private int rotTimer;

		public List<NPC> platforms = [];

		public ref float ExtraRadius => ref NPC.ai[0];

		public ref float Timer => ref NPC.ai[1];
		public ref float AttackTimer => ref NPC.ai[2];
		public ref float AttackState => ref NPC.ai[3];

		public int hurtRadius => Main.masterMode ? 700 : 750;

		public override string Texture => AssetDirectory.BrainRedux + Name;

		public override void Load()
		{
			GraymatterBiome.onDrawHallucinationMap += DrawGrayAura;
			GraymatterBiome.onDrawOverHallucinationMap += DrawShadedBody;
		}

		public override void SetStaticDefaults()
		{
			//ModContent.GetInstance<PixelationSystem>().RegisterScreenTarget("ThinkerArena", "StarlightRiver/Assets/Bosses/BrainRedux/ShellColors", RenderLayer.OverPlayers);
		}

		public override void SetDefaults()
		{
			NPC.noGravity = true;
			NPC.aiStyle = -1;
			NPC.width = 128;
			NPC.height = 128;
			NPC.damage = 10;
			NPC.lifeMax = 4000;
			NPC.knockBackResist = 0f;
			NPC.friendly = false;
			NPC.noTileCollide = true;
			NPC.hide = true;

			NPC.boss = true;

			toRender.Add(this);
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCrimson,
				new FlavorTextBestiaryInfoElement("")
			});
		}

		public override void AI()
		{
			if (home == default)
				home = NPC.Center;

			if (DeadBrain.TheBrain is null)
			{
				NPC.boss = false;
				NPC.Center += (home - NPC.Center) * 0.02f;

				if (ExtraRadius > -140)
					ExtraRadius--;
			}
			else
			{
				NPC.boss = true;
				Music = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/TheThinker");
			}

			GraymatterBiome.forceGrayMatter = true;

			Lighting.AddLight(NPC.Center, new Vector3(1f, 0.9f, 0.8f));

			if (DeadBrain.ArenaOpacity > 0.1f)
			{
				for (int k = 0; k < 200; k++)
				{
					Lighting.AddLight(home + Vector2.UnitX.RotatedBy(k / 200f * 6.28f) * hurtRadius, new Vector3(0.4f, 0.1f, 0.12f) * DeadBrain.ArenaOpacity);
				}
			}

			for (int k = 0; k < Main.maxPlayers; k++)
			{
				Player player = Main.player[k];

				if (Vector2.DistanceSquared(player.Center, NPC.Center) <= Math.Pow(140 + ExtraRadius, 2))
					player.AddBuff(ModContent.BuffType<CrimsonHallucination>(), 10);
			}

			if (active && (DeadBrain.TheBrain is null || !DeadBrain.TheBrain.NPC.active))
			{
				ResetArena();
			}

			if (platforms.Count > 0)
			{
				// Radius ease
				if (lastRadius == -1)
				{
					if (platformRadius != platformRadiusTarget)
					{
						lastRadius = platformRadius;
						radTimer = 0;
					}
				}
				else if (radTimer <= 60)
				{
					radTimer++;
					platformRadius = lastRadius + (platformRadiusTarget - lastRadius) * Helpers.Helper.BezierEase(radTimer / 60f);
				}
				else
				{
					lastRadius = -1;
				}

				// Rotation ease
				if (lastRotation == -1)
				{
					if (platformRotation != platformRotationTarget)
					{
						lastRotation = platformRotation;
						rotTimer = 0;
					}
				}
				else if (rotTimer <= 60)
				{
					rotTimer++;
					platformRotation = lastRotation + (platformRotationTarget - lastRotation) * Helpers.Helper.BezierEase(rotTimer / 60f);
				}
				else
				{
					lastRotation = -1;
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
							float aimX = (float)Math.Cos(rot) * platformRadiusTarget * 0.95f;
							float aimY = (float)Math.Sin(rot) * platformRadiusTarget;
							bp.targetPos = home + new Vector2(aimX, aimY);
						}
					}
					else
					{/*TODO: Restore platforms logic*/ }
				}
			}

			// Grow radius when first phase
			if (DeadBrain.TheBrain != null && DeadBrain.TheBrain.Phase == DeadBrain.Phases.FirstPhase)
			{
				if (ExtraRadius < 0)
					ExtraRadius++;
			}

			// Spike logic
			if (DeadBrain.TheBrain != null && DeadBrain.TheBrain.Phase >= DeadBrain.Phases.FirstPhase)
			{
				foreach (Player player in Main.ActivePlayers)
				{
					if (Vector2.Distance(player.Center, home) > hurtRadius && !player.immune)
					{
						player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " was calcified"), 50, 0);
						player.velocity += Vector2.Normalize(home - player.Center) * 28 * new Vector2(0.5f, 1f);
					}
				}
			}

			// Attacks
			if (DeadBrain.TheBrain != null && DeadBrain.TheBrain.Phase == DeadBrain.Phases.TempDead)
			{
				Timer++;
				AttackTimer++;

				NPC.Center += (home - NPC.Center) * 0.02f;

				if (ExtraRadius < 600 && Timer <= 1140)
					ExtraRadius += 4f;

				if (Timer > 1140)
					ExtraRadius -= 10;

				if (Timer >= 1200)
				{
					DeadBrain.TheBrain.Phase = DeadBrain.Phases.SecondPhase;
					DeadBrain.TheBrain.AttackState = -1;
					DeadBrain.TheBrain.AttackTimer = 1;
					DeadBrain.TheBrain.NPC.life = DeadBrain.TheBrain.NPC.lifeMax;
					DeadBrain.TheBrain.NPC.noGravity = true;
					DeadBrain.TheBrain.NPC.noTileCollide = true;
					DeadBrain.TheBrain.NPC.dontTakeDamage = false;
				}
			}
		}

		public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
		{
			if (DeadBrain.TheBrain != null && DeadBrain.TheBrain.Phase == DeadBrain.Phases.TempDead)
				return;

			modifiers.FinalDamage *= 0;
			NPC.life += 1;
			modifiers.HideCombatText();

			CombatText.NewText(NPC.Hitbox, Color.Gray, 0);
		}

		public override bool CheckDead()
		{
			NPC.Center = home;
			ResetArena();
			return false;
		}

		public override bool CheckActive()
		{
			return false;
		}

		public override bool NeedSaving()
		{
			return false;
		}

		/// <summary>
		/// Create the arena by toggling tiles as appropriate and add the arena record to the arena handler
		/// </summary>
		public void CreateArena()
		{
			List<Point16> tilesChanged = new();

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

				Main.npc[i].Center = pos;
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
			return Helpers.Helper.CheckCircularCollision(NPC.Center, 64, target.Hitbox);
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
			return false;
		}
	}
}
