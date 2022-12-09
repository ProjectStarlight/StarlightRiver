using StarlightRiver.Content.Buffs;

namespace StarlightRiver.Content.Items.Permafrost
{
	internal class UrnFreeze : SmartBuff
	{
		public override string Texture => AssetDirectory.PermafrostItem + Name;

		public UrnFreeze() : base("Urn Freeze", "Greatly increased defense\nBut you cant move!", true, false) { }

		public override void Update(Player player, ref int buffIndex)
		{

		}
	}
}
