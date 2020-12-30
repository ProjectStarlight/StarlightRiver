using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Abilities.Purify.TransformationHelpers
{
    class TransformationLoader : ILoadable
    {
        public static List<PurifyTransformation> transformations;

        public float Priority => 1;

        public void Load()
        {
            transformations = new List<PurifyTransformation>();

            //Register transformations here!
        }

        public void Unload()
        {
            transformations = null;
        }
    }
}
