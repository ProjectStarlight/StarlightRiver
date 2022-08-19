
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

using System;
using System.Linq;
using System.Collections.Generic;

using Terraria;
using Terraria.Enums;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace StarlightRiver.Content.Items.Breacher
{
	class BreachCannon : ModItem
	{
		public override string Texture => AssetDirectory.BreacherItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Breach Cannon");
			Tooltip.SetDefault("Summons a sentry that shoots a laser towards the cursor \nThese lasers can combine");
		}

		public override void SetDefaults()
		{
			Item.CloneDefaults(ItemID.QueenSpiderStaff);
			Item.damage = 12;
			Item.mana = 12;
			Item.width = 40;
			Item.height = 40;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.knockBack = 2.5f;
			Item.UseSound = SoundID.Item25;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.shoot = ModContent.ProjectileType<BreachCannonSentry>();
			Item.shootSpeed = 0f;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			position = Main.MouseWorld;
			Tile startTile = Main.tile[(int)(position.X / 16), (int)(position.Y / 16)];

			if (startTile.HasTile && Main.tileSolid[startTile.TileType])
				return false;

			//0 = right
			//1 = bottom
			//2 = left
			//3 = top

			int minDistance = 99;
			int minDirection = 0;

			Vector2 originTile = Vector2.Zero;

			for (int k = 0; k < 4; k++)
			{
				Vector2 testPosition = position / 16;
				testPosition.X = (int)testPosition.X;
				testPosition.Y = (int)testPosition.Y;
				Vector2 direction = Vector2.UnitX.RotatedBy(k * 1.57f);
				int testDistance = 0;
				while (testDistance < 40)
				{
					testPosition += direction;
					testDistance++;
					if (testDistance > minDistance)
						break;

					int i = (int)testPosition.X;
					int j = (int)testPosition.Y;
					Tile testTile = Main.tile[i, j];
					if (testTile.HasTile && Main.tileSolid[testTile.TileType])
					{
						minDistance = testDistance;
						minDirection = k;
						originTile = testPosition;
						break;
					}
				}
			}

			if (minDistance == 99)
				return false;

			originTile = MoveRightAndDown(originTile, minDirection);
			if (originTile == Vector2.Zero)
				return false;

			Projectile proj = Projectile.NewProjectileDirect(source, (originTile - Vector2.UnitX.RotatedBy(minDirection * 1.57f)) * 16, velocity, type, damage, knockback, player.whoAmI, minDirection);
			var mp = proj.ModProjectile as BreachCannonSentry;

			//Main.NewText("Spawned at (" + ((int)originTile.X).ToString() + "," + ((int)originTile.Y).ToString() + ")");
			mp.tileOrigin = originTile;
			proj.originalDamage = Item.damage;
			proj.rotation = (minDirection * 1.57f) + 3.14f;
			player.UpdateMaxTurrets();

			return false;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<Items.SpaceEvent.Astroscrap>(), 12);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}

		private Vector2 MoveRightAndDown(Vector2 originTile, int minDirection)
		{
			bool Overlapping(Vector2 innerRet)
			{
				return Main.projectile.Where(n => n.active && n.type == ModContent.ProjectileType<BreachCannonSentry>() && ((n.ModProjectile as BreachCannonSentry).tileOrigin - innerRet).Length() < 2).FirstOrDefault() != default;
			}

			bool tileOpen(Vector2 tileCheck)
			{
				return !Main.tile[(int)tileCheck.X, (int)tileCheck.Y].HasTile || !Main.tileSolid[Main.tile[(int)tileCheck.X, (int)tileCheck.Y].TileType];
			}

			Vector2 ret = originTile;
			Vector2 ret2 = originTile;
			Vector2 moveDirection = Vector2.UnitX.RotatedBy(minDirection * 1.57f);

			int moveDownTries = 0;
			while (Overlapping(ret) || tileOpen(ret))
			{
				ret += moveDirection.RotatedBy(1.57f);
				if (!Overlapping(ret))
				{
					ret2 = ret;
					while (tileOpen(ret2))
					{
						moveDownTries++;
						if (moveDownTries >= 99)
							return Vector2.Zero;
						ret2 += moveDirection;
					}
					if (Overlapping(ret2))
						ret2 = ret;
					else
						ret = ret2;
				}
			}

			return ret;
		}
	}
}