﻿using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	class DualCross : ModItem
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public int shotsTilSwitch; // Alternates every fire, whether it's even or odd indicates whether or not it skips an ammo.

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Double Cross");
			Tooltip.SetDefault("Shoots arrows from your highest two ammo slots at once");
		}

		public override void SetDefaults()
		{
			Item.damage = 4;
			Item.noMelee = true;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 28;
			Item.height = 42;
			Item.useTime = 1;
			Item.useAnimation = 40;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.shoot = ProjectileID.Shuriken;
			Item.useAmmo = AmmoID.Arrow;
			Item.knockBack = 1;
			Item.value = Terraria.Item.sellPrice(0, 0, 10, 0);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item5;
			Item.autoReuse = true;
			Item.shootSpeed = 7.8f;
			Item.crit = 2;
			Item.noUseGraphic = true;
		}

		public override bool CanConsumeAmmo(Item ammo, Player player)
		{
			return false;
		}

		public override bool CanUseItem(Player Player)
		{
			bool canShoot = false;
			bool foundAmmo = false;

			for (int index = 54; index < 58; ++index)
			{
				if (Player.inventory[index].ammo == Item.useAmmo && Player.inventory[index].stack > 0)
				{
					canShoot = true;
					foundAmmo = true;
					break;
				}
			}

			if (!foundAmmo)
			{
				for (int index = 0; index < 54; ++index)
				{
					if (Player.inventory[index].ammo == Item.useAmmo && Player.inventory[index].stack > 0)
					{
						canShoot = true;
						break;
					}
				}
			}

			if (canShoot)
				return true;

			return false;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (shotsTilSwitch % 2 == 1)
			{
				Vector2 offset = Vector2.Normalize(velocity).RotatedBy(-1.57f * player.direction) * 10;
				position += offset;
				player.itemTime = 39;
				Projectile.NewProjectile(source, position, Vector2.Zero, ModContent.ProjectileType<DualCrossHeld>(), 0, 0, player.whoAmI);
			}
			else
			{
				position -= velocity;
			}

			return base.Shoot(player, source, position, velocity, type, damage, knockback);
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.SilverBar, 10);
			recipe.AddRecipeGroup(RecipeGroupID.Wood, 5);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.TungstenBar, 10);
			recipe.AddRecipeGroup(RecipeGroupID.Wood, 5);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	public class DualCrossHeld : ModProjectile
	{
		private bool initialized = false;

		public override string Texture => AssetDirectory.MiscItem + Name;

		private Vector2 Direction => Projectile.rotation.ToRotationVector2();

		Player Owner => Main.player[Projectile.owner];

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Double Cross");
		}

		public override void SetDefaults()
		{
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.aiStyle = -1;
			Projectile.friendly = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 999999;
			Projectile.ignoreWater = true;
			Projectile.alpha = 255;
			Main.projFrames[Projectile.type] = 4;
		}

		public override void AI()
		{
			Owner.heldProj = Projectile.whoAmI;

			if (Owner.itemTime <= 1)
				Projectile.active = false;

			Projectile.Center = Owner.Center;

			if (!initialized)
			{
				initialized = true;
				Projectile.rotation = Projectile.DirectionTo(Main.MouseWorld).ToRotation();
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			Vector2 position = Owner.Center + Direction * 15 - Main.screenPosition;

			if (Owner.direction == 1)
			{
				SpriteEffects effects1 = SpriteEffects.None;
				Main.spriteBatch.Draw(texture, position, null, lightColor, Direction.ToRotation(), texture.Size() / 2, Projectile.scale, effects1, 0.0f);
			}
			else
			{
				SpriteEffects effects1 = SpriteEffects.FlipHorizontally;
				Main.spriteBatch.Draw(texture, position, null, lightColor * .91f, Direction.ToRotation() - 3.14f, texture.Size() / 2, Projectile.scale, effects1, 0.0f);
			}

			return false;
		}
	}

	public class DualCrossGlobalItem : GlobalItem
	{
		private static Item GetAmmo(int skip, Item weapon, Player Player, ref bool canShoot)
		{
			var obj = new Item();
			bool ammoInAmmoSlot = false;
			bool ret = false; //Whether it returns the item. If it's false, it returns null

			int ammosFound = 0;

			for (int k = 54; k < 58; ++k)
			{
				if (Player.inventory[k].ammo == weapon.useAmmo && Player.inventory[k].stack > 0)
				{
					obj = Player.inventory[k];
					ret = true;
					ammosFound++;

					if (ammosFound > skip)
					{
						ammoInAmmoSlot = true;
						break;
					}
				}
			}

			if (!ammoInAmmoSlot)
			{
				for (int index = 0; index < 54; ++index)
				{
					if (Player.inventory[index].ammo == weapon.useAmmo && Player.inventory[index].stack > 0)
					{
						obj = Player.inventory[index];
						ret = true;
						ammosFound++;

						if (ammosFound > skip)
							break;
					}
				}
			}

			if (ret)
			{
				canShoot = true;
				return obj;
			}
			else
			{
				canShoot = false;
				return null;
			}
		}

		private static float GetSpeed(Player Player, Item obj, float speed)
		{
			if (Player.magicQuiver)
			{
				speed *= 1.1f;
			}

			speed += obj.shootSpeed;

			if (Player.archery)
			{
				if (speed < 20)
				{
					speed *= 1.2f;
					if (speed > 20)
						speed = 20f;
				}
			}

			return speed;
		}

		//TODO: Remove? This seems entirely irrelevant to this. Or ideally move these utilities out to some global 'get ammo' helper
		private static int GetProjType(Item obj, int type)
		{
			if (obj.shoot > ProjectileID.None)
				type = obj.shoot;

			if (type == 42)
			{
				if (obj.type == ItemID.EbonsandBlock)
					type = 65;
				else if (obj.type == ItemID.PearlsandBlock)
					type = 68;
				else if (obj.type == ItemID.CrimsandBlock)
					type = 354;
			}

			return type;
		}

		public override void PickAmmo(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback)
		{
			if (weapon.type != ModContent.ItemType<DualCross>())
			{
				base.PickAmmo(weapon, ammo, player, ref type, ref speed, ref damage, ref knockback);
				return;
			}

			int type2 = 0;

			var mp = weapon.ModItem as DualCross;
			int ticker = mp.shotsTilSwitch; //Ticker represents whether it skips 1 when looking for ammo

			bool canShoot = false;

			Item obj = GetAmmo(ticker % 2, weapon, player, ref canShoot);
			Item obj2 = GetAmmo((ticker + 1) % 2, weapon, player, ref canShoot);

			damage.Flat += obj.damage;

			if (!canShoot)
				return;

			mp.shotsTilSwitch++;

			type = GetProjType(obj, type);
			type2 = GetProjType(obj2, type2);

			var proj = new Projectile();
			proj.SetDefaults(type);

			var proj2 = new Projectile();
			proj2.SetDefaults(type2);

			float speed1 = GetSpeed(player, obj, speed) * (proj.extraUpdates + 1);
			float speed2 = GetSpeed(player, obj2, speed) * (proj2.extraUpdates + 1);
			speed = (speed1 + speed2) / 2 / (proj.extraUpdates + 1);

			bool saveAmmo = false;

			if (player.magicQuiver && weapon.useAmmo == AmmoID.Arrow && Main.rand.NextBool(5)) //Copied from vanilla, as clean as I could get it
				saveAmmo = true;

			if (player.ammoBox && Main.rand.NextBool(5))
				saveAmmo = true;

			if (player.ammoPotion && Main.rand.NextBool(5))
				saveAmmo = true;

			if (player.ammoCost80 && Main.rand.NextBool(5))
				saveAmmo = true;

			if (player.ammoCost75 && Main.rand.NextBool(5))
				saveAmmo = true;

			if (type == 85 && player.itemAnimation < player.itemAnimationMax - 6)
				saveAmmo = true;

			if ((type == 145 || type == 146 || type == 147 || type == 148 || type == 149) && player.itemAnimation < player.itemAnimationMax - 5)
				saveAmmo = true;

			if (saveAmmo || !obj.consumable)
				return;

			obj.stack--;

			if (obj.stack > 0)
				return;

			obj.active = false;
			obj.TurnToAir();
		}
	}
}