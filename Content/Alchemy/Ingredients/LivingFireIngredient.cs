using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Alchemy.Ingredients
{
    class LivingFireIngredient : AlchemyIngredient
    {
        public LivingFireIngredient()
        {
            ingredientColor = new Color(255, 20, 0);
        }

        public override int getItemId()
        {
            return ItemID.LivingFireBlock;
        }
    }
}
