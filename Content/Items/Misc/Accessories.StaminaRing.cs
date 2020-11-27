using StarlightRiver.Abilities;
using Terraria;
using Terraria.ID;

using StarlightRiver.Core;
using StarlightRiver.Content.Items.BaseTypes;

namespace StarlightRiver.Items.Accessories
{
    public class StaminaRing : SmartAccessory
    {
        public StaminaRing() : base("Band of Endurance", "Increased Stamina Regeneration") { }
        public override void SafeSetDefaults() { item.rare = ItemRarityID.Blue; }
        public override void SafeUpdateEquip(Player player)
        {
            AbilityHandler mp = player.GetHandler();
            mp.StaminaMaxBonus += 1;
        }
    }
}
