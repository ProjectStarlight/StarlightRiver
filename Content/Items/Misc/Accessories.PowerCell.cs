using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.Misc
{
    public class PowerCell : SmartAccessory
    {
        public override string Texture => Directory.MiscItemDir + Name;
        public PowerCell() : base("Power Cell", "NaN") { }
    }
}