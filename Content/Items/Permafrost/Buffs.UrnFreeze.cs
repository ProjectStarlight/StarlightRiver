using StarlightRiver.Content.Buffs;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Permafrost
{
	internal class UrnFreeze : SmartBuff
	{
		public override string Texture => AssetDirectory.PermafrostItem + Name;

		public UrnFreeze() : base("Urn Freeze", "The pot is out of control!", true, false) { }

		public override void Update(Player player, ref int buffIndex)
		{
			player.AddBuff(BuffID.Frostburn, 2);
		}
	}
}
