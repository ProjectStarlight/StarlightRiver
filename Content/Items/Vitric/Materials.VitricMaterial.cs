using StarlightRiver.Content.Items.BaseTypes;

namespace StarlightRiver.Content.Items.Vitric
{
	public class VitricOre : BaseMaterial
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public VitricOre() : base("Vitric Shard", "", 500, 2) { }
	}

	public class SandstoneChunk : BaseMaterial
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public SandstoneChunk() : base("Ancient Ceramic", "", 200, 2) { }
	}

	public class MagmaCore : BaseMaterial
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public MagmaCore() : base("Magmatic Core", "A sample of the hot stuff.", 20000, 3) { }
	}
}