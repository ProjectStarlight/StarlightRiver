using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.ForbiddenWinds;

namespace StarlightRiver.Content.Items.Vitric
{
    internal class CeirosExpert : ModItem
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

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
            if (player.GetHandler().GetAbility<Dash>(out var dash))
                dash.CooldownBonus += 16;
        }
    }
}