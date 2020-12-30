using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Items.BaseTypes;

namespace StarlightRiver.Content.Items.Overgrow
{
    internal class MossSalve : SmartAccessory
    {
        public override string Texture => AssetDirectory.OvergrowItem + "MossSalve";

        public MossSalve() : base("Moss Salve", "Health potions grant a short regeneration effect") { }

        public override void SafeSetDefaults()
        {
            item.rare = ItemRarityID.Green;
            item.value = 10000;
        }

        public override bool Autoload(ref string name)
        {
            StarlightItem.GetHealLifeEvent += HealMoss;
            return true;
        }

        private void HealMoss(Item item, Player player, bool quickHeal, ref int healValue)
        {
            if (item.potion && Equipped(player)) player.AddBuff(BuffType<Buffs.MossRegen>(), 60 * 6);
        }
    }
}