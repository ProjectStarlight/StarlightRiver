using Microsoft.Xna.Framework;
using StarlightRiver.Projectiles.WeaponProjectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Vitric
{
    public class VitricStaff : ModItem
    {
        public override void SetDefaults()
        {
            item.width = 38;
            item.height = 34;
            item.useStyle = ItemUseStyleID.HoldingOut;
            Item.staff[item.type] = true;
            item.useAnimation = 9;
            item.useTime = 3;
            item.reuseDelay = 27;
            item.autoReuse = true;
            item.shootSpeed = 12f;
            item.knockBack = 2f;
            item.damage = 12;
            item.shoot = ProjectileType<VitricStaffProjectile>();
            item.rare = ItemRarityID.Green;
            item.noMelee = true;
            item.magic = true;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Vector2 muzzleOffset = Vector2.Normalize(new Vector2(speedX, speedY)) * 35f;
            if (Collision.CanHit(position, 0, 0, position + muzzleOffset, 0, 0))
            {
                position += muzzleOffset;
            }
            Main.PlaySound(SoundID.Item, -1, -1, 8, 1);
            Vector2 perturbedSpeed = new Vector2(speedX, speedY).RotatedByRandom(MathHelper.ToRadians(20));
            Projectile.NewProjectile(position.X, position.Y, perturbedSpeed.X, perturbedSpeed.Y, item.shoot, damage, knockBack, player.whoAmI);
            return false;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Staff");
            Tooltip.SetDefault("It like shoots crystals and like the cystals stick to enemies\nThe more crsytals stuck to enemy you hit the more damag!!!!");
        }
    }
}