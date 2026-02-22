using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Buffs.Summon;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Core.Systems.PixelationSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Crimson
{
	public class ThoughtProvoker : ModItem
	{
		public override string Texture => AssetDirectory.CrimsonItem + Name;

		public override void Load()
		{
			StarlightPlayer.PostDrawEvent += DrawShield;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Thought Provoker");
			Tooltip.SetDefault(
				"Summons Thinky Jr to protect you\n" +
				"Endurance is increased by 5% per Thinky Jr alive\n" +
				"If hit by an enemy or projectile, Thinky Jr will target the nearest enemy and explode violently");
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the Player target anywhere on the whole screen while using a controller.
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
		}

		public override void SetDefaults()
		{
			Item.damage = 32;
			Item.knockBack = 10f;
			Item.mana = 30;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.value = Item.buyPrice(gold: 3);
			Item.rare = ItemRarityID.Orange;
			Item.UseSound = SoundID.Item44;

			Item.noMelee = true;
			Item.DamageType = DamageClass.Summon;
			Item.buffType = ModContent.BuffType<ThoughtProvokerSummonBuff>();
			Item.shoot = ModContent.ProjectileType<ThoughtProvokerProjectile>();
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			player.AddBuff(Item.buffType, 2);
			Projectile.NewProjectileDirect(source, Main.MouseWorld, velocity, type, damage, knockback, Main.myPlayer).originalDamage = Item.damage;

			Vector2 pos = Main.MouseWorld;

			if (Vector2.Distance(Main.MouseWorld, player.Center) > 200f)
				pos = player.Center;

			for (int i = 0; i < 5; i++)
			{
				Color c = Main.rand.NextBool() ? new Color(236, 189, 64) : new Color(200, 80, 220);

				Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.PixelatedEmber>(), Main.rand.NextVector2CircularEdge(2f, 2f), 0, c with { A = 0 }, Main.rand.NextFloat(0.5f)).customData = -1;
			}

			return false;
		}

		private void DrawShield(Player player, SpriteBatch spriteBatch)
		{
			if (player.ownedProjectileCounts[ModContent.ProjectileType<ThoughtProvokerProjectile>()] > 0)
			{
				Texture2D tex = Assets.Bosses.TheThinkerBoss.ShieldMap.Value;

				Effect effect = ShaderLoader.GetShader("BrainShield").Value;

				if (effect != null)
				{
					float fade = player.GetModPlayer<ThoughtProvokerPlayer>().shieldFade / 30f;

					effect.Parameters["time"]?.SetValue(Main.GameUpdateCount * 0.02f);
					effect.Parameters["size"]?.SetValue(tex.Size() * 0.6f);
					effect.Parameters["opacity"]?.SetValue(fade);
					effect.Parameters["pixelRes"]?.SetValue(4f);

					effect.Parameters["drawTexture"]?.SetValue(tex);
					effect.Parameters["noiseTexture"]?.SetValue(Assets.Noise.SwirlyNoiseLooping.Value);
					effect.Parameters["pulseTexture"]?.SetValue(Assets.Noise.PerlinNoise.Value);
					effect.Parameters["edgeTexture"]?.SetValue(Assets.Bosses.TheThinkerBoss.ShieldEdge.Value);
					effect.Parameters["outTexture"]?.SetValue(Assets.Bosses.TheThinkerBoss.ShieldMapOut.Value);
					effect.Parameters["color"].SetValue(Vector3.Lerp(Vector3.One, new Vector3(1, 0.5f, 0.5f), 1f));

					spriteBatch.End();
					spriteBatch.Begin(default, BlendState.Additive, default, default, Main.Rasterizer, effect, Main.GameViewMatrix.TransformationMatrix);

					spriteBatch.Draw(tex, player.Center + new Vector2(0f, player.gfxOffY) - Main.screenPosition, null, Color.White, 0, tex.Size() / 2f, 0.18f, SpriteEffects.FlipVertically, 0);

					spriteBatch.End();
					spriteBatch.Begin(default, default, default, default, Main.Rasterizer, default, Main.GameViewMatrix.TransformationMatrix);
				}
			}
		}
	}

	public class ThoughtProvokerPlayer : ModPlayer
	{
		public int shieldFade;

		public override void UpdateEquips()
		{
			// 5% increased endurance per Thinky Jr
			int shieldingThinkers = Main.projectile.Where(p => p.active && p.type == ModContent.ProjectileType<ThoughtProvokerProjectile>()
				&& p.owner == Player.whoAmI && (p.ModProjectile as ThoughtProvokerProjectile).AttackState == 0 && (p.ModProjectile as ThoughtProvokerProjectile).resetTimer <= 0).Count();

			if (shieldingThinkers > 0)
			{
				Player.endurance += 0.05f * shieldingThinkers;

				if (shieldFade < 30)
					shieldFade++;
			}
			else if (shieldFade > 0)
			{
				shieldFade--;
			}
		}
	}

	public class ThoughtProvokerProjectile : ModProjectile
	{
		public const int RESET_TIME = 300;
		public const int MAX_CHASE_RANGE = 500;

		private List<Vector2> cache;
		private Trail trail;
		private Trail trail2;

		public int resetTimer;
		public int lifetime;

		public ref float TargetWhoAmI => ref Projectile.ai[1];
		public NPC Target => TargetWhoAmI > -1 ? Main.npc[(int)TargetWhoAmI] : null;
		public bool FoundTarget => Target != null;

		public ref float AttackState => ref Projectile.ai[0];
		public ref float EnemyWhoAmI => ref Projectile.ai[1];
		public ref float AttackDelay => ref Projectile.ai[2];

		public Player Owner => Main.player[Projectile.owner];

		public override string Texture => AssetDirectory.CrimsonItem + Name;

		public NPC MinionTarget
		{
			get
			{
				if (Owner.HasMinionAttackTargetNPC && Main.npc[Owner.MinionAttackTargetNPC].Distance(Projectile.Center) < 1000f)
					return Main.npc[Owner.MinionAttackTargetNPC];

				return null;
			}
		}

		public int Lifetime
		{
			get
			{
				if (Projectile.minionPos <= 0)
				{
					return lifetime;
				}
				else
				{
					Projectile proj = Main.projectile.FirstOrDefault(p => p.active && p.type == Type && p.owner == Projectile.owner && p.minionPos == 0);
					if (proj != null)
					{
						var brain = proj.ModProjectile as ThoughtProvokerProjectile;
						if (brain != null)
							return brain.lifetime;
					}
				}

				return 0;
			}
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Thinky Junior");

			Main.projPet[Projectile.type] = true; // Denotes that this Projectile is a pet or minion
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true; // This is necessary for right-click targeting
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true; // This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned			 
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true; // Don't mistake this with "if this is true, then it will automatically home". It is just for damage reduction for certain NPCs
		}
		public override void OnSpawn(IEntitySource source)
		{
			TargetWhoAmI = -1f;
			resetTimer = 35;
		}

		public override void SetDefaults()
		{
			Projectile.width = 24;
			Projectile.height = 24;

			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;

			Projectile.minion = true;

			Projectile.minionSlots = 1f;

			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Summon;

			Projectile.friendly = true;
		}

		public override void AI()
		{
			Player Player = Main.player[Projectile.owner];

			if (resetTimer > 0)
				resetTimer--;

			if (AttackDelay > 0)
				AttackDelay--;

			#region Active check
			if (Player.dead || !Player.active)
				Player.ClearBuff(ModContent.BuffType<ThoughtProvokerSummonBuff>());

			if (Player.HasBuff(ModContent.BuffType<ThoughtProvokerSummonBuff>()))
				Projectile.timeLeft = 2;

			lifetime++;

			//closest npc
			NPC target = FindTarget();
			if (target != default)
				TargetWhoAmI = target.whoAmI;

			#endregion

			if (AttackState == 0)
			{
				#region Shielding Behavior

				if (!Main.dedServ)
				{
					ManageCaches();
					ManageTrail();
				}

				float totalCount = Owner.ownedProjectileCounts[Type] > 0 ? Owner.ownedProjectileCounts[Type] : 1;

				Vector2 idlePos = Owner.Center + new Vector2(0, 80).RotatedBy(MathHelper.ToRadians(Projectile.minionPos / totalCount * 360f + (Projectile.minionPos == totalCount ? 360f / totalCount : 0f)) + MathHelper.ToRadians(Lifetime));

				float dist = Vector2.Distance(Projectile.Center, idlePos);

				Vector2 toIdlePos = idlePos - Projectile.Center;
				if (toIdlePos.Length() < 0.0001f)
				{
					toIdlePos = Vector2.Zero;
				}
				else
				{
					float speed = 50f;
					if (dist < 1000f)
						speed = MathHelper.Lerp(15f, 50f, dist / 1000f);

					if (dist < 100f)
						speed = MathHelper.Lerp(3f, 15f, dist / 100f);

					toIdlePos.Normalize();
					toIdlePos *= speed;
				}

				Projectile.velocity = (Projectile.velocity * (5f - 1) + toIdlePos) / 5f;

				if (dist > 2000f)
				{
					Projectile.Center = idlePos;
					Projectile.velocity = Vector2.Zero;
					Projectile.netUpdate = true;
				}

				Projectile.rotation = Projectile.velocity.X * 0.05f;

				// checks if the projectile is colliding with a npc or hostile projectile hitbox

				Entity colliding = CheckCollisions(target);

				if (colliding != null && resetTimer <= 0)
				{
					//special case for projectiles
					if (colliding is Projectile)
						(colliding as Projectile).penetrate--;

					AttackState = 1;

					AttackDelay = 30;
					Projectile.velocity *= 0.05f;
					Projectile.velocity += Main.rand.NextVector2CircularEdge(10f, 10f);

					for (int i = 0; i < 10; i++)
					{
						Dust.NewDustPerfect(Projectile.Center,
						ModContent.DustType<Dusts.PixelatedEmber>(), Main.rand.NextVector2CircularEdge(2f, 2f), 0, TrailColor(false) with { A = 0 }, Main.rand.NextFloat(0.5f)).customData = 1;
					}

					CameraSystem.shake += 3;
					SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.Center);

					DisposeTrails();
				}

				if (Projectile.Distance(Owner.Center) >= 200f)
					Projectile.Center = Owner.Center;

				#endregion
			}
			else
			{
				#region Exploding Behavior

				//prioritize minion target over closest npc
				if (MinionTarget != null)
					TargetWhoAmI = MinionTarget.whoAmI;

				if (Target is null)
				{
					TargetWhoAmI = -1;
					AttackDelay = 45;
				}

				if (AttackDelay <= 0)
				{
					Vector2 direction = Target.Center - Projectile.Center;
					direction.Normalize();
					if (Projectile.Distance(Target.Center) > 400f)
					{
						direction *= 20f;
					}
					else
					{
						float mult = MathHelper.Lerp(20f, 100f, 1f - Projectile.Distance(Target.Center) / 400f);
						direction *= mult;
					}

					Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction, 0.05f);
					Projectile.rotation += Projectile.velocity.Length() * 0.02f; // more aggressive spin when attacking

					Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2CircularEdge(2f, 2f),
						ModContent.DustType<Dusts.PixelatedEmber>(), Main.rand.NextVector2Circular(2f, 2f), 0, TrailColor(false) with { A = 0 }, Main.rand.NextFloat(0.5f)).customData = Main.rand.NextBool() ? -1 : 1;
				}
				else
				{
					Projectile.rotation += Projectile.velocity.Length() * 0.01f;
					Projectile.velocity *= 0.985f;
				}

				if (Target is null || !Target.active)
					ResetToIdle();

				#endregion
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;

			Texture2D tex = Assets.Items.Crimson.ThoughtProvokerProjectile.Value;
			Texture2D bloomTex = Assets.Masks.GlowAlpha.Value;
			Texture2D starTex = Assets.StarTexture_Alt.Value;

			Texture2D glow = Assets.Masks.Glow.Value;

			float fade = 1f;
			if (resetTimer > 0)
				fade = 1f - resetTimer / (float)RESET_TIME;

			float scaleCalc = 1f + 0.15f * (float)Math.Sin(Main.GameUpdateCount * 0.02f);

			spriteBatch.Draw(glow, Projectile.Center - Main.screenPosition, null, Color.Black * 0.15f * fade, 0f, glow.Size() / 2f, scaleCalc * 1.5f, 0, 0);

			spriteBatch.Draw(glow, Projectile.Center - Main.screenPosition, null, Color.Black * 0.5f * fade, 0f, glow.Size() / 2f, scaleCalc * 0.8f, 0, 0);

			if (AttackState == 0)
			{
				DrawTrail();

				ModContent.GetInstance<PixelationSystem>().QueueRenderAction("OverPlayers", () =>
				{
					spriteBatch.Draw(bloomTex, Owner.Center - Main.screenPosition, null, TrailColor() with { A = 0 }, 0f, bloomTex.Size() / 2f, 0.3f / Owner.ownedProjectileCounts[Type], 0f, 0f);
					spriteBatch.Draw(bloomTex, Owner.Center - Main.screenPosition, null, Color.White with { A = 0 }, 0f, bloomTex.Size() / 2f, 0.25f / Owner.ownedProjectileCounts[Type], 0f, 0f);
				});
			}

			//Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White * fade, Projectile.rotation, tex.Size() / 2f, Projectile.scale, 0f, 0f);		

			Effect bodyShader = ShaderLoader.GetShader("ThinkerBody").Value;

			if (bodyShader != null)
			{
				var c = new Vector3(0.7f, 0.3f, 0.3f);

				if (AttackDelay > 0)
					c = Vector3.Lerp(new Vector3(1f, 1f, 1f), new Vector3(0.7f, 0.3f, 0.3f), 1f - AttackDelay / 30f);

				bodyShader.Parameters["u_resolution"].SetValue(Assets.Bosses.TheThinkerBoss.Heart.Size() / 3 * scaleCalc);
				bodyShader.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.015f);

				bodyShader.Parameters["mainbody_t"].SetValue(Assets.Bosses.TheThinkerBoss.Heart.Value);
				bodyShader.Parameters["linemap_t"].SetValue(Assets.Bosses.TheThinkerBoss.HeartLine.Value);
				bodyShader.Parameters["noisemap_t"].SetValue(Assets.Noise.ShaderNoise.Value);
				bodyShader.Parameters["overlay_t"].SetValue(Assets.Bosses.TheThinkerBoss.HeartOver.Value);
				bodyShader.Parameters["normal_t"].SetValue(Assets.Bosses.TheThinkerBoss.HeartNormal.Value);
				bodyShader.Parameters["u_color"].SetValue(c * fade);
				bodyShader.Parameters["u_fade"].SetValue(Vector3.Lerp(new Vector3(0.0f, 0.2f, 0.4f), new Vector3(0.3f, 0.5f, 0.3f), (float)Math.Sin(Main.GameUpdateCount * 0.01f)) * fade); // Lerp here so this is the same as the flower core at 0 scale
				bodyShader.Parameters["mask_t"].SetValue(Assets.MagicPixel.Value);

				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Immediate, default, SamplerState.PointWrap, default, Main.Rasterizer, bodyShader, Main.GameViewMatrix.ZoomMatrix);

				Texture2D t = Assets.Bosses.TheThinkerBoss.Heart.Value;
				spriteBatch.Draw(t, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, t.Size() / 2f, scaleCalc * 0.35f, 0, 0);

				spriteBatch.End();
				spriteBatch.Begin(default, default, default, default, Main.Rasterizer, default, Main.GameViewMatrix.TransformationMatrix);
			}

			spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, TrailColor() with { A = 0 } * 0.35f * fade, 0f, bloomTex.Size() / 2f, scaleCalc * 0.7f, 0f, 0f);
			spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, TrailColor() with { A = 0 } * 0.5f * fade, 0f, bloomTex.Size() / 2f, scaleCalc * 0.3f, 0f, 0f);
			spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, Color.White with { A = 0 } * fade * 0.6f, 0f, bloomTex.Size() / 2f, scaleCalc * 0.1f, 0f, 0f);

			if (AttackDelay > 0 && AttackState == 1)
			{
				float fadeIn = AttackDelay / 30f;

				spriteBatch.Draw(starTex, Projectile.Center - Main.screenPosition, null, TrailColor() with { A = 0 } * fadeIn, 0f, starTex.Size() / 2f, Projectile.scale, 0f, 0f);
				spriteBatch.Draw(starTex, Projectile.Center - Main.screenPosition, null, TrailColor() with { A = 0 } * fadeIn, 0f, starTex.Size() / 2f, Projectile.scale * 0.5f, 0f, 0f);
				spriteBatch.Draw(starTex, Projectile.Center - Main.screenPosition, null, Color.White with { A = 0 } * fadeIn, Projectile.rotation, starTex.Size() / 2f, Projectile.scale, 0f, 0f);
			}

			return false;
		}

		public override void OnKill(int timeLeft)
		{
			DisposeTrails();
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Main.myPlayer == Projectile.owner)
				Projectile.NewProjectile(Projectile.GetSource_OnHit(target), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<ThoughtProvokerExplosion>(), Projectile.damage * 2, 5f, Projectile.owner, 50);

			CameraSystem.shake = 5;

			SoundHelper.PlayPitched("Impacts/GoreHeavy", 0.5f, 0.1f, Projectile.Center);

			for (int i = 0; i < 10; i++)
			{
				float r = 0.2f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.03f;
				float g = 0.3f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + 2f) * 0.05f;
				float b = 0.7f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + 4f) * 0.03f;
				var blue = new Color(r, g, b);

				r = 0.7f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.03f;
				g = 0.3f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + 2f) * 0.05f;
				b = 0.3f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + 4f) * 0.03f;
				var red = new Color(r, g, b);

				Color c = Main.rand.NextBool() ? red : blue;
				c.A = 0;

				Dust.NewDustPerfect(Projectile.Center,
					ModContent.DustType<Dusts.PixelatedImpactLineDustGlow>(), -Projectile.velocity.RotatedByRandom(1f) * Main.rand.NextFloat(1f), 0, c, Main.rand.NextFloat(0.1f, 0.15f)).customData = Main.rand.NextBool() ? -1 : 1;
			}

			ResetToIdle();
		}

		public override bool? CanHitNPC(NPC target)
		{
			return target.whoAmI == TargetWhoAmI && target.CanBeChasedBy(Projectile);
		}

		public override bool MinionContactDamage()
		{
			return AttackDelay <= 0 && AttackState == 1;
		}

		internal Entity CheckCollisions(NPC target)
		{
			// we dont want the shield to break if its not actively "protecting" its owner, for example on spawn
			if (Projectile.Distance(Owner.Center) > 125f)
				return null;

			Projectile closest = Main.projectile.Where(p => p.active && p.hostile && p.Distance(Projectile.Center) < 100f).OrderBy(p => p.Distance(Projectile.Center)).FirstOrDefault();

			if (closest != default && Projectile.Hitbox.Intersects(closest.Hitbox))
				return closest;

			if (target != null && Projectile.Hitbox.Intersects(target.Hitbox))
				return target;

			return null;
		}

		internal NPC FindTarget()
		{
			return Main.npc.Where(n => n.CanBeChasedBy() && n.Distance(Owner.Center) < 1000f).OrderBy(n => n.Distance(Projectile.Center)).FirstOrDefault();
		}

		internal void ResetToIdle()
		{
			AttackDelay = 45;
			resetTimer = RESET_TIME;

			DisposeTrails();

			AttackState = 0;
			Projectile.Center = Owner.Center + Main.rand.NextVector2CircularEdge(25f, 25f);
		}

		#region PRIMITIVE DRAWING
		private Color TrailColor(bool fadeColor = true)
		{
			var c = Color.Lerp(new Color(236, 189, 64), new Color(200, 80, 220), (float)Math.Sin(Main.GameUpdateCount * 0.01f));

			if (Projectile.minionPos % 2 == 0)
				c = Color.Lerp(new Color(200, 80, 220), new Color(236, 189, 64), (float)Math.Sin(Main.GameUpdateCount * 0.01f));

			float fade = 0f;
			if (resetTimer < 30f)
				fade = 1f - resetTimer / 30f;
			else if (resetTimer <= 0)
				fade = 1f;

			if (!fadeColor)
				fade = 1f;

			return c * fade;
		}

		private Color AltTrailColor(bool fadeColor = true)
		{
			var c = Color.Lerp(new Color(112, 249, 235), new Color(249, 255, 191), (float)Math.Sin(Main.GameUpdateCount * 0.035f));

			float fade = 0f;
			if (resetTimer < 30f)
				fade = 1f - resetTimer / 30f;
			else if (resetTimer <= 0)
				fade = 1f;

			if (!fadeColor)
				fade = 1f;

			return c * fade;
		}

		private Color FadingWhite()
		{
			float fade = 0f;
			if (resetTimer < 30f)
				fade = 1f - resetTimer / 30f;
			else if (resetTimer <= 0)
				fade = 1f;

			return Color.White * fade;
		}

		private BezierCurve GetBezierCurve()
		{
			float lerper = 1f - Vector2.Distance(Projectile.Center, Owner.Center) / 200f;

			if (lerper > 1f || lerper < 0f || resetTimer > 30)
				return null;

			Vector2[] curvePoints =
			{
				Vector2.Lerp(Projectile.Center + Projectile.velocity, Owner.Center, 0.3f) + new Vector2(0f, -MathHelper.Lerp(10f, 20f, lerper) * (float)Math.Sin(lifetime * 0.05f)).RotatedBy(Projectile.DirectionTo(Owner.Center).ToRotation()),
				Vector2.Lerp(Projectile.Center + Projectile.velocity, Owner.Center, 0.6f) + new Vector2(0f, MathHelper.Lerp(30f, 15f, lerper) * (float)Math.Cos(lifetime * -0.075f)).RotatedBy(Projectile.DirectionTo(Owner.Center).ToRotation()),
			};

			var curve = new BezierCurve(new Vector2[] {
				Projectile.Center + Projectile.velocity,
				curvePoints[0],
				curvePoints[1],
				Owner.Center
			});

			return curve;
		}

		private void ManageCaches()
		{
			BezierCurve curve = GetBezierCurve();
			if (curve is null)
			{
				DisposeTrails();

				return;
			}

			if (cache is null)
			{
				cache = [];

				for (int i = 0; i < 26; i++)
				{
					cache.Add(Projectile.Center + Projectile.velocity);
				}
			}

			for (int k = 0; k < 26; k++)
			{
				int points = 26;
				Vector2[] curvePositions = curve.GetPoints(points).ToArray();

				cache[k] = curvePositions[k];
			}
		}

		private void ManageTrail()
		{
			if (cache is null || GetBezierCurve() is null)
				return;

			trail ??= new Trail(Main.instance.GraphicsDevice, 26, new NoTip(), factor => 2.5f, factor => TrailColor() * 0.5f);

			trail.Positions = cache.ToArray();
			trail.NextPosition = cache[25];

			trail2 ??= new Trail(Main.instance.GraphicsDevice, 26, new TriangularTip(1), factor => 7f, factor => Color.Lerp(FadingWhite(), AltTrailColor(), (float)Math.Sin(Main.GameUpdateCount * 0.01f)) * 0.5f);

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = cache[25];
		}

		private void DrawTrail()
		{
			if (lifetime < 2)
				return;

			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
			{
				Effect effect = ShaderLoader.GetShader("CeirosRing").Value;

				if (effect != null)
				{
					var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
					Matrix view = Matrix.Identity;
					var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

					effect.Parameters["transformMatrix"].SetValue(world * view * projection);
					effect.Parameters["time"].SetValue(Projectile.timeLeft * -0.01f);
					effect.Parameters["repeats"].SetValue(5f);
					effect.Parameters["sampleTexture"].SetValue(Assets.GlowTrail.Value);

					trail?.Render(effect);

					effect.Parameters["repeats"].SetValue(1f);
					effect.Parameters["time"].SetValue((float)Main.timeForVisualEffects * 0.01f);
					effect.Parameters["sampleTexture"].SetValue(Assets.FireTrail.Value);

					trail2?.Render(effect);
				}
			});
		}

		private void DisposeTrails()
		{
			trail?.Dispose();
			trail2?.Dispose();

			trail = null;
			trail2 = null;
			cache = null;
		}

		#endregion PRIMITIVEDRAWING
	}

	public class ThoughtProvokerExplosion : ModProjectile
	{
		private List<Vector2> cache;

		private Trail trail;
		private Trail trail2;

		public override string Texture => AssetDirectory.Invisible;
		private float Progress => Utils.Clamp(1 - Projectile.timeLeft / 45f, 0f, 1f);

		private float Radius => Projectile.ai[0] * Eases.EaseQuinticOut(Progress);

		public override void Load()
		{
			GraymatterBiome.onDrawHallucinationMap += DrawHallucinationExplosion;
		}

		public override void SetDefaults()
		{
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 45;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 20;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Greymatter Explosion");
		}

		public override void AI()
		{
			GraymatterBiome.forceGrayMatter = true;

			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}

			Color c = TrailColor();
			c.A = 0;

			if (Main.rand.NextBool())
			{
				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2CircularEdge(Radius, Radius),
					ModContent.DustType<Dusts.PixelatedEmber>(), Main.rand.NextVector2Circular(2f, 2f), 0, c, Main.rand.NextFloat(0.5f)).customData = Main.rand.NextBool() ? -1 : 1;
			}

			for (int k = 0; k < 2; k++)
			{
				float rot = Main.rand.NextFloat(0, 6.28f);

				if (Main.rand.NextBool())
				{
					Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedBy(rot) * Radius, ModContent.DustType<PixelatedGlow>(),
					Vector2.One.RotatedBy(rot) * 0.5f, 0, default, Main.rand.NextFloat(0.5f, 1f));
				}

				Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedBy(rot) * Radius, ModContent.DustType<GraymatterDust>(),
					Vector2.One.RotatedBy(rot) * 1f, 0, new Color(255, 255, 255, 0) * Main.rand.NextFloat(0.2f, 0.6f), Main.rand.NextFloat(0.25f, 0.4f));

			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 line = targetHitbox.Center.ToVector2() - Projectile.Center;
			line.Normalize();
			line *= Radius;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + line);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			DrawPrimitives();
			return false;
		}

		private Color TrailColor()
		{
			var c = Color.Lerp(new Color(236, 189, 64), new Color(200, 80, 220), Progress);

			return c;
		}

		private void ManageCaches()
		{
			if (cache is null)
			{
				cache = [];

				for (int i = 0; i < 40; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			for (int k = 0; k < 40; k++)
			{
				cache[k] = Projectile.Center + Vector2.One.RotatedBy(k / 38f * 6.28f) * Radius;
			}

			while (cache.Count > 40)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 40, new NoTip(),
				factor => 40 * (1f - Progress), factor => TrailColor());

			trail2 ??= new Trail(Main.instance.GraphicsDevice, 40, new NoTip(),
				factor => 20 * (1f - Progress), factor => Color.White);

			trail.Positions = cache.ToArray();
			trail.NextPosition = cache[39];

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = cache[39];
		}
		private void DrawHallucinationExplosion(SpriteBatch batch)
		{
			foreach (Projectile proj in Main.ActiveProjectiles)
			{
				if (proj.type == Type)
				{
					var p = proj.ModProjectile as ThoughtProvokerExplosion;

					Texture2D tex = Assets.Masks.GlowAlpha.Value;
					//batch.Draw(tex, proj.Center - Main.screenPosition, null, new Color(255, 255, 255, 0) * (1f - p.Progress), 0, tex.Size() / 2f, MathHelper.Lerp(1f, 2.5f, Eases.EaseQuinticOut(p.Progress)), 0, 0);

					Main.spriteBatch.End();
					Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.EffectMatrix);

					Effect effect = ShaderLoader.GetShader("Decay").Value;

					if (effect is null)
						return;

					if (p.Progress >= 1f)
						return;

					Main.spriteBatch.End();
					Main.spriteBatch.Begin(default, BlendState.AlphaBlend, Main.DefaultSamplerState, default, Main.Rasterizer, effect, Main.GameViewMatrix.TransformationMatrix);

					effect.Parameters["uTime"].SetValue(p.Progress * 0.2f);

					effect.Parameters["uImage1"].SetValue(Assets.Noise.PerlinNoise.Value);
					effect.Parameters["uProgress"].SetValue(p.Progress);
					var color = new Color(255, 255, 255, 0);

					effect.Parameters["uColor"].SetValue(color.ToVector4());
					effect.Parameters["uOpacity"].SetValue(0f);

					effect.CurrentTechnique.Passes[0].Apply();

					batch.Draw(tex, proj.Center - Main.screenPosition, null, Color.White, 0f, tex.Size() / 2f, MathHelper.Lerp(0.7f, 2f, Eases.EaseQuinticOut(p.Progress)), 0f, 0f);

					Main.spriteBatch.End();
					Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
				}
			}
		}

		public void DrawPrimitives()
		{
			if (Projectile.timeLeft < 2)
				return;

			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
			{
				Effect effect = ShaderLoader.GetShader("CeirosRing").Value;

				if (effect != null)
				{
					var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
					Matrix view = Matrix.Identity;
					var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

					effect.Parameters["transformMatrix"].SetValue(world * view * projection);
					effect.Parameters["time"].SetValue(Projectile.timeLeft * -0.01f);
					effect.Parameters["repeats"].SetValue(5f);
					effect.Parameters["sampleTexture"].SetValue(Assets.BlurryTrail.Value);

					trail?.Render(effect);
					trail2?.Render(effect);
				}
			});
		}
	}
}
