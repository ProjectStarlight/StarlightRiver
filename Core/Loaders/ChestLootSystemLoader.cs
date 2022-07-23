using StarlightRiver.Content.Alchemy;
using StarlightRiver.Content.WorldGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Core.Loaders
{
    public class ChestLootSystemLoader : IOrderedLoadable
    {

        public float Priority => 2.0f;

        public void Load()
        {
            ChestLootSystem.Load();
        }

        public void Unload()
        {
            ChestLootSystem.Unload();
        }
    }
}
