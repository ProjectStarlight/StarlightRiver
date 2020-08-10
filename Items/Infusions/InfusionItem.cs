using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Items.Infusions
{
    public class InfusionItem : ModItem
    {
        private readonly int Rarity;

        public InfusionItem(int rarity)
        {
            Rarity = rarity;
        }

        public override void SetDefaults()
        {
            item.width = 64;
            item.height = 64;
            item.rare = Rarity;
        }

        public virtual void Unequip(Player player)
        {
        }
    }
}