using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace StarlightRiver.Content.Items.Misc
{
	public class CoachGun : ModItem
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void Load()
		{
			GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, AssetDirectory.MiscItem + "CoachGunCasing");		
		}

		private int cooldown = 0;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Coach Gun");
			Tooltip.SetDefault("M2 to throw out a lit bundle of dynamite\n" +
				"Explodes in 2.5 seconds, dealing DoT and weakening enemies\n" +
				"Shoot it to detonate it early\n" +
				"'My business, my rules'");

		}

		public override void SetDefaults()
		{
			Item.damage = 30;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 24;
			Item.height = 24;
			Item.useTime = 35;
			Item.useAnimation = 35;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 0;
			Item.rare = ItemRarityID.Blue;
			Item.shoot = ModContent.ProjectileType<CoachGunBomb>();
			Item.shootSpeed = 12f;
			Item.useAmmo = AmmoID.Bullet;
			Item.autoReuse = true;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Musket, 1);
			recipe.AddIngredient(ItemID.Explosives, 1);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}

		public override void HoldItem(Player Player)
        {
			cooldown--;
        }

        public override bool CanUseItem(Player Player)
        {
			if (Player.altFunctionUse == 2)
			{
				Item.useStyle = ItemUseStyleID.Swing;
				Item.noUseGraphic = true;

				Item.useTime = 15;
				Item.useAnimation = 15;
				if (cooldown > 0)
					return false;
			}
			else
            {
				Item.useTime = 35;
				Item.useAnimation = 35;
				Item.useStyle = ItemUseStyleID.Shoot;
				Item.noUseGraphic = false;
			}
			return base.CanUseItem(Player);
        }

        public override Vector2? HoldoutOffset()
		{
			return new Vector2(-15, 0);
		}

		public override bool AltFunctionUse(Player Player)
        {
			return true;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
			if (player.altFunctionUse == 2)
			{
				Vector2 dir = Vector2.Normalize(velocity) * 9;
				velocity = dir;
				type = ModContent.ProjectileType<CoachGunBomb>();
			}
		}

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
			if (player.altFunctionUse == 2)
				cooldown = 130;
			else
            {
				float rot = velocity.ToRotation();
				float spread = 0.4f;

                Vector2 offset = new Vector2(1, -0.05f * player.direction).RotatedBy(rot);

				for (int k = 0; k < 15; k++)
				{
					var direction = offset.RotatedByRandom(spread);

					Dust.NewDustPerfect(position + (offset * 70), ModContent.DustType<Dusts.Glow>(), direction * Main.rand.NextFloat(8), 125, new Color(150, 80, 40), Main.rand.NextFloat(0.2f, 0.5f));
				}

				Helper.PlayPitched("Guns/PlinkLever", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f), position);
				Helper.PlayPitched("Guns/RifleLight", 0.7f, Main.rand.NextFloat(-0.1f, 0.1f), position);
				Dust.NewDustPerfect(player.Center + offset * 70, ModContent.DustType<Dusts.Smoke>(), Vector2.UnitY * -2 + offset.RotatedByRandom(spread) * 5, 0, new Color(60, 55, 50) * 0.5f, Main.rand.NextFloat(0.5f, 1));

				Projectile proj = Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), position, velocity * 2, type, damage, knockback, player.whoAmI);
				proj.GetGlobalProjectile<CoachGunGlobalProj>().shotFromGun = true;

				Projectile.NewProjectile(player.GetSource_ItemUse(Item), position + (offset * 70), Vector2.Zero, ModContent.ProjectileType<CoachGunMuzzleFlash>(), 0, 0, player.whoAmI, rot);

				Gore.NewGore(source, player.Center + (offset * 20), new Vector2(player.direction * -1, -0.5f) * 2, Mod.Find<ModGore>("CoachGunCasing").Type, 1f);

				return false;
            }
			return true;
        }
    }

	public class CoachGunMuzzleFlash : ModProjectile
    {
		public override string Texture => AssetDirectory.MiscItem + Name;

		private Player owner => Main.player[Projectile.owner];

		private Vector2 offset = Vector2.Zero;

		private bool initialized = false;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Muzzle Flash");
		}
		public override void SetDefaults()
		{
			Projectile.width = 2;
			Projectile.damage = 0;
			Projectile.height = 2;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.timeLeft = 5;
			Projectile.friendly = false;
		}

        public override void AI()
        {
            if (!initialized)
            {
				initialized = true;
				offset = Projectile.Center - owner.Center;
            }
			Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.4f);
			Projectile.Center = owner.Center + offset;
			Projectile.rotation = Projectile.ai[0];
        }
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D mainTex = TextureAssets.Projectile[Projectile.type].Value;
			Main.spriteBatch.Draw(mainTex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, new Vector2(8, mainTex.Height / 2), Projectile.scale, SpriteEffects.None, 0f);

			Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
			Color glowColor = Color.Orange;
			glowColor.A = 0;
			Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, glowColor, Projectile.rotation, new Vector2(8, glowTex.Height / 2), Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}
	}
	public class CoachGunBomb : ModProjectile
    {
		public override string Texture => AssetDirectory.MiscItem + Name;

		private List<Vector2> cache;
		private Trail trail;

		private bool shot = false;

		private Player owner => Main.player[Projectile.owner];

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Dynamite");
		}
		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.Shuriken);
			Projectile.width = 18;
			Projectile.damage = 0;
			Projectile.height = 18;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.timeLeft = 150;
			Projectile.aiStyle = 14;
			Projectile.friendly = false;
		}

		public override void AI()
		{
			float progress = 1 - (Projectile.timeLeft / 150f);
			for (int i = 0; i < 3; i++)
			{
				Dust sparks = Dust.NewDustPerfect(Projectile.Center + (Projectile.rotation.ToRotationVector2()) * 17, ModContent.DustType<CoachGunSparks>(), (Projectile.rotation + Main.rand.NextFloat(-0.6f, 0.6f)).ToRotationVector2() * Main.rand.NextFloat(0.4f,1.2f));
				sparks.fadeIn = progress * 45;
			}

			Rectangle Hitbox = new Rectangle((int)Projectile.Center.X - 30, (int)Projectile.Center.Y - 30, 60, 60);
			var list = Main.projectile.Where(x => x.Hitbox.Intersects(Hitbox));
			foreach (var proj in list)
			{
				if (proj.GetGlobalProjectile<CoachGunGlobalProj>().shotFromGun && Projectile.timeLeft > 2 && proj.active)
				{
					shot = true;
					Projectile.timeLeft = 2;
					proj.active = false;

					for (int i = 0; i < 5; i++)
					{
						var velocity = Vector2.Normalize(proj.velocity).RotatedBy(Main.rand.NextFloat(-0.6f, 0.6f)) * Main.rand.NextFloat(1.3f, 3);
						Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<CoachGunEmber>(), Projectile.damage, 0, owner.whoAmI).scale = Main.rand.NextFloat(0.85f, 1.15f);
					}
				}
			}
			ManageCaches();
			ManageTrail();
		}

        public override void Kill(int timeLeft)
        {
			Core.Systems.CameraSystem.Shake += 8;

			Terraria.Audio.SoundEngine.PlaySound(new SoundStyle($"{nameof(StarlightRiver)}/Sounds/Magic/FireHit"), Projectile.Center);
			Helper.PlayPitched("Impacts/AirstrikeImpact", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f));

			for (int i = 0; i < 10; i++)
			{
				Dust dust = Dust.NewDustDirect(Projectile.Center - new Vector2(16, 16), 0, 0, ModContent.DustType<CoachGunDust>());
				dust.velocity = Main.rand.NextVector2Circular(10, 10);
				dust.scale = Main.rand.NextFloat(1.5f, 1.9f);
				dust.alpha = 70 + Main.rand.Next(60);
				dust.rotation = Main.rand.NextFloat(6.28f);
			}
			for (int i = 0; i < 10; i++)
			{
				Dust dust = Dust.NewDustDirect(Projectile.Center - new Vector2(16, 16), 0, 0, ModContent.DustType<CoachGunDustTwo>());
				dust.velocity = Main.rand.NextVector2Circular(10, 10);
				dust.scale = Main.rand.NextFloat(1.5f, 1.9f);
				dust.alpha = Main.rand.Next(80) + 40;
				dust.rotation = Main.rand.NextFloat(6.28f);

				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(25, 25), ModContent.DustType<CoachGunDustFour>()).scale = 0.9f;
			}
			if (!shot)
			{
				for (int i = 0; i < 5; i++)
				{
					var velocity = Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat(2, 3);
					Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<CoachGunEmber>(), 0, 0, owner.whoAmI).scale = Main.rand.NextFloat(0.85f, 1.15f);
				}
			}

			Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<CoachGunRing>(), Projectile.damage * (shot ? 2 : 1), 0, owner.whoAmI);
			for (int i = 0; i < 10; i++)
			{
				Vector2 vel = Main.rand.NextFloat(6.28f).ToRotationVector2();
				Dust dust = Dust.NewDustDirect(Projectile.Center - new Vector2(16, 16) + (vel * Main.rand.Next(70)), 0, 0, ModContent.DustType<CoachGunDustFive>());
				dust.velocity = vel * Main.rand.Next(15);
				dust.scale = Main.rand.NextFloat(0.5f, 1f);
				dust.alpha = 70 + Main.rand.Next(60);
				dust.rotation = Main.rand.NextFloat(6.28f);
			}

		}

        public override bool PreDraw(ref Color lightColor)
        {
			var spriteBatch = Main.spriteBatch;

			DrawTrail(spriteBatch);
			float progress = 1 - (Projectile.timeLeft / 150f);
			Color overlayColor = Color.White;
			if (progress < 0.5f)
				overlayColor = Color.Lerp(new Color(0, 0, 0, 0), Color.Orange * 0.5f, progress * 2);
			else
				overlayColor = Color.Lerp(Color.Orange * 0.5f, Color.White, (progress - 0.5f) * 2);

			Texture2D mainTex = TextureAssets.Projectile[Projectile.type].Value;
			spriteBatch.Draw(mainTex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, mainTex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);

			Texture2D overlayTex = ModContent.Request<Texture2D>(Texture + "_White").Value;
			spriteBatch.Draw(overlayTex, Projectile.Center - Main.screenPosition, null, overlayColor, Projectile.rotation, mainTex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);

			progress *= progress;
			Color glowColor = Color.White;
			if (progress < 0.5f)
				glowColor = Color.Lerp(new Color(0, 0, 0, 0), Color.Orange, progress * 2);
			else
				glowColor = Color.Lerp(Color.Orange, Color.White, (progress - 0.5f) * 2);

			Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
			spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, new Color(glowColor.R, glowColor.G, glowColor.B, 0) * 0.5f, Projectile.rotation, glowTex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);
			return false;
        }

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();
				for (int i = 0; i < 10; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			cache.Add(Projectile.Center);

			while (cache.Count > 10)
			{
				cache.RemoveAt(0);
			}

		}

		private void ManageTrail()
		{
			trail = trail ?? new Trail(Main.instance.GraphicsDevice, 10, new TriangularTip(4), factor => 10, factor =>
			{
				float progress = 1 - (Projectile.timeLeft / 150f);
				Color trailColor = Color.Lerp(Color.Red, Color.Yellow, progress);
				return trailColor * 0.8f;
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;
		}

		private void DrawTrail(SpriteBatch spriteBatch)
        {
			spriteBatch.End();
			Effect effect = Filters.Scene["CoachBombTrail"].GetShader().Shader;

			Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/MotionTrail").Value);

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
			Projectile.penetrate = 1;
			Projectile.tileCollide = true;
			Projectile.hostile = false;
			Projectile.friendly = true;
			Projectile.aiStyle = 1;
			Projectile.width = Projectile.height = 12;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 9;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			Projectile.extraUpdates = 1;
			Projectile.alpha = 255;
		}

		public override void AI()
		{
			Projectile.scale *= 0.98f;
			if (Main.rand.Next(2) == 0)
			{
				Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<CoachGunDustThree>(), Main.rand.NextVector2Circular(1.5f, 1.5f));
				dust.scale = 0.6f * Projectile.scale;
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

		private float Progress => 1 - (Projectile.timeLeft / 5f);

		private float Radius => 150 * (float)Math.Sqrt(Math.Sqrt(Progress));

		public override void SetDefaults()
		{
			Projectile.width = 80;
			Projectile.height = 80;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 5;
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
			Vector2 line = targetHitbox.Center.ToVector2() - Projectile.Center;
			line.Normalize();
			line *= Radius;
			if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + line))
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

        public override bool PreDraw(ref Color lightColor) => false;

		/*private void ManageCaches()
		{
			cache = new List<Vector2>();
			float radius = Radius;
			for (int i = 0; i < 33; i++) //TODO: Cache offsets, to improve performance
			{
				double rad = (i / 32f) * 6.28f;
				Vector2 offset = new Vector2((float)Math.Sin(rad), (float)Math.Cos(rad));
				offset *= radius;
				cache.Add(Projectile.Center + offset);
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
			trail.NextPosition = Projectile.Center + offset;

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = Projectile.Center + offset;
		}*/

		/*public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["OrbitalStrikeTrail"].GetShader().Shader;

			Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
			effect.Parameters["alpha"].SetValue(1);

			trail?.Render(effect);
			trail2?.Render(effect);
		}*/
	}

	class CoachDebuff : SmartBuff
	{
		public override string Texture => AssetDirectory.Debug;

		public CoachDebuff() : base("Coach Debuff", "10% increased damage", false) { }

		public override void Update(NPC NPC, ref int buffIndex)
		{
			NPC.GetGlobalNPC<CoachGlobalNPC>().damageIncreased = true;
			Vector2 vel = new Vector2(0, -1).RotatedByRandom(0.5f) * 0.4f;
			if (Main.rand.NextBool(4))
				Dust.NewDust(NPC.position, NPC.width, NPC.height, ModContent.DustType<CoachSmoke>(), vel.X, vel.Y, 0, new Color(60, 55, 50) * 0.5f, Main.rand.NextFloat(0.5f, 1));
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

        public override void ResetEffects(NPC NPC)
        {
			damageIncreased = false;
        }

        public override void ModifyHitByProjectile(NPC NPC, Projectile Projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
			if (Projectile.DamageType == DamageClass.Ranged && damageIncreased)
				damage = (int)(damage * 1.2f);
        }
    }
}