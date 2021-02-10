using Terraria;
using Terraria.ID;
using StarlightRiver.Core;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Abilities;

namespace StarlightRiver.Content.Items.Misc
{
    public class StaminaRing : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;
        public StaminaRing() : base("Band of Endurance", "Increased Stamina Regeneration") { }
        public override void SafeSetDefaults() => item.rare = ItemRarityID.Blue;
        public override void SafeUpdateEquip(Player player)
        {
            AbilityHandler mp = player.GetHandler();
            mp.StaminaMaxBonus += 1;
        }
    }
}
