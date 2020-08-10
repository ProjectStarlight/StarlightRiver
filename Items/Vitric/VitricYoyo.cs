using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Items.Vitric
{
	public class VitricYoyo : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Vitric Yoyo");
			Tooltip.SetDefault("Splits");
		}


		public override void SetDefaults()
		{
			item.width = 30;
			item.height = 26;
			item.value = Terraria.Item.sellPrice(0, 10, 0, 0);
			item.rare = ItemRarityID.Purple;
			item.crit += 4;
			item.damage = 115;
			item.knockBack = 4f;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.useTime = 25;
			item.useAnimation = 25;
			item.melee = true;
			item.channel = true;
			item.noMelee = true;
			item.noUseGraphic = true;
			item.shootSpeed = 12f;
			item.shoot = ModContent.ProjectileType<Projectiles.WeaponProjectiles.VitricYoyoProj>();
			item.UseSound = SoundID.Item1;
		}
	}
}
