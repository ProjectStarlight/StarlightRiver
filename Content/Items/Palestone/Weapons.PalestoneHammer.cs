using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace StarlightRiver.Items.Palestone
{
    public class PalestoneHammer : ModItem
    {

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Palecrusher");
        }

        public override void SetDefaults()
        {
            item.channel = true;
            item.damage = 12;
            item.width = 24;
            item.height = 24;
            item.useTime = 320;
            item.useAnimation = 320;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.melee = true;
            item.noMelee = true;
            item.knockBack = 8;
            item.useTurn = false;
            item.value = Terraria.Item.sellPrice(0, 1, 42, 0);
            item.rare = 0;
            item.autoReuse = false;
            item.shoot = mod.ProjectileType("PalecrusherProj");
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