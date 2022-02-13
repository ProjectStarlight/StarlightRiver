using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Items.BaseTypes;
//using StarlightRiver.Content.Items.Overgrow;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Misc
{
	public class StaminaRing : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public StaminaRing() : base("Band of Endurance", "Slowly regenerates stamina") { }

        public override void SafeSetDefaults() => item.rare = ItemRarityID.Green;

        public override void SafeUpdateEquip(Player player)
        {
            AbilityHandler mp = player.GetHandler();
            mp.StaminaRegenRate += 0.05f;
        }
    }
}
