using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	public class TarnishedRing : QuickMaterial
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public TarnishedRing() : base("Tarnished Ring", "'Ancient and heavily tarnished, but still solid. You could sculpt this into something useful...", 1, Item.sellPrice(gold: 1), ItemRarityID.Orange) { }
	}
}