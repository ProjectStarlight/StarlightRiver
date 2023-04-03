using StarlightRiver.Content.Projectiles;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.UndergroundTemple
{
	class TempleSpear : ModItem
	{
		public override string Texture => AssetDirectory.CaveTempleItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Point of Light");
			Tooltip.SetDefault("Hold <left> to charge a powerful spear\nFires a laser when fully charged");
		}

		public override void SetDefaults()
		{
			Item.DamageType = DamageClass.Melee;
			Item.width = 32;
			Item.height = 32;
			Item.damage = 11;
			Item.crit = 10;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useTime = 60;
			Item.useAnimation = 60;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.knockBack = 2f;
			Item.rare = ItemRarityID.Blue;
			Item.shoot = ProjectileType<TempleSpearProjectile>();
			Item.shootSpeed = 1;
			Item.channel = true;
		}
	}

	class TempleSpearProjectile : ModProjectile
	{
		public int maxCharge;

		public int oldCharge; // used for drawing

		public bool stabbing;

		public bool charged;

		public Vector2 offset;

		public Vector2 pullbackOffset; //cache the pullback offset for the stab

		public Vector2? OwnerMouse => (Main.myPlayer == Owner.whoAmI) ? Main.MouseWorld : null;

		public ref float Timer => ref Projectile.ai[0];

		public Player Owner => Main.player[Projectile.owner];

		public override string Texture => AssetDirectory.CaveTempleItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Point of Light");
		}

		public override void SetDefaults()
		{
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.penetrate = -1;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.DamageType = DamageClass.Melee;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}

		public override void OnSpawn(IEntitySource source)
		{
			maxCharge = (int)(Owner.HeldItem.useAnimation * (1f / Owner.GetTotalAttackSpeed(DamageClass.Melee)));
			maxCharge = Utils.Clamp(maxCharge, 15, 60);
		}

		public override void AI()
		{
			if (Owner.HeldItem.ModItem is not TempleSpear)
			{
				Projectile.Kill();
				return;
			}

			Owner.heldProj = Projectile.whoAmI;
			Owner.itemTime = 2;
			Owner.itemAnimation = 2;

			Projectile.velocity = Vector2.Normalize(Projectile.velocity);

			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			if (!Owner.channel && !stabbing && Timer >= 15)
			{
				stabbing = true;
				Timer = 0;
				Projectile.timeLeft = 35;

				CameraSystem.shake += 6;

				Projectile.friendly = true;

				Helpers.Helper.PlayPitched("Effects/HeavyWhooshShort", 1f, Main.rand.NextFloat(-0.05f, 0.05f), Owner.Center);

				for (int i = 0; i < 15; i++)
				{
					Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(15, 15), ModContent.DustType<Dusts.GlowFastDecelerate>(), Owner.DirectionTo(OwnerMouse.Value) * Main.rand.NextFloat(3f, 7f), 0, new Color(255, 200, 150), 0.6f);
				}

				if (charged)
				{
					Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<TempleSpearLaser>(), Projectile.damage * 5, 0f, Projectile.owner);
					(proj.ModProjectile as TempleSpearLaser).parent = Projectile;
				}
			}

			if (!stabbing)
			{
				Projectile.velocity = Owner.DirectionTo(OwnerMouse.Value);

				Projectile.timeLeft = 2;

				Timer++;
				
				if (Timer < 15) // Pullback
				{
					float lerper = EaseBuilder.EaseQuinticOut.Ease(Timer / 15f);
					offset = Vector2.Lerp(new Vector2(100, 0), new Vector2(50, 0), lerper);

					if (Timer > 1)
						Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(15, 15), ModContent.DustType<Dusts.GlowFastDecelerate>(), Vector2.Zero, 0, new Color(255, 200, 150), 0.4f);
				}
				else if (Timer < maxCharge + 15) //charging
				{
					offset = new Vector2(50, 0);
				}
				else if (Timer == maxCharge + 15)
				{
					SoundEngine.PlaySound(SoundID.MaxMana, Owner.Center);
					charged = true;

					for (int i = 0; i < 25; i++)
					{
						double rad = i / 25f * 6.28f;
						var offset = new Vector2((float)Math.Sin(rad), (float)Math.Cos(rad));
						offset *= 1.5f;
						Dust.NewDustPerfect(Projectile.Center + offset * 5f, ModContent.DustType<Dusts.GlowFastDecelerate>(), offset, 0, new Color(255, 255, 155), 0.45f);
					}
				}

				oldCharge = (int)Timer;
			}
			else
			{
				Timer++;

				if (Timer < 10)
				{
					float lerper = EaseBuilder.EaseQuinticOut.Ease(Timer / 10f);
					offset = Vector2.Lerp(new Vector2(50, 0), new Vector2(110, 0), lerper);
				}
				else if (Timer < 35f)
				{
					float lerper = EaseBuilder.EaseQuinticIn.Ease((Timer - 10f) / 25f);
					offset = Vector2.Lerp(new Vector2(110, 0), new Vector2(40, 0), lerper);
				}
			}

			Projectile.Center = Owner.MountedCenter + offset.RotatedBy(Projectile.rotation - MathHelper.PiOver2);

			Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.Pi);
			Owner.ChangeDir(Projectile.direction);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D texGlow = ModContent.Request<Texture2D>(Texture + "_Glow").Value;

			Texture2D starTex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "StarTexture").Value;

			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;

			float fade = 1f;
			if (stabbing && Timer > 20f)
				fade = 1f - (Timer - 20) / 15f;

			Vector2 offset = new(0);
			if (!stabbing)
			{
				float lerper = MathHelper.Lerp(0f, 0.65f, Timer / (maxCharge + 15f));
				offset = Main.rand.NextVector2Circular(lerper, lerper);
			}

			offset += new Vector2(-55, 0).RotatedBy(Projectile.rotation - MathHelper.PiOver2);

			Color glowColor = Color.White;

			if (stabbing)
			{
				Color oldColor = Color.Lerp(Color.Transparent, new Color(255, 240, 130), oldCharge / (maxCharge + 15f));
				glowColor = Color.Lerp(oldColor, Color.Transparent, Timer / 25f);
			}
			else
			{
				glowColor = Color.Lerp(Color.Transparent, new Color(255, 240, 130), Timer / (maxCharge + 15f));

				if (Timer >= maxCharge + 15f && Timer < maxCharge + 25f)
					glowColor = Color.Lerp(Color.White, new Color(255, 240, 130), (Timer - (maxCharge + 15f)) / 10f);
			}

			glowColor.A *= 0;

			Main.spriteBatch.Draw(texGlow, Projectile.Center - Main.screenPosition + new Vector2(0, Main.player[Projectile.owner].gfxOffY) + offset, null, glowColor * fade, Projectile.rotation - MathHelper.PiOver4, texGlow.Size() / 2f, Projectile.scale, 0, 0);
			
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + new Vector2(0, Main.player[Projectile.owner].gfxOffY) + offset, null, Color.White * fade, Projectile.rotation - MathHelper.PiOver4, tex.Size() / 2f, Projectile.scale, 0, 0);
			
			Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition - new Vector2(10, 0).RotatedBy(Projectile.rotation - MathHelper.PiOver2), null, glowColor * fade * 0.6f, 0f, bloomTex.Size() / 2f, 0.65f, 0, 0);

			offset = new Vector2(-5, 0).RotatedBy(Projectile.rotation - MathHelper.PiOver2);

			fade = 0f;
			float scale = 0.25f;
			float rot = MathHelper.Lerp(0f, 1.28f, Timer / 15f);
			if (!stabbing)
			{
				if (Timer < 15f)
					fade = 1f - Timer / 15f;
			}
			else
			{
				if (Timer < 20f)
					fade = 1f - Timer / 20f;

				scale = 0.35f;

				rot = MathHelper.Lerp(0f, -2f, Timer / 20f);
			}

			Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition + offset, null, new Color(150, 150, 10, 0) * fade, 0f, bloomTex.Size() / 2f, scale * 1.35f, 0, 0);

			Main.spriteBatch.Draw(starTex, Projectile.Center - Main.screenPosition + offset, null, new Color(150, 150, 10, 0) * fade, rot, starTex.Size() / 2f, scale, 0, 0);

			Main.spriteBatch.Draw(starTex, Projectile.Center - Main.screenPosition + offset, null, new Color(255, 255, 255, 0) * fade, rot, starTex.Size() / 2f, scale * 0.65f, 0, 0);

			Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition + offset, null, new Color(255, 255, 255, 0) * fade * 0.5f, 0f, bloomTex.Size() / 2f, scale * 2.55f, 0, 0);

			return false;
		}
	}

	class TempleSpearLaser : ModProjectile
	{
		private List<Vector2> cache;
		private Trail trail;
		private Trail trail2;

		public Projectile parent;

		public Player Owner => Main.player[Projectile.owner];

		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Point of Light");
		}

		public override void SetDefaults()
		{
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.DamageType = DamageClass.Melee;
		}

		public override void AI()
		{
			if (parent is null || !parent.active)
			{
				Projectile.Kill();
				return;
			}

			Projectile.Center = parent.Center;
			Projectile.timeLeft = parent.timeLeft;

			Projectile.rotation += 0.025f;

			Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4, 4), ModContent.DustType<Dusts.GlowFastDecelerate>(), Owner.DirectionTo((parent.ModProjectile as TempleSpearProjectile).OwnerMouse.Value).RotatedByRandom(0.35f) * Main.rand.NextFloat(3f, 7f), 0, new Color(255, 200, 150), 0.3f);

			if (Main.rand.NextBool(7))
				Dust.NewDustPerfect(Projectile.Center + new Vector2(100, 0f).RotatedBy(parent.rotation - MathHelper.PiOver2) + Main.rand.NextVector2Circular(4, 4), ModContent.DustType<Dusts.GlowFastDecelerate>(), Vector2.UnitY.RotatedByRandom(0.35f) * -Main.rand.NextFloat(2f, 5f), 0, new Color(255, 200, 150), 0.3f);

			if (!Main.dedServ)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public override void Kill(int timeLeft)
		{
			Vector2 pos = Projectile.Center + new Vector2(100, 0f).RotatedBy(parent.rotation - MathHelper.PiOver2);
			Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), pos, Vector2.Zero, ModContent.ProjectileType<TempleSpearLight>(), Projectile.damage, 0f, Projectile.owner);
			proj.rotation = Projectile.rotation;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float useless = 0f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center,
				Projectile.Center + new Vector2(100, 0f).RotatedBy(parent.rotation - MathHelper.PiOver2), 5, ref useless);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D starTex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "StarTexture").Value;

			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;

			DrawLaser(Main.spriteBatch);

			Vector2 laserEnd = Projectile.Center + new Vector2(100, 0f).RotatedBy(parent.rotation - MathHelper.PiOver2) - Main.screenPosition;

			Main.spriteBatch.Draw(bloomTex, laserEnd, null, new Color(150, 150, 10, 0), 0f, bloomTex.Size() / 2f, 0.5f, 0, 0);

			Main.spriteBatch.Draw(starTex, laserEnd, null, new Color(150, 150, 10, 0), Projectile.rotation, starTex.Size() / 2f, 0.25f, 0, 0);

			Main.spriteBatch.Draw(starTex, laserEnd, null, new Color(255, 255, 255, 0), Projectile.rotation, starTex.Size() / 2f, 0.25f, 0, 0);

			Main.spriteBatch.Draw(bloomTex, laserEnd, null, new Color(255, 255, 255, 0) * 0.5f, 0f, bloomTex.Size() / 2f, 1f, 0, 0);

			return false;
		}

		private void ManageCaches()
		{
			cache = new List<Vector2>();
			for (int i = 0; i < 20; i++)
			{
				cache.Add(Vector2.Lerp(Projectile.Center + Projectile.velocity, Projectile.Center + new Vector2(100, 0f).RotatedBy(parent.rotation - MathHelper.PiOver2), i / 20f));
			}

			cache.Add(Projectile.Center + Projectile.velocity + new Vector2(100, 0f).RotatedBy(parent.rotation - MathHelper.PiOver2));
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 21, new TriangularTip(4), factor => 6f, factor => new Color(255, 255, 100) * TrailFade());

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity + new Vector2(100, 0f).RotatedBy(parent.rotation - MathHelper.PiOver2);

			trail2 ??= new Trail(Main.instance.GraphicsDevice, 21, new TriangularTip(4), factor => 3.5f, factor => new Color(255, 255, 255) * TrailFade());

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = Projectile.Center + Projectile.velocity + new Vector2(100, 0f).RotatedBy(parent.rotation - MathHelper.PiOver2);
		}

		private void DrawLaser(SpriteBatch spriteBatch)
		{
			spriteBatch.End();
			Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

			Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * -0.03f);
			effect.Parameters["repeats"].SetValue(1);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "GlowTrail").Value);

			trail?.Render(effect);
			trail2?.Render(effect);

			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "FireTrail").Value);

			trail?.Render(effect);
			trail2?.Render(effect);

			effect.Parameters["time"].SetValue(Projectile.timeLeft * -0.02f);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "EnergyTrail").Value);

			trail?.Render(effect);
			trail2?.Render(effect);

			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
		}

		private float TrailFade()
		{
			return parent.timeLeft / 35f * 0.5f;
		}
	}

	class TempleSpearLight : ModProjectile
	{
		public override string Texture => AssetDirectory.CaveTempleItem + Name;

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.timeLeft = 1800;

			Projectile.friendly = false;
			Projectile.tileCollide = false;
		}

		public override void AI()
		{
			Projectile.rotation += 0.025f;

			Lighting.AddLight(Projectile.Center, new Vector3(1, 1, 1) * Projectile.timeLeft / 1800f);

			if (Main.rand.NextBool(7))
				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4, 4), ModContent.DustType<Dusts.GlowFastDecelerate>(), Vector2.UnitY.RotatedByRandom(0.35f) * -Main.rand.NextFloat(2f, 5f), 0, new Color(255, 200, 150), 0.3f);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D starTex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "StarTexture").Value;

			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;

			lightColor = new Color(150, 150, 10, 0);
			lightColor *= Projectile.timeLeft / 1800f;

			Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, lightColor, 0f, bloomTex.Size() / 2f, 0.5f, 0, 0);

			Main.spriteBatch.Draw(starTex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, starTex.Size() / 2f, 0.25f, 0, 0);

			Main.spriteBatch.Draw(starTex, Projectile.Center - Main.screenPosition, null, new Color(255, 255, 255, 0) * (Projectile.timeLeft / 1800f), Projectile.rotation, starTex.Size() / 2f, 0.25f, 0, 0);

			Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, new Color(255, 255, 255, 0) * (Projectile.timeLeft / 1800f) * 0.5f, 0f, bloomTex.Size() / 2f, 1f, 0, 0);

			return false;
		}
	}
}