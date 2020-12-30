using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.Misc
{
    public class ShockAbsorber : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;
        public ShockAbsorber() : base("Shock Absorber", "NaN") { }
    }
}