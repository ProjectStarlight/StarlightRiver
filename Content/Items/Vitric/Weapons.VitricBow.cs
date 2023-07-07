using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Vitric
{
	public class VitricBow : ModItem
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ancient Vitric Bow");
			Tooltip.SetDefault("Fires additional smaller arrows");
		}

		public override void SetDefaults()
		{
			Item.width = 38;
			Item.height = 34;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 28;
			Item.useTime = 28;
			Item.shootSpeed = 8f;
			Item.shoot = ProjectileID.WoodenArrowFriendly;
			Item.knockBack = 2f;
			Item.damage = 25;
			Item.useAmmo = AmmoID.Arrow;
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item5;
			Item.noMelee = true;
			Item.DamageType = DamageClass.Ranged;

			Item.value = Item.sellPrice(gold: 1);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			var aim = Vector2.Normalize(Main.MouseWorld - player.Center);

			int proj = Projectile.NewProjectile(source, player.Center, (aim * 8.5f).RotatedBy(0.1f), type, damage, knockback, player.whoAmI);
			Main.projectile[proj].scale = 0.5f;
			Main.projectile[proj].damage /= 2;
			Main.projectile[proj].noDropItem = true;
			NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj);

			int proj2 = Projectile.NewProjectile(source, player.Center, (aim * 8.5f).RotatedBy(-0.1f), type, damage, knockback, player.whoAmI);
			Main.projectile[proj2].scale = 0.5f;
			Main.projectile[proj2].damage /= 2;
			Main.projectile[proj2].noDropItem = true;
			NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj2);
			return true;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<SandstoneChunk>(), 8);
			recipe.AddIngredient(ModContent.ItemType<VitricOre>(), 12);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}