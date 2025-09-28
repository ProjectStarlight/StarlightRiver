using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems.PixelationSystem;
using System;
using System.IO;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.TheThinkerBoss
{
	internal class Neurysm : ModNPC
	{
		/// <summary>
		/// The dead brain this minion is attached to
		/// </summary>
		public NPC brain;

		public float opacity;
		public float tellDirection;
		public float tellLen;
		public float hurtTime;

		public Vector2 tellStart;

		/// <summary>
		/// If this minion should use colored or red trails
		/// </summary>
		public bool AlternateAppearance => (ThisBrain?.Phase ?? 0) >= DeadBrain.Phases.SecondPhase;

		/// <summary>
		/// Helper property to get the ModNPC instance of the tied brain
		/// </summary>
		private DeadBrain ThisBrain => brain?.ModNPC as DeadBrain;

		public ref float Timer => ref NPC.ai[0];
		public ref float State => ref NPC.ai[1];
		public ref float TellTime => ref NPC.ai[3];

		public override string Texture => AssetDirectory.TheThinkerBoss + Name;

		public override void SetStaticDefaults()
		{
			NPCID.Sets.TrailCacheLength[NPC.type] = 20;
			NPCID.Sets.TrailingMode[NPC.type] = 1;
		}

		public override void SetDefaults()
		{
			NPC.lifeMax = 200;
			NPC.damage = 25;
			NPC.width = 34;
			NPC.height = 34;
			NPC.noTileCollide = true;
			NPC.noGravity = true;
			NPC.aiStyle = -1;
			NPC.knockBackResist = 0f;
			NPC.defense = 3;

			NPC.HitSound = SoundID.NPCDeath12.WithPitchOffset(-0.25f);
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCrimson,
				new FlavorTextBestiaryInfoElement("The thinkers organized 'workforce', ready to do anything from build forcefields to reducing living things to red mist.")
			});
		}

		public override bool CheckActive()
		{
			return false;
		}

		public override bool PreKill()
		{
			return false;
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			return opacity > 0.5f;
		}

		public override bool? CanBeHitByItem(Player player, Item item)
		{
			return opacity > 0.5f ? null : false;
		}

		public override bool? CanBeHitByProjectile(Projectile projectile)
		{
			return opacity > 0.5f ? null : false;
		}

		public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
		{
			modifiers.FinalDamage *= 0;
			modifiers.HideCombatText();
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			CombatText.NewText(NPC.Hitbox, Color.Gray, 0);
		}

		public void DoTell(float length, float rotation)
		{
			tellStart = NPC.Center;

			TellTime = 60;
			tellLen = length;
			tellDirection = rotation;
		}

		public override void AI()
		{
			Timer++;

			NPC.life = NPC.lifeMax;

			if (TellTime > 0)
				TellTime--;

			if (hurtTime > 0)
				hurtTime--;

			float prog = Math.Min(1, Timer / 30f);

			if (State == 1)
				opacity = 1 - prog;
			else if (State == 2)
				opacity = prog;

			if (ThisBrain is null || !ThisBrain.NPC.active)
			{
				State = 1;

				if (Timer > 60)
					NPC.active = false;

				NPC.netUpdate = true;
			}

			NPC.immortal = true;

			float speed = Vector2.Distance(NPC.position, NPC.oldPosition);

			if (speed > 2f)
				Dust.NewDustPerfect(NPC.Center, DustID.Blood, Vector2.UnitY.RotatedByRandom(1f) * Main.rand.NextFloat(2, 5));

			if (speed < 20f)
				Lighting.AddLight(NPC.Center, new Vector3(0.5f, 0.3f, 0.35f) * (0.5f + speed * 0.025f));
		}

		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
		{
			return false;
		}

		public Color Rainbow(float offset)
		{
			return new Color(
				0.75f + MathF.Sin(Main.GameUpdateCount / 60f * 3.14f + offset) * 0.25f,
				0.75f + MathF.Sin(Main.GameUpdateCount / 60f * 3.14f + 2 + offset) * 0.25f,
				0.75f + MathF.Sin(Main.GameUpdateCount / 60f * 3.14f + 4 + offset) * 0.25f);
		}

		public Color RedRainbow(float offset)
		{
			return new Color(
				0.7f + MathF.Sin(Main.GameUpdateCount / 60f * 3.14f + offset) * 0.06f,
				0.3f + MathF.Sin(Main.GameUpdateCount / 60f * 3.14f + 2 + offset) * 0.1f,
				0.3f + MathF.Sin(Main.GameUpdateCount / 60f * 3.14f + 4 + offset) * 0.06f);
		}

		public void InnerDraw(SpriteBatch spriteBatch, Vector2 screenPos, float prog)
		{
			Texture2D tex = Assets.Bosses.TheThinkerBoss.Neurysm.Value;

			Color glowColor = AlternateAppearance ? RedRainbow(0 + NPC.whoAmI) : Rainbow(0 + NPC.whoAmI);

			Color trailOne = AlternateAppearance ? RedRainbow(0 + NPC.whoAmI) : Rainbow(0 + NPC.whoAmI);
			Color trailTwo = AlternateAppearance ? RedRainbow(0 + NPC.whoAmI) : Rainbow(1.5f + NPC.whoAmI);

			for (int k = 0; k < 20; k++)
			{
				Texture2D trail = Assets.Bosses.TheThinkerBoss.NeurysmTrail.Value;
				Texture2D trail2 = Assets.Masks.GlowAlpha.Value;

				Vector2 pos = NPC.oldPos[k] + NPC.Size / 2f;
				Color col = Color.Lerp(trailOne, trailTwo, k / 20f) * opacity * (1f - k / 20f) * 0.25f;
				col *= Math.Min(1, Vector2.Distance(NPC.position, NPC.oldPos[1]) / 1f);
				col.A = 0;

				spriteBatch.Draw(trail, pos - screenPos, null, col, NPC.rotation, trail.Size() / 2f, 1 - k / 20f, 0, 0);
				spriteBatch.Draw(trail2, pos - screenPos, null, col * 0.5f, NPC.rotation, trail2.Size() / 2f, 1 - k / 20f, 0, 0);
			}

			Effect effect = ShaderLoader.GetShader("Neurysm").Value;

			if (effect != null)
			{
				effect.Parameters["u_resolution"].SetValue(Assets.Misc.StarView.Size() * 0.5f);
				effect.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.015f);

				effect.Parameters["mainbody_t"].SetValue(Assets.Misc.StarView.Value);
				effect.Parameters["noisemap_t"].SetValue(Assets.Misc.AuroraWaterMap.Value);

				effect.Parameters["u_color"].SetValue(glowColor.ToVector3() * opacity * 2.0f);
				effect.Parameters["u_fade"].SetValue(Vector3.Zero * opacity);
				effect.Parameters["u_strength"].SetValue(0.2f);

				var rasterizer = new RasterizerState() { ScissorTestEnable = true, CullMode = CullMode.None };

				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Immediate, default, SamplerState.PointWrap, default, rasterizer, effect);

				Texture2D tex2 = Assets.Misc.StarView.Value;
				spriteBatch.Draw(tex2, NPC.Center - screenPos, null, Color.White, NPC.rotation, tex2.Size() / 2f, NPC.scale * 0.25f, 0, 0);

				spriteBatch.End();
				spriteBatch.Begin(default, default, default, default, rasterizer, default);
			}

			spriteBatch.Draw(tex, NPC.Center - screenPos, null, glowColor * opacity, NPC.rotation, tex.Size() / 2f, NPC.scale, 0, 0);

			if (State == 1)
			{
				for (int k = 0; k < 6; k++)
				{
					float rot = k / 6f * 6.28f + prog * 3.14f;
					Vector2 offset = Vector2.UnitX.RotatedBy(rot) * prog * 32;
					spriteBatch.Draw(tex, NPC.Center + offset - screenPos, null, glowColor * opacity * 0.2f, NPC.rotation, tex.Size() / 2f, 1, 0, 0);
				}
			}

			if (State == 2)
			{
				for (int k = 0; k < 6; k++)
				{
					float rot = k / 6f * 6.28f + (1 - prog) * 3.14f;
					Vector2 offset = Vector2.UnitX.RotatedBy(rot) * (1 - prog) * 32;
					spriteBatch.Draw(tex, NPC.Center + offset - screenPos, null, glowColor * opacity * 0.2f, NPC.rotation, tex.Size() / 2f, 1, 0, 0);
				}
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D tex = Assets.Bosses.TheThinkerBoss.Neurysm.Value;

			Color glowColor = AlternateAppearance ? RedRainbow(0 + NPC.whoAmI) : Rainbow(0 + NPC.whoAmI);

			Color trailOne = AlternateAppearance ? RedRainbow(0 + NPC.whoAmI) : Rainbow(0 + NPC.whoAmI);
			Color trailTwo = AlternateAppearance ? RedRainbow(0 + NPC.whoAmI) : Rainbow(1.5f + NPC.whoAmI);

			float prog = Math.Min(1, Timer / 30f);

			if (NPC.IsABestiaryIconDummy)
			{
				opacity = 1;
				InnerDraw(spriteBatch, screenPos, 1f);
				return false;
			}

			if (opacity >= 0.05f)
			{

				float speed = Vector2.Distance(NPC.position, NPC.oldPos[1]);
				float glowPower = Math.Max(speed / 10f, 0.4f);

				Texture2D glow = Assets.Masks.GlowAlpha.Value;
				Texture2D glow2 = Assets.Bosses.TheThinkerBoss.NeurysmTrail.Value;
				Color col2 = glowColor * glowPower * opacity;
				col2.A = 0;

				if (State == 0)
				{
					//spriteBatch.Draw(glow, NPC.Center - Main.screenPosition, null, col2, NPC.rotation, glow.Size() / 2f, 1f, 0, 0);
					//spriteBatch.Draw(glow2, NPC.Center - Main.screenPosition, null, col2 * 2.5f, NPC.rotation, glow2.Size() / 2f, 1.1f, 0, 0);
				}
				else
				{
					for (int k = 0; k < 6; k++)
					{
						float rot = State == 1 ? k / 6f * 6.28f + prog * 3.14f : k / 6f * 6.28f + (1 - prog) * 3.14f;
						Vector2 offset = State == 1 ? Vector2.UnitX.RotatedBy(rot) * prog * 32 : Vector2.UnitX.RotatedBy(rot) * (1 - prog) * 32;
						spriteBatch.Draw(glow, NPC.Center + offset - screenPos, null, col2 * 0.2f, NPC.rotation, glow.Size() / 2f, 1f, 0, 0);
						spriteBatch.Draw(glow2, NPC.Center + offset - screenPos, null, col2 * 0.425f, NPC.rotation, glow2.Size() / 2f, 1.1f, 0, 0);
					}
				}

				ModContent.GetInstance<PixelationSystem>().QueueRenderAction("OverPlayers", () => InnerDraw(spriteBatch, screenPos, prog));
			}

			if (TellTime > 0)
			{
				Texture2D tell = Assets.GlowTrailNoEnd.Value;
				var source = new Rectangle(0, 0, tell.Width, tell.Height);
				var target = new Rectangle((int)(NPC.Center.X - Main.screenPosition.X), (int)(NPC.Center.Y - Main.screenPosition.Y), (int)tellLen, 64);
				var origin = new Vector2(0, tell.Height / 2f);
				float opacity = (float)Math.Sin(TellTime / 60f * 3.14f) * 0.5f;
				Color color = new Color(160, 60, 60) * opacity * 0.2f;
				color.A = 0;

				spriteBatch.Draw(tell, target, source, color, tellDirection + 3.14f, origin, 0, 0);

				for (int k = 0; k < tellLen; k += 48)
				{
					Texture2D trail = Assets.Bosses.SquidBoss.SqueezeTellArrow.Value;
					Vector2 pos = Vector2.Lerp(tellStart, tellStart + Vector2.UnitX.RotatedBy(tellDirection + 3.14f) * tellLen, k / tellLen) - Main.screenPosition;

					float progTell = 1f - k / tellLen;
					float minTell = progTell * 30;
					float maxTell = 30 + minTell;

					float thisOpacity = 0.25f;

					if (TellTime > minTell && TellTime < maxTell)
						thisOpacity += 0.25f * MathF.Sin((TellTime - minTell) / 30f * 3.14f);

					spriteBatch.Draw(trail, pos, null, new Color(255, 180, 80, 0) * opacity * thisOpacity, tellDirection + 3.14f, trail.Size() / 2f, thisOpacity * 2, 0, 0);
				}
			}

			return false;
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(brain?.whoAmI ?? -1);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			int brainWhoAmI = reader.ReadInt32();

			if (brainWhoAmI >= 0)
				brain = Main.npc[brainWhoAmI];
		}
	}
}