using Microsoft.Xna.Framework;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.Misc;
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
    internal class DullBladeIngredient : AlchemyIngredient
    {
        public DullBladeIngredient()
        {
            ingredientColor = new Color(200, 200, 205);
        }

        public override int getItemId()
        {
            return ModContent.ItemType<DullBlade>();
        }

        public override void visualUpdate(AlchemyWrapper wrapper)
        {
            base.visualUpdate(wrapper);

            if (Main.rand.NextBool(20))
                Dust.NewDust(wrapper.cauldronRect.TopLeft() + new Vector2(0, 20), wrapper.cauldronRect.Width, 0, ModContent.DustType<BuzzSpark>(), 0, -2, 0, new Color(200, 200, 205) * 0.8f, 0.75f);
        }

        public override bool addToStack(Item Item)
        {
            return false; //unstackable equipment
        }
    }
}
