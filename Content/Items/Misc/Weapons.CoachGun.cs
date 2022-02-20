using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Items.Misc
{
	public class CoachGun : ModItem
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Coach Gun");
			Tooltip.SetDefault("I dont get paid to write descriptions lol");

		}

		public override void SetDefaults()
		{
			item.damage = 30;
			item.ranged = true;
			item.width = 24;
			item.height = 24;
			item.useTime = 35;
			item.useAnimation = 35;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.noMelee = true;
			item.knockBack = 0;
			item.rare = ItemRarityID.Orange;
			item.shoot = ModContent.ProjectileType<CoachGunBomb>();
			item.shootSpeed = 12f;
			item.useAmmo = AmmoID.Bullet;
			item.autoReuse = true;
		}

        public override bool CanUseItem(Player player)
        {
			if (player.altFunctionUse == 2)
			{
				item.useStyle = ItemUseStyleID.SwingThrow;
				item.noUseGraphic = true;
			}
			else
            {
				item.useStyle = ItemUseStyleID.HoldingOut;
				item.noUseGraphic = false;
			}
			return base.CanUseItem(player);
        }

        public override Vector2? HoldoutOffset()
		{
			return new Vector2(-10, 0);
		}

		public override bool AltFunctionUse(Player player)
        {
			return true;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
			if (player.altFunctionUse == 2)
            {
				Vector2 dir = Vector2.Normalize(new Vector2(speedX, speedY)) * 9;
				speedX = dir.X;
				speedY = dir.Y;
				type = ModContent.ProjectileType<CoachGunBomb>();
			}
			else
            {
				float rot = new Vector2(speedX, speedY).ToRotation();
				float spread = 0.4f;

                Vector2 offset = new Vector2(1, -0.1f * player.direction).RotatedBy(rot);

				for (int k = 0; k < 15; k++)
				{
					var direction = offset.RotatedByRandom(spread);

					Dust.NewDustPerfect(position + (offset * 70), ModContent.DustType<Dusts.Glow>(), direction * Main.rand.NextFloat(8), 125, new Color(150, 80, 40), Main.rand.NextFloat(0.2f, 0.5f));
				}
				Dust.NewDustPerfect(player.Center + offset * 70, ModContent.DustType<Dusts.Smoke>(), Vector2.UnitY * -2 + offset.RotatedByRandom(spread) * 5, 0, new Color(60, 55, 50) * 0.5f, Main.rand.NextFloat(0.5f, 1));

				Projectile proj = Projectile.NewProjectileDirect(position, new Vector2(speedX, speedY), type, damage, knockBack, player.whoAmI);
				proj.GetGlobalProjectile<CoachGunGlobalProj>().shotFromGun = true;
				return false;
            }
			return true;
        }
    }
	public class CoachGunBomb : ModProjectile
    {
		public override string Texture => AssetDirectory.MiscItem + Name;

		private List<Vector2> cache;
		private Trail trail;

		private bool shot = false;

		private Player owner => Main.player[projectile.owner];

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Dynamite");
		}
		public override void SetDefaults()
		{
			projectile.CloneDefaults(ProjectileID.Shuriken);
			projectile.width = 18;
			projectile.damage = 0;
			projectile.height = 18;
			projectile.ranged = false;
			projectile.timeLeft = 150;
			projectile.aiStyle = 14;
			projectile.friendly = false;
		}

		public override void AI()
		{
			float progress = 1 - (projectile.timeLeft / 150f);
			for (int i = 0; i < 3; i++)
			{
				Dust sparks = Dust.NewDustPerfect(projectile.Center + (projectile.rotation.ToRotationVector2()) * 14, ModContent.DustType<CoachGunSparks>(), (projectile.rotation + Main.rand.NextFloat(-0.6f, 0.6f)).ToRotationVector2() * Main.rand.NextFloat(0.4f,1.2f));
				sparks.fadeIn = progress * 45;
			}

			Rectangle Hitbox = new Rectangle((int)projectile.Center.X - 30, (int)projectile.Center.Y - 30, 60, 60);
			var list = Main.projectile.Where(x => x.Hitbox.Intersects(Hitbox));
			foreach (var proj in list)
			{
				if (proj.GetGlobalProjectile<CoachGunGlobalProj>().shotFromGun && projectile.timeLeft > 2 && proj.active)
				{
					shot = true;
					projectile.timeLeft = 2;
					proj.active = false;

					for (int i = 0; i < 5; i++)
					{
						Projectile.NewProjectileDirect(projectile.Center, Vector2.Normalize(proj.velocity).RotatedBy(Main.rand.NextFloat(-0.6f,0.6f)) * Main.rand.NextFloat(1.3f, 3), ModContent.ProjectileType<CoachGunEmber>(), projectile.damage, 0, owner.whoAmI).scale = Main.rand.NextFloat(0.85f, 1.15f);
					}
				}
			}
			ManageCaches();
			ManageTrail();
		}

        public override void Kill(int timeLeft)
        {
			owner.GetModPlayer<StarlightPlayer>().Shake += 8;

			Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Magic/FireHit"), projectile.Center);

			for (int i = 0; i < 10; i++)
			{
				Dust dust = Dust.NewDustDirect(projectile.Center - new Vector2(16, 16), 0, 0, ModContent.DustType<CoachGunDust>());
				dust.velocity = Main.rand.NextVector2Circular(10, 10);
				dust.scale = Main.rand.NextFloat(1.5f, 1.9f);
				dust.alpha = 70 + Main.rand.Next(60);
				dust.rotation = Main.rand.NextFloat(6.28f);
			}
			for (int i = 0; i < 10; i++)
			{
				Dust dust = Dust.NewDustDirect(projectile.Center - new Vector2(16, 16), 0, 0, ModContent.DustType<CoachGunDustTwo>());
				dust.velocity = Main.rand.NextVector2Circular(10, 10);
				dust.scale = Main.rand.NextFloat(1.5f, 1.9f);
				dust.alpha = Main.rand.Next(80) + 40;
				dust.rotation = Main.rand.NextFloat(6.28f);

				Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(25, 25), ModContent.DustType<CoachGunDustFour>()).scale = 0.9f;
			}
			if (!shot)
			{
				for (int i = 0; i < 5; i++)
				{
					Projectile.NewProjectileDirect(projectile.Center, Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat(2, 3), ModContent.ProjectileType<CoachGunEmber>(), 0, 0, owner.whoAmI).scale = Main.rand.NextFloat(0.85f, 1.15f);
				}
			}

			Projectile.NewProjectileDirect(projectile.Center, Vector2.Zero, ModContent.ProjectileType<CoachGunRing>(), projectile.damage * (shot ? 2 : 1), 0, owner.whoAmI);
			for (int i = 0; i < 10; i++)
			{
				Vector2 vel = Main.rand.NextFloat(6.28f).ToRotationVector2();
				Dust dust = Dust.NewDustDirect(projectile.Center - new Vector2(16, 16) + (vel * Main.rand.Next(70)), 0, 0, ModContent.DustType<CoachGunDustFive>());
				dust.velocity = vel * Main.rand.Next(15);
				dust.scale = Main.rand.NextFloat(0.5f, 1f);
				dust.alpha = 70 + Main.rand.Next(60);
				dust.rotation = Main.rand.NextFloat(6.28f);
			}

		}

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
			DrawTrail(spriteBatch);
			float progress = 1 - (projectile.timeLeft / 150f);
			Color overlayColor = Color.White;
			if (progress < 0.5f)
				overlayColor = Color.Lerp(new Color(0, 0, 0, 0), Color.Orange * 0.5f, progress * 2);
			else
				overlayColor = Color.Lerp(Color.Orange * 0.5f, Color.White, (progress - 0.5f) * 2);

			Texture2D mainTex = Main.projectileTexture[projectile.type];
			spriteBatch.Draw(mainTex, projectile.Center - Main.screenPosition, null, lightColor, projectile.rotation, mainTex.Size() / 2, projectile.scale, SpriteEffects.None, 0f);

			Texture2D overlayTex = ModContent.GetTexture(Texture + "_White");
			spriteBatch.Draw(overlayTex, projectile.Center - Main.screenPosition, null, overlayColor, projectile.rotation, mainTex.Size() / 2, projectile.scale, SpriteEffects.None, 0f);

			progress *= progress;
			Color glowColor = Color.White;
			if (progress < 0.5f)
				glowColor = Color.Lerp(new Color(0, 0, 0, 0), Color.Orange, progress * 2);
			else
				glowColor = Color.Lerp(Color.Orange, Color.White, (progress - 0.5f) * 2);

			Texture2D glowTex = ModContent.GetTexture(Texture + "_Glow");
			spriteBatch.Draw(glowTex, projectile.Center - Main.screenPosition, null, new Color(glowColor.R, glowColor.G, glowColor.B, 0) * 0.5f, projectile.rotation, glowTex.Size() / 2, projectile.scale, SpriteEffects.None, 0f);
			return false;
        }

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();
				for (int i = 0; i < 10; i++)
				{
					cache.Add(projectile.Center);
				}
			}

			cache.Add(projectile.Center);

			while (cache.Count > 10)
			{
				cache.RemoveAt(0);
			}

		}

		private void ManageTrail()
		{
			trail = trail ?? new Trail(Main.instance.GraphicsDevice, 10, new TriangularTip(4), factor => 10, factor =>
			{
				float progress = 1 - (projectile.timeLeft / 150f);
				Color trailColor = Color.Lerp(Color.Red, Color.Yellow, progress);
				return trailColor * 0.8f;
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = projectile.Center + projectile.velocity;
		}

		private void DrawTrail(SpriteBatch spriteBatch)
        {
			spriteBatch.End();
			Effect effect = Filters.Scene["CoachBombTrail"].GetShader().Shader;

			Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.GetTexture("StarlightRiver/Assets/MotionTrail"));

			trail?.Render(effect);

			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
		}
	}

	public class CoachGunEmber : ModProjectile
	{
		public override string Texture => AssetDirectory.VitricItem + "NeedlerEmber";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ember");
		}

		public override void SetDefaults()
		{
			projectile.penetrate = 1;
			projectile.tileCollide = true;
			projectile.hostile = false;
			projectile.friendly = true;
			projectile.aiStyle = 1;
			projectile.width = projectile.height = 12;
			ProjectileID.Sets.TrailCacheLength[projectile.type] = 9;
			ProjectileID.Sets.TrailingMode[projectile.type] = 0;
			projectile.extraUpdates = 1;
			projectile.alpha = 255;
		}

		public override void AI()
		{
			projectile.scale *= 0.98f;
			if (Main.rand.Next(2) == 0)
			{
				Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<CoachGunDustThree>(), Main.rand.NextVector2Circular(1.5f, 1.5f));
				dust.scale = 0.6f * projectile.scale;
				dust.rotation = Main.rand.NextFloatDirection();
			}
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(ModContent.BuffType<CoachDebuff>(), 170);
			target.AddBuff(BuffID.OnFire, 170);
		}
	}

	internal class CoachGunRing : ModProjectile
	{
		public override string Texture => AssetDirectory.BreacherItem + "OrbitalStrike";

		//private List<Vector2> cache;

		//private Trail trail;
		//private Trail trail2;

		private float Progress => 1 - (projectile.timeLeft / 5f);

		private float Radius => 150 * (float)Math.Sqrt(Math.Sqrt(Progress));

		public override void SetDefaults()
		{
			projectile.width = 80;
			projectile.height = 80;
			projectile.ranged = true;
			projectile.friendly = true;
			projectile.tileCollide = false;
			projectile.penetrate = -1;
			projectile.timeLeft = 5;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Coach Bomb");
		}

		public override void AI()
		{
			//ManageCaches();
			//ManageTrail();
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 line = targetHitbox.Center.ToVector2() - projectile.Center;
			line.Normalize();
			line *= Radius;
			if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), projectile.Center, projectile.Center + line))
			{
				return true;
			}
			return false;
		}

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
			target.AddBuff(ModContent.BuffType<CoachDebuff>(), 170);
			target.AddBuff(BuffID.OnFire, 170);
		}

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;

		/*private void ManageCaches()
		{
			cache = new List<Vector2>();
			float radius = Radius;
			for (int i = 0; i < 33; i++) //TODO: Cache offsets, to improve performance
			{
				double rad = (i / 32f) * 6.28f;
				Vector2 offset = new Vector2((float)Math.Sin(rad), (float)Math.Cos(rad));
				offset *= radius;
				cache.Add(projectile.Center + offset);
			}

			while (cache.Count > 33)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{

			trail = trail ?? new Trail(Main.instance.GraphicsDevice, 33, new TriangularTip(1), factor => 38 * (1 - Progress), factor =>
			{
				return Color.Orange;
			});

			trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 33, new TriangularTip(1), factor => 20 * (1 - Progress), factor =>
			{
				return Color.White;
			});
			float nextplace = 33f / 32f;
			Vector2 offset = new Vector2((float)Math.Sin(nextplace), (float)Math.Cos(nextplace));
			offset *= Radius;

			trail.Positions = cache.ToArray();
			trail.NextPosition = projectile.Center + offset;

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = projectile.Center + offset;
		}*/

		/*public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["OrbitalStrikeTrail"].GetShader().Shader;

			Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.GetTexture("StarlightRiver/Assets/GlowTrail"));
			effect.Parameters["alpha"].SetValue(1);

			trail?.Render(effect);
			trail2?.Render(effect);
		}*/
	}

	class CoachDebuff : SmartBuff
	{
		public CoachDebuff() : base("Coach Debuff", "10% increased damage", false) { }

		public override void Update(NPC npc, ref int buffIndex)
		{
			npc.GetGlobalNPC<CoachGlobalNPC>().damageIncreased = true;
			Vector2 vel = new Vector2(0, -1).RotatedByRandom(0.5f) * 0.4f;
			if (Main.rand.NextBool(4))
				Dust.NewDust(npc.position, npc.width, npc.height, ModContent.DustType<CoachSmoke>(), vel.X, vel.Y, 0, new Color(60, 55, 50) * 0.5f, Main.rand.NextFloat(0.5f, 1));
		}
	}

	public class CoachGunGlobalProj : GlobalProjectile
	{
		public override bool InstancePerEntity => true;
		public bool shotFromGun = false;
	}

	public class CoachGlobalNPC : GlobalNPC
    {
		public override bool InstancePerEntity => true;
		public bool damageIncreased = true;

        public override void ResetEffects(NPC npc)
        {
			damageIncreased = false;
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
			if (projectile.ranged && damageIncreased)
				damage = (int)(damage * 1.2f);
        }
    }
}