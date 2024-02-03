using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	public class DullBlade : QuickMaterial
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public DullBlade() : base("Dull Blade", "Ancient and heavily worn, but still solid. You could forge this into something useful...", 1, Item.sellPrice(gold: 1), ItemRarityID.Orange) { }
	}
}