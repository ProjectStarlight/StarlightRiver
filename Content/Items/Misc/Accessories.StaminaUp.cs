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
        public StaminaUp() : base("Makeshift Stamina Vessel", "Increased maximum stamina") { }
        public override void SafeSetDefaults() => item.rare = ItemRarityID.Blue;
        public override void SafeUpdateEquip(Player player)
        {
            AbilityHandler mp = player.GetHandler();
            mp.StaminaMaxBonus += 1;
        }
    }
}