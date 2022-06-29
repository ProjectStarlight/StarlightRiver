using Microsoft.Xna.Framework;
using StarlightRiver.Content.Dusts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Alchemy.Ingredients
{
    class VitricOreIngredient : AlchemyIngredient
    {
        public VitricOreIngredient()
        {
            ingredientColor = Color.Aquamarine;
        }

        public override int getItemId()
        {
            return ModContent.ItemType<Items.Vitric.VitricOre>();
        }

        public override void visualUpdate(AlchemyWrapper wrapper)
        {
            base.visualUpdate(wrapper);
            if (timeSinceAdded == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    Dust.NewDust(wrapper.cauldronRect.TopLeft() - new Vector2(0, 25), wrapper.cauldronRect.Width, 25, ModContent.DustType<CrystalSparkle>(), 0, 0);
                    Dust.NewDust(wrapper.cauldronRect.TopLeft() - new Vector2(0, 25), wrapper.cauldronRect.Width, 25, ModContent.DustType<CrystalSparkle2>(), 0, 0);
                }
            }

            if (Main.rand.Next(50) == 0)
            {
                if (Main.rand.NextBool())
                    Dust.NewDust(wrapper.cauldronRect.TopLeft() - new Vector2(0, 25), wrapper.cauldronRect.Width, 25, ModContent.DustType<CrystalSparkle>(), 0, 0);
                else
                    Dust.NewDust(wrapper.cauldronRect.TopLeft() - new Vector2(0, 25), wrapper.cauldronRect.Width, 25, ModContent.DustType<CrystalSparkle2>(), 0, 0);
            }
        }


    }
}
