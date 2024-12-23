using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Content.Items.Vitric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Crimson
{
	internal class MirageBow : ModItem
	{
		int cloneCooldown = 0;

		public override string Texture => AssetDirectory.Debug;

		public override void Load()
		{
			StarlightProjectile.OnHitNPCEvent += SpawnOnCrit;
		}

		public override void SetDefaults()
		{
			Item.damage = 15;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 16;
			Item.height = 64;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 1;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(0, 5, 0, 0);
			Item.shoot = 10;
			Item.shootSpeed = 5f;
			Item.autoReuse = true;
			Item.useAmmo = AmmoID.Arrow;
			Item.useTurn = true;
		}

		public override void UpdateInventory(Player player)
		{
			if (cloneCooldown > 0)
				cloneCooldown--;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			var clone = Main.projectile.FirstOrDefault(n => n.active && n.type == ModContent.ProjectileType<MirageBowClone>() && n.owner == player.whoAmI);

			if (clone is null)
				return true;

			Projectile.NewProjectile(Item.GetSource_FromThis(), clone.Center, clone.Center.DirectionTo(Main.MouseWorld) * Item.shootSpeed * 2, type, damage / 2, knockback, player.whoAmI);

			return true;
		}

		private void SpawnOnCrit(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
		{
			var owner = Main.player[projectile.owner];

			if (owner.active && owner.HeldItem.type == ModContent.ItemType<MirageBow>() && hit.Crit)
			{
				var moditem = owner.HeldItem.ModItem as MirageBow;

				if (moditem is null)
					return;

				if (moditem.cloneCooldown <= 0)
				{
					Projectile.NewProjectile(owner.HeldItem.GetSource_FromThis(), owner.Center, Vector2.Zero, ModContent.ProjectileType<MirageBowClone>(), 1, 0, owner.whoAmI);
					moditem.cloneCooldown = 60 * 8;
				}
			}
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<DendriteBar>(), 10);
			recipe.AddIngredient(ModContent.ItemType<ImaginaryTissue>(), 5);
			recipe.AddIngredient(ItemID.TendonBow);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	internal class MirageBowClone : ModProjectile
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 60 * 5;
		}

		public override void AI()
		{
			var owner = Main.player[Projectile.owner];
			if (owner.HeldItem.type != ModContent.ItemType<MirageBow>())
			{
				Projectile.timeLeft = 30;
			}

			if (owner == Main.LocalPlayer)
			{
				var relative = owner.Center - Main.MouseWorld;
				var target = Main.MouseWorld + relative.RotatedBy(3.14f);

				Projectile.Center += (target - Projectile.Center) * 0.1f;
			}
		}

		public override bool? CanHitNPC(NPC target)
		{
			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (!PlayerTarget.canUseTarget)
				return false;

			var owner = Main.player[Projectile.owner];
			var source = PlayerTarget.getPlayerTargetSourceRectangle(Projectile.owner);

			var alpha = Projectile.timeLeft < 30 ? Projectile.timeLeft / 30f * 0.5f : 0.5f;

			Main.spriteBatch.Draw(PlayerTarget.Target, Projectile.Center - Main.screenPosition, source, Color.White * alpha, owner.fullRotation, source.Size() / 2f, 1, SpriteEffects.FlipHorizontally, 0);
			

			return false;
		}
	}
}
