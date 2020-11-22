using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Permafrost.Weapons
{
    public class TentacleSummon : ModItem
    {
		public override void SetDefaults()
		{
			item.damage = 10;
			item.knockBack = 3f;
			item.mana = 10;
			item.width = 32;
			item.height = 32;
			item.useTime = 36;
			item.useAnimation = 36;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.value = Item.buyPrice(0, 5, 0, 0);
			item.rare = ItemRarityID.Green;
			item.UseSound = SoundID.Item44;

			item.noMelee = true;
			item.summon = true;
			item.buffType = mod.BuffType("TentacleSummonBuff");
			item.shoot = mod.ProjectileType("TentacleSummonHead");
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			player.AddBuff(item.buffType, 2);
			position = player.Center - new Vector2(player.direction * 64,16);

			int index = 0;

			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile currentProjectile = Main.projectile[i];
				if (currentProjectile.active
				&& currentProjectile.owner == player.whoAmI
				&& currentProjectile.type == type)
				{
					if (i == currentProjectile.whoAmI)
						index += 1;
				}
			}

			Projectile.NewProjectile(position, Vector2.Zero, type, damage, knockBack, player.whoAmI, index * 0.7f, 0);

			return false;
		}

		public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Tentacle staff");
            Tooltip.SetDefault("Dock Ock");
        }
    }
}