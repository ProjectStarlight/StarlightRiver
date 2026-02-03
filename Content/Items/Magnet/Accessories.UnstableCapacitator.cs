using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Core.Systems.PixelationSystem;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Magnet
{
	public class UnstableCapacitator : SmartAccessory
	{
		public int cooldown;
		public override string Texture => AssetDirectory.MagnetItem + Name;
		public override void Load()
		{
			StarlightProjectile.OnHitNPCEvent += OnHitProjectile;
		}

		public override void Unload()
		{
			StarlightProjectile.OnHitNPCEvent -= OnHitProjectile;
		}
		public UnstableCapacitator() : base("Unstable Capacitator", 
			"-5% magic critical strike chance\n" +
			"Critically striking enemies with magic weapons consume all of your mana to cast a lightning explosion\n" +
			"The explosion deals more damage based on how much mana was consumed") { }

		public override void SafeUpdateEquip(Player Player)
		{
			if ((GetEquippedInstance(Player) as UnstableCapacitator).cooldown > 0)
				cooldown--;

			Player.GetCritChance(DamageClass.Magic) -= 5;
		}

		private void OnHitProjectile(Projectile proj, NPC target, NPC.HitInfo info, int damageDone)
		{
			if (Equipped(Main.player[proj.owner]) && (GetEquippedInstance(Main.player[proj.owner]) as UnstableCapacitator).cooldown <= 0)
			{
				Player Player = Main.player[proj.owner];

				if (proj.DamageType == DamageClass.Magic && info.Crit)
				{
					(GetEquippedInstance(Main.player[proj.owner]) as UnstableCapacitator).cooldown = 240;

					int manaUsed = Player.statMana;

					float percentOfMax = manaUsed / (float)Player.statManaMax2;

					int damageToDeal = (int)MathHelper.Lerp(20, 100, percentOfMax);

					Player.statMana = 0;

					Projectile.NewProjectile(proj.GetSource_OnHit(target), Main.player[proj.owner].Center, Vector2.UnitY * -5f, ModContent.ProjectileType<UnstableCapacitatorIcon>(), 0, 0f, proj.owner);

					Projectile.NewProjectile(proj.GetSource_OnHit(target), target.Center + new Vector2(Main.rand.NextFloat(-30f, 30f), -500f), Vector2.Zero,
						ModContent.ProjectileType<UnstableCapacitatorBolt>(), damageToDeal, 5f, proj.owner, target.whoAmI);
				}
			}		
		}
	}

	public class UnstableCapacitatorIcon : ModProjectile
	{
		public override string Texture => AssetDirectory.MagnetItem + "UnstableCapacitator";
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Icon");
		}

		public override void SetDefaults()
		{
			Projectile.width = 18;
			Projectile.height = 20;

			Projectile.friendly = false;
			Projectile.hostile = false;

			Projectile.penetrate = -1;

			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;

			Projectile.timeLeft = 60;
		}

		public override void AI()
		{
			Projectile.Center = Main.player[Projectile.owner].Center + new Vector2(0f, -MathHelper.Lerp(0f, 60f, Eases.EaseCircularOut(1f - Projectile.timeLeft / 60f)));
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = Assets.Items.Magnet.UnstableCapacitator.Value;
			Texture2D starTex = Assets.StarTexture_Alt.Value;
			Texture2D bloomTex = Assets.Masks.GlowSoftAlpha.Value;

			SpriteBatch sb = Main.spriteBatch;

			sb.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor * (Projectile.timeLeft / 60f), 0f, tex.Size() / 2f, 1f, 0f, 0f);
			
			if (Projectile.timeLeft > 40)
			{
				float lerper = (Projectile.timeLeft - 40) / 20f;

				sb.Draw(bloomTex, Projectile.Center - new Vector2(0f, 12f) - Main.screenPosition, null, new Color(255, 50, 255, 0) * lerper, 0f, bloomTex.Size() / 2f, 1f, 0f, 0f);

				sb.Draw(starTex, Projectile.Center - new Vector2(0f, 12f) - Main.screenPosition, null, new Color(255, 50, 255, 0) * lerper, 0f, starTex.Size() / 2f, 1f, 0f, 0f);
				sb.Draw(starTex, Projectile.Center - new Vector2(0f, 12f) - Main.screenPosition, null, new Color(255, 255, 255, 0) * lerper, 0f, starTex.Size() / 2f, 1f, 0f, 0f);
			}

			return false;
		}
	}

	public class UnstableCapacitatorBolt : ModProjectile
	{
		public int deathTimer;
		public int flashTimer;

		private List<Vector2> cache;
		private Trail trail;
		private Trail trail2;

		public Vector2 startPos;
		public override string Texture => AssetDirectory.Invisible;
		public int TargetWhoAmI => (int)Projectile.ai[0];
		public float Progress => 1f - Projectile.timeLeft / 30f;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Lightning Explosion");
		}

		public override void SetDefaults()
		{
			Projectile.Size = new Vector2(6);

			Projectile.DamageType = DamageClass.Magic;

			Projectile.hostile = false;
			Projectile.friendly = false;

			Projectile.tileCollide = false;

			// five frame lifetime

			Projectile.timeLeft = 30;
			Projectile.extraUpdates = 6;
		}

		public override void OnSpawn(IEntitySource source)
		{
			SoundHelper.PlayPitched("Magic/LightningCast", 1.2f, 2f, Projectile.Center);

			startPos = Projectile.Center;
			flashTimer = 90;
		}

		public override void AI()
		{
			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}

			if (flashTimer > 0)
				flashTimer--;

			if (deathTimer > 0)
			{
				Projectile.timeLeft = 2;

				deathTimer--;

				if (deathTimer == 1)
					Projectile.Kill();

				return;
			}

			Projectile.Center = Vector2.Lerp(startPos, Main.npc[TargetWhoAmI].Center, Progress) + Main.rand.NextVector2Circular(25f, 25f);
			
			Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<PixelatedGlow>(), Projectile.position.DirectionTo(Projectile.oldPosition), 0, Color.Lerp(new Color(200, 20, 255, 0), new Color(255, 255, 255, 0), Progress), 0.3f);
			
			Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<PixelatedGlow>(), Projectile.position.DirectionTo(Projectile.oldPosition).RotatedByRandom(1f) * Main.rand.NextFloat(0.8f, 3f), 0, new Color(180, 80, 255, 0), 0.2f);

			if (Projectile.timeLeft == 1)
			{
				if (Main.myPlayer == Projectile.owner)
				{
					Projectile.NewProjectile(Projectile.GetSource_FromAI(), Main.npc[TargetWhoAmI].Center, Vector2.Zero, ModContent.ProjectileType<UnstableCapacitatorExplosion>(),
						Projectile.damage, Projectile.knockBack, Projectile.owner, 50);
				}

				SoundHelper.PlayPitched("Magic/LightningStrike", 0.5f, Main.rand.NextFloat(0.9f, 1.1f), Projectile.Center);
				SoundHelper.PlayPitched("Magic/LightningExplode", 1.5f, 0.5f, Projectile.Center);

				for (int i = 0; i < 10; i++)
				{
					Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<PixelatedEmber>(),
						-Vector2.UnitY.RotatedByRandom(1.5f) * Main.rand.NextFloat(0.8f, 2f), 0, new Color(Main.rand.Next(150, 220), 50, 255, 0), 0.1f).customData = Main.rand.NextBool() ? -1 : 1;
				}

				for (int i = 0; i < 10; i++)
				{
					Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<PixelatedGlow>(),
						-Vector2.UnitY.RotatedByRandom(1.5f) * Main.rand.NextFloat(4f, 14f), 0, new Color(Main.rand.Next(150, 220), 50, 255, 0), 0.35f);
				}

				CameraSystem.shake += 4;

				deathTimer = 600;
				Projectile.timeLeft = 2;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			DrawPrimitives();

			Texture2D starTex = Assets.StarTexture_Alt.Value;
			Texture2D bloomTex = Assets.Masks.GlowSoftAlpha.Value;
			Texture2D bloomCircle = Assets.Masks.GlowWithRing.Value;

			SpriteBatch sb = Main.spriteBatch;

			Vector2 pos = startPos - Main.screenPosition;

			float flashProg = flashTimer / 90f;

			if (flashProg > 0)
			{
				Vector2 stretch = new Vector2(MathHelper.Lerp(5f, 1f, Eases.EaseQuinticOut(flashProg)), 1f);
				
				sb.Draw(bloomCircle, pos, null, new Color(200, 80, 255, 0) * flashProg, 0f, bloomCircle.Size() / 2f, MathHelper.Lerp(0.1f, 0.3f, 1f - flashProg), 0f, 0f);
				sb.Draw(bloomCircle, pos, null, new Color(255, 255, 255, 0) * flashProg, 0f, bloomCircle.Size() / 2f, MathHelper.Lerp(0.1f, 0.25f, 1f - flashProg), 0f, 0f);

				sb.Draw(bloomTex, pos, null, new Color(200, 80, 255, 0) * flashProg, 0f, bloomTex.Size() / 2f, 3f, 0f, 0f);
				sb.Draw(bloomTex, pos, null, new Color(255, 255, 255, 0) * flashProg * 0.8f, 0f, bloomTex.Size() / 2f, 2.5f, 0f, 0f);

				sb.Draw(bloomTex, pos, null, new Color(200, 80, 255, 0) * flashProg, 0f, bloomTex.Size() / 2f, stretch, 0f, 0f);
				sb.Draw(bloomTex, pos, null, new Color(255, 255, 255, 0) * flashProg * 0.8f, 0f, bloomTex.Size() / 2f, stretch * 0.75f, 0f, 0f);

				sb.Draw(starTex, pos, null, new Color(200, 80, 255, 0) * flashProg, 0f, starTex.Size() / 2f, stretch, 0f, 0f);
				sb.Draw(starTex, pos, null, new Color(255, 255, 255, 0) * flashProg * 0.8f, 0f, starTex.Size() / 2f, stretch * 0.75f, 0f, 0f);
			}

			return false;
		}

		private void ManageCaches()
		{
			Vector2 offset = new Vector2(Main.rand.NextFloat(-5f, 5f), 0f);

			if (cache == null)
			{
				cache = new List<Vector2>();
				for (int i = 0; i < 100; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			cache.Add(Projectile.Center);

			while (cache.Count > 100)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 100, new TriangularTip(190), factor => factor * 4f, factor =>
			Color.Lerp(Color.White, new Color(150, 65, 255), Progress) * TrailFade());

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center;

			trail2 ??= new Trail(Main.instance.GraphicsDevice, 100, new TriangularTip(190), factor => factor * 12f, factor =>
			Color.Lerp(new Color(210, 100, 255, 0), new Color(85, 65, 255, 0), Progress) * TrailFade());

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = Projectile.Center;
		}

		private float TrailFade()
		{
			if (deathTimer <= 0)
				return 1f;
			else 
				return deathTimer / 600f;
		}

		public void DrawPrimitives()
		{
			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderTiles", () =>
			{
				Effect effect = ShaderLoader.GetShader("CeirosRing").Value;

				if (effect != null)
				{
					var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
					Matrix view = Matrix.Identity;
					var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

					effect.Parameters["time"].SetValue(Projectile.timeLeft * -0.05f);
					effect.Parameters["repeats"].SetValue(1);
					effect.Parameters["transformMatrix"].SetValue(world * view * projection);
					effect.Parameters["sampleTexture"].SetValue(Assets.GlowTrail.Value);
					
					trail2?.Render(effect);
					trail?.Render(effect);
				}
			});
		}
	}

	public class UnstableCapacitatorExplosion : ModProjectile
	{
		// not sure how performance intensive this is

		private List<Vector2> cache;
		private List<Vector2> cache2;

		private Trail trail;
		private Trail trail2;

		private Trail trail3;
		private Trail trail4;
		public override string Texture => AssetDirectory.Invisible;
		private float Progress => Utils.Clamp(1 - Projectile.timeLeft / 30f, 0f, 1f);

		private float Radius => Projectile.ai[0] * Eases.EaseBackOut(Progress);

		public override void SetDefaults()
		{
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 30;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 20;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Lightning Explosion");
		}

		public override void AI()
		{
			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}

			if (Main.rand.NextBool(3))
			{
				for (int k = 0; k < 6; k++)
				{
					float rot = Main.rand.NextFloat(0, 6.28f);

					Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedBy(rot) * Radius, ModContent.DustType<PixelatedGlow>(),
						Vector2.One.RotatedBy(rot) * 0.5f, 0, new Color(Main.rand.Next(150, 220), 50, 255, 0), Main.rand.NextFloat(0.15f, 0.25f)).noGravity = true;
				}
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

			Texture2D starTex = Assets.StarTexture_Alt.Value;
			Texture2D bloomTex = Assets.Masks.GlowSoftAlpha.Value;
			Texture2D bloomCircle = Assets.Masks.GlowWithRing.Value;

			SpriteBatch sb = Main.spriteBatch;

			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("OverPlayers", () =>
			{
				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.EffectMatrix);

				sb.Draw(bloomCircle, Projectile.Center - Main.screenPosition, null, new Color(220, 150, 255, 0) * (1f - Progress), 0f, bloomCircle.Size() / 2f, MathHelper.Lerp(0.2f, 0.45f, Eases.EaseCircularOut(Progress)), 0f, 0f);

				Effect effect = ShaderLoader.GetShader("ElectricExplosion").Value;

				if (effect is null)
					return;

				if (Progress >= 1f)
					return;

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, BlendState.AlphaBlend, Main.DefaultSamplerState, default, Main.Rasterizer, effect, Main.GameViewMatrix.EffectMatrix);

				effect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.005f);

				effect.Parameters["uImage1"].SetValue(Assets.Noise.ElectricNoise.Value);
				effect.Parameters["uImage2"].SetValue(Assets.Noise.PerlinNoise.Value);
				effect.Parameters["uProgress"].SetValue(Eases.EaseQuinticIn(Progress));
				Color color = Color.Lerp(new Color(210, 100, 255), new Color(255, 255, 255), Eases.EaseQuinticIn(1f - Progress));

				effect.Parameters["uColor"].SetValue(color.ToVector4());
				effect.Parameters["uOpacity"].SetValue(0f);

				effect.CurrentTechnique.Passes[0].Apply();

				sb.Draw(bloomCircle, Projectile.Center - Main.screenPosition, null, Color.White, 0f, bloomCircle.Size() / 2f, MathHelper.Lerp(0.2f, 0.45f, Eases.EaseCircularOut(Progress)), 0f, 0f);

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
			});

			return false;
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

			float strength = MathHelper.Lerp(15f, 5f, Progress);

			for (int k = 0; k < 40; k++)
			{
				cache[k] = Projectile.Center + Main.rand.NextVector2CircularEdge(strength, strength) + Vector2.One.RotatedBy(k / 38f * 6.28f) * Radius;
			}

			while (cache.Count > 40)
			{
				cache.RemoveAt(0);
			}

			if (cache2 is null)
			{
				cache2 = [];

				for (int i = 0; i < 40; i++)
				{
					cache2.Add(Projectile.Center);
				}
			}

			strength = MathHelper.Lerp(3f, 1f, Progress);

			for (int k = 0; k < 40; k++)
			{
				cache2[k] = Projectile.Center + Main.rand.NextVector2CircularEdge(strength, strength) + Vector2.One.RotatedBy(k / 38f * 6.28f) * Radius * 0.85f;
			}

			while (cache2.Count > 40)
			{
				cache2.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 40, new TriangularTip(1), factor => 9 * (1f - Progress),
				factor => Color.White);

			trail2 ??= new Trail(Main.instance.GraphicsDevice, 40, new TriangularTip(1), factor => 18 * (1f - Progress),
				factor => Color.Lerp(new Color(210, 100, 255, 0), new Color(85, 65, 255, 0), Eases.EaseQuinticInOut(1f - Progress)));

			trail.Positions = cache.ToArray();
			trail.NextPosition = cache[39];

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = cache[39];

			trail3 ??= new Trail(Main.instance.GraphicsDevice, 40, new TriangularTip(1), factor => 9 * (1f - Progress),
				factor => Color.White);

			trail4 ??= new Trail(Main.instance.GraphicsDevice, 40, new TriangularTip(1), factor => 18 * (1f - Progress),
				factor => Color.Lerp(new Color(210, 100, 255, 0), new Color(85, 65, 255, 0), Eases.EaseQuinticInOut(1f - Progress)));

			trail3.Positions = cache2.ToArray();
			trail3.NextPosition = cache2[39];

			trail4.Positions = cache2.ToArray();
			trail4.NextPosition = cache2[39];
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
					effect.Parameters["sampleTexture"].SetValue(Assets.GlowTrail.Value);
					
					trail3?.Render(effect);
					trail4?.Render(effect);

					trail2?.Render(effect);
					trail?.Render(effect);
				}
			});
		}
	}
}
