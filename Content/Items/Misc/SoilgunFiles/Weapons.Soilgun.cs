using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc.SoilgunFiles
{
	public class Soilgun : MultiAmmoWeapon
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public override List<AmmoStruct> ValidAmmos => new()
		{
			new AmmoStruct(ItemID.DirtBlock, ModContent.ProjectileType<SoilgunDirtSoil>(), "", 2),
			new AmmoStruct(ItemID.SandBlock, ModContent.ProjectileType<SoilgunSandSoil>(), "Deals area of effect damage", 2),
			new AmmoStruct(ItemID.EbonsandBlock, ModContent.ProjectileType<SoilgunEbonsandSoil>(), "Inflicts {{BUFF:EbonsandDebuff}}", 4),
			new AmmoStruct(ItemID.PearlsandBlock, ModContent.ProjectileType<SoilgunPearlsandSoil>(), "Homes onto enemies", 15),
			new AmmoStruct(ItemID.CrimsandBlock, ModContent.ProjectileType<SoilgunCrimsandSoil>(), "Gain regeneration on hit", 4),
			new AmmoStruct(ItemID.SiltBlock, ModContent.ProjectileType<SoilgunSiltSoil>(), "Extracts some treasure when fired", 3),
			new AmmoStruct(ItemID.SlushBlock, ModContent.ProjectileType<SoilgunSlushSoil>(), "Inflicts {{BUFF:Frostburn}}", 3),
			new AmmoStruct(Mod.Find<ModItem>("VitricSandItem").Type, ModContent.ProjectileType<SoilgunVitricSandSoil>(), "Pierces enemies", 8),
			new AmmoStruct(ItemID.MudBlock, ModContent.ProjectileType<SoilgunMudSoil>(), "Chance to spawn bees on death", 3),
			new AmmoStruct(ItemID.AshBlock, ModContent.ProjectileType<SoilgunAshSoil>(), "Inflicts {{BUFF:OnFire}}", 6),
		};

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Soilgun");
			Tooltip.SetDefault("Hold <left> to charge up a volley of soil\n" +
				"Release to fire the soil\n" +
				"Fully charge to fire a high velocity clump of soil\n" +
				"'Soiled it! SOILED IT!'");
		}

		public override void SafeSetDefaults()
		{
			Item.DamageType = DamageClass.Ranged;
			Item.damage = 4;
			Item.width = 60;
			Item.height = 36;
			Item.useAnimation = Item.useTime = 50;
			Item.shoot = ProjectileID.PurificationPowder;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.rare = ItemRarityID.Blue;
			Item.shootSpeed = 16f;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.channel = true;
			Item.knockBack = 1f;

			Item.value = Item.sellPrice(gold: 1);
		}

		public override bool CanConsumeAmmo(Item ammo, Player player)
		{
			return false;
		}

		public override bool SafeCanUseItem(Player player)
		{
			return player.ownedProjectileCounts[ModContent.ProjectileType<SoilgunHoldout>()] <= 0;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			var proj = Projectile.NewProjectileDirect(source, position, velocity, ModContent.ProjectileType<SoilgunHoldout>(),
				damage, knockback, player.whoAmI);

			(proj.ModProjectile as SoilgunHoldout).projectileID = type;
			(proj.ModProjectile as SoilgunHoldout).ammoID = currentAmmoStruct.ammoID;

			return false;
		}

		public override void AddRecipes()
		{
			CreateRecipe().
				AddIngredient(ItemID.DirtRod).
				AddIngredient(ItemID.Sandgun).
				AddIngredient(ItemID.DirtBlock, 45).
				AddTile(TileID.WorkBenches).
				Register();
		}
	}

	class SoilgunHoldout : ModProjectile
	{
		private bool updateVelocity = true;
		private bool flashed;

		private int flashTimer;
		public int ammoID;
		public int projectileID;

		public Vector2 ArmOffset;
		public Vector2 BarrelOffset;

		public Projectile ghostProjectile = new();

		public bool CanHold => Owner.channel && !Owner.CCed && !Owner.noItems;
		public bool Shot { get => Projectile.ai[0] != 0f; set => Projectile.ai[0] = value is true ? 1f : 0f; }
		public float ChargeProgress => Charge / MaxCharge;
		public ref float Charge => ref Projectile.ai[1];
		public ref float MaxCharge => ref Projectile.ai[2];
		public Vector2 ArmPosition => Owner.RotatedRelativePoint(Owner.MountedCenter, true)
			+ Vector2.Lerp(Vector2.Zero, new Vector2(-6f, 0f), Eases.EaseCircularInOut(ChargeProgress < 0.35f ? ChargeProgress / 0.35f : 1f)).RotatedBy(Projectile.rotation)
			+ new Vector2(18f, -4f * Owner.direction).RotatedBy(Projectile.velocity.ToRotation()) + ArmOffset;

		public Vector2 BarrelPosition => ArmPosition + Projectile.velocity * Projectile.width * 0.5f + BarrelOffset;
		public Player Owner => Main.player[Projectile.owner];
		public override string Texture => AssetDirectory.MiscItem + "Soilgun";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Soilgun");
		}

		public override void SetDefaults()
		{
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.width = 68;
			Projectile.height = 40;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
		}

		public override void AI()
		{
			BarrelOffset = Vector2.Zero;
			ArmOffset = Vector2.Zero;

			if (flashTimer > 0)
				flashTimer--;

			if (!CanHold && !Shot)
			{
				if (ChargeProgress >= 0.35f)
				{
					Shoot();

					Projectile.timeLeft = 45;
					updateVelocity = false;
					Shot = true;
				}
			}

			if (Charge == 0f)
			{
				ghostProjectile.SetDefaults(projectileID);
				MaxCharge = CombinedHooks.TotalUseTime(Owner.HeldItem.useTime, Owner, Owner.HeldItem);
				Projectile.velocity = Owner.DirectionTo(Main.MouseWorld);
			}

			UpdateHeldProjectile(!Shot);

			if (Charge < MaxCharge && !Shot)
			{
				Charge++;
			}

			if (Charge >= MaxCharge && !flashed)
			{
				flashTimer = 25;
				flashed = true;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (Charge <= 2)
				return false;

			Texture2D tex = Assets.Items.Misc.Soilgun.Value;
			Texture2D texGlow = Assets.Items.Misc.Soilgun_Glow.Value;
			Texture2D texBlur = Assets.Items.Misc.Soilgun_Blur.Value;
			Texture2D starTex = Assets.StarTexture_Alt.Value;
			Texture2D bloomTex = Assets.Masks.GlowAlpha.Value;

			float fade = 0f;
			if (Charge < 8f)
				fade = Charge / 8f;
			else
				fade = 1f;

			Color color = (ghostProjectile.ModProjectile as SoilProjectile).Colors["TrailInsideColor"] with { A = 0 };

			SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			float rotation = Projectile.rotation + (spriteEffects == SpriteEffects.FlipHorizontally ? MathHelper.Pi : 0f);

			Vector2 position = Projectile.Center - Main.screenPosition;

			if (Shot)
			{
				float progress = 1f - Projectile.timeLeft / 45f;

				if (Projectile.timeLeft < 8f)
					fade = Eases.EaseCircularIn(Projectile.timeLeft / 8f);

				color *= 1f - progress;

				// recoil animation is exaggerated when fully charged
				float recoilDist = MathHelper.Lerp(-12f, -18f, ChargeProgress);
				float recoilRot = MathHelper.Lerp(-0.35f, -0.45f, ChargeProgress);

				if (progress < 0.1f)
				{
					float lerper = progress / 0.1f;

					position += Projectile.rotation.ToRotationVector2() * MathHelper.Lerp(0f, recoilDist, Eases.EaseCircularOut(lerper));

					rotation += MathHelper.Lerp(0f, recoilRot * Projectile.direction, Eases.EaseQuinticOut(lerper));
				}
				else
				{
					float lerper = (progress - 0.1f) / 0.9f;
					position += Projectile.rotation.ToRotationVector2() * MathHelper.Lerp(recoilDist, 0f, Eases.EaseBackOut(lerper));

					rotation += MathHelper.Lerp(recoilRot * Projectile.direction, 0f, Eases.EaseBackOut(lerper));
				}
			}

			float shake = MathHelper.Lerp(0f, 0.5f, ChargeProgress);

			position += Main.rand.NextVector2CircularEdge(shake, shake);

			Main.spriteBatch.Draw(texGlow, position, null, color * fade * ChargeProgress, rotation, texGlow.Size() / 2f, Projectile.scale, spriteEffects, 0f);

			Main.spriteBatch.Draw(tex, position, null, lightColor * fade, rotation, tex.Size() / 2f, Projectile.scale, spriteEffects, 0f);

			Main.spriteBatch.Draw(texBlur, position, null, new Color(255, 255, 255, 0) * fade * ChargeProgress, rotation, texBlur.Size() / 2f, Projectile.scale, spriteEffects, 0f);

			if (flashTimer > 0)
			{
				rotation = 2f * Eases.EaseCircularInOut(flashTimer / 25f);

				Main.spriteBatch.Draw(starTex, BarrelPosition - Main.screenPosition,
					null, color * (flashTimer / 25f), rotation, starTex.Size() / 2f, Projectile.scale * 0.55f, 0f, 0f);

				Main.spriteBatch.Draw(starTex, BarrelPosition - Main.screenPosition,
					null, new Color(255, 255, 255, 0) * (flashTimer / 25f) * 0.25f, rotation, starTex.Size() / 2f, Projectile.scale * 0.55f, 0f, 0f);

				Main.spriteBatch.Draw(bloomTex, BarrelPosition - Main.screenPosition,
					null, color * (flashTimer / 25f), 0f, bloomTex.Size() / 2f, Projectile.scale * 0.85f, 0f, 0f);
			}

			return false;
		}

		/// <summary>
		/// Called when the held projectile should shoot its projectile
		/// </summary>
		private void Shoot()
		{
			Item heldItem = Owner.HeldItem;

			int damage = Projectile.damage;

			float shootSpeed = heldItem.shootSpeed;

			float knockBack = Owner.GetWeaponKnockback(heldItem, heldItem.knockBack);

			Vector2 shootVelocity = Projectile.velocity * shootSpeed;

			if (Main.myPlayer == Projectile.owner)
			{
				if (ChargeProgress >= 1f)
				{
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), BarrelPosition,
							shootVelocity * 1f, (ghostProjectile.ModProjectile as SoilProjectile).ClumpType, damage * 5, knockBack, Owner.whoAmI);
				}
				else
				{
					for (int i = 0; i < 4 + Main.rand.Next(3); i++)
					{
						float minSpeed = MathHelper.Lerp(0.8f, 1f, ChargeProgress);

						Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), BarrelPosition,
							shootVelocity.RotatedByRandom(MathHelper.Lerp(1f, 0.1f, ChargeProgress)) * Main.rand.NextFloat(minSpeed, 1.1f) * MathHelper.Lerp(0.45f, 0.9f, ChargeProgress), projectileID, damage, knockBack, Owner.whoAmI);

						proj.timeLeft = (int)MathHelper.Lerp(15f, 30f, ChargeProgress);
						(proj.ModProjectile as SoilProjectile).maxTimeleft = MathHelper.Lerp(15f, 30f, ChargeProgress);
					}
				}
			}

			Color outColor = (ghostProjectile.ModProjectile as SoilProjectile).Colors["TrailColor"];
			Color inColor = (ghostProjectile.ModProjectile as SoilProjectile).Colors["TrailInsideColor"];

			Color smokeColor = (ghostProjectile.ModProjectile as SoilProjectile).Colors["SmokeColor"];

			for (int i = 0; i < 12; i++)
			{
				Dust.NewDustPerfect(BarrelPosition + Projectile.velocity * 10f, ModContent.DustType<PixelatedGlow>(),
					Projectile.velocity.RotatedByRandom(1f) * Main.rand.NextFloat(0.5f, 3f), 0, outColor with { A = 0 }, 0.55f);
			}

			for (int i = 0; i < 5; i++)
			{
				Dust.NewDustPerfect(BarrelPosition, ModContent.DustType<PixelatedGlow>(),
					Projectile.velocity.RotatedByRandom(1.25f) * Main.rand.NextFloat(0f, 2.5f), 0, inColor with { A = 0 }, 0.35f);

				Dust.NewDustPerfect(BarrelPosition, ModContent.DustType<PixelatedImpactLineDust>(),
					Projectile.velocity.RotatedByRandom(1.25f) * Main.rand.NextFloat(5f, 15f), 0, inColor with { A = 0 }, 0.15f);
			}

			for (int i = 0; i < 3; i++)
			{
				Dust dust = Dust.NewDustPerfect(BarrelPosition, ModContent.DustType<PixelSmokeColor>(),
					Projectile.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.3f, 2f) + Main.rand.NextVector2Circular(1f, 1f), Main.rand.Next(80, 125), new Color(81, 47, 27), Main.rand.NextFloat(0.08f, 0.12f));

				dust.rotation = Main.rand.NextFloat(6.28f);
				dust.customData = new Color(105, 67, 44);

				dust = Dust.NewDustPerfect(BarrelPosition, ModContent.DustType<PixelSmokeColor>(),
					Projectile.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(2.5f, 7f) + Main.rand.NextVector2Circular(1f, 1f), Main.rand.Next(80, 135), new Color(81, 47, 27), Main.rand.NextFloat(0.05f, 0.08f));

				dust.rotation = Main.rand.NextFloat(6.28f);
				dust.customData = new Color(105, 67, 44);

				dust = Dust.NewDustPerfect(BarrelPosition, ModContent.DustType<PixelSmokeColor>(),
					Projectile.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(5f, 7.5f) + Main.rand.NextVector2Circular(1f, 1f), Main.rand.Next(70, 100), smokeColor, Main.rand.NextFloat(0.05f, 0.095f));

				dust.rotation = Main.rand.NextFloat(6.28f);
				dust.customData = smokeColor;
			}

			CameraSystem.shake += (int)MathHelper.Lerp(3, 8, ChargeProgress);

			Owner.reuseDelay = 15;

			SoundEngine.PlaySound(SoundID.Item61, Projectile.position);

			if (Owner.HeldItem.ModItem is Soilgun soilGun)
			{
				int type = soilGun.currentAmmoStruct.projectileID;

				bool dontConsumeAmmo = CheckAmmo(type, soilGun.ammoItem.ammo);

				if (!dontConsumeAmmo)
				{
					soilGun.ammoItem.ModItem?.OnConsumedAsAmmo(Owner.HeldItem, Owner);

					soilGun.OnConsumeAmmo(soilGun.ammoItem, Owner);

					soilGun.ammoItem.stack--;
					if (soilGun.ammoItem.stack <= 0)
						soilGun.ammoItem.TurnToAir();
				}
			}
		}

		/// <summary>
		/// Updates the basic variables needed for a held projectile
		/// </summary>
		private void UpdateHeldProjectile(bool updateTimeleft = true)
		{
			Owner.ChangeDir(Projectile.direction);
			Owner.heldProj = Projectile.whoAmI;

			//Owner.itemTime = 2;
			//Owner.itemAnimation = 2;

			if (Owner.HeldItem.ModItem is not Soilgun)
				Projectile.Kill();

			if (updateTimeleft)
				Projectile.timeLeft = 2;

			Projectile.rotation = Projectile.velocity.ToRotation();
			Owner.itemRotation = Utils.ToRotation(Projectile.velocity * Projectile.direction);

			Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - (Projectile.direction == 1 ? MathHelper.ToRadians(70f) : MathHelper.ToRadians(110f)));

			Projectile.position = ArmPosition - Projectile.Size * 0.5f;

			if (Main.myPlayer == Projectile.owner && updateVelocity)
			{
				float interpolant = Utils.GetLerpValue(5f, 25f, Projectile.Distance(Main.MouseWorld), true);

				Vector2 oldVelocity = Projectile.velocity;

				Projectile.velocity = Vector2.Lerp(Projectile.velocity, Owner.DirectionTo(Main.MouseWorld), .15f);

				if (Projectile.velocity != oldVelocity)
				{
					Projectile.netSpam = 0;
					Projectile.netUpdate = true;
				}
			}

			Projectile.spriteDirection = Projectile.direction;
		}

		public override bool? CanDamage()
		{
			return false;
		}

		private bool CheckAmmo(int type, int ammoID)
		{
			bool dontConsumeAmmo = false;

			if (Owner.magicQuiver && ammoID == AmmoID.Arrow && Main.rand.NextBool(5))
				dontConsumeAmmo = true;
			if (Owner.ammoBox && Main.rand.NextBool(5))
				dontConsumeAmmo = true;
			if (Owner.ammoPotion && Main.rand.NextBool(5))
				dontConsumeAmmo = true;
			if (Owner.ammoCost80 && Main.rand.NextBool(5))
				dontConsumeAmmo = true;
			if (Owner.ammoCost75 && Main.rand.NextBool(4))
				dontConsumeAmmo = true;
			if (type == 85 && Owner.itemAnimation < Owner.itemAnimationMax - 6)
				dontConsumeAmmo = true;
			if ((type == 145 || type == 146 || type == 147 || type == 148 || type == 149) && Owner.itemAnimation < Owner.itemAnimationMax - 5)
				dontConsumeAmmo = true;

			return dontConsumeAmmo;
		}
	}
}