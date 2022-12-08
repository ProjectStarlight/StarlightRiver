using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Items.BaseTypes;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	public class StaminaUp : SmartAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public StaminaUp() : base("Glass Stamina Vessel", "Increased maximum stamina by 1") { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Orange;
		}

		public override void SafeUpdateEquip(Player Player)
		{
			AbilityHandler mp = Player.GetHandler();
			mp.StaminaMaxBonus += 1;
		}

		public override void UpdateAccessory(Player Player, bool hideVisual)
		{
			if (!GUI.Stam.specialVesselTextures.Contains(Texture) && Main.myPlayer == Player.whoAmI)
				GUI.Stam.specialVesselTextures.Add(Texture);
		}
	}
}