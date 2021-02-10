using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Gores
{
    public class UltrasharkCasing : ModGore
    {
        public override void OnSpawn(Gore gore)
        {
            gore.timeLeft = 10;
        }

        public override bool Update(Gore gore)
        {
            if (gore.timeLeft <= 5)
            {
                gore.alpha += 10;
            }
            gore.velocity.X *= 0.95f;
            return base.Update(gore);
        }
    }
}