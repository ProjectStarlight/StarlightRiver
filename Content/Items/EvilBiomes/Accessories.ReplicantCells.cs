using StarlightRiver.Content.Items.BaseTypes;

namespace StarlightRiver.Content.Items.EvilBiomes
{
	class ReplicantCells : SmartAccessory
	{
		public override string Texture => AssetDirectory.EvilBiomesItem + Name;

		public ReplicantCells() : base("Replicant Cells", "+15% Inoculation\nHealth regeneration starts slightly faster") { }

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(silver: 30);
		}

		public override void SafeUpdateEquip(Player Player)
		{
			Player.GetModPlayer<DoTResistancePlayer>().DoTResist += 0.15f;

			if (Player.lifeRegenTime % 5 == 0)
				Player.lifeRegenTime++;
		}
	}
}