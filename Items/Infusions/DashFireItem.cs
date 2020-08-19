using StarlightRiver.Abilities;
using StarlightRiver.Abilities.Content;
using Terraria;

namespace StarlightRiver.Items.Infusions
{
    public class DashFireItem : InfusionBase<Dash>
    {
        public DashFireItem() : base(3)
        {
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Flaming Slash");
            Tooltip.SetDefault("Forbidden Winds Infusion\nDeal damage to enemies when you dash into them\nDamage dealth is equal to 5% of an enemys HP (up to 200)");
        }
    }
}