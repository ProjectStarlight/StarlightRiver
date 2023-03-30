namespace StarlightRiver.Content.Items.Vitric
{
	public class TempleKey : QuickMaterial
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public TempleKey() : base("Small forge key", "Opens a door inside the vitric forge.", Terraria.ID.ItemRarityID.Quest, 0, 1) { }
	}
}