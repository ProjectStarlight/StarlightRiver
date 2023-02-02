using StarlightRiver.Content.Items.BaseTypes;

namespace StarlightRiver.Content.Items.Misc
{
	public class ShockAbsorber : SmartAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public ShockAbsorber() : base("Shock Absorber", "Dropping from great heights creates a shockwave\nNullifies fall damage") { }
	}
}