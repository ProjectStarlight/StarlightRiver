using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Items.Hell;
using StarlightRiver.Content.Tiles.Crimson;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	internal class TheThinker : ModNPC
	{
		public static readonly List<TheThinker> toRender = new();

		public bool active = false;
		public List<Point16> tilesChanged = new();
		public Vector2 home;

		public float platformRadius = 550;
		public float platformRotation = 0;

		public float platformRadiusTarget = 550;
		public float platformRotationTarget = 0;

		private float lastRadius = -1;
		private float lastRotation = -1;

		private int radTimer;
		private int rotTimer;

		public List<NPC> platforms = new();

		public ref float ExtraRadius => ref NPC.ai[0];

		public ref float Timer => ref NPC.ai[1];
		public ref float AttackTimer => ref NPC.ai[2];
		public ref float AttackState => ref NPC.ai[3];

		public override string Texture => AssetDirectory.BrainRedux + Name;

		public override void Load()
		{
			GraymatterBiome.onDrawHallucinationMap += DrawAura;
			GraymatterBiome.onDrawOverHallucinationMap += DrawMe;
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
			NPC.boss = true;

			Music = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/WhipAndNaenae");

			toRender.Add(this);
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCrimson,
				new FlavorTextBestiaryInfoElement("An incredibly dense collection of gray matter, this strange entity sits waiting for it's second half to emerge from hiding. Despite what it is, the Brain of Cthulhu seems to act rather mindlessly without this... thing... to think for it...")
			});
		}

		public override void AI()
		{
			if (home == default)
				home = NPC.Center;

			if (BrainOfCthulu.TheBrain is null)
			{
				NPC.Center += (home - NPC.Center) * 0.02f;
			}

			GraymatterBiome.forceGrayMatter = true;

			Lighting.AddLight(NPC.Center, new Vector3(1f, 0.2f, 0.2f));

			for (int k = 0; k < Main.maxPlayers; k++)
			{
				Player player = Main.player[k];

				if (Vector2.DistanceSquared(player.Center, NPC.Center) <= Math.Pow(140 + ExtraRadius, 2))
					player.AddBuff(ModContent.BuffType<CrimsonHallucination>(), 10);
			}

			if (active && (NPC.crimsonBoss < 0 || !Main.npc[NPC.crimsonBoss].active))
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
						var target = home + Vector2.UnitX.RotatedBy(prog * 6.28f + platformRotation) * platformRadius;
						platforms[k].velocity = target - platforms[k].Center;
						//platforms[k].Center = target;
					}
					else
					{/*TODO: Restore platforms logic*/ }					
				}
			}

			// Attacks
			if (BrainOfCthulu.TheBrain != null && BrainOfCthulu.TheBrain.State == 5)
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
					BrainOfCthulu.TheBrain.State = 3;
					BrainOfCthulu.TheBrain.AttackState = -1;
					BrainOfCthulu.TheBrain.AttackTimer = 1;
					BrainOfCthulu.TheBrain.npc.life = BrainOfCthulu.TheBrain.npc.lifeMax;
					BrainOfCthulu.TheBrain.npc.noGravity = true;
					BrainOfCthulu.TheBrain.npc.noTileCollide = true;
					BrainOfCthulu.TheBrain.npc.dontTakeDamage = false;
				}
			}
		}

		public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
		{
			if (BrainOfCthulu.TheBrain != null && BrainOfCthulu.TheBrain.State == 5)
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
			return true;
		}

		public void CreateArena()
		{
			for (int x = -60; x <= 60; x++)
			{
				for (int y = -60; y <= 60; y++)
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

					if (dist > Math.Pow(50, 2) && dist <= Math.Pow(60, 2))
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
				Vector2 pos = NPC.Center + Vector2.UnitX.RotatedBy(k / 12f * 6.28f) * 550;
				int i = NPC.NewNPC(null, (int)pos.X, (int)pos.Y, ModContent.NPCType<BrainPlatform>());

				Main.npc[i].Center = pos;
				platforms.Add(Main.npc[i]);
			}

			active = true;
		}

		public void ResetArena()
		{
			foreach (Point16 point in tilesChanged)
			{
				Tile tile = Main.tile[(int)home.X / 16 + point.X, (int)home.Y / 16 + point.Y];

				if (tile.IsActuated)
					tile.IsActuated = false;

				if (tile.TileType == ModContent.TileType<BrainBlocker>())
					tile.HasTile = false;
			}

			foreach (NPC npc in Main.npc.Where(n => n.active && n.type == ModContent.NPCType<BrainPlatform>()))
			{
				npc.active = false;
			}

			tilesChanged.Clear();
			active = false;
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			return Helpers.Helper.CheckCircularCollision(NPC.Center, 64, target.Hitbox);
		}

		public override void SaveData(TagCompound tag)
		{
			tag.Add("active", active);
			tag.Add("tiles", tilesChanged);
			tag.Add("home", home);
		}

		public override void LoadData(TagCompound tag)
		{
			active = tag.GetBool("active");
			tilesChanged = tag.GetList<Point16>("tiles") as List<Point16>;
			home = tag.Get<Vector2>("home");
		}

		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
		{
			return false;
		}

		private void DrawAura(SpriteBatch sb)
		{
			Texture2D glow = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowAlpha").Value;
			Color color = Color.White;
			color.A = 0;

			foreach (TheThinker thinker in toRender)
			{
				for (int k = 0; k < 8; k++)
				{
					sb.Draw(glow, thinker.NPC.Center - Main.screenPosition, null, color, 0, glow.Size() / 2f, (140 + thinker.ExtraRadius) * 4 / glow.Width, 0, 0);
				}
			}

			toRender.RemoveAll(n => n is null || !n.NPC.active);
		}

		private void DrawMe(SpriteBatch sb)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

			foreach (TheThinker thinker in toRender)
			{
				sb.Draw(tex, thinker.NPC.Center - Main.screenPosition, null, Color.White, 0, tex.Size() / 2f, 1, 0, 0);
			}
		}
	}
}
