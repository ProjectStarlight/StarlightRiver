using StarlightRiver.Buffs;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Herbology.Potions
{
    internal class StarfallCocktail : QuickPotion
    {
        public StarfallCocktail() : base("Starfall Cocktail", "Increases the chance for fallen stars to fall", 36000, BuffType<StarfallCocktailBuff>(), 3)
        {
        }
    }
}