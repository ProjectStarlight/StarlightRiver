using StarlightRiver.Core;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Permafrost
{
	public class SquidBossSpawn : ModItem
    {
        public override string Texture => AssetDirectory.PermafrostItem + "SquidBossSpawn";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Suspicious Looking Offering");
            Tooltip.SetDefault("Drop in prismatic waters to summon the one the Squiddites worship");
        }

        public override void SetDefaults()
        {
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 20;
        }
    }
}