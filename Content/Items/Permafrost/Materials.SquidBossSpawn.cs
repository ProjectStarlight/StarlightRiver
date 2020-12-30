using Terraria.ID;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.Permafrost
{
    public class SquidBossSpawn : ModItem
    {
        public override string Texture => AssetDirectory.PermafrostItem + "SquidBossSpawn";

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