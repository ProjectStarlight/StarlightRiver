using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core.Systems.BarrierSystem;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Dungeon
{
	public class AquaSapphire : SmartAccessory
	{
		public override string Texture => AssetDirectory.DungeonItem + Name;

		public AquaSapphire() : base("Aqua Sapphire", "{{Barrier}} negates 15% more damage \n+20 {{Barrier}}") { }

		public override void SafeSetDefaults()
		{
			Item.width = 30;
			Item.height = 28;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.buyPrice(0, 5, 0, 0);
		}

		public override void UpdateAccessory(Player Player, bool hideVisual)
		{
			Player.GetModPlayer<BarrierPlayer>().maxBarrier += 20;
			Player.GetModPlayer<BarrierPlayer>().barrierDamageReduction += 0.15f;
		}
	}
}