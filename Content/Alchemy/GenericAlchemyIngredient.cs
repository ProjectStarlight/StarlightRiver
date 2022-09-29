using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace StarlightRiver.Content.Alchemy
{
    public class GenericAlchemyIngredient : AlchemyIngredient
    {
        //this class is for instantiating generic ingredients that do not have any custom logic / visuals assigned to them
        //defaults to using this for new ingredients if chosen Item Id is not in the AlchemyRecipeSystem cache
        //ideally every alchemy ingredient will have its own awesome visuals eventually but this is a nice stopgap

        private readonly int ItemId; 

        public GenericAlchemyIngredient(int ItemId)
        {
            ingredientColor = new Color(Main.rand.Next(255), Main.rand.Next(255), Main.rand.Next(255)); //just randomize it if its not a defined ingredient
            this.ItemId = ItemId;
        }
        public override int getItemId()
        {
            return ItemId;
        }
    }
}
