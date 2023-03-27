namespace StarlightRiver.Content.Items.Vitric
{
	public class TempleEntranceKey : QuickMaterial
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public TempleEntranceKey() : base("Large forge key", "Opens a door to the vitric forge.", Terraria.ID.ItemRarityID.Quest, 0, 1) { }
	}
}