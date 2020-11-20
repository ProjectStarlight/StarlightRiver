using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Gores
{
    internal class ChainGore : ModGore
    {
        public override void OnSpawn(Gore gore) => gore.timeLeft = 180;
    }
}