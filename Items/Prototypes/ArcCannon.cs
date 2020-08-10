using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Prototypes
{
    internal class ArcCannon : PrototypeWeapon
    {
        public ArcCannon() : base(500, BreakType.MaxUses)
        {
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Electro-Beam");
            Tooltip.SetDefault("Fires rapid-fire chain lightning to melt your foes armor");
        }

        public override void SetDefaults()
        {
            item.damage = 50;
            item.useStyle = ItemUseStyleID.HoldingUp;
            item.useTime = 2;
            item.useAnimation = 8;
            item.autoReuse = true;
        }

        public override bool SafeUseItem(Player player)
        {
            int index = Projectile.NewProjectile(Main.MouseWorld, Vector2.Zero, ProjectileType<Projectiles.WeaponProjectiles.LightningNode>(), 120, 0, player.whoAmI, 1, 500);
            NPC npc = Main.npc.FirstOrDefault(n => n.Hitbox.Contains(Main.MouseWorld.ToPoint()));
            Helper.DrawElectricity(player.Center, npc == null ? Main.projectile[index].Center : npc.Center, DustType<Dusts.Electric>());
            return true;
        }
    }
}