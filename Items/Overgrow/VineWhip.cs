using Microsoft.Xna.Framework;
using StarlightRiver.Projectiles.WeaponProjectiles;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Overgrow
{
    internal class VineWhip : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vine Whip");
        }

        public override void SetDefaults()
        {
            item.width = 34;
            item.height = 40;
            item.damage = 20;
            item.melee = true;
            item.noUseGraphic = true;
            item.noMelee = true;
            item.rare = ItemRarityID.LightRed;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.useAnimation = 30;
            item.useTime = 30;
            item.channel = true;
            item.shoot = ProjectileID.WoodenArrowFriendly;
        }

        public override bool CanUseItem(Player player) => !Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ProjectileType<WhipSegment1>());

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            for (int k = 0; k <= 10; k++)
            {
                Vector2 vel = Vector2.Normalize(Main.MouseWorld - player.Center);
                Projectile.NewProjectile(player.Center.X, player.Center.Y, vel.X, vel.Y, ProjectileType<WhipSegment1>(), damage, 0, player.whoAmI, k, vel.ToRotation());
            }
            return false;
        }
    }
}