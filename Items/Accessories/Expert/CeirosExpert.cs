using StarlightRiver.Abilities;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Items.Accessories.Expert
{
    internal class CeirosExpert : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Reduces the cooldown of forbidden winds by 16%");
            DisplayName.SetDefault("Wind Crystal");
        }

        public override void SetDefaults()
        {
            item.width = 32;
            item.height = 32;
            item.rare = -12;
            item.accessory = true;
        }

        public override void UpdateEquip(Player player)
        {
            AbilityHandler mp = player.GetModPlayer<AbilityHandler>();
            if (mp.dash.Cooldown == 90)
            {
                mp.dash.Cooldown = 74;
            }
        }
    }
}