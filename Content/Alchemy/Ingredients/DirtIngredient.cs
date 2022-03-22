using Microsoft.Xna.Framework;
using StarlightRiver.Content.Dusts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Alchemy.Ingredients
{
    internal class DirtIngredient : AlchemyIngredient
    {
        //TODO: this is being treated as though it is the tarnished ring that does not exist yet, replace with tarnished ring
        public DirtIngredient()
        {
            ingredientColor = Color.DarkRed;
        }

        public override int getItemId()
        {
            return ItemID.DirtBlock;
        }

        public override void visualUpdate(AlchemyWrapper wrapper)
        {
            base.visualUpdate(wrapper);

            if (Main.rand.Next(20) == 0)
            {
                Dust.NewDust(wrapper.cauldronRect.TopLeft() - new Vector2(0, 20), wrapper.cauldronRect.Width, 0, ModContent.DustType<NeedlerDustTwo>(), 0, -2, 120, Color.Black, 0.5f);
            }
        }

        public override bool addToStack(Item item)
        {
            return false; //unstackable equipment
        }
    }
}
