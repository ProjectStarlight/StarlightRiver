using StarlightRiver.Abilities;
using StarlightRiver.Abilities.Content;
using Terraria;

namespace StarlightRiver.Items.Infusions
{
    public class WispHomingItem : InfusionBase<Wisp>
    {
        public WispHomingItem() : base(3)
        {
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Feral Wisp");
            Tooltip.SetDefault("Faeflame Infusion\nRelease homing bolts that lower enemies' damage");
        }
    }
}