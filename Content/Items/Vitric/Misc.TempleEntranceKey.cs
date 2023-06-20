namespace StarlightRiver.Content.Items.Vitric
{
	public class TempleEntranceKey : QuickMaterial
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public TempleEntranceKey() : base("Forge Key", "Opens a door to the Vitric Forge.", 1, 0, Terraria.ID.ItemRarityID.Quest) { }
	}
}