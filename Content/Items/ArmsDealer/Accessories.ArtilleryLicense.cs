using StarlightRiver.Content.Items.BaseTypes;
using Terraria.ID;

namespace StarlightRiver.Content.Items.ArmsDealer
{
	internal class ArtilleryLicense : SmartAccessory
	{
		public override string Texture => AssetDirectory.ArmsDealerItem + Name;

		public ArtilleryLicense() : base("Artillery License", "Increases your max number of sentries by 1\n`Totally not forged`") { }

		public override void SafeSetDefaults()
		{
			Item.value = Item.buyPrice(gold: 10);
			Item.rare = ItemRarityID.Green;
		}

		public override void SafeUpdateEquip(Player player)
		{
			player.maxTurrets += 1;
		}
	}
}