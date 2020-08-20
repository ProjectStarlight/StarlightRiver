using StarlightRiver.Abilities;
using StarlightRiver.Abilities.Content;
using Terraria;

namespace StarlightRiver.Items.Infusions
{
    public class DashAstralItem : InfusionBase<Dash>
    {
        public DashAstralItem() : base(3)
        {
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Comet Rush");
            Tooltip.SetDefault("Forbidden Winds Infusion\nDash farther and faster");
        }
    }
}