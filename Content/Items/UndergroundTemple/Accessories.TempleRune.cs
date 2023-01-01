using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core.Systems.BarrierSystem;
using Terraria.ID;

namespace StarlightRiver.Content.Items.UndergroundTemple
{
	class TempleRune : SmartAccessory
	{
		public override string Texture => AssetDirectory.CaveTempleItem + Name;

		public TempleRune() : base("Rune of Warding", "+20 maximum barrier") { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;
		}

		public override void SafeUpdateEquip(Player Player)
		{
			Player.GetModPlayer<BarrierPlayer>().maxBarrier += 20;
		}
	}
}
