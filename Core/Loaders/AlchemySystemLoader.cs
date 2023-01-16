using StarlightRiver.Content.Alchemy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Core.Loaders
{
    public class AlchemySystemLoader : IOrderedLoadable
    {

        public float Priority => 2.0f;

        public void Load()
        {
            AlchemyRecipeSystem.Load();
        }

        public void Unload()
        {
            AlchemyRecipeSystem.Unload();
        }
    }
}
