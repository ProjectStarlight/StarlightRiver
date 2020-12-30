using Terraria;
using Terraria.ID;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using StarlightRiver.Content.Abilities;

namespace StarlightRiver.Content.Items.Misc
{
    public class StaminaUp : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;
        public StaminaUp() : base("Makeshift Stamina Vessel", "+1 Maximum Stamina") { }
        public override void SafeSetDefaults() => item.rare = ItemRarityID.Blue;
        public override void SafeUpdateEquip(Player player)
        {
            AbilityHandler mp = player.GetHandler();
            mp.StaminaMaxBonus += 1;
        }
    }
}