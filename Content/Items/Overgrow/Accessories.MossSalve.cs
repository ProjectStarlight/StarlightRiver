using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Overgrow
{
	internal class MossSalve : SmartAccessory
    {
        public override string Texture => AssetDirectory.OvergrowItem + "MossSalve";

        public MossSalve() : base("Moss Salve", "Health potions grant a short regeneration effect") { }

        public override void SafeSetDefaults()
        {
            Item.rare = ItemRarityID.Green;
            Item.value = 10000;
        }

        public override void Load()
        {
            StarlightItem.GetHealLifeEvent += HealMoss;
            return true;
        }

        private void HealMoss(Item Item, Player Player, bool quickHeal, ref int healValue)
        {
            if (Item.potion && Equipped(Player)) Player.AddBuff(BuffType<Buffs.MossRegen>(), 60 * 6);
        }
    }
}