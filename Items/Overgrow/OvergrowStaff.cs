using StarlightRiver.Projectiles.WeaponProjectiles;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Overgrow
{
    public class OvergrowStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Entanglement Rod");
        }

        public override void SetDefaults()
        {
            item.damage = 20;
            item.magic = true;
            item.width = 40;
            item.height = 20;
            item.useTime = 30;
            item.useAnimation = 30;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.noMelee = true;
            item.knockBack = 4;
            item.rare = ItemRarityID.Green;
            item.shoot = ProjectileType<EntangleThorn>();
            item.shootSpeed = 5;
        }
    }
}