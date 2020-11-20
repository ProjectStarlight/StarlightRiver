using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.StarJuice
{
    internal class BountyKnife : StarjuiceStoringItem
    {
        public BountyKnife() : base(2500)
        {
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hunters Dagger");
            Tooltip.SetDefault("Infuse a beast with starlight\nInfused enemies become powerful and gain abilities\nSlain enemies drop crystals");
        }

        public override void SetDefaults()
        {
            item.width = 16;
            item.height = 16;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.useAnimation = 10;
            item.useTime = 10;
            item.rare = ItemRarityID.Orange;
            item.shoot = ProjectileType<Projectiles.WeaponProjectiles.BountyKnife>();
            item.shootSpeed = 2;
            item.damage = 1;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (charge == maxCharge)
            {
                charge = 0;
                return true;
            }
            return false;
        }
    }
}