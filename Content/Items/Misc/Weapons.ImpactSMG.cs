using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Items.Vitric;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.UI.Chat;
using Terraria.Audio;

namespace StarlightRiver.Content.Items.Misc
{
	public class ImpactSMG : ModItem
	{
		public int reloadDelay;
		public int maxReloadDelay;
		public int flashTimer;
		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Impact SMG");

			Tooltip.SetDefault("Fires a burst of high impact bullets\nHitting more than 20 bullets in one burst causes the SMG to inherit boomerang properties, granting a buff to the next burst\nOtherwise, it explodes");
		}

		public override void SetDefaults()
		{
			Item.damage = 10;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 24;
			Item.height = 24;
			Item.useTime = 5;
			Item.useAnimation = 5;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 2f;
			Item.rare = ItemRarityID.Blue;
			Item.shoot = ModContent.ProjectileType<ImpactSMGHoldout>();
			Item.shootSpeed = 1f;
			Item.noUseGraphic = true;
			Item.useAmmo = AmmoID.Bullet;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			Projectile.NewProjectile(source, position, velocity, Item.shoot, damage, knockback, player.whoAmI);
			return false;
		}

		public override bool CanUseItem(Player Player)
		{
			return Player.ownedProjectileCounts[ModContent.ProjectileType<ImpactSMGHoldout>()] <= 0 && reloadDelay <= 0;
		}

		public override void UpdateInventory(Player player)
		{
			if (reloadDelay > 0)
			{
				reloadDelay--;
				if (reloadDelay == 0)
				{
					Helper.PlayPitched("Guns/PlinkLever", 1f, 0.15f, player.Center);
					flashTimer = 25;
				}				
			}

			if (flashTimer > 0)
				flashTimer--;
		}

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			if (reloadDelay > 0)
			{
				spriteBatch.Draw(tex, position, null, drawColor * MathHelper.Lerp(1f, 0.5f, reloadDelay / (float)maxReloadDelay), 0f, origin, scale, 0f, 0f);
				return false;
			}

			return true;	
		}

		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			if (flashTimer > 0)
				spriteBatch.Draw(tex, position, null, new Color(255, 255, 255, 0) * MathHelper.Lerp(1f, 0f, 1f - flashTimer / 25f), 0f, origin, scale, 0f, 0f);
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddRecipeGroup(RecipeGroupID.IronBar, 8);
			recipe.AddIngredient(ItemID.IllegalGunParts);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	public class ImpactSMGHoldout : ModProjectile
	{
		public int shootDelay;

		public int shots;

		public int hitShots;

		public int flashTimer;

		public int throwTimer;

		public bool thrown;

		public bool flashed;

		public bool updateVelo = true;

		public bool draw; //only draw two ticks after spawning

		public bool CanHold => Owner.channel && !Owner.CCed && !Owner.noItems;

		public Vector2 mousePos;

		public ref float ShootDelay => ref Projectile.ai[0];

		public ref float MaxShootDelay => ref Projectile.ai[1];

		private Player Owner => Main.player[Projectile.owner];

		public ImpactSMG Holdout => Owner.HeldItem.ModItem as ImpactSMG;

		public override string Texture => AssetDirectory.MiscItem + "ImpactSMG";

		public override bool? CanDamage()
		{
			return thrown;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Impact SMG");
		}

		public override void SetDefaults()
		{
			Projectile.DamageType = DamageClass.Ranged;

			Projectile.width = 52;
			Projectile.height = 34;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;

			Projectile.penetrate = -1;
		}

		public override void AI()
		{
			Vector2 armPos = Owner.RotatedRelativePoint(Owner.MountedCenter, true);
			armPos += Utils.SafeNormalize(Projectile.velocity, Vector2.UnitX) * 15f;

			if (MaxShootDelay == 0f)
				MaxShootDelay = CombinedHooks.TotalUseTime(Owner.HeldItem.useTime, Owner, Owner.HeldItem);

			if (flashTimer > 0)
				flashTimer--;

			if (shots >= 30)
			{
				if (hitShots >= 20)
					BoomerangAI();
				else
					ExplodingAI();
			}
			else
			{
				shootDelay++;

				if (shootDelay > 2)
					draw = true;

				if (shootDelay % (int)MaxShootDelay == 0)
					ShootBullet(armPos);


				Owner.ChangeDir(Projectile.direction);

				Owner.heldProj = Projectile.whoAmI;
				Owner.itemTime = 2;
				Owner.itemAnimation = 2;

				Projectile.timeLeft = 2;
				Projectile.rotation = Utils.ToRotation(Projectile.velocity);
				Owner.itemRotation = Utils.ToRotation(Projectile.velocity * Projectile.direction);

				Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);

				Projectile.position = armPos - Projectile.Size * 0.5f;

				Projectile.spriteDirection = Projectile.direction;

				if (Main.myPlayer == Projectile.owner && updateVelo)
				{
					updateVelo = false;
					float interpolant = Utils.GetLerpValue(1f, 5f, Projectile.Distance(Main.MouseWorld), true);

					Vector2 oldVelocity = Projectile.velocity;

					Projectile.velocity = Vector2.One.RotatedBy(Vector2.Lerp(Projectile.velocity, Owner.DirectionTo(Main.MouseWorld), interpolant).ToRotation() - MathHelper.PiOver4).RotatedByRandom(0.1f);
					if (Projectile.velocity != oldVelocity)
					{
						Projectile.netSpam = 0;
						Projectile.netUpdate = true;
					}
				}

				if (hitShots >= 20 && !flashed)
				{
					SoundEngine.PlaySound(SoundID.MaxMana, Projectile.Center);
					flashTimer = 20;
					flashed = true;
				}
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (!draw)
				return false;

			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;

			if (hitShots > 0)
			{
				Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, Color.Lerp(Color.Transparent, new Color(180, 50, 50, 0), hitShots / 30f), Projectile.rotation, bloomTex.Size() / 2f, 0.65f, 0f, 0f);
				Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, Color.Lerp(Color.Transparent, new Color(180, 50, 50, 0), hitShots / 30f), Projectile.rotation, glowTex.Size() / 2f, Projectile.scale, Projectile.spriteDirection == -1 ? SpriteEffects.FlipVertically : 0f, 0f);
			}

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() / 2f, Projectile.scale, Projectile.spriteDirection == -1 ? SpriteEffects.FlipVertically : 0f, 0f);

			if (flashed)
			{
				Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, Color.Lerp(Color.Transparent, new Color(180, 50, 50, 0), flashTimer / 20f), Projectile.rotation, glowTex.Size() / 2f, Projectile.scale, Projectile.spriteDirection == -1 ? SpriteEffects.FlipVertically : 0f, 0f);
				Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, Color.Lerp(Color.Transparent, new Color(180, 50, 50, 0), flashTimer / 20f), Projectile.rotation, bloomTex.Size() / 2f, 0.85f, 0f, 0f);
			}

			return false;
		}

		private void ShootBullet(Vector2 armPos)
		{
			shots++;

			Vector2 pos = armPos + Projectile.rotation.ToRotationVector2() * 20f + Vector2.UnitY.RotatedBy(Projectile.velocity.ToRotation()) * -10f * Owner.direction;
			Projectile.NewProjectile(Projectile.GetSource_FromAI(), pos, Projectile.rotation.ToRotationVector2() * 20f, ModContent.ProjectileType<ImpactSMGShot>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
			updateVelo = true;

			Helper.PlayPitched("Guns/RifleLight", 0.35f, Main.rand.NextFloat(-0.1f, 0.1f), pos);

			for (int i = 0; i < 3; i++)
			{
				Dust.NewDustPerfect(pos, ModContent.DustType<GlowFastDecelerate>(), Projectile.rotation.ToRotationVector2().RotatedByRandom(0.25f) * 5f, 0, new Color(190, 40, 40), 0.25f);

				Dust.NewDustPerfect(pos, ModContent.DustType<GlowFastDecelerate>(), Projectile.rotation.ToRotationVector2().RotatedByRandom(0.35f) * 7f, 0, new Color(255, 40, 40), 0.3f);
			}
		}

		private void BoomerangAI()
		{
			if (Main.myPlayer == Projectile.owner && mousePos == Vector2.Zero)
				mousePos = Owner.DirectionTo(Main.MouseWorld);

			Vector2 armPos = Owner.RotatedRelativePoint(Owner.MountedCenter, true);
			armPos += Utils.SafeNormalize(Projectile.velocity, Vector2.UnitX) * 15f;

			if (!thrown)
			{
				throwTimer++;

				if (Main.myPlayer == Owner.whoAmI)
					Owner.ChangeDir(Main.MouseWorld.X < Owner.Center.X ? -1 : 1);

				Owner.heldProj = Projectile.whoAmI;
				Owner.itemTime = 2;
				Owner.itemAnimation = 2;

				Projectile.timeLeft = 2;
				Projectile.rotation = Utils.ToRotation(Projectile.velocity);
				Owner.itemRotation = Utils.ToRotation(Projectile.velocity * Projectile.direction);

				Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);

				Projectile.position = armPos - Projectile.Size * 0.5f;

				Projectile.spriteDirection = Owner.direction;
			}

			if (throwTimer < 20)
			{
				float progress = EaseBuilder.EaseCircularInOut.Ease(throwTimer / 20f);
				Projectile.velocity = Vector2.One.RotatedBy(mousePos.ToRotation() + MathHelper.Lerp(0f, -1.85f, progress) * Owner.direction - MathHelper.PiOver4);
			}
			else if (throwTimer < 30)
			{
				float progress = EaseBuilder.EaseCircularInOut.Ease((throwTimer - 20f) / 10f);
				Projectile.velocity = Vector2.One.RotatedBy(mousePos.ToRotation() + MathHelper.Lerp(-1.85f, 0.35f, progress) * Owner.direction - MathHelper.PiOver4);
			}
			else
			{
				if (!thrown && Main.myPlayer == Owner.whoAmI)
				{
					Projectile.friendly = true;
					SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing, Projectile.Center);
					Projectile.velocity = Owner.DirectionTo(Main.MouseWorld) * 20f;
					Projectile.timeLeft = 2400;
					Projectile.ignoreWater = false;
					Projectile.width = Projectile.height = 32;
					ShootDelay = 0;
					thrown = true;
				}

				if (++ShootDelay >= 25)
				{
					Vector2 playerCenter = Owner.Center;
					Vector2 pos = Projectile.Center;

					float betweenX = Owner.Center.X - pos.X;
					float betweenY = Owner.Center.Y - pos.Y;

					float distance = (float)Math.Sqrt(betweenX * betweenX + betweenY * betweenY);
					float speed = 15f;
					float adjust = 0.95f;

					if (distance > 3000f)
					{
						Projectile.Kill();
					}

					distance = speed / distance;
					betweenX *= distance;
					betweenY *= distance;

					if (Projectile.velocity.X < betweenX)
					{
						Projectile.velocity.X += adjust;
						if (Projectile.velocity.X < 0f && betweenX > 0f)
							Projectile.velocity.X += adjust;
					}
					else if (Projectile.velocity.X > betweenX)
					{
						Projectile.velocity.X -= adjust;
						if (Projectile.velocity.X > 0f && betweenX < 0f)
							Projectile.velocity.X -= adjust;
					}
					if (Projectile.velocity.Y < betweenY)
					{
						Projectile.velocity.Y += adjust;
						if (Projectile.velocity.Y < 0f && betweenY > 0f)
							Projectile.velocity.Y += adjust;
					}
					else if (Projectile.velocity.Y > betweenY)
					{
						Projectile.velocity.Y -= adjust;
						if (Projectile.velocity.Y > 0f && betweenY < 0f)
							Projectile.velocity.Y -= adjust;
					}

					if (Vector2.Distance(Projectile.Center, Owner.Center) < 20f)
					{
						Projectile.Kill();
					}
				}

				Projectile.rotation += 0.3f * (Projectile.velocity.Length() * 0.07f) * Projectile.direction;
			}
		}

		private void ExplodingAI()
		{
			if (Main.myPlayer == Projectile.owner && mousePos == Vector2.Zero)
				mousePos = Owner.DirectionTo(Main.MouseWorld);

			Vector2 armPos = Owner.RotatedRelativePoint(Owner.MountedCenter, true);
			armPos += Utils.SafeNormalize(Projectile.velocity, Vector2.UnitX) * 15f;

			if (!thrown)
			{
				throwTimer++;

				if (Main.myPlayer == Owner.whoAmI)
					Owner.ChangeDir(Main.MouseWorld.X < Owner.Center.X ? -1 : 1);

				Owner.heldProj = Projectile.whoAmI;
				Owner.itemTime = 2;
				Owner.itemAnimation = 2;

				Projectile.timeLeft = 2;
				Projectile.rotation = Utils.ToRotation(Projectile.velocity);
				Owner.itemRotation = Utils.ToRotation(Projectile.velocity * Projectile.direction);

				Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);

				Projectile.position = armPos - Projectile.Size * 0.5f;

				Projectile.spriteDirection = Owner.direction;
			}

			if (throwTimer < 20)
			{
				float progress = EaseBuilder.EaseCircularInOut.Ease(throwTimer / 20f);
				Projectile.velocity = Vector2.One.RotatedBy(mousePos.ToRotation() + MathHelper.Lerp(0f, -1.85f, progress) * Owner.direction - MathHelper.PiOver4);
			}
			else if (throwTimer < 30)
			{
				float progress = EaseBuilder.EaseCircularInOut.Ease((throwTimer - 20f) / 10f);
				Projectile.velocity = Vector2.One.RotatedBy(mousePos.ToRotation() + MathHelper.Lerp(-1.85f, 0.35f, progress) * Owner.direction - MathHelper.PiOver4);
			}
			else
			{
				if (!thrown && Main.myPlayer == Owner.whoAmI)
				{
					Projectile.friendly = true;
					SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing, Projectile.Center);
					Projectile.velocity = Owner.DirectionTo(Main.MouseWorld) * 19f;
					Projectile.timeLeft = 240;
					Projectile.penetrate = 1;
					Projectile.tileCollide = true;
					Projectile.ignoreWater = false;
					Projectile.width = Projectile.height = 32;
					thrown = true;
				}

				Projectile.rotation += 0.25f * (Projectile.velocity.X * 0.05f) * Projectile.direction;
				Projectile.velocity.Y += 0.35f;
				if (Projectile.velocity.Y > 0)
				{
					if (Projectile.velocity.Y < 13f)
						Projectile.velocity.Y *= 1.075f;
					else
						Projectile.velocity.Y *= 1.045f;
				}

				if (Projectile.velocity.Y > 20f)
					Projectile.velocity.Y = 20f;
			}		
		}
	}

	public class ImpactSMGShot : ModProjectile
	{
		private List<Vector2> cache;
		private Trail trail;
		private Trail trail2;

		public override string Texture => AssetDirectory.Invisible;

		public override void SetDefaults()
		{
			Projectile.width = 4;
			Projectile.height = 4;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = true;

			Projectile.timeLeft = 240;
			Projectile.extraUpdates = 4;

			Projectile.penetrate = 2;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Impact SMG Shot");
		}

		public override void AI()
		{
			Projectile.velocity *= 0.96f;

			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			Projectile.friendly = false;

			Player player = Main.player[Projectile.owner];

			if (player.heldProj != -1 && Main.projectile[player.heldProj].ModProjectile is ImpactSMGHoldout smg)
				smg.hitShots++;

			for (int i = 0; i < 2; i++)
			{
				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<GlowFastDecelerate>(), -Projectile.velocity.RotatedByRandom(0.25f) * Main.rand.NextFloat(), 0, new Color(190, 40, 40), 0.25f);

				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<GlowFastDecelerate>(), -Projectile.velocity.RotatedByRandom(0.35f) * Main.rand.NextFloat(), 0, new Color(255, 40, 40), 0.3f);
			}

			Projectile.velocity *= 0f;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Projectile.friendly = false;

			for (int i = 0; i < 2; i++)
			{
				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<GlowFastDecelerate>(), -Projectile.velocity.RotatedByRandom(0.25f) * Main.rand.NextFloat(), 0, new Color(190, 40, 40), 0.25f);

				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<GlowFastDecelerate>(), -Projectile.velocity.RotatedByRandom(0.35f) * Main.rand.NextFloat(), 0, new Color(255, 40, 40), 0.3f);
			}

			Projectile.velocity *= 0f;

			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			DrawPrimitives();
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
			trail = trail ?? new Trail(Main.instance.GraphicsDevice, 10, new TriangularTip(160), factor => factor * 3.5f, factor =>
			{
				return new Color(255, 255, 255) * factor.X * (Projectile.timeLeft / 180f);
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center;

			trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 10, new TriangularTip(160), factor => factor * 6f, factor =>
			{
				return new Color(190, 40, 40) * factor.X * (Projectile.timeLeft / 180f);
			});

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = Projectile.Center;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

			Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.05f);
			effect.Parameters["repeats"].SetValue(1f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);

			trail2?.Render(effect);
			trail?.Render(effect);
		}
	}
}