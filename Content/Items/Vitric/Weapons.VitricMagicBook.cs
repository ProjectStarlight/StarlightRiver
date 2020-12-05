using Microsoft.Xna.Framework;
using StarlightRiver.Projectiles.WeaponProjectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.Vitric
{
    public class VitricMagicBook : ModItem
    {
        public override string Texture => Directory.VitricItemDir + Name;

        public override void SetDefaults()
        {
            item.width = 38;
            item.height = 34;
            item.useStyle = ItemUseStyleID.HoldingOut;
            Item.staff[item.type] = true;
            item.useAnimation = 10;
            item.useTime = 10;
            item.reuseDelay = 20;
            item.autoReuse = true;
            item.shootSpeed = 12f;
            item.knockBack = 0f;
            item.damage = 12;
            item.shoot = ProjectileType<VitricBookProjectile>();
            item.rare = ItemRarityID.Green;
            item.noMelee = true;
            item.magic = true;
            item.mana = 15;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            for (int i = 0; i < 181; i += 180)
            {
                Vector2 muzzleOffset = MathHelper.ToRadians(i).ToRotationVector2() * 35f;
                if (Collision.CanHit(position, 0, 0, position + muzzleOffset, 0, 0))
                    position += muzzleOffset;
                Main.PlaySound(SoundID.Item, (int)position.X, (int)position.Y, 8, 1);
                muzzleOffset.Normalize();
                Projectile.NewProjectile(position.X, position.Y, muzzleOffset.X * 16f, 0, item.shoot, damage, knockBack, player.whoAmI);
            }
            return false;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Book");
            Tooltip.SetDefault("Summons waves that leave behind broken glass traps on nearby ground, with spacing between them\nThese rapidly spike enemies who step on them");
        }
    }
}