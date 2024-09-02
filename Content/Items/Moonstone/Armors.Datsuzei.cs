﻿using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Moonstone
{
	public class Datsuzei : InworldItem
	{
		public static int activationTimer = 0; //static since this is clientside only and there really shouldnt ever be more than one of these in that context
		public int comboState = 0;

		public static ParticleSystem sparkles;

		public override string Texture => AssetDirectory.MoonstoneItem + Name;

		public override bool VisibleInUI => false;

		public override void Load()
		{
			StarlightPlayer.PostUpdateEvent += PlayerFrame;
			On_Main.DrawInterface_30_Hotbar += OverrideHotbar;
			activationTimer = 0;
			sparkles = new ParticleSystem(AssetDirectory.Dust + "Aurora", updateSparkles, ParticleSystem.AnchorOptions.UI);
		}

		public override void Unload()
		{
			StarlightPlayer.PostUpdateEvent -= PlayerFrame;
			On_Main.DrawInterface_30_Hotbar -= OverrideHotbar;
			sparkles = null;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Datsuzei");
			Tooltip.SetDefault("Unleash the moon");
		}

		public override void SetDefaults()
		{
			Item.DamageType = DamageClass.Melee;
			Item.damage = 50;
			Item.width = 16;
			Item.height = 16;
			Item.useStyle = ItemUseStyleID.Thrust;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.shoot = ProjectileType<DatsuzeiProjectile>();
			Item.shootSpeed = 20;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.crit = 10;
		}

		private void OverrideHotbar(On_Main.orig_DrawInterface_30_Hotbar orig, Main self)
		{
			orig(self);

			if (Main.LocalPlayer.HeldItem.type != ItemType<Datsuzei>())
			{
				if (activationTimer > 0)
				{
					activationTimer -= 2;
				}
				else
				{
					activationTimer = 0;
					sparkles.ClearParticles();
				}
			}

			if (activationTimer > 0 && !Main.playerInventory)
			{
				int activationTimerNoCurve = Datsuzei.activationTimer;
				float activationTimer = Helper.BezierEase(Math.Min(1, activationTimerNoCurve / 60f));

				var hideTarget = new Rectangle(20, 20, 446, 52);

				if (!Main.screenTarget.IsDisposed)
					Main.spriteBatch.Draw(Main.screenTarget, hideTarget, hideTarget, Color.White * activationTimer);

				Texture2D backTex = Assets.Items.Moonstone.DatsuzeiHotbar.Value;
				var target = new Rectangle(111, 20, (int)(backTex.Width * activationTimer), backTex.Height);
				var source = new Rectangle(0, 0, (int)(backTex.Width * activationTimer), backTex.Height);

				Main.spriteBatch.Draw(backTex, target, source, Color.White);

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, BlendState.Additive, Main.DefaultSamplerState, default, default, default, Main.UIScaleMatrix);

				Texture2D glowTex = Assets.Items.Moonstone.DatsuzeiHotbarGlow.Value;

				var glowColor = new Color(200, (byte)(255 - 100 * activationTimer), 255);

				if (activationTimerNoCurve > 50)
					glowColor *= (60 - activationTimerNoCurve) / 10f;

				if (activationTimerNoCurve < 10)
					glowColor *= activationTimerNoCurve / 10f;

				Main.spriteBatch.Draw(glowTex, target.TopLeft() + new Vector2(target.Width, backTex.Height / 2), null, glowColor, 0, glowTex.Size() / 2, 1, 0, 0);

				if (activationTimer >= 1)
				{
					Texture2D glowTex2 = Assets.Items.Moonstone.DatsuzeiHotbarGlow2.Value;
					Color glowColor2 = new Color(200, (byte)(200 - 50 * (float)Math.Sin(Main.GameUpdateCount * 0.05f)), 255) * Math.Min(1, (activationTimerNoCurve - 60) / 60f);

					Main.spriteBatch.Draw(glowTex2, target.Center.ToVector2() + Vector2.UnitY * -1, null, glowColor2 * 0.8f, 0, glowTex2.Size() / 2, 1, 0, 0);
				}

				sparkles.DrawParticles(Main.spriteBatch);

				//the shader for the flames
				Effect effect1 = Filters.Scene["MagicalFlames"].GetShader().Shader;
				effect1.Parameters["sampleTexture1"].SetValue(Assets.Items.Moonstone.DatsuzeiFlameMap1.Value);
				effect1.Parameters["sampleTexture2"].SetValue(Assets.Items.Moonstone.DatsuzeiFlameMap2.Value);
				effect1.Parameters["uTime"].SetValue(Main.GameUpdateCount * 0.008f);

				if (activationTimerNoCurve > 85)
				{
					Main.spriteBatch.End();
					Main.spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, default, effect1, Main.UIScaleMatrix);

					Texture2D spearTex = Assets.Items.Moonstone.DatsuzeiHotbarSprite.Value;
					Main.spriteBatch.Draw(spearTex, target.Center() + new Vector2(0, -40), null, Color.White, 0, spearTex.Size() / 2, 1, 0, 0);
				}

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, default, default, Main.UIScaleMatrix);

				if (activationTimerNoCurve >= 80)
				{
					int overlayTime = activationTimerNoCurve - 80;
					float overlayAlpha;

					if (overlayTime < 5)
						overlayAlpha = 1 - overlayTime / 5f;
					else if (overlayTime <= 25)
						overlayAlpha = 0;
					else if (overlayTime > 25)
						overlayAlpha = (overlayTime - 25) / 15f;
					else
						overlayAlpha = 1;

					Texture2D spearShapeTex = Assets.Items.Moonstone.DatsuzeiHotbarSpriteShape.Value;
					Main.spriteBatch.Draw(spearShapeTex, target.Center() + new Vector2(0, -40), null, Color.White * (1 - overlayAlpha), 0, spearShapeTex.Size() / 2, 1, 0, 0);
				}

				//particles!
				if (activationTimer < 1)
				{
					for (int k = 0; k < 3; k++)
					{
						sparkles.AddParticle(
							new Particle(
								new Vector2(111 + backTex.Width * activationTimer, 20 + Main.rand.Next(backTex.Height)),
								new Vector2(Main.rand.NextFloat(-0.6f, -0.3f), Main.rand.NextFloat(3f)),
								Main.rand.NextFloat(6.28f),
								1,
								Color.White,
								60,
								new Vector2(Main.rand.NextFloat(0.15f, 0.2f), Main.rand.NextFloat(6.28f)),
								new Rectangle(0, 0, 100, 100)));
					}
				}

				if (Main.rand.NextBool(4))
				{
					sparkles.AddParticle(
						new Particle(
							new Vector2(111, 20) + new Vector2(Main.rand.Next(backTex.Width),
							Main.rand.Next(backTex.Height)),
							new Vector2(0, Main.rand.NextFloat(0.4f)),
							0,
							0,
							new Color(255, 230, 0),
							120,
							new Vector2(Main.rand.NextFloat(0.05f, 0.15f), 0.02f),
							new Rectangle(0, 0, 100, 100)));
				}
			}
		}

		private static void updateSparkles(Particle particle)
		{
			particle.Timer--;

			if (particle.Velocity.X == 0)
			{
				particle.Scale = (float)Math.Sin(particle.Timer / 120f * 3.14f) * particle.StoredPosition.X;
				particle.Color = new Color(180, 100 + (byte)(particle.Timer / 120f * 155), 255) * (float)Math.Sin(particle.Timer / 120f * 3.14f);
			}
			else
			{
				particle.Scale = particle.Timer / 60f * particle.StoredPosition.X;
				particle.Color = new Color(180, 100 + (byte)(particle.Timer / 60f * 155), 255) * (particle.Timer / 45f);
				particle.Color *= (float)(0.5f + Math.Sin(particle.Timer / 20f * 6.28f + particle.StoredPosition.Y) * 0.5f);
			}

			particle.Rotation += 0.05f;
			particle.Position += particle.Velocity;
		}

		private void PlayerFrame(Player player)
		{
			Projectile proj = Main.projectile.FirstOrDefault(n => n.active && n.type == ProjectileType<DatsuzeiProjectile>() && n.owner == player.whoAmI);

			if (proj != null && proj.ai[0] == -1)
				player.bodyFrame = new Rectangle(0, 56 * 1, 40, 56);
		}

		public override bool CanUseItem(Player player)
		{
			return !Main.projectile.Any(n => n.active && n.type == ProjectileType<DatsuzeiProjectile>() && n.owner == player.whoAmI);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			Helper.PlayPitched("Magic/HolyCastShort", 1, comboState / 4f, player.Center);

			switch (comboState)
			{
				case 0:
					int i = Projectile.NewProjectile(source, player.Center, velocity, ProjectileType<DatsuzeiProjectile>(), damage, knockback, player.whoAmI, 0, 40);
					break;

				case 1:
					i = Projectile.NewProjectile(source, player.Center, velocity, ProjectileType<DatsuzeiProjectile>(), damage, knockback, player.whoAmI, 1, 30);
					break;

				case 2:
					i = Projectile.NewProjectile(source, player.Center, velocity, ProjectileType<DatsuzeiProjectile>(), damage, knockback, player.whoAmI, 2, 30);
					break;

				case 3:
					i = Projectile.NewProjectile(source, player.Center, velocity, ProjectileType<DatsuzeiProjectile>(), damage, knockback, player.whoAmI, 3, 120);
					break;
			}

			comboState++;
			if (comboState > 3)
				comboState = 0;

			return false;
		}

		public override void HoldItem(Player Player)
		{
			if (Player.whoAmI == Main.myPlayer)
			{
				if (!(Player.armor[0].ModItem is MoonstoneHead) || !(Player.armor[0].ModItem as MoonstoneHead).IsArmorSet(Player))
				{
					Item.TurnToAir();
					Main.LocalPlayer.QuickSpawnItem(null, Main.mouseItem, Main.mouseItem.stack); //null since we don't want anything to get any ideas about altering this item being spawned
					Main.mouseItem = new Item();
				}

				if (activationTimer < 120)
					activationTimer++;
			}
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips[0].OverrideColor = new Color(100, 255, 255);
		}
	}

	public class DatsuzeiProjectile : ModProjectile, IDrawPrimitive
	{
		private List<Vector2> cache;
		private Trail trail;
		private Trail trail2;

		private List<Vector2> cacheBack;
		private Trail trailBack;

		private float storedRotation;
		private Vector2 storedPos;

		private bool hasSetTimeLeft = false;

		public override string Texture => AssetDirectory.MoonstoneItem + Name;

		public ref float ComboState => ref Projectile.ai[0];
		public ref float Maxtime => ref Projectile.ai[1];
		public float Timer => Maxtime - Projectile.timeLeft;

		public Player Owner => Main.player[Projectile.owner];

		public override void SetDefaults()
		{
			Projectile.DamageType = DamageClass.Melee;
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 1;
		}

		public override void AI()
		{
			if (!hasSetTimeLeft)
			{
				Projectile.timeLeft = (int)Maxtime;
				hasSetTimeLeft = true;
			}

			Owner.heldProj = Projectile.whoAmI;

			if (ComboState != -1 && Timer % 2 == 0)
			{
				for (int k = 0; k < 3; k++)
					Dust.NewDustPerfect(Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * 140 + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(15), DustType<Dusts.Glow>(), Vector2.Zero, 0, new Color(50, 50, 255), 0.4f);

				if (Main.rand.NextBool(2))
				{
					var d = Dust.NewDustPerfect(Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * 140 + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(15), DustType<Dusts.Aurora>(), Vector2.Zero, 0, new Color(20, 20, 100), 0.8f);
					d.customData = Main.rand.NextFloat(0.6f, 1.3f);
				}
			}

			switch (ComboState)
			{
				case -1: //spawning

					Projectile.rotation = 1.57f;

					if (Timer == 1)
					{
						Helper.PlayPitched("Impacts/Clink", 1, 0, Projectile.Center);
						Helper.PlayPitched("Magic/MysticCast", 1, -0.2f, Projectile.Center);

						for (int k = 0; k < 40; k++)
						{
							float dustRot = Main.rand.NextFloat(6.28f);
							Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedBy(dustRot) * 64, DustType<Dusts.GlowLine>(), Vector2.One.RotatedBy(dustRot) * Main.rand.NextFloat(5), 0, new Color(100, 255, 255), 0.5f);
						}
					}

					if (Timer < 20)
						Projectile.scale = Timer / 20f;

					if (Timer < 120)
					{
						Projectile.Center = Owner.Center + new Vector2(0, -240);
					}
					else
					{
						Projectile.Center = Owner.Center + Vector2.SmoothStep(new Vector2(0, -240), Vector2.Zero, (Timer - 120) / 40f);
						Projectile.alpha = (int)((Timer - 120) / 40f * 255);
					}

					if (Timer == 159)
					{
						for (int k = 0; k < 40; k++)
						{
							float dustRot = Main.rand.NextFloat(6.28f);
							Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedBy(dustRot) * 64, DustType<Dusts.GlowLine>(), Vector2.One.RotatedBy(dustRot) * Main.rand.NextFloat(2), 0, new Color(100, 255, 255), 0.5f);
						}
					}

					break;

				case 0: //wide swing

					if (Timer == 1)
						storedRotation = Projectile.velocity.ToRotation();

					float rot = storedRotation + (-1.5f + Helper.BezierEase(Timer / 40f) * 3f);
					Projectile.Center = Owner.Center + Vector2.UnitX.RotatedBy(rot) * (-30 + (float)Math.Sin(Timer / 40f * 3.14f) * 100);
					Projectile.rotation = rot;

					break;

				case 1: //thin swing

					if (Timer == 1)
						storedRotation = Projectile.velocity.ToRotation();

					rot = storedRotation + (1f - Helper.BezierEase(Timer / 30f) * 2f);
					Projectile.Center = Owner.Center + Vector2.UnitX.RotatedBy(rot) * (-30 + (float)Math.Sin(Timer / 30f * 3.14f) * 100);
					Projectile.rotation = rot;

					break;

				case 2: //stab

					if (Timer == 1)
						storedRotation = Projectile.velocity.ToRotation();

					Projectile.Center = Owner.Center + Vector2.UnitX.RotatedBy(storedRotation) * (-120 + (float)Math.Sin(Timer / 30f * 3.14f) * 240);
					Projectile.rotation = storedRotation;

					break;

				case 3: //spin

					Projectile.rotation += Projectile.velocity.Length() / 200f * 6.28f;
					Projectile.velocity *= 0.97f;

					if (Timer == 60)
						storedPos = Projectile.Center;

					if (Timer > 60)
						Projectile.Center = Vector2.SmoothStep(storedPos, Owner.Center, (Timer - 60) / 60f);
					break;
			}

			if (Timer > 1 && Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return ComboState != -1 &&
				Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center - Vector2.UnitX.RotatedBy(Projectile.rotation) * 140, Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * 140);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Helper.PlayPitched("Magic/FireHit", 0.5f, 1f, target.Center);

			for (int k = 0; k < 40; k++)
			{
				float dustRot = Main.rand.NextFloat(6.28f);
				Dust.NewDustPerfect(target.Center + Vector2.One.RotatedBy(dustRot) * 32, DustType<Dusts.GlowLine>(), Vector2.One.RotatedBy(dustRot) * Main.rand.NextFloat(2), 0, new Color(50, 50, 255), 0.3f);
			}

			for (int k = 0; k < 10; k++)
			{
				float dustRot = (target.Center - Owner.Center).ToRotation() + Main.rand.NextFloat(-0.5f, 0.5f);
				Dust.NewDustPerfect(target.Center + Vector2.UnitX.RotatedBy(dustRot) * 128, DustType<Dusts.GlowLine>(), Vector2.UnitX.RotatedBy(dustRot) * Main.rand.NextFloat(4), 0, new Color(50, 50, 255), 0.8f);
				Dust.NewDustPerfect(target.Center, DustType<Dusts.Glow>(), Vector2.UnitX.RotatedBy(dustRot) * Main.rand.NextFloat(8), 0, new Color(50, 50, 255), 0.8f);
			}

			if (CameraSystem.shake <= 10)
				CameraSystem.shake += 10;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;
			if (ComboState == -1)
			{
				Texture2D tex = Request<Texture2D>(Texture).Value;
				spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor * (1 - Projectile.alpha / 255f), Projectile.rotation, new Vector2(tex.Width / 2, tex.Height / 2), Projectile.scale, 0, 0);

				Texture2D texShape = Request<Texture2D>(Texture + "Shape").Value;
				float shapeOpacity = 0;

				if (Timer < 5)
					shapeOpacity = Timer / 5f;
				else if (Timer < 25)
					shapeOpacity = 1;
				else if (Timer < 40)
					shapeOpacity = 1 - (Timer - 25) / 15f;

				spriteBatch.Draw(texShape, Projectile.Center - Main.screenPosition, null, Color.White * shapeOpacity, Projectile.rotation, new Vector2(texShape.Width / 2, texShape.Height / 2), Projectile.scale, 0, 0);
			}
			else
			{
				Texture2D tex = Request<Texture2D>(Texture + "Long").Value;
				spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(200, 200, 255) * (1 - Projectile.alpha / 255f), Projectile.rotation, new Vector2(tex.Width / 2, tex.Height / 2), Projectile.scale, 0, 0);
			}

			return false;
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 50; i++)
				{
					cache.Add(Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * 140);
				}
			}

			cache.Add(Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * 140);

			while (cache.Count > 50)
			{
				cache.RemoveAt(0);
			}

			if (cacheBack == null)
			{
				cacheBack = new List<Vector2>();

				for (int i = 0; i < 50; i++)
				{
					cacheBack.Add(Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * -120);
				}
			}

			cacheBack.Add(Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * -120);

			while (cacheBack.Count > 50)
			{
				cacheBack.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			if (trail is null || trail.IsDisposed)
			{
				trail = new Trail(Main.instance.GraphicsDevice, 50, new NoTip(), factor => 10 + factor * 25, factor =>
							{
								if (factor.X == 1)
									return Color.Transparent;

								return new Color(120, 20 + (int)(100 * factor.X), 255) * factor.X * (float)Math.Sin(Projectile.timeLeft / Maxtime * 3.14f);
							});
			}

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;

			if (trail2 is null || trail2.IsDisposed)
			{
				trail2 = new Trail(Main.instance.GraphicsDevice, 50, new NoTip(), factor => 80 + 0 + factor * 0, factor =>
							{
								if (factor.X == 1)
									return Color.Transparent;

								return new Color(100, 20 + (int)(60 * factor.X), 255) * factor.X * 0.15f * (float)Math.Sin(Projectile.timeLeft / Maxtime * 3.14f);
							});
			}

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = Projectile.Center + Projectile.velocity;

			if (trailBack is null || trailBack.IsDisposed)
			{
				trailBack = new Trail(Main.instance.GraphicsDevice, 50, new NoTip(), factor => 20 + 0 + factor * 0, factor =>
							{
								if (factor.X == 1)
									return Color.Transparent;

								return new Color(100, 20 + (int)(60 * factor.X), 255) * factor.X * (float)Math.Sin(Projectile.timeLeft / Maxtime * 3.14f);
							});
			}

			trailBack.Positions = cacheBack.ToArray();
			trailBack.NextPosition = Projectile.Center + Projectile.velocity;
		}

		public void DrawPrimitives()
		{
			if (ComboState == -1)
				return;

			Effect effect = Filters.Scene["DatsuzeiTrail"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.02f);
			effect.Parameters["repeats"].SetValue(8f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(Assets.GlowTrail.Value);
			effect.Parameters["sampleTexture2"].SetValue(Assets.Items.Moonstone.DatsuzeiFlameMap2.Value);

			trail?.Render(effect);

			if (ComboState == 3)
				trailBack?.Render(effect);

			effect.Parameters["sampleTexture2"].SetValue(TextureAssets.MagicPixel.Value);

			trail2?.Render(effect);
		}
	}
}