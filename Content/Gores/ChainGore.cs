using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Gores
{
    internal class ChainGore : ModGore
    {
        public override void OnSpawn(Gore gore) => gore.timeLeft = 180;
    }
}