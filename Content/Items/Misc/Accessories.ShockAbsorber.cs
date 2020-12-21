using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.Misc
{
    public class ShockAbsorber : SmartAccessory
    {
        public override string Texture => Directory.MiscItem + Name;
        public ShockAbsorber() : base("Shock Absorber", "NaN") { }
    }
}