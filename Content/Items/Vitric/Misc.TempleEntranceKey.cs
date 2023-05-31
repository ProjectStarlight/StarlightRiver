namespace StarlightRiver.Content.Items.Vitric
{
	public class TempleEntranceKey : QuickMaterial
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public TempleEntranceKey() : base("Forge Key", "Opens a door to the Vitric Forge.", Terraria.ID.ItemRarityID.Quest, 0, 1) { }
	}
}