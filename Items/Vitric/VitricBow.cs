using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Items.Vitric
{
    public class VitricBow : ModItem
    {
        public override void SetDefaults()
        {
            item.width = 38;
            item.height = 34;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.useAnimation = 28;
            item.useTime = 28;
            item.shootSpeed = 8f;
            item.shoot = ProjectileID.WoodenArrowFriendly;
            item.knockBack = 2f;
            item.damage = 12;
            item.useAmmo = AmmoID.Arrow;
            item.rare = ItemRarityID.Green;
            item.UseSound = SoundID.Item5;
            item.noMelee = true;
            item.ranged = true;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Vector2 aim = Vector2.Normalize(Main.MouseWorld - player.Center);

            int proj = Projectile.NewProjectile(player.Center, (aim * 8.5f).RotatedBy(0.1f), type, damage, knockBack, player.whoAmI);
            Main.projectile[proj].scale = 0.5f;
            Main.projectile[proj].damage /= 2;
            Main.projectile[proj].noDropItem = true;
            int proj2 = Projectile.NewProjectile(player.Center, (aim * 8.5f).RotatedBy(-0.1f), type, damage, knockBack, player.whoAmI);
            Main.projectile[proj2].scale = 0.5f;
            Main.projectile[proj2].damage /= 2;
            Main.projectile[proj2].noDropItem = true;
            return true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Bow");
            Tooltip.SetDefault("Shoots two arrows at once");
        }
    }
}