using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Alchemy.Ingredients
{
    internal class DirtIngredient : AlchemyIngredient
    {
        public DirtIngredient()
        {
            ingredientColor = new Color(107, 66, 12);
        }

        public override int getItemId()
        {
            return ItemID.DirtBlock;
        }
    }
}
