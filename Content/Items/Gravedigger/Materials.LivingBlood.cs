using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.Gravedigger
{
	public class LivingBlood : QuickMaterial
    {
        public override string Texture => AssetDirectory.Debug;

        public LivingBlood() : base("Living Blood", "", 999, 50, 2) { }
    }
}