using Microsoft.Xna.Framework;
using StarlightRiver.Projectiles.WeaponProjectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Misc
{
    public class ShadowflameDrumGrenadeLauncher : ModItem
    {
        public override void SetDefaults()
        {
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.useAnimation = 38;
            item.useTime = 38;
            item.shootSpeed = 2f;
            item.knockBack = 4f;
            item.width = 60;
            item.height = 72;
            item.damage = 22;
            item.UseSound = SoundID.Item61;
            item.rare = ItemRarityID.LightRed;
            item.value = Item.sellPrice(0, 10, 0, 0);
            item.noMelee = true;
            item.autoReuse = true;
            item.useAmmo = 168;
            item.ranged = true;
            item.shoot = ProjectileType<ShadowflameGrenade>();
        }

        public override bool ConsumeAmmo(Player player)
        {
            return Main.rand.NextFloat() >= .5f;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Shadowflame Drum Grenade Launcher");
            Tooltip.SetDefault("You like click it and like, it shoot quickly the entire drum mag\nThe grenades explode into shadowflame tendrils\nThey also release said tendrils sometimes i guess ye");
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-20, 0);
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Vector2 muzzleOffset = Vector2.Normalize(new Vector2(speedX, speedY)) * 25f;
            if (Collision.CanHit(position, 0, 0, position + muzzleOffset, 0, 0))
            {
                position += muzzleOffset;
            }
            Projectile.NewProjectile(position, new Vector2(speedX, speedY), ProjectileType<ShadowflameGrenade>(), damage, knockBack, player.whoAmI);
            return false;
        }
    }

    internal class MakeGrenadesAmmo : GlobalItem
    {
        public override void SetDefaults(Item item)
        {
            if (item.type == ItemID.Grenade)
            {
                item.ammo = ItemID.Grenade;
                item.maxStack = 999;
            }
        }
    }
}