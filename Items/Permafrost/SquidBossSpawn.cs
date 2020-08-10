using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Items.Permafrost
{
    public class SquidBossSpawn : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("[PH] Auroracle Bait");
            Tooltip.SetDefault("Drop in prismatic waters to summon a fearsome creature");
        }

        public override void SetDefaults()
        {
            item.rare = ItemRarityID.Green;
            item.maxStack = 20;
        }
    }
}