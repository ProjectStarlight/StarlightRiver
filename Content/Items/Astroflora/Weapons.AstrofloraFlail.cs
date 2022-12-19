using StarlightRiver.Core.Systems.CameraSystem;
using System;
using System.Linq;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Astroflora
{
	public class AstrofloraFlail : BaseFlailItem
	{
		public override string Texture => AssetDirectory.AstrofloraItem + "AstrofloraFlail";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Astroflora Flail");
			Tooltip.SetDefault("Spinning the flail releases toxic spores that ignite when the flail is thrown");
		}

		public override void SafeSetDefaults()
		{
			Item.Size = new Vector2(34, 30);
			Item.damage = 30;
			Item.rare = ItemRarityID.Green;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.shoot = ModContent.ProjectileType<AstrofloraProj>();
			Item.shootSpeed = 16;
			Item.knockBack = 4;
		}
	}
	public class AstrofloraProj : BaseFlailProj, IDrawAdditive
	{
		public override string Texture => AssetDirectory.AstrofloraItem + "AstrofloraProj";

		public AstrofloraProj() : base(new Vector2(0.7f, 1.3f), new Vector2(0.5f, 3f)) { }

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Astroflora Flail");
		}

		public override void SpinExtras(Player Player)
		{
			if (++Projectile.localAI[0] % 4 == 0)
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Main.rand.NextVector2Circular(1, 1) + Main.player[Projectile.owner].velocity / 5, ModContent.ProjectileType<AstrofloraGas>(), Projectile.damage * 3, 0, Projectile.owner);
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			if (released)
			{
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, default, default);

				Texture2D bloom = ModContent.Request<Texture2D>(Texture + "_bloom").Value;
				spriteBatch.Draw(bloom, Projectile.Center - Main.screenPosition, null, Color.LightGoldenrodYellow * 0.5f, Projectile.rotation, bloom.Size() / 2, Projectile.scale * 1.2f, SpriteEffects.None, 0);

				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, default, default);
			}
		}

		public override void NotSpinningExtras(Player Player)
		{
			Lighting.AddLight(Projectile.Center, Color.LightGoldenrodYellow.ToVector3());
			Rectangle hitbox = Projectile.Hitbox;
			hitbox.Inflate(hitbox.Width / 2, hitbox.Height / 2);
			System.Collections.Generic.IEnumerable<Projectile> gasclouds = Main.projectile.Where(x => x.type == ModContent.ProjectileType<AstrofloraGas>() && x.active && x.owner == Projectile.owner && x.Hitbox.Intersects(hitbox));
			if (gasclouds.Any())
			{
				foreach (Projectile proj in gasclouds)
				{
					if (proj.ai[0] == 0)
					{
						Terraria.Audio.SoundEngine.PlaySound(SoundID.Item110 with { PitchVariance = 0.1f, Volume = 0.6f }, Projectile.Center);
						proj.ai[0] = 1;
						proj.velocity += Projectile.velocity.RotatedByRandom(MathHelper.Pi / 8) / 3;
						for (int i = 0; i < 4; i++)
						{
							var dust = Dust.NewDustDirect(proj.Center + Main.rand.NextVector2Circular(proj.width / 2, proj.height / 2), 0, 0, DustID.GoldCoin);
							dust.noGravity = true;
							dust.velocity *= 4f;
							dust.scale = 1.6f;
							dust.fadeIn = 1f;
						}
					}
				}
			}

			if (++Projectile.localAI[0] % 15 == 0)
			{
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.3f, PitchVariance = 0.2f }, Projectile.Center);
				for (int i = 0; i < 3; i++)
				{
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<AstrofloraSpark>(), 0, 0, Projectile.owner, Main.rand.NextVector2Unit().ToRotation(), Main.rand.NextBool() ? -1 : 1);
				}
			}
		}
	}

	public class AstrofloraGas : ModProjectile, IDrawAdditive
	{
		public override string Texture => AssetDirectory.AstrofloraItem + "AstrofloraProj";

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.Size = new Vector2(40, 40);
			Projectile.tileCollide = true;
			Projectile.timeLeft = 60;
			Projectile.penetrate = -1;
			Projectile.hide = true;
		}

		private bool Ignited => Projectile.ai[0] == 1;
		private static Color green = new(228, 255, 196);
		private static Color gray = new(209, 209, 209);
		private static Color orange = new Color(252, 139, 45) * 1.2f;
		private static Color white = new Color(255, 255, 255) * 1.2f;
		private static readonly float chargetime = 50;

		public override void AI()
		{
			Projectile.velocity *= 0.97f;
			if (!Ignited)
			{
				Dustcloud(green);
			}
			else
			{
				if (++Projectile.ai[1] < chargetime)
				{
					if (Projectile.ai[1] <= chargetime)
						Projectile.scale -= 0.01f;

					Lighting.AddLight(Projectile.Center, Color.Yellow.ToVector3() * 0.75f);
					Projectile.timeLeft = 20;
					if (Main.rand.NextBool(3))
					{
						var dust = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(Projectile.width / 2, Projectile.height / 2), 0, 0, DustID.GoldCoin);
						dust.noGravity = true;
						dust.velocity = Vector2.Normalize(Projectile.Center - dust.position);
						dust.scale = 1.2f;
						dust.fadeIn = 0.5f;
					}

					if (Main.rand.NextBool(5))
						Dustcloud(Color.Lerp(green, white, Math.Min(Projectile.ai[1] / (chargetime / 2), 1)));
				}
				else
				{
					if (Projectile.ai[1] == chargetime)
					{
						Projectile.scale = 1.2f;
						Terraria.Audio.SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.4f, PitchVariance = 0.1f }, Projectile.Center);
						CameraSystem.shake = 8;
					}

					Lighting.AddLight(Projectile.Center, Color.OrangeRed.ToVector3());
					if (Main.rand.NextBool())
					{
						Dustcloud(orange, 4);
						Dustcloud(gray, 8);
						var dust = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(Projectile.width / 2, Projectile.height / 2), 0, 0, 6);
						//dust.noGravity = true;
						dust.velocity *= 4f;
						dust.scale = 0.65f;
					}

					if (Main.rand.NextBool(3))
					{
						var gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, Main.rand.NextVector2Circular(2, 2), GoreID.ChimneySmoke1);
						gore.rotation = Main.rand.NextFloatDirection();
						gore.timeLeft = 10;

					}
				}
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Projectile.velocity = new Vector2(
				Projectile.velocity.X == oldVelocity.X ? oldVelocity.X : -oldVelocity.X / 2,
				Projectile.velocity.Y == oldVelocity.Y ? oldVelocity.Y : -oldVelocity.Y / 2);
			return false;
		}
		private void Dustcloud(Color color, float velocity = 3)
		{
			/*for (int i = 0; i < 5; i++)
			{
				Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, ModContent.DustType<GasDust>());
				dust.position += Main.rand.NextVector2Square(-Projectile.width/2 * Projectile.scale, Projectile.width/2 * Projectile.scale);
				dust.velocity = Main.rand.NextVector2Circular(velocity, velocity) + Projectile.velocity/2;
				dust.scale = 2.4f;
				dust.color = color;
			}*/
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (!Ignited)
			{
				if (target.Hitbox.Intersects(Projectile.Hitbox) && target.CanBeChasedBy(this))
					target.AddBuff(BuffID.Poisoned, 30);

				return false;
			}
			else if (Projectile.ai[1] < chargetime)
			{
				return false;
			}

			return null;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(BuffID.OnFire, 90);
		}

		public override void ModifyDamageHitbox(ref Rectangle hitbox)
		{
			hitbox.Inflate(hitbox.Width, hitbox.Height);
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			if (Ignited && Projectile.ai[1] < chargetime)
			{
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, default, default);

				Texture2D bloom = ModContent.Request<Texture2D>(Texture + "_bloom").Value;
				float opacity = Projectile.ai[1] / chargetime * 0.75f;
				Color color = (Projectile.ai[1] < chargetime / 2) ? Color.White : Color.Lerp(Color.White, Color.Orange, (Projectile.ai[1] - chargetime / 2) / (chargetime / 2));
				spriteBatch.Draw(bloom, Projectile.Center - 2 * Projectile.velocity - Main.screenPosition, null, color * opacity,
					Projectile.rotation, bloom.Size() / 2, MathHelper.Lerp(Projectile.scale, 1, 0.15f) * 1.2f, SpriteEffects.None, 0);

				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, default, default);
			}
			else if (Ignited)
			{
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, default, default);

				Texture2D bloom = ModContent.Request<Texture2D>(Texture + "_bloom").Value;
				float opacity = Projectile.timeLeft / 20f * 2f;
				Color color = Color.Orange;
				spriteBatch.Draw(bloom, Projectile.Center - 2 * Projectile.velocity - Main.screenPosition, null, color * opacity,
					Projectile.rotation, bloom.Size() / 2, 0.2f + (20 - Projectile.timeLeft) / 20f * 2.5f, SpriteEffects.None, 0);

				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, default, default);
			}
		}
	}

	public class AstrofloraSpark : ModProjectile
	{
		public override string Texture => AssetDirectory.AstrofloraItem + "AstrofloraProj";

		public override void SetDefaults()
		{
			Projectile.Size = new Vector2(2, 2);
			Projectile.tileCollide = false;
			Projectile.timeLeft = 30;
			Projectile.hide = true;
		}

		public override void AI()
		{
			Lighting.AddLight(Projectile.Center, Color.LightGoldenrodYellow.ToVector3());
			Vector2 basevel = Vector2.UnitX.RotatedBy(Projectile.ai[0]) * 4;
			if (Main.rand.Next(6) == 0)
				Projectile.ai[1] *= -1;

			Projectile.velocity = basevel.RotatedBy(Projectile.ai[1] * MathHelper.Pi / 6);
			var dust = Dust.NewDustPerfect(Projectile.Center, DustID.GoldCoin, Vector2.Zero);
			dust.noGravity = true;
		}
	}
}
