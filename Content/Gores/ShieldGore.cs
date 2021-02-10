using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using StarlightRiver.Core;

namespace StarlightRiver.Content.Bosses.GlassBoss
{
    class ShieldGore : ModGore
    {
        public override Color? GetAlpha(Gore gore, Color lightColor) => Color.White * (0.2f * (gore.timeLeft / 180f));

        public override void OnSpawn(Gore gore) => gore.timeLeft = 180;
    }
}
