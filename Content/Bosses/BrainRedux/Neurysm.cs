using System;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	internal class Neurysm : ModNPC
	{
		public float opacity;
		public float tellDirection;
		public float tellLen;
		public float hurtTime;

		public bool Dead => (DeadBrain.TheBrain?.State ?? 0) >= 3;

		public ref float Timer => ref NPC.ai[0];
		public ref float State => ref NPC.ai[1];
		public ref float TellTime => ref NPC.ai[3];

		public override string Texture => AssetDirectory.BrainRedux + Name;

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
				new FlavorTextBestiaryInfoElement("The brain is able to manifest smaller, yet deadlier, shards of itself when empowered by the strage light of The Thinker. These fragments are far more organized than it's regular creeper companions.")
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

		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			if (!Dead)
				hurtTime = 15;
		}

		public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
		{
			if (!Dead)
				hurtTime = 15;
		}

		public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
		{
			if (Dead)
			{
				modifiers.FinalDamage *= 0;
				modifiers.HideCombatText();

				CombatText.NewText(NPC.Hitbox, Color.Gray, 0);
			}
		}

		public override void AI()
		{
			Timer++;

			NPC.realLife = DeadBrain.TheBrain?.thinker?.whoAmI ?? NPC.realLife;
			NPC.life = NPC.lifeMax;

			if (TellTime > 0)
				TellTime--;

			if (hurtTime > 0)
				hurtTime--;

			if (State == 1)
				opacity = 1 - Timer / 30f;
			else if (State == 2)
				opacity = Timer / 30f;

			if (DeadBrain.TheBrain is null)
			{
				State = 1;

				if (Timer > 60)
					NPC.active = false;
			}
			else
			{
				if (Dead)
					NPC.realLife = -1;
			}

			float speed = Vector2.Distance(NPC.position, NPC.oldPosition);

			if (speed > 2f)
				Dust.NewDustPerfect(NPC.Center, DustID.Blood, Vector2.UnitY.RotatedByRandom(1f) * Main.rand.NextFloat(2, 5));

			if (speed < 20f)
				Lighting.AddLight(NPC.Center, new Vector3(0.5f, 0.3f, 0.35f) * (0.5f + speed * 0.025f));
		}

		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
		{
			return opacity > 0;
		}

		public Color Rainbow(float offset)
		{
			return new Color(
				1f + MathF.Sin(Main.GameUpdateCount / 60f * 3.14f + offset) * 0.5f, 
				1f + MathF.Sin(Main.GameUpdateCount / 60f * 3.14f + 1 + offset) * 0.5f, 
				1f + MathF.Sin(Main.GameUpdateCount / 60f * 3.14f + 2 + offset) * 0.5f);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D tex = Assets.Bosses.BrainRedux.Neurysm.Value;

			Color glowColor = Dead ? new Color(25, 40, 6) : Rainbow(0 + NPC.whoAmI);

			Color trailOne = Dead ? new Color(100, 180, 30) : Rainbow(0 + NPC.whoAmI);
			Color trailTwo = Dead ? new Color(50, 70, 20) : Rainbow(1.5f + NPC.whoAmI);

			if (opacity >= 0.05f)
			{
				for (int k = 0; k < 20; k++)
				{
					Texture2D trail = Assets.Bosses.BrainRedux.NeurysmTrail.Value;

					Vector2 pos = NPC.oldPos[k] + NPC.Size / 2f;
					Color col = Color.Lerp(trailOne, trailTwo, k / 20f) * opacity * (1f - k / 20f) * 0.25f;
					spriteBatch.Draw(trail, pos - Main.screenPosition, null, col, NPC.rotation, tex.Size() / 2f, 1 - k / 20f, 0, 0);
				}

				float speed = Vector2.Distance(NPC.position, NPC.oldPos[1]);

				Texture2D glow = Assets.Keys.GlowAlpha.Value;
				Color col2 = glowColor * (speed / 10f) * opacity;
				col2.A = 0;

				if (State == 0)
				{
					spriteBatch.Draw(glow, NPC.Center - Main.screenPosition, null, col2, NPC.rotation, glow.Size() / 2f, 1f, 0, 0);
				}
				else
				{
					for (int k = 0; k < 6; k++)
					{
						float rot = State == 1 ? k / 6f * 6.28f + Timer / 30f * 3.14f : k / 6f * 6.28f + (1 - Timer / 30f) * 3.14f;
						Vector2 offset = State == 1 ? Vector2.UnitX.RotatedBy(rot) * Timer / 30f * 32 : Vector2.UnitX.RotatedBy(rot) * (1 - Timer / 30f) * 32;
						spriteBatch.Draw(glow, NPC.Center + offset - Main.screenPosition, null, col2 * 0.2f, NPC.rotation, glow.Size() / 2f, 1f, 0, 0);
					}
				}
			}

			if (hurtTime > 0 && DeadBrain.TheBrain != null && DeadBrain.TheBrain.NPC.active)
			{
				NPC brain = DeadBrain.TheBrain.NPC;

				float len = Vector2.Distance(NPC.Center, brain.Center);
				float rot = NPC.Center.DirectionTo(brain.Center).ToRotation();

				Texture2D tell = Assets.GlowTrailNoEnd.Value;
				var source = new Rectangle(0, 0, tell.Width, tell.Height);
				var target = new Rectangle((int)(NPC.Center.X - Main.screenPosition.X), (int)(NPC.Center.Y - Main.screenPosition.Y), (int)len, 24);
				var origin = new Vector2(0, 12);
				Color color = Color.Gray * (hurtTime / 15f) * 0.5f;
				color.A = 0;

				spriteBatch.Draw(tell, target, source, color, rot, origin, 0, 0);
			}

			if (TellTime > 0)
			{
				Texture2D tell = Assets.GlowTrailNoEnd.Value;
				var source = new Rectangle(0, 0, tell.Width, tell.Height);
				var target = new Rectangle((int)(NPC.Center.X - Main.screenPosition.X), (int)(NPC.Center.Y - Main.screenPosition.Y), (int)tellLen, 24);
				var origin = new Vector2(0, 12);
				Color color = new Color(160, 40, 40) * (float)Math.Sin(TellTime / 30f * 3.14f) * 0.5f;
				color.A = 0;

				spriteBatch.Draw(tell, target, source, color, tellDirection + 3.14f, origin, 0, 0);
			}

			if (State == 0)
				spriteBatch.Draw(tex, NPC.Center - Main.screenPosition, null, drawColor, NPC.rotation, tex.Size() / 2f, 1, 0, 0);

			if (State == 1)
			{
				for (int k = 0; k < 6; k++)
				{
					float rot = k / 6f * 6.28f + Timer / 30f * 3.14f;
					Vector2 offset = Vector2.UnitX.RotatedBy(rot) * Timer / 30f * 32;
					spriteBatch.Draw(tex, NPC.Center + offset - Main.screenPosition, null, drawColor * opacity * 0.2f, NPC.rotation, tex.Size() / 2f, 1, 0, 0);
				}
			}

			if (State == 2)
			{
				for (int k = 0; k < 6; k++)
				{
					float rot = k / 6f * 6.28f + (1 - Timer / 30f) * 3.14f;
					Vector2 offset = Vector2.UnitX.RotatedBy(rot) * (1 - Timer / 30f) * 32;
					spriteBatch.Draw(tex, NPC.Center + offset - Main.screenPosition, null, drawColor * opacity * 0.2f, NPC.rotation, tex.Size() / 2f, 1, 0, 0);
				}
			}

			return false;
		}
	}
}
