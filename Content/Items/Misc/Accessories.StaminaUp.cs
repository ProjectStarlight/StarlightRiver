using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	public class StaminaUp : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public StaminaUp() : base("Glass Stamina Vessel", "Increased maximum stamina by 1") { }

        public override void SafeSetDefaults()
        {
            item.rare = ItemRarityID.Orange;
        }

		public override void SafeUpdateEquip(Player player)
		{
            AbilityHandler mp = player.GetHandler();
            mp.StaminaMaxBonus += 1;
        }

		public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if(!GUI.Stam.specialVesselTextures.Contains(Texture) && Main.myPlayer == player.whoAmI)
                GUI.Stam.specialVesselTextures.Add(Texture);
        }
    }
}