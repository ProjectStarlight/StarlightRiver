namespace StarlightRiver.Content.Items.Vitric
{
	public class VitricOre : QuickMaterial
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public VitricOre() : base("Vitric Shard", "", 999, 200, 2) { }
	}

	public class SandstoneChunk : QuickMaterial
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public SandstoneChunk() : base("Ancient Ceramic", "", 999, 200, 2) { }
	}

	public class MagmaCore : QuickMaterial
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public MagmaCore() : base("Magmatic Core", "A sample of the hot stuff.", 999, 20000, 3) { }
	}
}