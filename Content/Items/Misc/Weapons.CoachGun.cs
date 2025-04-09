using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Core.Systems.PixelationSystem;
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
		public float shootRotation;
		public int shootDirection;

		public bool justAltUsed; // to prevent custom recoil anim

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

				justAltUsed = true;

				if (cooldown > 0)
					return false;
			}
			else
			{
				Item.useTime = 35;
				Item.useAnimation = 35;
				Item.useStyle = ItemUseStyleID.Shoot;
				Item.noUseGraphic = false;

				justAltUsed = false;
			}

			shootRotation = (Player.Center - Main.MouseWorld).ToRotation();
			shootDirection = (Main.MouseWorld.X < Player.Center.X) ? -1 : 1;

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

		public override void UseStyle(Player player, Rectangle heldItemFrame)
		{
			if (justAltUsed)
				return;

			Vector2 itemPosition = CommonGunAnimations.SetGunUseStyle(player, Item, shootDirection, -6f, new Vector2(82f, 22f), new Vector2(-50f, 0f));

			float animProgress = 1f - player.itemTime / (float)player.itemTimeMax;

			if (animProgress >= 0.35f)
			{
				float lerper = (animProgress - 0.35f) / 0.65f;
				Dust.NewDustPerfect(itemPosition + new Vector2(55f, -2f * player.direction).RotatedBy(player.compositeFrontArm.rotation + 1.5707964f * player.gravDir), DustID.Smoke, Vector2.UnitY * -2f, (int)MathHelper.Lerp(210f, 200f, lerper), default, MathHelper.Lerp(0.5f, 1f, lerper)).noGravity = true;
			}
		}

		public override void UseItemFrame(Player player)
		{
			if (justAltUsed)
				return;

			CommonGunAnimations.SetGunUseItemFrame(player, shootDirection, shootRotation, -0.2f, true);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (player.altFunctionUse == 2)
			{
				cooldown = 130;
			}
			else
			{
				Vector2 barrelPos = position + new Vector2(70f, -2f * player.direction).RotatedBy(velocity.ToRotation());

				for (int i = 0; i < 5; i++)
				{
					Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), DustID.Torch, velocity.RotatedByRandom(0.5f).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.25f), 0, default, 1.2f).noGravity = true;

					Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<PixelatedEmber>(), velocity.RotatedByRandom(0.5f).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.25f), 0, new Color(255, 125, 0, 0), 0.15f).customData = -player.direction;

					Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<PixelatedEmber>(), velocity.RotatedByRandom(0.5f).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.25f), 0, new Color(255, 80, 0, 0), 0.15f).customData = -player.direction;

					Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<PixelatedGlow>(), velocity.RotatedByRandom(0.5f).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.25f), 0, new Color(255, 50, 0, 0), 0.35f).noGravity = true;
				}

				for (int i = 0; i < 2; i++)
				{
					Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<CoachGunSmokeDust>(), velocity * 0.025f, 130, new Color(255, 150, 0, 0), 0.1f);

					Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<CoachGunSmokeDust>(), velocity * 0.05f, 160, new Color(255, 150, 0, 0), 0.15f);

					Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<CoachGunSmokeDust>(), velocity * Main.rand.NextFloat(0.075f, 0.125f) - Vector2.UnitY * Main.rand.NextFloat(0.5f, 1.5f), 175, new Color(255, 150, 50, 0), 0.125f).noGravity = true;

					Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<CoachGunSmokeDust>(), velocity * Main.rand.NextFloat(0.075f, 0.125f) - Vector2.UnitY * Main.rand.NextFloat(0.5f, 2f), 175, new Color(255, 150, 50, 0), 0.1f).noGravity = true;

				}

				Dust.NewDustPerfect(barrelPos, ModContent.DustType<CoachGunMuzzleFlashDust>(), Vector2.Zero, 0, default, Main.rand.NextFloat(1f, 1.15f)).rotation = velocity.ToRotation();

				Vector2 shellPos = player.Center + new Vector2(10f, -4f * player.direction).RotatedBy(velocity.ToRotation());

				Gore.NewGoreDirect(source, shellPos, new Vector2(player.direction * -1, -0.5f) * 2, Mod.Find<ModGore>("CoachGunCasing").Type, 1f).timeLeft = 60;

				Dust.NewDustPerfect(shellPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<CoachGunSmokeDust>(), new Vector2(player.direction * -1, -0.5f), 150, new Color(255, 255, 255, 0), 0.05f);

				for (int i = 0; i < 3; i++)
				{
					Dust.NewDustPerfect(shellPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<PixelatedEmber>(), new Vector2(player.direction * -1, -0.5f).RotatedByRandom(0.4f) * Main.rand.NextFloat(2f), 0, new Color(255, 125, 0, 0), 0.15f).customData = player.direction;

					Dust.NewDustPerfect(shellPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<PixelatedEmber>(), new Vector2(player.direction * -1, -0.5f).RotatedByRandom(0.4f) * Main.rand.NextFloat(2f), 0, new Color(255, 80, 0, 0), 0.15f).customData = player.direction;

					Dust.NewDustPerfect(shellPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<PixelatedGlow>(), new Vector2(player.direction * -1, -0.5f).RotatedByRandom(0.4f) * Main.rand.NextFloat(4f), 0, new Color(255, 150, 0, 0), 0.35f);
				}

				SoundHelper.PlayPitched("Guns/PlinkLever", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f), position);
				SoundHelper.PlayPitched("Guns/RifleLight", 0.7f, Main.rand.NextFloat(-0.1f, 0.1f), position);

				var proj = Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), position, velocity * 2, type, damage, knockback, player.whoAmI);
				proj.GetGlobalProjectile<CoachGunGlobalProj>().shotFromGun = true;

				CameraSystem.shake += 2;
				Item.noUseGraphic = true;

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
			Texture2D mainTex = Assets.Items.Misc.CoachGunMuzzleFlash.Value;
			Main.spriteBatch.Draw(mainTex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, new Vector2(8, mainTex.Height / 2), Projectile.scale, SpriteEffects.None, 0f);

			Texture2D glowTex = Assets.Items.Misc.CoachGunMuzzleFlash_Glow.Value;
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

		public override void OnKill(int timeLeft)
		{
			CameraSystem.shake += 8;
			SoundHelper.PlayPitched("Magic/FireHit", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f));
			SoundHelper.PlayPitched("Impacts/AirstrikeImpact", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f));

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

			Texture2D mainTex = Assets.Items.Misc.CoachGunBomb.Value;
			spriteBatch.Draw(mainTex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, mainTex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);

			Texture2D overlayTex = Assets.Items.Misc.CoachGunBomb_White.Value;
			spriteBatch.Draw(overlayTex, Projectile.Center - Main.screenPosition, null, overlayColor, Projectile.rotation, mainTex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);

			progress *= progress;
			Color glowColor;

			if (progress < 0.5f)
				glowColor = Color.Lerp(new Color(0, 0, 0, 0), Color.Orange, progress * 2);
			else
				glowColor = Color.Lerp(Color.Orange, Color.White, (progress - 0.5f) * 2);

			Texture2D glowTex = Assets.Items.Misc.CoachGunBomb_Glow.Value;
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
			if (trail is null || trail.IsDisposed)
			{
				trail = new Trail(Main.instance.GraphicsDevice, 10, new NoTip(), factor => 10, factor =>
							{
								float progress = 1 - Projectile.timeLeft / 150f;
								var trailColor = Color.Lerp(Color.Red, Color.Yellow, progress);
								return trailColor * 0.8f;
							});
			}

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;
		}

		private void DrawTrail(SpriteBatch spriteBatch)
		{
			Effect effect = ShaderLoader.GetShader("CoachBombTrail").Value;

			if (effect != null)
			{
				spriteBatch.End();

				var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
				Matrix view = Main.GameViewMatrix.TransformationMatrix;
				var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

				effect.Parameters["transformMatrix"].SetValue(world * view * projection);
				effect.Parameters["sampleTexture"].SetValue(Assets.MotionTrail.Value);

				trail?.Render(effect);

				spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, Main.Rasterizer, default, Main.GameViewMatrix.TransformationMatrix);
			}
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

	public class CoachGunMuzzleFlashDust : ModDust
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void OnSpawn(Dust dust)
		{
			dust.frame = new Rectangle(0, 0, 4, 4);
		}

		public override bool Update(Dust dust)
		{
			dust.alpha += 20;
			dust.alpha = (int)(dust.alpha * 1.05f);

			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}

		public override bool PreDraw(Dust dust)
		{
			float lerper = 1f - dust.alpha / 255f;

			Texture2D tex = Assets.Items.Misc.CoachGunMuzzleFlashDust.Value;
			Texture2D texBlur = Assets.Items.Misc.CoachGunMuzzleFlashDust_Blur.Value;
			Texture2D texGlow = Assets.Items.Misc.CoachGunMuzzleFlashDust_Glow.Value;
			Texture2D bloomTex = Assets.Masks.GlowAlpha.Value;

			int frame = 0;
			if (lerper < 0.5f)
				frame = 1;

			Main.spriteBatch.Draw(bloomTex, dust.position - Main.screenPosition, null, new Color(255, 100, 0, 0) * 0.25f * lerper, dust.rotation, bloomTex.Size() / 2f, dust.scale * 1.25f, 0f, 0f);

			Main.spriteBatch.Draw(texGlow, dust.position - Main.screenPosition, texGlow.Frame(verticalFrames: 2, frameY: frame), new Color(255, 100, 0, 0) * lerper, dust.rotation, texGlow.Frame(verticalFrames: 2, frameY: frame).Size() / 2f, dust.scale, 0f, 0f);

			Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, tex.Frame(verticalFrames: 2, frameY: frame), Color.White * lerper, dust.rotation, tex.Frame(verticalFrames: 2, frameY: frame).Size() / 2f, dust.scale, 0f, 0f);

			Main.spriteBatch.Draw(texBlur, dust.position - Main.screenPosition, texBlur.Frame(verticalFrames: 2, frameY: frame), Color.White with { A = 0 } * 0.5f * lerper, dust.rotation, texBlur.Frame(verticalFrames: 2, frameY: frame).Size() / 2f, dust.scale, 0f, 0f);

			return false;
		}
	}

	public class CoachGunSmokeDust : ModDust
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void OnSpawn(Dust dust)
		{
			dust.frame = new Rectangle(0, 0, 4, 4);
			dust.customData = 1 + Main.rand.Next(2);
			dust.rotation = Main.rand.NextFloat(6.28f);
		}

		public override bool Update(Dust dust)
		{
			dust.position.Y -= 0.1f;
			if (dust.noGravity)
				dust.position.Y -= 0.5f;

			dust.position += dust.velocity;

			if (!dust.noGravity)
			{
				dust.velocity *= 0.99f;
			}
			else
			{
				dust.velocity *= 0.975f;
				dust.velocity.X *= 0.99f;
			}

			dust.rotation += dust.velocity.Length() * 0.01f;

			if (dust.noGravity)
				dust.alpha += 2;
			else
				dust.alpha += 5;

			dust.alpha = (int)(dust.alpha * 1.005f);

			if (!dust.noGravity)
				dust.scale *= 1.02f;
			else
				dust.scale *= 0.99f;

			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}

		public override bool PreDraw(Dust dust)
		{
			float lerper = 1f - dust.alpha / 255f;

			Texture2D tex = Assets.SmokeAlpha_1.Value;
			if ((int)dust.customData > 1)
				tex = Assets.SmokeAlpha_2.Value;

			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("Dusts", () => Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, Color.Lerp(dust.color, new Color(255, 255, 255, 0), Eases.EaseQuinticIn(1f - lerper)) * lerper, dust.rotation, tex.Size() / 2f, dust.scale, 0f, 0f));

			return false;
		}
	}
}