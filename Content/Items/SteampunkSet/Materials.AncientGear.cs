using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.SteampunkSet
{
	public class AncientGear : QuickMaterial
    {
        public override string Texture => AssetDirectory.Debug;

        public AncientGear() : base("Ancient Gear", "", 999, 200, 2) { }
    }
}