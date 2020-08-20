using StarlightRiver.Abilities;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Items.Infusions
{
    public class InfusionBase<T> : InfusionItem<T> where T : Ability
    {
        private readonly int Rarity;

        public InfusionBase(int rarity)
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