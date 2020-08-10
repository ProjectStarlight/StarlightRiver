using StarlightRiver.Abilities;
using Terraria;

namespace StarlightRiver.Items.Infusions
{
    public class DashFireItem : InfusionItem
    {
        public DashFireItem() : base(3)
        {
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Flaming Slash");
            Tooltip.SetDefault("Forbidden Winds Infusion\nDeal damage to enemies when you dash into them\nDamage dealth is equal to 5% of an enemys HP (up to 200)");
        }

        public override void UpdateEquip(Player player)
        {
            AbilityHandler mp = player.GetModPlayer<AbilityHandler>();
            if (!(mp.dash is DashFlame) && !(mp.dash is DashCombo))
            {
                if (mp.dash is DashAstral) { mp.dash = new DashCombo(player); }
                else { mp.dash = new DashFlame(player); }
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