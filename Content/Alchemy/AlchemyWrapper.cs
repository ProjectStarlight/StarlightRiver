using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Alchemy
{
    public class AlchemyWrapper
    {
        //used to pass around to all the alchemy components to maintain context about bubble color, position, modifiers etc
        //add variables to this for more data
        //cauldron should be resetting any relevant fields every frame

        public Rectangle cauldronRect;

        public Color bubbleColor;
        public Color bloomColor;

        public float bubbleAnimationTimer; //float for % multipliers potentially

        public int bubbleAnimationFrame;

    }
}
