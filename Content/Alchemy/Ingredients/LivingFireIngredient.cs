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

        //TODO: this is a placeholder for Blood replace with that once added
        public LivingFireIngredient()
        {
            ingredientColor = Color.Red;
        }

        public override int getItemId()
        {
            return ItemID.LivingFireBlock;
        }
    }
}
