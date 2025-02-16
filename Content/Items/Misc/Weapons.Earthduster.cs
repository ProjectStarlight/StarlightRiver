using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Items.Misc.SoilgunFiles;
using StarlightRiver.Content.Items.UndergroundTemple;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Core.Systems.ExposureSystem;
using StarlightRiver.Core.Systems.PixelationSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using static tModPorter.ProgressUpdate;

namespace StarlightRiver.Content.Items.Misc
{
	public class Earthduster : MultiAmmoWeapon
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public override List<AmmoStruct> ValidAmmos => new()
		{
			new AmmoStruct(ItemID.DirtBlock, ModContent.ProjectileType<EarthdusterDirtShot>(), "", 2),
			new AmmoStruct(ItemID.SandBlock, ModContent.ProjectileType<EarthdusterSandShot>(), "Deals area of effect damage", 2),
			new AmmoStruct(ItemID.EbonsandBlock, ModContent.ProjectileType<EarthdusterEbonsandShot>(), "Inflicts {{BUFF:EbonsandDebuff}}", 4),
			new AmmoStruct(ItemID.PearlsandBlock, ModContent.ProjectileType<EarthdusterPearlsandShot>(), "Homes onto enemies", 15),
			new AmmoStruct(ItemID.CrimsandBlock, ModContent.ProjectileType<EarthdusterCrimsandShot>(), "Gain regeneration on hit", 4),
			new AmmoStruct(ItemID.SiltBlock, ModContent.ProjectileType<EarthdusterSiltShot>(), "Extracts some treasure when fired", 3),
			new AmmoStruct(ItemID.SlushBlock, ModContent.ProjectileType<EarthdusterSlushShot>(), "Inflicts {{BUFF:Frostburn}}", 3),
			new AmmoStruct(Mod.Find<ModItem>("VitricSandItem").Type, ModContent.ProjectileType<EarthdusterVitricSandShot>(), "Pierces enemies", 8),
			new AmmoStruct(ItemID.MudBlock, ModContent.ProjectileType<EarthdusterMudShot>(), "Chance to spawn bees on death", 3),
			new AmmoStruct(ItemID.AshBlock, ModContent.ProjectileType<EarthdusterAshShot>(), "Inflicts {{BUFF:OnFire}}", 6),
		};

		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("Hold <left> to fire a rapid stream of earth\nCan use many different blocks as ammo, each with unique effects\n33% chance to not consume ammo");
		}

		public override void SafeSetDefaults()
		{
			Item.DamageType = DamageClass.Ranged;
			Item.damage = 6;
			Item.width = 70;
			Item.height = 40;
			Item.useAnimation = Item.useTime = 4;
			Item.shoot = ProjectileID.PurificationPowder;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.rare = ItemRarityID.Orange;
			Item.shootSpeed = 15f;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.channel = true;
			Item.knockBack = 3f;
			Item.autoReuse = true;

			Item.value = Item.sellPrice(gold: 2);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			var proj = Projectile.NewProjectileDirect(source, position, velocity, ModContent.ProjectileType<EarthdusterHoldout>(), damage, knockback, player.whoAmI);

			(proj.ModProjectile as EarthdusterHoldout).projectileID = type;
			(proj.ModProjectile as EarthdusterHoldout).ammoID = currentAmmoStruct.ammoID;

			return false;
		}

		public override bool CanConsumeAmmo(Item ammo, Player player)
		{
			return false;
		}

		public override bool SafeCanUseItem(Player player)
		{
			return player.ownedProjectileCounts[ModContent.ProjectileType<EarthdusterHoldout>()] <= 0;
		}

		public override void AddRecipes()
		{
			CreateRecipe().
				AddIngredient(ModContent.ItemType<Soilgun>()).
				AddIngredient(ItemID.CrimtaneBar, 12).
				AddIngredient(ItemID.Bone, 50).
				AddTile(TileID.Anvils).
				Register();

			CreateRecipe().
				AddIngredient(ModContent.ItemType<Soilgun>()).
				AddIngredient(ItemID.DemoniteBar, 12).
				AddIngredient(ItemID.Bone, 50).
				AddTile(TileID.Anvils).
				Register();
		}
	}

	class EarthdusterHoldout : ModProjectile
	{
		public const int MAX_SHOTS = 50;
		public const int MAX_PRESSURE_TIMER = 750;

		public int ammoID;
		public int projectileID;

		public int shots;
		public int ventingTimer;
		public int maxPressureTimer;
		public int pressureFlashTimer;
		public int recoilTimer;

		public float recoilStrength = 1f;

		public Projectile ghostProjectile = new();

		public bool startRightClickAnimation = false;
		public int rightClickAnimationTimer;
		public int newPressureTimer;
		public int oldPressureTimer;

		public bool pressureFlashed;

		public bool updateVelocity = true;
		public float Pressure => PressureTimer / MAX_PRESSURE_TIMER;
		public bool MaxPressure => Pressure >= 1f;
		public ref float Timer => ref Projectile.ai[0];
		public ref float UseTime => ref Projectile.ai[1];
		public ref float PressureTimer => ref Projectile.ai[2];
		public bool CanHold => Owner.channel && !Owner.CCed && !Owner.noItems;
		public Vector2 ArmPosition => Owner.RotatedRelativePoint(Owner.MountedCenter, true) + new Vector2(28f + MathHelper.Lerp(0f, -12f, EaseBuilder.EaseQuarticIn.Ease(Timer < 150f ? Timer / 150f : 1f)), 16f * Owner.direction).RotatedBy(Projectile.rotation);
		public Vector2 BarrelPosition => ArmPosition + Projectile.velocity * Projectile.width * 0.5f + new Vector2(-2f, -2f * Owner.direction).RotatedBy(Projectile.rotation);
		public Player Owner => Main.player[Projectile.owner];
		public override string Texture => AssetDirectory.MiscItem + Name;
		public override void Load()
		{
			GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, Texture + "_Gore1");
			GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, Texture + "_Gore2");
		}
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Earthduster");

			Main.projFrames[Type] = 3;
		}

		public override void SetDefaults()
		{
			Projectile.width = 64;
			Projectile.height = 34;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
		}

		public override void AI()
		{
			if (!CanHold)
			{
				Projectile.Kill();
				return;
			}

			if (Timer == 0f)
			{
				ghostProjectile.SetDefaults(projectileID);
				Projectile.velocity = Owner.DirectionTo(Main.MouseWorld);
				UseTime = CombinedHooks.TotalUseTime(Owner.HeldItem.useTime, Owner, Owner.HeldItem);
			}

			if (MaxPressure && maxPressureTimer < 300)
				maxPressureTimer++;

			// flashes for a bit when max pressure is reached as thats the optimal time to fire rmb
			if (MaxPressure && !pressureFlashed)
			{
				SoundEngine.PlaySound(SoundID.MaxMana, Projectile.Center);

				pressureFlashTimer = 20;
				pressureFlashed = true;
			}

			if (ventingTimer > 0)
			{
				/*PressureTimer = MathHelper.Lerp(MAX_PRESSURE_TIMER, 0f, 1f - ventingTimer / 60f);
				if (ventingTimer == 1)
					PressureTimer = 0;*/

				ventingTimer--;
			}

			if (pressureFlashTimer > 0)
				pressureFlashTimer--;

			if (recoilTimer > 0)
			{
				Lighting.AddLight(BarrelPosition, new Color(255, 255, 20).ToVector3() * new Vector3(2.5f * recoilTimer / 30f, 2.5f * recoilTimer / 30f, 2.5f * recoilTimer / 30f));

				if (recoilTimer == 1)
					recoilStrength = 1f;

				recoilTimer--;
			}

			UpdateHeldProjectile();

			if (ventingTimer <= 0)
			{
				if (!MaxPressure)
					PressureTimer++;
			}

			Timer++;

			if (rightClickAnimationTimer > 0)
			{
				float lerper = 1f - rightClickAnimationTimer / 35f;

				PressureTimer = MathHelper.Lerp(oldPressureTimer, newPressureTimer, lerper);

				Vector2 firePos = BarrelPosition + Projectile.rotation.ToRotationVector2() * MathHelper.Lerp(0f, -15f, EaseBuilder.EaseCircularOut.Ease(lerper));

				Lighting.AddLight(firePos, new Color(255, 255, 20).ToVector3() * new Vector3(1.5f * (1f - lerper), 1.5f * (1f - lerper), 1.5f * (1f - lerper)));

				Dust smoke = Dust.NewDustPerfect(firePos, ModContent.DustType<PixelSmokeColor>(),
				-Vector2.UnitY * Main.rand.NextFloat(1f, 5f), 150, new Color(150, 150, 150), Main.rand.NextFloat(0.08f, 0.016f));

				smoke.rotation = Main.rand.NextFloat(6.28f);
				smoke.customData = new Color(150, 150, 150);
				smoke.noGravity = true;

				for (int i = 0; i < 2; i++)
				{
					Dust dust2 = Dust.NewDustPerfect(firePos, ModContent.DustType<PixelSmokeColor>(),
						Main.rand.NextVector2Circular(2f, 2f), 185, new Color(150, 150, 150), Main.rand.NextFloat(0.08f, 0.016f));

					dust2.rotation = Main.rand.NextFloat(6.28f);
					dust2.customData = new Color(150, 150, 150);
					dust2.noGravity = true;

					Vector2 pos = firePos + Main.rand.NextVector2Circular(150f, 150f) * EaseBuilder.EaseCircularInOut.Ease(1f - lerper);

					Dust.NewDustPerfect(pos, ModContent.DustType<PixelatedImpactLineDust>(), pos.DirectionTo(firePos) * 1.5f, 0, new Color(255, 100, 20, 0), 0.075f);

					pos = firePos + Main.rand.NextVector2Circular(150f, 150f) * EaseBuilder.EaseCircularInOut.Ease(1f - lerper);

					Dust.NewDustPerfect(pos, ModContent.DustType<PixelatedGlow>(), pos.DirectionTo(firePos) * 1.5f, 0, new Color(255, 100, 20, 0), 0.2f);
				}

				if (rightClickAnimationTimer == 1)
				{
					Item heldItem = Owner.HeldItem;

					int damage = Projectile.damage;

					float shootSpeed = heldItem.shootSpeed;

					float knockBack = Owner.GetWeaponKnockback(heldItem, heldItem.knockBack);

					Vector2 shootVelocity = Projectile.velocity * shootSpeed;

					Vector2 barrelPos = BarrelPosition + new Vector2(-20f, 0f).RotatedBy(Projectile.rotation);

					Projectile.NewProjectile(Projectile.GetSource_FromThis(), barrelPos,
						shootVelocity * 1.5f, (ghostProjectile.ModProjectile as EarthdusterProjectile).ClumpType, damage * 5, knockBack, Owner.whoAmI);

					barrelPos = BarrelPosition + new Vector2(-20f, 0f).RotatedBy(Projectile.rotation);

					Vector2 off = new Vector2(0f, -4f * Projectile.direction).RotatedBy(Projectile.rotation);

					Color outColor = (ghostProjectile.ModProjectile as EarthdusterProjectile).Colors["TrailColor"];
					Color inColor = (ghostProjectile.ModProjectile as EarthdusterProjectile).Colors["TrailInsideColor"];

					Color smokeColor = (ghostProjectile.ModProjectile as EarthdusterProjectile).Colors["SmokeColor"];

					for (int i = 0; i < 5; i++)
					{
						Dust.NewDustPerfect(barrelPos + Projectile.velocity * 20f, ModContent.DustType<PixelatedEmber>(),
								Projectile.velocity.RotatedByRandom(1f) * Main.rand.NextFloat(1.5f, 3f), 0, smokeColor with { A = 0 }, 0.3f).customData = -Projectile.direction;

						Dust.NewDustPerfect(barrelPos + Projectile.velocity * 20f, ModContent.DustType<PixelatedEmber>(),
							Projectile.velocity.RotatedByRandom(1f) * Main.rand.NextFloat(1.5f, 3f), 0, new Color(255, 100, 20, 0), 0.3f).customData = -Projectile.direction;
					}

					for (float k = 0; k < 6.28f; k += 0.1f)
					{
						float x = (float)Math.Cos(k) * 30;
						float y = (float)Math.Sin(k) * 10;

						Dust dust = Dust.NewDustPerfect(BarrelPosition + Projectile.velocity * 25f, ModContent.DustType<PixelSmokeColor>(),
							-Projectile.velocity * 0.5f + new Vector2(x, y).RotatedBy(Projectile.velocity.ToRotation() + MathHelper.PiOver2) * 0.08f, Main.rand.Next(50, 80), new Color(255, 150, 0), Main.rand.NextFloat(0.01f, 0.02f));

						dust.rotation = Main.rand.NextFloat(6.28f);
						dust.customData = new Color(150, 150, 150);

						dust = Dust.NewDustPerfect(BarrelPosition, ModContent.DustType<PixelSmokeColor>(),
							-Projectile.velocity * 0.5f + new Vector2(x, y).RotatedBy(Projectile.velocity.ToRotation() + MathHelper.PiOver2) * 0.05f, Main.rand.Next(50, 80), new Color(255, 150, 0), Main.rand.NextFloat(0.01f, 0.02f));

						dust.rotation = Main.rand.NextFloat(6.28f);
						dust.customData = new Color(150, 150, 150);

						dust = Dust.NewDustPerfect(BarrelPosition + Projectile.velocity * 25f, ModContent.DustType<PixelSmokeColor>(),
							-Projectile.velocity * 0.5f + new Vector2(x, y).RotatedBy(Projectile.velocity.ToRotation() + MathHelper.PiOver2) * 0.05f, Main.rand.Next(130, 170), new Color(150, 150, 150), Main.rand.NextFloat(0.03f, 0.06f));

						dust.rotation = Main.rand.NextFloat(6.28f);
						dust.customData = new Color(150, 150, 150);

						Dust.NewDustPerfect(BarrelPosition, ModContent.DustType<PixelatedGlow>(),
							new Vector2(x, y).RotatedBy(Projectile.velocity.ToRotation() + MathHelper.PiOver2) * 0.05f, 0, new Color(255, 100, 20, 0), 0.35f).customData = -Projectile.direction;

						Dust.NewDustPerfect(BarrelPosition + Projectile.velocity * 25f, ModContent.DustType<PixelatedGlow>(),
							new Vector2(x, y).RotatedBy(Projectile.velocity.ToRotation() + MathHelper.PiOver2) * 0.08f, 0, new Color(255, 100, 20, 0), 0.35f).customData = -Projectile.direction;
					}

					for (int i = 0; i < 15; i++)
					{
						Dust.NewDustPerfect(barrelPos + Projectile.velocity * 20f + off, ModContent.DustType<PixelatedGlow>(),
							Main.rand.NextVector2Circular(4f, 4f), 0, new Color(255, 100, 20, 0), 0.3f).customData = -Projectile.direction;

						Dust.NewDustPerfect(barrelPos + Projectile.velocity * 20f + off, ModContent.DustType<PixelatedGlow>(),
							Main.rand.NextVector2Circular(8f, 8f), 0, new Color(255, 100, 20, 0), 0.3f).customData = -Projectile.direction;

						Dust.NewDustPerfect(barrelPos + Projectile.velocity * 20f + off, ModContent.DustType<PixelatedGlow>(),
							Projectile.velocity.RotatedByRandom(25f) * Main.rand.NextFloat(1f, 3f), 0, new Color(255, 100, 20, 0), 0.7f).customData = -Projectile.direction;

						Dust.NewDustPerfect(barrelPos + Projectile.velocity * 20f + off, ModContent.DustType<PixelatedGlow>(),
							Projectile.velocity.RotatedByRandom(0.25f) * Main.rand.NextFloat(3f, 6f), 0, new Color(255, 100, 20, 0), 0.3f).customData = -Projectile.direction;

						Dust.NewDustPerfect(barrelPos + Projectile.velocity * 20f + off, ModContent.DustType<PixelatedGlow>(),
							Projectile.velocity.RotatedByRandom(1.5f) * Main.rand.NextFloat(1f, 3f), 0, new Color(255, 100, 20, 0), 0.3f).customData = -Projectile.direction;

						Dust.NewDustPerfect(barrelPos + Projectile.velocity * 20f + off, ModContent.DustType<PixelatedGlow>(),
							Projectile.velocity.RotatedByRandom(1f) * Main.rand.NextFloat(8f), 0, inColor with { A = 0 }, 0.4f).customData = -Projectile.direction;
					}

					//SoundEngine.PlaySound(SoundID.Item61, Projectile.position);
					SoundHelper.PlayPitched("Magic/FireHit", 1f, -0.5f, Projectile.Center);

					recoilStrength = 2f;

					if (Owner.velocity.Y != 0f)
						Owner.velocity -= Projectile.velocity * 15f;
					else
						Owner.velocity -= Projectile.velocity * 5f;

					CameraSystem.shake += 18;
					ventingTimer = 120;
					maxPressureTimer = 0;
					pressureFlashed = false;

					recoilTimer = 30;

					if (Owner.HeldItem.ModItem is Earthduster earthDuster)
					{
						int type = earthDuster.currentAmmoStruct.projectileID;

						bool dontConsumeAmmo = CheckAmmo(type, earthDuster.ammoItem.ammo);

						if (!dontConsumeAmmo && Main.rand.NextFloat() < 0.66f)
						{
							earthDuster.ammoItem.ModItem?.OnConsumedAsAmmo(Owner.HeldItem, Owner);

							earthDuster.OnConsumeAmmo(earthDuster.ammoItem, Owner);

							earthDuster.ammoItem.stack--;
							if (earthDuster.ammoItem.stack <= 0)
								earthDuster.ammoItem.TurnToAir();
						}
					}
				}

				rightClickAnimationTimer--;
				return;
			}

			float spinUpTime = (int)(UseTime * MathHelper.Lerp(5f, 1f, EaseBuilder.EaseCircularOut.Ease(Pressure)));

			if (maxPressureTimer > 0)
				spinUpTime = (int)(UseTime * MathHelper.Lerp(1f, 3f, maxPressureTimer / 300f));

			if (++Projectile.frameCounter % (int)Utils.Clamp(spinUpTime - 3, 1, 50) == 0 && ventingTimer <= 0)
				Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];

			if (ventingTimer > 0)
				spinUpTime = (int)MathHelper.Lerp(spinUpTime * 3f, spinUpTime, 1f - ventingTimer / 120f);

			if ((int)Timer % spinUpTime == 0 && recoilTimer <= 0)
			{
				Shoot();
			}

			if (Pressure > 0.5f && Main.rand.NextBool(5))
			{
				float interpolant = (Pressure - 0.5f) / 0.5f;

				if (MaxPressure)
					interpolant = 1f - maxPressureTimer / 300f;

				Dust dust = Dust.NewDustPerfect(BarrelPosition, ModContent.DustType<PixelSmokeColor>(),
				-Vector2.UnitY * Main.rand.NextFloat(1f, 5f), 150, new Color(150, 150, 150) * interpolant, Main.rand.NextFloat(0.08f, 0.016f));

				dust.rotation = Main.rand.NextFloat(6.28f);
				dust.customData = new Color(150, 150, 150);
				dust.noGravity = true;
			}

			if (Main.myPlayer == Projectile.owner)
			{
				// you need atleast half pressure to fire a clump
				if (Main.mouseRight && ventingTimer <= 0 && Pressure > 0.5f)
				{
					oldPressureTimer = (int)PressureTimer;

					newPressureTimer = (int)(PressureTimer * 0.5f - maxPressureTimer * 2f);
					if (newPressureTimer < 0)
						newPressureTimer = 0;

					rightClickAnimationTimer = 35;

					SoundHelper.PlayPitched("Impacts/HammerSteamSlam", 2f, -0.25f, Projectile.Center);
				}
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (Timer <= 2)
				return false;

			SpriteBatch sb = Main.spriteBatch;

			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D holderTex = ModContent.Request<Texture2D>(Texture + "_AmmoHolder").Value;
			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;
			Texture2D starTex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "StarTexture_Alt").Value;

			Texture2D blockTex = TextureAssets.Item[ammoID].Value;

			Rectangle frame = tex.Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame);

			SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			float rotation = Projectile.rotation + (spriteEffects == SpriteEffects.FlipHorizontally ? MathHelper.Pi : 0f);

			Vector2 position = Projectile.Center - Main.screenPosition;

			if (recoilTimer > 0)
			{
				float progress = 1f - recoilTimer / 30f;

				if (progress < 0.1f)
				{
					float interpolant = progress / 0.1f;

					position += Projectile.rotation.ToRotationVector2() * MathHelper.Lerp(0f, -15f * recoilStrength, EaseBuilder.EaseCircularOut.Ease(interpolant));

					rotation += MathHelper.Lerp(0f, -0.5f * Projectile.direction, EaseBuilder.EaseQuinticOut.Ease(interpolant));
				}
				else
				{
					float interpolant = (progress - 0.1f) / 0.9f;
					position += Projectile.rotation.ToRotationVector2() * MathHelper.Lerp(-15f * recoilStrength, 0f, EaseBuilder.EaseBackOut.Ease(interpolant));

					rotation += MathHelper.Lerp(-0.5f * Projectile.direction, 0f, EaseBuilder.EaseBackOut.Ease(interpolant));
				}
			}

			if (rightClickAnimationTimer > 0)
			{
				float progress = 1f - rightClickAnimationTimer / 35f;

				position += Projectile.rotation.ToRotationVector2() * MathHelper.Lerp(0f, -15f, EaseBuilder.EaseCircularOut.Ease(progress));
			}

			float lerper = Timer < 150f ? Timer / 150f : 1f;

			Vector2 off = Main.rand.NextVector2Circular(2.5f * lerper, 0.8f * lerper).RotatedBy(Projectile.rotation);

			sb.Draw(tex, position + off, frame, lightColor, rotation, frame.Size() / 2f, Projectile.scale, spriteEffects, 0f);

			sb.Draw(blockTex, position + new Vector2(-17f, 4f * Projectile.direction).RotatedBy(Projectile.rotation) + off, null, lightColor, rotation, blockTex.Size() / 2f, Projectile.scale, spriteEffects, 0f);

			sb.Draw(holderTex, position + off, null, lightColor, rotation, holderTex.Size() / 2f, Projectile.scale, spriteEffects, 0f);

			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("Dusts", () =>
			{
				Effect effect = Filters.Scene["ColoredFireAlpha"].GetShader().Shader;

				if (effect is null)
					return;

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.EffectMatrix);

				Vector2 firePos = BarrelPosition;

				if (rightClickAnimationTimer > 0)
				{
					float progress = 1f - rightClickAnimationTimer / 35f;

					position += Projectile.rotation.ToRotationVector2() * MathHelper.Lerp(0f, -15f, EaseBuilder.EaseCircularOut.Ease(progress));

					firePos += Projectile.rotation.ToRotationVector2() * MathHelper.Lerp(0f, -15f, EaseBuilder.EaseCircularOut.Ease(progress));
				}

				float fadeIn = PressureTimer < 250f ? PressureTimer / 250f : 1f;

				float pressureFade = maxPressureTimer > 0 ? Utils.Clamp(1f - maxPressureTimer / 300f, 0.5f, 1f) : fadeIn;

				sb.Draw(bloomTex, firePos - Main.screenPosition, null, new Color(255, 120, 0, 0) * pressureFade * 0.2f, Projectile.rotation - MathHelper.PiOver2, bloomTex.Size() / 2f, 0.75f, 0, 0);

				effect.Parameters["u_time"].SetValue(Timer * 0.01f % 2f);
				effect.Parameters["primary"].SetValue(new Vector3(1, 0.7f, 0.1f) * pressureFade);
				effect.Parameters["primaryScaling"].SetValue(new Vector3(1, 1, 1));
				effect.Parameters["secondary"].SetValue(new Vector3(1f, 0.2f, 0.05f) * pressureFade);

				effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Noise/MiscNoise3").Value);
				effect.Parameters["mapTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Noise/MiscNoise3").Value);

				effect.CurrentTechnique.Passes[0].Apply();

				sb.Draw(bloomTex, firePos + new Vector2(-16f * fadeIn, 0f).RotatedBy(Projectile.rotation) - Main.screenPosition, null, Color.White * pressureFade, Projectile.rotation - MathHelper.PiOver2, bloomTex.Size() / 2f, new Vector2(0.3f, 0.3f * fadeIn), 0, 0);

				fadeIn = PressureTimer < 500f ? PressureTimer / 500f : 1f;

				effect.Parameters["u_time"].SetValue(Timer * 0.02f % 2f);
				effect.Parameters["primary"].SetValue(new Vector3(1, 0.7f, 0.1f) * pressureFade);
				effect.Parameters["primaryScaling"].SetValue(new Vector3(1, 1, 1));
				effect.Parameters["secondary"].SetValue(new Vector3(1f, 0.2f, 0.05f) * pressureFade);

				effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Noise/MiscNoise3").Value);
				effect.Parameters["mapTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Noise/MiscNoise3").Value);

				effect.CurrentTechnique.Passes[0].Apply();

				sb.Draw(bloomTex, firePos + new Vector2(-16f * fadeIn, 0f).RotatedBy(Projectile.rotation) - Main.screenPosition, null, Color.White * pressureFade, Projectile.rotation - MathHelper.PiOver2, bloomTex.Size() / 2f, new Vector2(0.3f, 0.5f * fadeIn), 0, 0);

				fadeIn = Pressure;

				effect.Parameters["u_time"].SetValue(Timer * 0.01f % 2f);
				effect.Parameters["primary"].SetValue(new Vector3(1, 0.7f, 0.1f) * pressureFade);
				effect.Parameters["primaryScaling"].SetValue(new Vector3(1, 1, 1));
				effect.Parameters["secondary"].SetValue(new Vector3(1f, 0.2f, 0.05f) * pressureFade);

				effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Noise/MiscNoise3").Value);
				effect.Parameters["mapTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Noise/MiscNoise3").Value);

				effect.CurrentTechnique.Passes[0].Apply();

				sb.Draw(bloomTex, firePos + new Vector2(-32f * fadeIn, 0f).RotatedBy(Projectile.rotation) - Main.screenPosition, null, Color.White * pressureFade, Projectile.rotation - MathHelper.PiOver2, bloomTex.Size() / 2f, new Vector2(0.3f, 1f * fadeIn), 0, 0);

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.EffectMatrix);
			});

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			if (Pressure > 0.5f)
			{
				Vector2 firePos = BarrelPosition;

				if (rightClickAnimationTimer > 0)
				{
					float progress = 1f - rightClickAnimationTimer / 35f;

					firePos += Projectile.rotation.ToRotationVector2() * MathHelper.Lerp(0f, -15f, EaseBuilder.EaseCircularOut.Ease(progress));
				}

				float interpolant = (Pressure - 0.5f) / 0.5f;

				if (MaxPressure)
					interpolant = 1f - maxPressureTimer / 300f;

				Main.spriteBatch.Draw(bloomTex, firePos + off - Main.screenPosition,
					null, new Color(255, 0, 0, 0) * interpolant * 0.45f, 0f, bloomTex.Size() / 2f, Projectile.scale * 1f, 0f, 0f);

				Main.spriteBatch.Draw(bloomTex, firePos + off - Main.screenPosition,
					null, new Color(255, 150, 0, 0) * interpolant * 0.45f, 0f, bloomTex.Size() / 2f, Projectile.scale * 0.85f, 0f, 0f);

				Main.spriteBatch.Draw(bloomTex, position + new Vector2(-17f, 4f * Projectile.direction).RotatedBy(Projectile.rotation) + off,
					null, new Color(255, 150, 0, 0) * interpolant * 0.45f, 0f, bloomTex.Size() / 2f, Projectile.scale * 0.55f, 0f, 0f);
			}

			Color color = (ghostProjectile.ModProjectile as EarthdusterProjectile).Colors["TrailInsideColor"] with { A = 0 };

			if (pressureFlashTimer > 0)
			{
				rotation = 2f * EaseBuilder.EaseCircularInOut.Ease(pressureFlashTimer / 20f);

				Main.spriteBatch.Draw(starTex, position + new Vector2(-17f, 4f * Projectile.direction).RotatedBy(Projectile.rotation) + off,
					null, color * (pressureFlashTimer / 20f), rotation, starTex.Size() / 2f, Projectile.scale * 0.55f, 0f, 0f);

				Main.spriteBatch.Draw(starTex, position + new Vector2(-17f, 4f * Projectile.direction).RotatedBy(Projectile.rotation) + off,
					null, new Color(255, 255, 255, 0) * (pressureFlashTimer / 20f) * 0.25f, rotation, starTex.Size() / 2f, Projectile.scale * 0.55f, 0f, 0f);

				Main.spriteBatch.Draw(bloomTex, position + new Vector2(-17f, 4f * Projectile.direction).RotatedBy(Projectile.rotation) + off,
					null, color * (pressureFlashTimer / 20f), 0f, bloomTex.Size() / 2f, Projectile.scale * 0.85f, 0f, 0f);
			}

			return false;
		}

		/// <summary>
		/// Called when the held projectile should shoot its projectile (in this case, Needle)
		/// </summary>
		private void Shoot()
		{
			if (shots < MAX_SHOTS)
				shots++;

			Item heldItem = Owner.HeldItem;

			int damage = Projectile.damage;

			float shootSpeed = heldItem.shootSpeed;

			float knockBack = Owner.GetWeaponKnockback(heldItem, heldItem.knockBack);

			Vector2 shootVelocity = Projectile.velocity * shootSpeed;

			Vector2 barrelPos = BarrelPosition + new Vector2(-20f, 0f).RotatedBy(Projectile.rotation);

			Vector2 off = new Vector2(0f, -4f * Projectile.direction).RotatedBy(Projectile.rotation);

			if (Main.myPlayer == Projectile.owner)
			{
				Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), barrelPos + off,
				shootVelocity.RotatedByRandom(0.05f) * Main.rand.NextFloat(0.9f, 1.1f), projectileID, damage, knockBack, Owner.whoAmI);

				proj.timeLeft = 300;
				(proj.ModProjectile as EarthdusterProjectile).maxTimeleft = 300;
			}

			if (CameraSystem.shake < 3)
				CameraSystem.shake += 2;

			Color outColor = (ghostProjectile.ModProjectile as EarthdusterProjectile).Colors["TrailColor"];
			Color inColor = (ghostProjectile.ModProjectile as EarthdusterProjectile).Colors["TrailInsideColor"];

			Color smokeColor = (ghostProjectile.ModProjectile as EarthdusterProjectile).Colors["SmokeColor"];

			Dust.NewDustPerfect(barrelPos + Projectile.velocity * 20f, ModContent.DustType<PixelatedEmber>(),
						Projectile.velocity.RotatedByRandom(1f) * Main.rand.NextFloat(1.5f, 3f), 0, smokeColor with { A = 0 }, 0.15f).customData = -Projectile.direction;

			if (Main.rand.NextBool(5))
			{
				for (int i = 0; i < 2; i++)
				{
					Dust.NewDustPerfect(barrelPos + Projectile.velocity * 20f, ModContent.DustType<PixelatedEmber>(),
						Projectile.velocity.RotatedByRandom(1f) * Main.rand.NextFloat(1.5f, 3f), 0, new Color(255, 100, 20, 0), 0.15f).customData = -Projectile.direction;
				}
			}

			Dust.NewDustPerfect(Projectile.Center + new Vector2(-17f, 4f * Projectile.direction).RotatedBy(Projectile.rotation), (ghostProjectile.ModProjectile as EarthdusterProjectile).dustID,
				-Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(1f, 2f) - Vector2.UnitY * 2f, Main.rand.Next(50, 150), default, 1.2f);

			Dust dust = Dust.NewDustPerfect(BarrelPosition, ModContent.DustType<PixelSmokeColor>(),
				Projectile.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.3f, 2f) + Main.rand.NextVector2Circular(1f, 1f), Main.rand.Next(120, 155), new Color(81, 47, 27), Main.rand.NextFloat(0.05f, 0.10f));

			dust.rotation = Main.rand.NextFloat(6.28f);
			dust.customData = new Color(105, 67, 44);

			dust = Dust.NewDustPerfect(BarrelPosition, ModContent.DustType<PixelSmokeColor>(),
				Projectile.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(1f, 4f) + Main.rand.NextVector2Circular(1f, 1f), Main.rand.Next(120, 155), new Color(81, 47, 27), Main.rand.NextFloat(0.03f, 0.06f));

			dust.rotation = Main.rand.NextFloat(6.28f);
			dust.customData = new Color(105, 67, 44);

			dust = Dust.NewDustPerfect(BarrelPosition, ModContent.DustType<PixelSmokeColor>(),
				Projectile.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(2f, 5f) + Main.rand.NextVector2Circular(1f, 1f), Main.rand.Next(120, 150), smokeColor, Main.rand.NextFloat(0.04f, 0.08f));

			dust.rotation = Main.rand.NextFloat(6.28f);
			dust.customData = smokeColor;

			float lerper = shots / (float)MAX_SHOTS;

			dust = Dust.NewDustPerfect(BarrelPosition, ModContent.DustType<PixelSmokeColor>(),
				-Vector2.UnitY * Main.rand.NextFloat(1f, 4f), (int)MathHelper.Lerp(255, 160, lerper), new Color(150, 150, 150), Main.rand.NextFloat(0.04f, 0.05f));

			dust.rotation = Main.rand.NextFloat(6.28f);
			dust.customData = new Color(150, 150, 150);
			dust.noGravity = true;

			Dust.NewDustPerfect(BarrelPosition + Projectile.velocity * 20f + off,
				ModContent.DustType<EarthdusterMuzzleFlashDust>(), Projectile.velocity * 1f, 0, default, Main.rand.NextFloat(0.8f, 1.2f)).rotation = Projectile.velocity.ToRotation();

			for (int i = 0; i < 2; i++)
			{
				Dust.NewDustPerfect(barrelPos + Projectile.velocity * 20f + off, ModContent.DustType<PixelatedGlow>(),
					Projectile.velocity.RotatedByRandom(0.25f) * Main.rand.NextFloat(3f, 6f), 0, new Color(255, 100, 20, 0), 0.15f).customData = -Projectile.direction;

				Dust.NewDustPerfect(barrelPos + Projectile.velocity * 20f + off, ModContent.DustType<PixelatedGlow>(),
					Projectile.velocity.RotatedByRandom(1.5f) * Main.rand.NextFloat(1f, 3f), 0, new Color(255, 100, 20, 0), 0.15f).customData = -Projectile.direction;
			}

			SoundHelper.PlayPitched("VitricBoss/ceramicimpact", 1f, Pressure, Projectile.Center);

			if (MaxPressure)
				SoundHelper.PlayPitched("Guns/dry_fire", 1f, -0.5f, Projectile.Center);

			if (Owner.HeldItem.ModItem is Earthduster earthDuster)
			{
				if (earthDuster.ammoItem is null)
				{
					Projectile.Kill();
					return;
				}

				int type = earthDuster.currentAmmoStruct.projectileID;

				bool dontConsumeAmmo = CheckAmmo(type, earthDuster.ammoItem.ammo);

				if (!dontConsumeAmmo && Main.rand.NextFloat() < 0.66f)
				{
					earthDuster.ammoItem.ModItem?.OnConsumedAsAmmo(Owner.HeldItem, Owner);

					earthDuster.OnConsumeAmmo(earthDuster.ammoItem, Owner);

					earthDuster.ammoItem.stack--;
					if (earthDuster.ammoItem.stack <= 0)
						earthDuster.ammoItem.TurnToAir();
				}
			}
		}

		/// <summary>
		/// Updates the basic variables needed for a held projectile
		/// </summary>
		private void UpdateHeldProjectile()
		{
			Owner.ChangeDir(Projectile.direction);
			Owner.heldProj = Projectile.whoAmI;
			Owner.itemTime = 2;
			Owner.itemAnimation = 2;

			Projectile.timeLeft = 2;
			Projectile.rotation = Utils.ToRotation(Projectile.velocity);
			Owner.itemRotation = Utils.ToRotation(Projectile.velocity * Projectile.direction);

			Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - (Projectile.direction == 1 ? MathHelper.ToRadians(70f) : MathHelper.ToRadians(110f)));

			Projectile.position = ArmPosition - Projectile.Size * 0.5f;

			if (Main.myPlayer == Projectile.owner && updateVelocity)
			{
				float interpolant = Utils.GetLerpValue(5f, 25f, Projectile.Distance(Main.MouseWorld), true);

				Vector2 oldVelocity = Projectile.velocity;

				Projectile.velocity = Vector2.Lerp(Projectile.velocity, Owner.DirectionTo(Main.MouseWorld), 0.2f);
				if (Projectile.velocity != oldVelocity)
				{
					Projectile.netSpam = 0;
					Projectile.netUpdate = true;
				}
			}

			Projectile.spriteDirection = Projectile.direction;
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

		public override bool? CanDamage()
		{
			return false;
		}
	}

	public class EarthdusterMuzzleFlashDust : ModDust
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void OnSpawn(Dust dust)
		{
			dust.frame = new Rectangle(0, 0, 4, 4);
		}

		public override bool Update(Dust dust)
		{
			dust.customData ??= 0;

			if ((int)dust.customData < 4)
				dust.customData = (int)dust.customData + 1;

			dust.alpha += 20;
			dust.alpha = (int)(dust.alpha * 1.05f);

			if (dust.alpha >= 255)
				dust.active = false;

			Lighting.AddLight(dust.position, new Color(255, 255, 20).ToVector3() * new Vector3(1.5f * 1f - dust.alpha / 255f, 1.5f * 1f - dust.alpha / 255f, 1.5f * 1f - dust.alpha / 255f));

			return false;
		}

		public override bool PreDraw(Dust dust)
		{
			float lerper = 1f - dust.alpha / 255f;

			Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.MiscItem + Name).Value;
			Texture2D texBlur = ModContent.Request<Texture2D>(AssetDirectory.MiscItem + Name + "_Blur").Value;
			Texture2D texGlow = ModContent.Request<Texture2D>(AssetDirectory.MiscItem + Name + "_Glow").Value;
			Texture2D texFireGlow = ModContent.Request<Texture2D>(AssetDirectory.MiscItem + Name + "_FireGlow").Value;
			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;

			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("Dusts", () =>
			{
				if (dust.customData is null)
					return;

				if (dust.customData is not int)
					return;

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.EffectMatrix);

				Main.spriteBatch.Draw(bloomTex, dust.position - Main.screenPosition, null, new Color(255, 75, 0, 0) * 0.25f * lerper, dust.rotation, bloomTex.Size() / 2f, dust.scale * 1.25f, 0f, 0f);

				Rectangle frame = texGlow.Frame(verticalFrames: 3, frameY: (int)Math.Floor((float)(int)dust.customData / 2));

				Main.spriteBatch.Draw(texGlow, dust.position - Main.screenPosition, frame, new Color(255, 75, 0, 0) * lerper, dust.rotation, frame.Size() / 2f, dust.scale, 0f, 0f);

				frame = tex.Frame(verticalFrames: 3, frameY: (int)Math.Floor((float)(int)dust.customData / 2));

				Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, frame, Color.White * lerper, dust.rotation, frame.Size() / 2f, dust.scale, 0f, 0f);

				frame = texBlur.Frame(verticalFrames: 3, frameY: (int)Math.Floor((float)(int)dust.customData / 2));

				Main.spriteBatch.Draw(texBlur, dust.position - Main.screenPosition, frame, Color.White with { A = 0 } * 0.5f * lerper, dust.rotation, frame.Size() / 2f, dust.scale, 0f, 0f);

				Effect effect = Filters.Scene["ColoredFireAlpha"].GetShader().Shader;

				if (effect is null)
					return;

				effect.Parameters["u_time"].SetValue((float)(Main.timeForVisualEffects * 0.01f % 2f));
				effect.Parameters["primary"].SetValue(new Vector3(1, 0.7f, 0.1f) * lerper);
				effect.Parameters["primaryScaling"].SetValue(new Vector3(1, 1, 1));
				effect.Parameters["secondary"].SetValue(new Vector3(1f, 0.2f, 0.05f) * lerper);

				effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Noise/MiscNoise3").Value);
				effect.Parameters["mapTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Noise/MiscNoise3").Value);

				effect.CurrentTechnique.Passes[0].Apply();

				Main.spriteBatch.Draw(texFireGlow, dust.position - Main.screenPosition, null, Color.White with { A = 0 } * lerper, dust.rotation - MathHelper.PiOver2, texFireGlow.Size() / 2f, new Vector2(1.5f, 2f * lerper), 0, 0);

				effect.Parameters["u_time"].SetValue((float)(Main.timeForVisualEffects * 0.005f % 2f));
				effect.Parameters["primary"].SetValue(new Vector3(1, 0.7f, 0.1f) * lerper);
				effect.Parameters["primaryScaling"].SetValue(new Vector3(1, 1, 1));
				effect.Parameters["secondary"].SetValue(new Vector3(1f, 0.2f, 0.05f) * lerper);

				effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Noise/MiscNoise3").Value);
				effect.Parameters["mapTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Noise/MiscNoise3").Value);

				effect.CurrentTechnique.Passes[0].Apply();

				Main.spriteBatch.Draw(texFireGlow, dust.position - Main.screenPosition, null, Color.White with { A = 0 } * lerper, dust.rotation - MathHelper.PiOver2, texFireGlow.Size() / 2f, new Vector2(1f, 3f * lerper), 0, 0);

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.EffectMatrix);
			}, 1);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			return false;
		}
	}
}