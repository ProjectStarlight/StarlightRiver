using StarlightRiver.Abilities;
using Terraria;

namespace StarlightRiver.Items.Infusions
{
    public class DashAstralItem : InfusionItem
    {
        public DashAstralItem() : base(3)
        {
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Comet Rush");
            Tooltip.SetDefault("Forbidden Winds Infusion\nDash farther and faster");
        }

        public override void UpdateEquip(Player player)
        {
            AbilityHandler mp = player.GetModPlayer<AbilityHandler>();
            if (!(mp.dash is DashAstral) && !(mp.dash is DashCombo))
            {
                if (mp.dash is DashFlame) { mp.dash = new DashCombo(player); }
                else { mp.dash = new DashAstral(player); }
                mp.dash.Locked = false;
                mp.dash.Cooldown = 90;
            }
        }

        public override bool CanEquipAccessory(Player player, int slot)
        {
            AbilityHandler mp = player.GetModPlayer<AbilityHandler>();
            return !mp.dash.Locked;
        }

        public override void Unequip(Player player)
        {
            AbilityHandler mp = player.GetModPlayer<AbilityHandler>();
            mp.dash = new Dash(player)
            {
                Locked = false,
                Cooldown = 90
            };
        }
    }
}