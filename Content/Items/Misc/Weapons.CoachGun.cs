using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	public class CoachGun : ModItem
	{
		private int cooldown = 0;

		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void Load()
		{
			GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, AssetDirectory.MiscItem + "CoachGunCasing");
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Coach Gun");
			Tooltip.SetDefault("<right> to throw out an exploding bundle of dynamite\n" +
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
			Item.value = Item.sellPrice(gold: 2);
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
			{
				cooldown = 130;
			}
			else
			{
				float rot = velocity.ToRotation();
				float spread = 0.4f;

				Vector2 offset = new Vector2(1, -0.05f * player.direction).RotatedBy(rot);

				for (int k = 0; k < 15; k++)
				{
					Vector2 direction = offset.RotatedByRandom(spread);
					Dust.NewDustPerfect(position + offset * 70, ModContent.DustType<Dusts.Glow>(), direction * Main.rand.NextFloat(8), 125, new Color(150, 80, 40), Main.rand.NextFloat(0.2f, 0.5f));
				}

				Helper.PlayPitched("Guns/PlinkLever", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f), position);
				Helper.PlayPitched("Guns/RifleLight", 0.7f, Main.rand.NextFloat(-0.1f, 0.1f), position);
				Dust.NewDustPerfect(player.Center + offset * 70, ModContent.DustType<Dusts.Smoke>(), Vector2.UnitY * -2 + offset.RotatedByRandom(spread) * 5, 0, new Color(60, 55, 50) * 0.5f, Main.rand.NextFloat(0.5f, 1));

				var proj = Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), position, velocity * 2, type, damage, knockback, player.whoAmI);
				proj.GetGlobalProjectile<CoachGunGlobalProj>().shotFromGun = true;

				Projectile.NewProjectile(player.GetSource_ItemUse(Item), position + offset * 70, Vector2.Zero, ModContent.ProjectileType<CoachGunMuzzleFlash>(), 0, 0, player.whoAmI, rot);

				Gore.NewGore(source, player.Center + offset * 20, new Vector2(player.direction * -1, -0.5f) * 2, Mod.Find<ModGore>("CoachGunCasing").Type, 1f);

				return false;
			}

			return true;
		}
	}

	public class CoachGunMuzzleFlash : ModProjectile
	{
		private Vector2 offset = Vector2.Zero;

		public override string Texture => AssetDirectory.MiscItem + Name;

		private Player Owner => Main.player[Projectile.owner];

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
				offset = Projectile.Center - Owner.Center;
			}

			Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.4f);
			Projectile.Center = Owner.Center + offset;
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
		private List<Vector2> cache;
		private Trail trail;

		private bool shot = false;

		public override string Texture => AssetDirectory.MiscItem + Name;

		private Player Owner => Main.player[Projectile.owner];

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
			float progress = 1 - Projectile.timeLeft / 150f;

			for (int i = 0; i < 3; i++)
			{
				var sparks = Dust.NewDustPerfect(Projectile.Center + Projectile.rotation.ToRotationVector2() * 17, ModContent.DustType<CoachGunSparks>(), (Projectile.rotation + Main.rand.NextFloat(-0.6f, 0.6f)).ToRotationVector2() * Main.rand.NextFloat(0.4f, 1.2f));
				sparks.fadeIn = progress * 45;
			}

			var Hitbox = new Rectangle((int)Projectile.Center.X - 30, (int)Projectile.Center.Y - 30, 60, 60);
			IEnumerable<Projectile> list = Main.projectile.Where(x => x.Hitbox.Intersects(Hitbox));
			foreach (Projectile proj in list)
			{
				if (proj.GetGlobalProjectile<CoachGunGlobalProj>().shotFromGun && Projectile.timeLeft > 2 && proj.active)
				{
					shot = true;
					Projectile.timeLeft = 2;
					proj.active = false;

					for (int i = 0; i < 5; i++)
					{
						Vector2 velocity = Vector2.Normalize(proj.velocity).RotatedBy(Main.rand.NextFloat(-0.6f, 0.6f)) * Main.rand.NextFloat(1.3f, 3);
						Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<CoachGunEmber>(), Projectile.damage, 0, Owner.whoAmI).scale = Main.rand.NextFloat(0.85f, 1.15f);
					}
				}
			}

			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public override void Kill(int timeLeft)
		{
			CameraSystem.shake += 8;
			Helper.PlayPitched("Magic/FireHit", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f));
			Helper.PlayPitched("Impacts/AirstrikeImpact", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f));

			for (int i = 0; i < 10; i++)
			{
				var dust = Dust.NewDustDirect(Projectile.Center - new Vector2(16, 16), 0, 0, ModContent.DustType<CoachGunDust>());
				dust.velocity = Main.rand.NextVector2Circular(10, 10);
				dust.scale = Main.rand.NextFloat(1.5f, 1.9f);
				dust.alpha = 70 + Main.rand.Next(60);
				dust.rotation = Main.rand.NextFloat(6.28f);
			}

			for (int i = 0; i < 10; i++)
			{
				var dust = Dust.NewDustDirect(Projectile.Center - new Vector2(16, 16), 0, 0, ModContent.DustType<CoachGunDustTwo>());
				dust.velocity = Main.rand.NextVector2Circular(10, 10);
				dust.scale = Main.rand.NextFloat(1.5f, 1.9f);
				dust.alpha = Main.rand.Next(80) + 40;
				dust.rotation = Main.rand.NextFloat(6.28f);

				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(25, 25), ModContent.DustType<CoachGunDustGlow>()).scale = 0.9f;
			}

			if (!shot)
			{
				for (int i = 0; i < 5; i++)
				{
					Vector2 velocity = Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat(2, 3);
					Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<CoachGunEmber>(), 0, 0, Owner.whoAmI).scale = Main.rand.NextFloat(0.85f, 1.15f);
				}
			}

			Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<CoachGunRing>(), Projectile.damage * (shot ? 2 : 1), 0, Owner.whoAmI);
			for (int i = 0; i < 10; i++)
			{
				Vector2 vel = Main.rand.NextFloat(6.28f).ToRotationVector2();
				var dust = Dust.NewDustDirect(Projectile.Center - new Vector2(16, 16) + vel * Main.rand.Next(70), 0, 0, ModContent.DustType<CoachGunDustFive>());
				dust.velocity = vel * Main.rand.Next(15);
				dust.scale = Main.rand.NextFloat(0.5f, 1f);
				dust.alpha = 70 + Main.rand.Next(60);
				dust.rotation = Main.rand.NextFloat(6.28f);
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;

			DrawTrail(spriteBatch);

			float progress = 1 - Projectile.timeLeft / 150f;
			Color overlayColor;

			if (progress < 0.5f)
				overlayColor = Color.Lerp(new Color(0, 0, 0, 0), Color.Orange * 0.5f, progress * 2);
			else
				overlayColor = Color.Lerp(Color.Orange * 0.5f, Color.White, (progress - 0.5f) * 2);

			Texture2D mainTex = TextureAssets.Projectile[Projectile.type].Value;
			spriteBatch.Draw(mainTex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, mainTex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);

			Texture2D overlayTex = ModContent.Request<Texture2D>(Texture + "_White").Value;
			spriteBatch.Draw(overlayTex, Projectile.Center - Main.screenPosition, null, overlayColor, Projectile.rotation, mainTex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);

			progress *= progress;
			Color glowColor;

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
			trail ??= new Trail(Main.instance.GraphicsDevice, 10, new NoTip(), factor => 10, factor =>
			{
				float progress = 1 - Projectile.timeLeft / 150f;
				var trailColor = Color.Lerp(Color.Red, Color.Yellow, progress);
				return trailColor * 0.8f;
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;
		}

		private void DrawTrail(SpriteBatch spriteBatch)
		{
			spriteBatch.End();
			Effect effect = Filters.Scene["CoachBombTrail"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(Assets.MotionTrail.Value);

			trail?.Render(effect);

			spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
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

			if (Main.rand.NextBool(2))
			{
				var dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<CoachGunDustThree>(), Main.rand.NextVector2Circular(1.5f, 1.5f));
				dust.scale = 0.6f * Projectile.scale;
				dust.rotation = Main.rand.NextFloatDirection();
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(ModContent.BuffType<CoachDebuff>(), 170);
			target.AddBuff(BuffID.OnFire, 170);
		}
	}

	internal class CoachGunRing : ModProjectile
	{
		public override string Texture => AssetDirectory.BreacherItem + "OrbitalStrike";

		private float Progress => 1 - Projectile.timeLeft / 5f;

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

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 line = targetHitbox.Center.ToVector2() - Projectile.Center;
			line.Normalize();
			line *= Radius;

			if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + line))
				return true;

			return false;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(ModContent.BuffType<CoachDebuff>(), 170);
			target.AddBuff(BuffID.OnFire, 170);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}
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

	//TODO: Look for a way to eliminate these potentially
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

		public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
		{
			if (projectile.DamageType == DamageClass.Ranged && damageIncreased)
				modifiers.SourceDamage *= 1.2f;
		}
	}
}