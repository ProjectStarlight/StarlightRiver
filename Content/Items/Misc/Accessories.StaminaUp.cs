using StarlightRiver.Abilities;
using Terraria;
using Terraria.ID;

using StarlightRiver.Core;

namespace StarlightRiver.Items.Accessories
{
    public class StaminaUp : SmartAccessory
    {
        public StaminaUp() : base("Makeshift Stamina Vessel", "+1 Maximum Stamina") { }
        public override void SafeSetDefaults() { item.rare = ItemRarityID.Blue; }
        public override void SafeUpdateEquip(Player player)
        {
            AbilityHandler mp = player.GetHandler();
            mp.StaminaMaxBonus += 1;
        }
    }
}