using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.Misc
{
    public class PowerCell : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;
        public PowerCell() : base("Power Cell", "NaN") { }
    }
}