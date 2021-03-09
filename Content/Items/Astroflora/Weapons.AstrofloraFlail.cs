using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.Astroflora
{
	public class AstrofloraFlail : BaseFlailItem
	{
		public override string Texture => AssetDirectory.AstrofloraItem + "AstrofloraFlail";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Astroflora Flail");
			Tooltip.SetDefault("Spinning the flail creates a toxic mist\nThe toxic mist ignites when the flail is thrown");
		}

		public override void SafeSetDefaults()
		{
			item.Size = new Vector2(34, 30);
			item.damage = 30;
			item.rare = ItemRarityID.Green;
			item.useTime = 30;
			item.useAnimation = 30;
			item.shoot = ModContent.ProjectileType<AstrofloraProj>();
			item.shootSpeed = 16;
			item.knockBack = 4;
		}
	}
	public class AstrofloraProj : BaseFlailProj, IDrawAdditive
	{
		public override string Texture => AssetDirectory.AstrofloraItem + "AstrofloraProj";

		public AstrofloraProj() : base(new Vector2(0.7f, 1.3f), new Vector2(0.5f, 3f)) { }

		public override void SetStaticDefaults() => DisplayName.SetDefault("Astroflora Flail");

		public override void SpinExtras(Player player)
		{
			if(++projectile.localAI[0] % 4 == 0)
				Projectile.NewProjectile(projectile.Center, Main.rand.NextVector2Circular(1, 1) + (Main.player[projectile.owner].velocity / 5), mod.ProjectileType("AstrofloraGas"), projectile.damage * 3, 0, projectile.owner);
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			if (released)
			{
				spriteBatch.End(); spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
				Texture2D bloom = mod.GetTexture(Texture.Remove(0, mod.Name.Length + 1) + "_bloom");
				spriteBatch.Draw(bloom, projectile.Center - Main.screenPosition, null, Color.LightGoldenrodYellow * 0.5f, projectile.rotation, bloom.Size() / 2, projectile.scale * 1.2f, SpriteEffects.None, 0);
				spriteBatch.End(); spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
			}
		}

		public override void NotSpinningExtras(Player player)
		{
			Lighting.AddLight(projectile.Center, Color.LightGoldenrodYellow.ToVector3());
			Rectangle hitbox = projectile.Hitbox;
			hitbox.Inflate(hitbox.Width / 2, hitbox.Height / 2);
			var gasclouds = Main.projectile.Where(x => x.type == mod.ProjectileType("AstrofloraGas") && x.active && x.owner == projectile.owner && x.Hitbox.Intersects(hitbox));
			if (gasclouds.Any())
			{
				foreach (Projectile proj in gasclouds)
				{
					if (proj.ai[0] == 0)
					{
						Main.PlaySound(new LegacySoundStyle(SoundID.Item, 110).WithPitchVariance(0.1f).WithVolume(0.6f), projectile.Center);
						proj.ai[0] = 1;
						proj.velocity += projectile.velocity.RotatedByRandom(MathHelper.Pi / 8) / 3;
						for (int i = 0; i < 4; i++)
						{
							Dust dust = Dust.NewDustDirect(proj.Center + Main.rand.NextVector2Circular(proj.width / 2, proj.height / 2), 0, 0, DustID.GoldCoin);
							dust.noGravity = true;
							dust.velocity *= 4f;
							dust.scale = 1.6f;
							dust.fadeIn = 1f;
						}
					}
				}
			}

			if (++projectile.localAI[0] % 15 == 0)
			{
				Main.PlaySound(new LegacySoundStyle(SoundID.Item, 122).WithPitchVariance(0.2f).WithVolume(0.3f), projectile.Center);
				for (int i = 0; i < 3; i++)
				{
					Projectile.NewProjectile(projectile.Center, Vector2.Zero, mod.ProjectileType("AstrofloraSpark"), 0, 0, projectile.owner, Main.rand.NextVector2Unit().ToRotation(), Main.rand.NextBool() ? -1 : 1);
				}
			}
		}
	}

	public class AstrofloraGas : ModProjectile, IDrawAdditive
	{
		public override string Texture => AssetDirectory.AstrofloraItem + "AstrofloraProj";

		public override void SetDefaults()
		{
			projectile.friendly = true;
			projectile.melee = true;
			projectile.Size = new Vector2(40, 40);
			projectile.tileCollide = true;
			projectile.timeLeft = 60;
			projectile.penetrate = -1;
			projectile.hide = true;
		}

		private bool Ignited => projectile.ai[0] == 1;
		private static Color green = new Color(228, 255, 196);
		private static Color gray = new Color(209, 209, 209);
		private static Color orange = new Color(252, 139, 45) * 1.2f;
		private static Color white = new Color(255, 255, 255) * 1.2f;
		private static readonly float chargetime = 50;

		public override void AI()
		{
			projectile.velocity *= 0.97f;
			if (!Ignited)
				Dustcloud(green);

			else
			{
				if (++projectile.ai[1] < chargetime)
				{
					if (projectile.ai[1] <= chargetime)
						projectile.scale -= 0.01f;

					Lighting.AddLight(projectile.Center, Color.Yellow.ToVector3() * 0.75f);
					projectile.timeLeft = 20;
					if (Main.rand.NextBool(3))
					{
						Dust dust = Dust.NewDustDirect(projectile.Center + Main.rand.NextVector2Circular(projectile.width / 2, projectile.height / 2), 0, 0, DustID.GoldCoin);
						dust.noGravity = true;
						dust.velocity = Vector2.Normalize(projectile.Center - dust.position);
						dust.scale = 1.2f;
						dust.fadeIn = 0.5f;
					}
					if(Main.rand.NextBool(5))
						Dustcloud(Color.Lerp(green, white, Math.Min(projectile.ai[1] / (chargetime / 2), 1)));
				}
				else
				{
					if (projectile.ai[1] == chargetime)
					{
						projectile.scale = 1.2f;
						Main.PlaySound(new LegacySoundStyle(SoundID.Item, 14).WithVolume(0.4f).WithPitchVariance(0.1f), projectile.Center);
						Main.player[projectile.owner].GetModPlayer<StarlightPlayer>().Shake = 8;
					}
					Lighting.AddLight(projectile.Center, Color.OrangeRed.ToVector3());
					if (Main.rand.NextBool())
					{
						Dustcloud(orange, 4);
						Dustcloud(gray, 8);
						Dust dust = Dust.NewDustDirect(projectile.Center + Main.rand.NextVector2Circular(projectile.width / 2, projectile.height / 2), 0, 0, 6);
						//dust.noGravity = true;
						dust.velocity *= 4f;
						dust.scale = 0.65f;
					}
					if(Main.rand.NextBool(3))
					{
						Gore gore = Gore.NewGoreDirect(projectile.Center, Main.rand.NextVector2Circular(2, 2), GoreID.ChimneySmoke1);
						gore.rotation = Main.rand.NextFloatDirection();
						gore.timeLeft = 10;

					}
				}
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			projectile.velocity = new Vector2(
				projectile.velocity.X == oldVelocity.X ? oldVelocity.X : -oldVelocity.X / 2,
				projectile.velocity.Y == oldVelocity.Y ? oldVelocity.Y : -oldVelocity.Y / 2);
			return false;
		}
		private void Dustcloud(Color color, float velocity = 3)
		{
			for (int i = 0; i < 5; i++)
			{
				Dust dust = Dust.NewDustDirect(projectile.Center, 0, 0, mod.DustType("GasDust"));
				dust.position += Main.rand.NextVector2Square(-projectile.width/2 * projectile.scale, projectile.width/2 * projectile.scale);
				dust.velocity = Main.rand.NextVector2Circular(velocity, velocity) + projectile.velocity/2;
				dust.scale = 2.4f;
				dust.color = color;
			}
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (!Ignited)
			{
				if (target.Hitbox.Intersects(projectile.Hitbox) && target.CanBeChasedBy(this))
					target.AddBuff(BuffID.Poisoned, 30);
				return false;
			}
			else if (projectile.ai[1] < chargetime)
				return false;
			return null;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) => target.AddBuff(BuffID.OnFire, 90);

		public override void ModifyDamageHitbox(ref Rectangle hitbox) => hitbox.Inflate(hitbox.Width, hitbox.Height);

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			if (Ignited && projectile.ai[1] < chargetime)
			{
				spriteBatch.End(); spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
				Texture2D bloom = mod.GetTexture(Texture.Remove(0, mod.Name.Length + 1) + "_bloom");
				float opacity = (projectile.ai[1] / chargetime) * 0.75f;
				Color color = (projectile.ai[1] < chargetime / 2) ? Color.White : Color.Lerp(Color.White, Color.Orange, (projectile.ai[1] - chargetime / 2) / (chargetime / 2));
				spriteBatch.Draw(bloom, projectile.Center - (2 * projectile.velocity) - Main.screenPosition, null, color * opacity,
					projectile.rotation, bloom.Size() / 2, MathHelper.Lerp(projectile.scale, 1, 0.15f) * 1.2f, SpriteEffects.None, 0);
				spriteBatch.End(); spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
			}
			else if (Ignited)
			{
				spriteBatch.End(); spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
				Texture2D bloom = mod.GetTexture(Texture.Remove(0, mod.Name.Length + 1) + "_bloom");
				float opacity = (projectile.timeLeft / 20f) * 2f;
				Color color = Color.Orange;
				spriteBatch.Draw(bloom, projectile.Center - (2 * projectile.velocity) - Main.screenPosition, null, color * opacity,
					projectile.rotation, bloom.Size() / 2, 0.2f + (((20 - projectile.timeLeft) / 20f) * 2.5f), SpriteEffects.None, 0);
				spriteBatch.End(); spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
			}
		}
	}

	public class AstrofloraSpark : ModProjectile
	{
		public override string Texture => AssetDirectory.AstrofloraItem + "AstrofloraProj";

		public override void SetDefaults()
		{
			projectile.Size = new Vector2(2, 2);
			projectile.tileCollide = false;
			projectile.timeLeft = 30;
			projectile.hide = true;
		}

		public override void AI()
		{
			Lighting.AddLight(projectile.Center, Color.LightGoldenrodYellow.ToVector3());
			Vector2 basevel = Vector2.UnitX.RotatedBy(projectile.ai[0]) * 4;
			if (Main.rand.Next(6) == 0)
				projectile.ai[1] *= -1;

			projectile.velocity = basevel.RotatedBy(projectile.ai[1] * MathHelper.Pi / 6);
			Dust dust = Dust.NewDustPerfect(projectile.Center, DustID.GoldCoin, Vector2.Zero);
			dust.noGravity = true;
		}
	}
}
