using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Starwood
{
	public class StarwoodStaff : StarwoodItem
	{
		public override string Texture => AssetDirectory.StarwoodItem + Name;

		public StarwoodStaff() : base(ModContent.Request<Texture2D>(AssetDirectory.StarwoodItem + "StarwoodStaff_Alt").Value) { }

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Starwood Staff");
			Tooltip.SetDefault("Creates a burst of small stars\nStriking an enemy with every star causes a larger star to drop on them");
			Item.staff[Item.type] = true;
		}

		public override void SetDefaults()
		{
			Item.damage = 8;
			Item.DamageType = DamageClass.Magic;
			Item.mana = 10;
			Item.width = 18;
			Item.height = 34;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.UseSound = SoundID.Item43;
			Item.knockBack = 0f;
			Item.shoot = ModContent.ProjectileType<StarwoodStaffProjectile>();
			Item.shootSpeed = 15f;
			Item.noMelee = true;
			Item.autoReuse = true;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			StarlightPlayer mp = Main.player[player.whoAmI].GetModPlayer<StarlightPlayer>();
			int amount = mp.empowered ? 4 : 3;
			int projDamage = (int)(damage * (mp.empowered ? 1.3f : 1f));//TODO: actually change the Item itself's damage
			Vector2 newVelocity = velocity * (mp.empowered ? 1.05f : 1f);

			Vector2 staffEndPosition = player.Center + Vector2.Normalize(Main.MouseWorld - position) * 45;//this makes it spawn a distance from the Player, useful for other stuff

			for (int k = 0; k < amount; k++)
			{
				Vector2 projVelocity = newVelocity.RotatedBy(Main.rand.NextFloat(-0.05f, 0.05f) * (k * 0.10f + 1)) * Main.rand.NextFloat(0.9f, 1.1f) * (k * 0.15f + 1);
				Projectile.NewProjectile(source, staffEndPosition, projVelocity, type, projDamage, knockback, player.whoAmI, Main.rand.NextFloat(-0.025f, 0.025f), Main.rand.Next(50));
			}

			for (int k = 0; k < 10; k++)
			{
				Vector2 pos = staffEndPosition + new Vector2(Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-5f, 15f));
				Vector2 dustVelocity = (newVelocity * Main.rand.NextFloat(0.01f, 0.1f)).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) + player.velocity * 0.5f;
				Dust.NewDustPerfect(pos, mp.empowered ? ModContent.DustType<Dusts.BlueStamina>() : ModContent.DustType<Dusts.Stamina>(), dustVelocity, 0, default, 1.5f);
			}

			return false;
		}
	}
}