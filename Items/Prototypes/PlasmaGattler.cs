using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Prototypes
{
    internal class PlasmaGattler : PrototypeWeapon
    {
        public PlasmaGattler() : base(3000, BreakType.Time)
        {
        }

        private int Heat { get; set; }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Plasmic Converter");
            Tooltip.SetDefault("'convert' your foes into dust\nless accurate over time");
        }

        public override void SetDefaults()
        {
            item.damage = 20;
            item.useStyle = ItemUseStyleID.HoldingUp;
            item.useTime = 5;
            item.useAnimation = 5;
            item.UseSound = SoundID.Item75;
            item.autoReuse = true;
        }

        public override bool SafeUseItem(Player player)
        {
            player.channel = true;
            Vector2 dir = Vector2.Normalize(player.Center - Main.MouseWorld);
            Projectile.NewProjectile(player.Center + dir * -10, dir.RotatedByRandom(Heat / 250f) * -4, ProjectileType<Projectiles.WeaponProjectiles.PlasmaGattlerPulse>(),
                item.damage, item.knockBack, player.whoAmI, Heat);
            //Main.NewText(Heat, new Color(Heat, 100, 200 - Heat));
            if (Heat <= 200) Heat += 10;
            return true;
        }

        public override void SafeUpdateInventory(Player player)
        {
            if (!player.channel && Heat > 0) Heat--;
        }
    }
}