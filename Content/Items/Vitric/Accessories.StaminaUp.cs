using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Items.BaseTypes;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Vitric
{
	public class StaminaUp : SmartAccessory
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public StaminaUp() : base("Glass Starlight Vessel", "Increased maximum Starlight by 1") { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Orange;

			Item.value = Item.sellPrice(gold: 1);
		}

		public override void SafeUpdateEquip(Player Player)
		{
			AbilityHandler mp = Player.GetHandler();
			mp.StaminaMaxBonus += 1;
		}

		public override void UpdateAccessory(Player Player, bool hideVisual)
		{
			if (!GUI.StaminaBar.specialVesselTextures.Contains(Texture) && Main.myPlayer == Player.whoAmI)
				GUI.StaminaBar.specialVesselTextures.Add(Texture);
		}
	}
}