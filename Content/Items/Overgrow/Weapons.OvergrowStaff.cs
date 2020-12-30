
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.Overgrow
{
    public class OvergrowStaff : ModItem
    {
        public override string Texture => AssetDirectory.OvergrowItem + "OvergrowStaff";

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
            //item.shoot = ProjectileType<EntangleThorn>(); TODO: Reimplement better
            item.shootSpeed = 5;
        }
    }
}