using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Alchemy.Ingredients
{
    internal class IronBarIngredient : AlchemyIngredient
    {
        public IronBarIngredient()
        {
            ingredientColor = new Color(88, 74, 69);
        }

        public override int getItemId()
        {
            return ItemID.IronBar;
        }
    }
}
