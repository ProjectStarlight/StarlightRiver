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
using static Humanizer.In;

namespace StarlightRiver.Content.Items.Misc.SoilgunFiles
{
	// This entire thing needs balancing, hopefully this code is better than before and hopefully all issues are fixed

	public class Soilgun : MultiAmmoWeapon
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public override List<AmmoStruct> ValidAmmos => new()
		{
			new AmmoStruct(ItemID.SandBlock, ModContent.ProjectileType<SoilgunSandSoil>(), 2),
			new AmmoStruct(ItemID.EbonsandBlock, ModContent.ProjectileType<SoilgunEbonsandSoil>(), 4),
			new AmmoStruct(ItemID.PearlsandBlock, ModContent.ProjectileType<SoilgunPearlsandSoil>(), 15),
			new AmmoStruct(ItemID.CrimsandBlock, ModContent.ProjectileType<SoilgunCrimsandSoil>(), 4),
			new AmmoStruct(ItemID.DirtBlock, ModContent.ProjectileType<SoilgunDirtSoil>(), 2),
			new AmmoStruct(ItemID.SiltBlock, ModContent.ProjectileType<SoilgunSiltSoil>(), 3),
			new AmmoStruct(ItemID.SlushBlock, ModContent.ProjectileType<SoilgunSlushSoil>(), 3),
			new AmmoStruct(Mod.Find<ModItem>("VitricSandItem").Type, ModContent.ProjectileType<SoilgunVitricSandSoil>(), 8),
			new AmmoStruct(ItemID.MudBlock, ModContent.ProjectileType<SoilgunMudSoil>(), 3),
		};

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Soilgun");
			Tooltip.SetDefault("Hold <left> to charge up a volley of soil\nRelease to fire the soil at high velocities\n" +
				"Can use many different types of soils\n'Soiled it! SOILED IT!'");
		}

		public override void SafeSetDefaults()
		{
			Item.DamageType = DamageClass.Ranged;
			Item.damage = 4;
			Item.width = 60;
			Item.height = 36;
			Item.useAnimation = Item.useTime = 160;
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

	class SoilgunGlobalItem : GlobalItem
	{
		public TooltipLine ammoTooltip;

		public override bool InstancePerEntity => true;

		public List<int> ValidSoils => new()
		{
			ItemID.SandBlock,
			ItemID.EbonsandBlock,
			ItemID.PearlsandBlock,
			ItemID.CrimsandBlock,
			ItemID.DirtBlock,
			ItemID.SiltBlock,
			ItemID.SlushBlock,
			Mod.Find<ModItem>("VitricSandItem").Type,
			ItemID.MudBlock
		};

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			if (Main.LocalPlayer.HasItem(ModContent.ItemType<Soilgun>()) || Main.LocalPlayer.HasItem(ModContent.ItemType<Earthduster>()))
			{
				if (ValidSoils.Contains(item.type))
				{
					var tooltip = new TooltipLine(Mod, "SoilgunAmmoTooltip", "This item can be used as ammo for the Soilgun and Earthduster");
					tooltips.Add(tooltip);
					tooltip.OverrideColor = new Color(202, 148, 115);

					if (item.type == Mod.Find<ModItem>("VitricSandItem").Type)
					{
						var infoTooltip = new TooltipLine(Mod, "AmmoInfoTooltip", "When used with the Soilgun or Earthduster, it will fire out blocks of glassy sand, that cause crystals to grow out of enemies\nFor each crystal an enemy has, they take 2 damage per second, plus a base damage of 4, up to a maximum of 10 crystals\nIf an enemy has had 10 crystals on them for more than 4 seconds, all crystals become charged, exploding shorty after");
						tooltips.Add(infoTooltip);
						infoTooltip.OverrideColor = new Color(202, 148, 115);
						return;
					}

					switch (item.type)
					{
						case ItemID.SandBlock: ammoTooltip = new TooltipLine(Mod, "AmmoInfoTooltip", "When used with the Soilgun or Earthduster, it will fire out blocks of sand that split into many grains of sand upon death"); break;
						case ItemID.CrimsandBlock: ammoTooltip = new TooltipLine(Mod, "AmmoInfoTooltip", "When used with the Soilgun or Earthduster, it will fire out blocks of Crimsand that steal life from hit enemies"); break;
						case ItemID.EbonsandBlock: ammoTooltip = new TooltipLine(Mod, "AmmoInfoTooltip", "When used with the Soilgun or Earthduster, it will fire out blocks of Ebonsand that apply stacks of Haunted to enemies"); break;
						case ItemID.PearlsandBlock: ammoTooltip = new TooltipLine(Mod, "AmmoInfoTooltip", "When used with the Soilgun or Earthduster, it will fire out blocks of Pearlsand that home in on enemies"); break;
						case ItemID.DirtBlock: ammoTooltip = new TooltipLine(Mod, "AmmoInfoTooltip", "When used with the Soilgun or Earthduster, it will fire out blocks of dirt"); break;
						case ItemID.SiltBlock: ammoTooltip = new TooltipLine(Mod, "AmmoInfoTooltip", "When used with the Soilgun or Earthduster, it will fire out blocks of silt, that spawn coins upon hitting enemies"); break;
						case ItemID.SlushBlock: ammoTooltip = new TooltipLine(Mod, "AmmoInfoTooltip", "When used with the Soilgun or Earthduster, it will fire out blocks of slush that cause hit enemies to have icicles impale them\nHitting and enemy with more than 10 icicles causes all icicles to shatter, causing large amounts of damage"); break;
						case ItemID.MudBlock: ammoTooltip = new TooltipLine(Mod, "AmmoInfoTooltip", "When used with the Soilgun or Earthduster, it will fire out blocks of mud that bounce off tiles and enemies"); break;
					}

					tooltips.Add(ammoTooltip);
					ammoTooltip.OverrideColor = new Color(202, 148, 115);
				}
			}
		}
	}

	//TODO: Port to stackable buffs
	class SoilgunGlobalNPC : GlobalNPC
	{
		public const int MAXHAUNTEDSTACKS = 20;

		public const int MAXVITRICSHARDS = 10;

		public int GlassAmount;

		public int GlassPlayerID;

		public int HauntedStacks;

		public int HauntedTimer;

		public int HauntedSoulDamage;

		public int HauntedSoulOwner;

		public int SpawnHauntedSoulTimer = 60;

		public int ShardAmount;

		public int ShardTimer;

		public int ShardOwner;

		public int HowLongShardHasBeenOnTarget;
		public override bool InstancePerEntity => true;

		public override void ResetEffects(NPC npc)
		{
			if (HauntedTimer > 0)
			{
				HauntedTimer--;
				if (HauntedTimer <= 0)
				{
					HauntedTimer = 0;
					HauntedStacks = 0;
				}
			}

			HauntedStacks = Utils.Clamp(HauntedStacks, 0, MAXHAUNTEDSTACKS);

			if (ShardTimer > 0)
			{
				ShardTimer--;
				if (ShardTimer <= 0)
				{
					ShardTimer = 0;
					ShardAmount = 0;
				}
			}

			ShardAmount = Utils.Clamp(ShardAmount, 0, MAXVITRICSHARDS);
		}
		public override void AI(NPC npc)
		{
			if (GlassAmount > 0)
			{
				if (Main.rand.NextBool(20 - GlassAmount))
					Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Ice, 0f, 0f, 25, default, Main.rand.NextFloat(0.8f, 1.1f));
			}

			if (HauntedTimer > 0)
			{
				float Rand = MathHelper.Clamp(20 - HauntedStacks, 1f, 20f);
				if (Main.rand.NextBool((int)Rand))
				{
					Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Shadowflame, 0f, 0f, 25, default, Main.rand.NextFloat(0.75f, 1.2f));
					if (Main.rand.NextBool(3))
						Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Shadowflame, 0f, 0f, 0, default, Main.rand.NextFloat(0.1f, 1.5f));
				}

				if (HauntedStacks == MAXHAUNTEDSTACKS)
				{
					if (SpawnHauntedSoulTimer == 60)
						Helper.PlayPitched("ShadowSpawn", 1f, Main.rand.NextFloat(-0.1f, 0.1f), npc.position);

					if (SpawnHauntedSoulTimer % 5 == 0)
						CameraSystem.shake += 1;

					SpawnHauntedSoulTimer--;
					if (SpawnHauntedSoulTimer <= 0)
					{
						for (int i = 0; i < 20; i++)
						{
							Dust.NewDustPerfect(npc.Top, DustID.Shadowflame, (Vector2.UnitY * Main.rand.NextFloat(-6f, -1)).RotatedByRandom(0.45f), 25, default, Main.rand.NextFloat(1.1f, 1.35f));
						}

						for (int i = 0; i < 2 + Main.rand.Next(2); i++)
						{
							Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Top, (Vector2.UnitY * Main.rand.NextFloat(-8f, -5f)).RotatedByRandom(0.55f), ModContent.ProjectileType<HauntedSoul>(), HauntedSoulDamage, 1f, HauntedSoulOwner, npc.whoAmI);
						}

						CameraSystem.shake += 5;
						HauntedStacks = 0;
						SpawnHauntedSoulTimer = 60;
					}
				}
			}

			if (ShardAmount == MAXVITRICSHARDS)
				HowLongShardHasBeenOnTarget++;

			if (HowLongShardHasBeenOnTarget > 240 && ShardAmount == MAXVITRICSHARDS)
			{
				ShardTimer = 120;
				for (int i = 0; i < Main.maxProjectiles; i++)
				{
					Projectile proj = Main.projectile[i];

					if (proj.active && proj.owner == ShardOwner && proj.type == ModContent.ProjectileType<SoilgunVitricCrystals>() && proj.ModProjectile is SoilgunVitricCrystals crystal && crystal.enemyID == npc.whoAmI)
					{
						crystal.exploding = true;
						proj.timeLeft = 120;
					}
				}

				HowLongShardHasBeenOnTarget = 0;
				npc.AddBuff(BuffID.OnFire, 120);
			}
		}

		public override void UpdateLifeRegen(NPC npc, ref int damage)
		{
			if (HauntedStacks > 0)
			{
				if (npc.lifeRegen > 0)
					npc.lifeRegen = 0;

				npc.lifeRegen -= HauntedStacks;
				if (damage < 1)
					damage = 1;
			}

			if (ShardAmount > 0)
			{
				if (npc.lifeRegen > 0)
					npc.lifeRegen = 0;

				npc.lifeRegen -= 4 + ShardAmount * 2;
				if (damage < 1)
					damage = 1;
			}
		}
	}

	class SoilgunHoldout : ModProjectile
	{
		private bool flip;
		private bool updateVelocity = true;
		private int oldDir;

		private bool flashed;
		private int flashTimer;

		public int ammoID;
		public int projectileID;

		public Projectile ghostProjectile = new();

		public bool CanHold => Owner.channel && !Owner.CCed && !Owner.noItems;
		public bool Shot { get => Projectile.ai[0] != 0f; set => Projectile.ai[0] = value is true ? 1f : 0f; }
		public float ChargeProgress => Charge / MaxCharge;
		public ref float Charge => ref Projectile.ai[1];
		public ref float MaxCharge => ref Projectile.ai[2];
		public Vector2 ArmOffset;
		public Vector2 ArmPosition => Owner.RotatedRelativePoint(Owner.MountedCenter, true) + Vector2.Lerp(Vector2.Zero, new Vector2(-6f, 0f), EaseBuilder.EaseCircularInOut.Ease(ChargeProgress < 0.35f ? ChargeProgress / 0.35f : 1f)).RotatedBy(Projectile.rotation) + new Vector2(18f, -4f * Owner.direction).RotatedBy(Projectile.velocity.ToRotation()) + ArmOffset;
		public Vector2 BarrelOffset;
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

					Projectile.timeLeft = 30;
					updateVelocity = false;
					Shot = true;
				}				
			}

			if (Charge == 0f)
			{
				ghostProjectile.SetDefaults(projectileID);
				MaxCharge = CombinedHooks.TotalUseTime(Owner.HeldItem.useTime, Owner, Owner.HeldItem);
				Projectile.velocity = Owner.DirectionTo(Main.MouseWorld);

				oldDir = Projectile.direction;
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

			float spinUpTime = (int)(MaxCharge * MathHelper.Lerp(2.5f, 1f, Charge < 75f ? Charge / 75f : 1f)); // the time between shots / time between the sprite frame changes is greater when first starting firing

			if (++Projectile.frameCounter % (int)Utils.Clamp(spinUpTime - 3, 1, 50) == 0)
				Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];		
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (Charge <= 2)
				return false;

			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D texGlow = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
			Texture2D texBlur = ModContent.Request<Texture2D>(Texture + "_Blur").Value;
			Texture2D starTex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "StarTexture_Alt").Value;
			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;

			float fade = 0f;
			if (Charge < 8f)
				fade = Charge / 8f;
			else
				fade = 1f;

			Color color = (ghostProjectile.ModProjectile as BaseSoilProjectile).Colors["RingOutsideColor"] with { A = 0 };

			SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			float rotation = Projectile.rotation + (spriteEffects == SpriteEffects.FlipHorizontally ? MathHelper.Pi : 0f);

			Vector2 position = Projectile.Center - Main.screenPosition;

			if (Shot)
			{
				float progress = 1f - Projectile.timeLeft / 30f;

				if (Projectile.timeLeft < 8f)
					fade = EaseBuilder.EaseCircularIn.Ease(Projectile.timeLeft / 8f);

				// recoil animation is exaggerated when fully charged
				float recoilDist = MathHelper.Lerp(-8f, -15f, ChargeProgress);
				float recoilRot = MathHelper.Lerp(-0.25f, -0.45f, ChargeProgress);

				if (progress < 0.05f)
				{
					float lerper = progress / 0.05f;

					position += Projectile.rotation.ToRotationVector2() * MathHelper.Lerp(0f, recoilDist, EaseBuilder.EaseCircularOut.Ease(lerper));

					rotation += MathHelper.Lerp(0f, recoilRot * Projectile.direction, EaseBuilder.EaseCircularOut.Ease(lerper));
				}
				else
				{
					float lerper = (progress - 0.05f) / 0.95f;
					position += Projectile.rotation.ToRotationVector2() * MathHelper.Lerp(recoilDist, 0f, EaseBuilder.EaseBackOut.Ease(lerper));

					rotation += MathHelper.Lerp(recoilRot * Projectile.direction, 0f, EaseBuilder.EaseBackOut.Ease(lerper));
				}
			}

			float shake = MathHelper.Lerp(0f, 0.5f, ChargeProgress);

			position += Main.rand.NextVector2CircularEdge(shake, shake);

			Main.spriteBatch.Draw(texGlow, position, null, color * fade * ChargeProgress, rotation, texGlow.Size() / 2f, Projectile.scale, spriteEffects, 0f);

			Main.spriteBatch.Draw(tex, position, null, lightColor * fade, rotation, tex.Size() / 2f, Projectile.scale, spriteEffects, 0f);
			
			Main.spriteBatch.Draw(texBlur, position, null, new Color(255, 255, 255, 0) * fade * ChargeProgress, rotation, texBlur.Size() / 2f, Projectile.scale, spriteEffects, 0f);

			if (flashTimer > 0)
			{
				rotation = 2f * EaseBuilder.EaseCircularInOut.Ease(flashTimer / 25f);

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
				for (int i = 0; i < 4 + Main.rand.Next(3); i++)
				{
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), BarrelPosition,
						shootVelocity.RotatedByRandom(MathHelper.ToRadians(18)) * Main.rand.NextFloat(0.9f, 1.1f), projectileID, damage, knockBack, Owner.whoAmI);
				}
			}

			Color outColor = (ghostProjectile.ModProjectile as BaseSoilProjectile).Colors["RingOutsideColor"];
			Color inColor = (ghostProjectile.ModProjectile as BaseSoilProjectile).Colors["RingInsideColor"];

			for (int i = 0; i < 12; i++)
			{
				Dust.NewDustPerfect(BarrelPosition + Projectile.velocity * 10f, ModContent.DustType<PixelatedGlow>(),
					Projectile.velocity.RotatedByRandom(1f) * Main.rand.NextFloat(0.5f, 3f), 0, outColor with { A = 0 }, 0.25f);
			}

			for (int i = 0; i < 5; i++)
			{
				Dust.NewDustPerfect(BarrelPosition, ModContent.DustType<PixelatedGlow>(),
					Projectile.velocity.RotatedByRandom(1.25f) * Main.rand.NextFloat(0f, 2.5f), 0, inColor with { A = 0 }, 0.35f);
			}

			for (int i = 0; i < 3; i++)
			{
				Dust dust = Dust.NewDustPerfect(BarrelPosition, ModContent.DustType<PixelSmokeColor>(),
					Projectile.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.3f, 2f) + Main.rand.NextVector2Circular(1f, 1f), Main.rand.Next(100, 155), new Color(81, 47, 27), Main.rand.NextFloat(0.08f, 0.12f));

				dust.rotation = Main.rand.NextFloat(6.28f);
				dust.customData = new Color(105, 67, 44);

				dust = Dust.NewDustPerfect(BarrelPosition, ModContent.DustType<PixelSmokeColor>(),
					Projectile.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(2.5f, 7f) + Main.rand.NextVector2Circular(1f, 1f), Main.rand.Next(100, 175), new Color(81, 47, 27), Main.rand.NextFloat(0.05f, 0.08f));

				dust.rotation = Main.rand.NextFloat(6.28f);
				dust.customData = new Color(105, 67, 44);

				dust = Dust.NewDustPerfect(BarrelPosition, ModContent.DustType<PixelSmokeColor>(),
					Projectile.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(5f, 7.5f) + Main.rand.NextVector2Circular(1f, 1f), Main.rand.Next(100, 175), outColor, Main.rand.NextFloat(0.05f, 0.095f));

				dust.rotation = Main.rand.NextFloat(6.28f);
				dust.customData = inColor;			
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
			Owner.itemTime = 2;
			Owner.itemAnimation = 2;

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

		private int ChooseChargeDust()
		{
			int VitricSand = Mod.Find<ModItem>("VitricSandItem").Type;
			if (ammoID == VitricSand)
			{
				return ModContent.DustType<VitricSandDust>();
			}

			return ammoID switch
			{
				ItemID.SandBlock => DustID.Sand,
				ItemID.CrimsandBlock => DustID.CrimsonPlants,
				ItemID.EbonsandBlock => DustID.Ebonwood,
				ItemID.PearlsandBlock => DustID.Pearlsand,
				ItemID.DirtBlock => DustID.Dirt,
				ItemID.SiltBlock => DustID.Silt,
				ItemID.SlushBlock => DustID.Slush,
				ItemID.MudBlock => DustID.Mud,
				_ => 0,
			};
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