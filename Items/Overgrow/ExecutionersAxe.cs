using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.Projectiles.WeaponProjectiles;
using Microsoft.Xna.Framework;

namespace StarlightRiver.Items.Overgrow
{
    public class ExecutionersAxe : ModItem
    {

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Executioner's Axe");
        }

        public override void SetDefaults()
        {
            item.channel = true;
            item.damage = 30;
            item.width = 60;
            item.height = 60;
            item.useTime = 320;
            item.useAnimation = 320;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.melee = true;
            item.noMelee = true;
            item.knockBack = 12;
            item.useTurn = false;
            item.value = Terraria.Item.sellPrice(0, 1, 42, 0);
            item.rare = ItemRarityID.Orange;
            item.autoReuse = false;
            item.shoot = ModContent.ProjectileType<AxeHead>();
            item.shootSpeed = 6f;
            item.noUseGraphic = true;
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            return base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
        }
        public override bool CanUseItem(Player player)
        {
            return base.CanUseItem(player);
        }
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-10, 0);
        }
    }
}