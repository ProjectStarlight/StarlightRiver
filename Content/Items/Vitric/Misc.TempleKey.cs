namespace StarlightRiver.Content.Items.Vitric
{
	public class TempleKey : QuickMaterial
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public TempleKey() : base("Small Forge Key", "Opens a door inside the Vitric Forge.", Terraria.ID.ItemRarityID.Quest, 0, 1) { }
	}
}