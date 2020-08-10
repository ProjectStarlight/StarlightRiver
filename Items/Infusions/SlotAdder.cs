using StarlightRiver.Abilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Items.Infusions
{
    public class SlotAdder : ModItem
    {
        public override void SetDefaults()
        {
            item.width = 32;
            item.height = 32;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.useAnimation = 30;
            item.useTime = 30;
            item.rare = ItemRarityID.Lime;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("[PH] Infusion Transfusion");
            Tooltip.SetDefault("Permanently increases the number of infusion slots");
        }

        public override bool UseItem(Player player)
        {
            player.GetModPlayer<AbilityHandler>().HasSecondSlot = true;
            return true;
        }
    }
}