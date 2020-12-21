using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.Misc
{
    public class LegBrace : SmartAccessory
    {
        //TODO: this
        public override string Texture => Directory.MiscItem + Name;
        public LegBrace() : base("Leg Brace", "NaN") { }
    }
}