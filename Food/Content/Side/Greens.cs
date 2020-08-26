using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace StarlightRiver.Food.Content.Side
{
    internal class Greens : Ingredient
    {
        public Greens() : base("+1 defense", 300, IngredientType.Side) { }

        public override void BuffEffects(Player player, float multiplier)
        {
            player.statDefense += (int)(1 * multiplier);
        }
    }
}
