
using StarlightRiver.Core;
using Terraria.ID;
using Terraria.ModLoader;

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
            Item.damage = 20;
            Item.magic = true;
            Item.width = 40;
            Item.height = 20;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.HoldingOut;
            Item.noMelee = true;
            Item.knockBack = 4;
            Item.rare = ItemRarityID.Green;
            //Item.shoot = ProjectileType<EntangleThorn>(); TODO: Reimplement better
            Item.shootSpeed = 5;
        }
    }
}