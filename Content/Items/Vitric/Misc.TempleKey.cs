using StarlightRiver.Content.Items.BaseTypes;

namespace StarlightRiver.Content.Items.Vitric
{
	public class TempleKey : BaseMaterial
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public TempleKey() : base("Small Forge Key", "Opens a door inside the Vitric Forge.", 9999, 0, Terraria.ID.ItemRarityID.Quest) { }
	}
}