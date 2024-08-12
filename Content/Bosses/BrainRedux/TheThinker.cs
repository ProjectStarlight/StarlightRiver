using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Tiles.Crimson;
using StarlightRiver.Core.Systems.LightingSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	internal class TheThinker : ModNPC
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

			if (BrainOfCthulu.TheBrain is null)
			{
				NPC.boss = false;
				NPC.Center += (home - NPC.Center) * 0.02f;

				if (ExtraRadius > -140)
					ExtraRadius--;
			}
			else
			{
				NPC.boss = true;
				Music = MusicID.Boss4;
			}

			GraymatterBiome.forceGrayMatter = true;

			Lighting.AddLight(NPC.Center, new Vector3(1f, 0.9f, 0.8f));

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
						float rot = prog * 6.28f + platformRotation;
						float targetX = (float)Math.Cos(rot) * platformRadius * 0.95f;
						float targetY = (float)Math.Sin(rot) * platformRadius;
						Vector2 target = home + new Vector2(targetX, targetY);

						platforms[k].velocity = target - platforms[k].Center;
					}
					else
					{/*TODO: Restore platforms logic*/ }
				}
			}

			// Grow radius when first phase
			if (BrainOfCthulu.TheBrain != null && BrainOfCthulu.TheBrain.State == 2)
			{
				if (ExtraRadius < 0)
					ExtraRadius++;
			}

			// Spike logic
			if (BrainOfCthulu.TheBrain != null && BrainOfCthulu.TheBrain.State >= 2)
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

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D tex = Assets.Bosses.BrainRedux.ShellBack.Value;
			Vector2 pos = home - Main.screenPosition - tex.Size() / 2f;

			LightingBufferRenderer.DrawWithLighting(pos, tex);

			if (active)
			{
				var spike = Assets.Misc.SpikeTell.Value;
				var solid = Assets.Bosses.BrainRedux.ShellSpike.Value;

				for (int k = 0; k < 36; k++)
				{
					var rot = k / 36f * 6.28f;
					Vector2 edge = home + Vector2.UnitX.RotatedBy(rot) * (hurtRadius + 50);
					spriteBatch.Draw(spike, edge - Main.screenPosition, new Rectangle(spike.Width / 2, 0, spike.Width / 2, spike.Height), new Color(255, 50, 80, 0) * 0.25f, rot - 1.57f, new Vector2(spike.Width / 4f, spike.Height), 1.5f, 0, 0);
					spriteBatch.Draw(solid, edge - Main.screenPosition, null, new Color(Lighting.GetSubLight(edge)), rot - 1.57f / 2f, solid.Size(), 1f, 0, 0);
				}

				for (int k = 0; k < 36; k++)
				{
					var rot = (k + 0.5f) / 36f * 6.28f;
					var sin = (float)Math.Sin(Main.GameUpdateCount * 0.01f + k);
					Vector2 edge = home + Vector2.UnitX.RotatedBy(rot) * (hurtRadius + 90 + sin * 40);
					spriteBatch.Draw(spike, edge - Main.screenPosition, new Rectangle(spike.Width / 2, 0, spike.Width / 2, spike.Height), new Color(255, 50, 80, 0) * 0.25f * (1f - sin + 0.5f), rot - 1.57f, new Vector2(spike.Width / 4f, spike.Height), 1.5f, 0, 0);
					spriteBatch.Draw(solid, edge - Main.screenPosition, null, new Color(Lighting.GetSubLight(edge)), rot - 1.57f / 2f, solid.Size(), 1f, 0, 0);
				}

				// TODO: Replace this with a mesh render mapping a texture to it
				for (int k = 0; k < 72; k++)
				{
					var rot = k / 72f * 6.28f;
					Vector2 edge = home + Vector2.UnitX.RotatedBy(rot) * (hurtRadius + 50);
					spriteBatch.Draw(solid, edge - Main.screenPosition, null, Lighting.GetColor((edge / 16).ToPoint()), rot + 1.57f / 2f, solid.Size() / 2f , 1.5f, 0, 0);
				}
				for (int k = 0; k < 72; k++)
				{
					var rot = k / 72f * 6.28f;
					Vector2 edge = home + Vector2.UnitX.RotatedBy(rot) * (hurtRadius + 70);
					spriteBatch.Draw(solid, edge - Main.screenPosition, null, Lighting.GetColor((edge / 16).ToPoint()), rot + 1.57f / 2f + 3.14f, solid.Size() / 2f, 1.5f, 0, 0);
				}
				for (int k = 0; k < 72; k++)
				{
					var rot = k / 72f * 6.28f;
					Vector2 edge = home + Vector2.UnitX.RotatedBy(rot) * (hurtRadius + 90);
					spriteBatch.Draw(solid, edge - Main.screenPosition, null, Lighting.GetColor((edge / 16).ToPoint()), rot + 1.57f / 2f, solid.Size() / 2f, 1.5f, 0, 0);
				}
			}

			return false;
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
			if (bodyShader is null)
				bodyShader = Terraria.Graphics.Effects.Filters.Scene["ThinkerBody"].GetShader().Shader;

			foreach (TheThinker thinker in toRender)
			{
				bodyShader.Parameters["u_resolution"].SetValue(Assets.Bosses.BrainRedux.Heart.Size());
				bodyShader.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.015f);

				bodyShader.Parameters["mainbody_t"].SetValue(Assets.Bosses.BrainRedux.Heart.Value);
				bodyShader.Parameters["linemap_t"].SetValue(Assets.Bosses.BrainRedux.HeartLine.Value);
				bodyShader.Parameters["noisemap_t"].SetValue(Assets.Noise.PerlinNoise.Value);
				bodyShader.Parameters["overlay_t"].SetValue(Assets.Bosses.BrainRedux.HeartOver.Value);
				bodyShader.Parameters["normal_t"].SetValue(Assets.Bosses.BrainRedux.HeartNormal.Value);

				sb.End();
				sb.Begin(default, default, SamplerState.PointWrap, default, default, bodyShader, Main.GameViewMatrix.TransformationMatrix);

				Texture2D tex = Assets.Bosses.BrainRedux.Heart.Value;
				sb.Draw(tex, thinker.NPC.Center - Main.screenPosition, null, Color.White, thinker.NPC.rotation, tex.Size() / 2f, thinker.NPC.scale, 0, 0);

				sb.End();
				sb.Begin(default, default, SamplerState.PointWrap, default, default, default, Main.GameViewMatrix.TransformationMatrix);
			}
		}
	}
}
