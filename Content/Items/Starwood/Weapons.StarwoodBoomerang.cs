using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using StarlightRiver.Core;
using StarlightRiver.Projectiles.WeaponProjectiles;

namespace StarlightRiver.Content.Items.Starwood.Weapons
{
    public class StarwoodBoomerang : StarwoodItem
    {
        public StarwoodBoomerang() : base(ModContent.GetTexture("StarlightRiver/Assets/Items/Starwood/StarwoodBoomerang_Alt")) { }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Starwood Boomerang");
            Tooltip.SetDefault("Tooltip");
        }

        public override void SetDefaults()
        {
            item.damage = 20;
            item.melee = true;
            item.width = 18;
            item.height = 34;
            item.useTime = 10;
            item.noUseGraphic = true;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.shootSpeed = 10f;
            item.knockBack = 4f;
            item.UseSound = SoundID.Item19;
            item.shoot = ModContent.ProjectileType<StarwoodBoomerangProjectile>();
            item.useAnimation = 10;
            item.noMelee = true;
        }

        public override bool CanUseItem(Player player)
        {
            for (int k = 0; k <= Main.maxProjectiles; k++)
            {
                if (Main.projectile[k].active && Main.projectile[k].owner == player.whoAmI && Main.projectile[k].type == ModContent.ProjectileType<StarwoodBoomerangProjectile>())
                {
                    return false;
                }
            }
            return base.CanUseItem(player);
        }
    }
}