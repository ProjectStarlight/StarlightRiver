using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core.Systems.BarrierSystem;
using Terraria.ID;

namespace StarlightRiver.Content.Items.UndergroundTemple
{
	class TempleRune : SmartAccessory
	{
		public override string Texture => AssetDirectory.CaveTempleItem + Name;

		public TempleRune() : base("Rune of Warding", "+50 {{barrier}}") { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 25);
		}

		public override void SafeUpdateEquip(Player Player)
		{
			Player.GetModPlayer<BarrierPlayer>().maxBarrier += 50;
		}
	}
}