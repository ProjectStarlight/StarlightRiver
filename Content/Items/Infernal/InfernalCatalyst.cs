using Terraria.ID;

namespace StarlightRiver.Content.Items.Infernal
{
	internal class InfernalCatalyst : QuickMaterial
	{
		public override string Texture => AssetDirectory.Debug;

		public InfernalCatalyst() : base("Infernal Catalyst", "Primes the lavas of hell for transmutation", 9999, 0, ItemRarityID.Orange) { }
	}
}
