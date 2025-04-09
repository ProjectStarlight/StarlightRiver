namespace StarlightRiver.Content.Buffs
{
	public class ProtectiveShard : SmartBuff
	{
		public ProtectiveShard() : base("Protective Shards", "Incoming damage reduced by 25%", true) { }

		public override string Texture => AssetDirectory.Buffs + "ProtectiveShard";
	}
}