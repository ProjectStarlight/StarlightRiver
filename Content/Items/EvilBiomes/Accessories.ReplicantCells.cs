using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using Terraria;

namespace StarlightRiver.Content.Items.EvilBiomes
{
	class ReplicantCells : SmartAccessory
	{
		public override string Texture => AssetDirectory.EvilBiomesItem + Name;

		public ReplicantCells() : base("Replicant Cells", "+15% DoT Resist\nHealth regeneration starts slightly faster") { }

		public override void SafeUpdateEquip(Player Player)
		{
			Player.GetModPlayer<DoTResistancePlayer>().DoTResist += 0.15f;

			if (Player.lifeRegenTime % 5 == 0)
				Player.lifeRegenTime++;
		}
	}
}
