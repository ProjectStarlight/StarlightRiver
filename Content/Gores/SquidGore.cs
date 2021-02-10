using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Gores
{
    internal class SquidGore : ModGore
    {
        public override void OnSpawn(Gore gore)
        {
            gore.timeLeft = 90;
            gore.frame = (byte)Main.rand.Next(7);
            gore.numFrames = 7;
        }

        public override bool Update(Gore gore)
        {
            if (gore.timeLeft > 90) gore.timeLeft = 90;

            var color = new Color(50, 150, 255) * (gore.timeLeft / 55f);

            Dust.NewDustPerfect(gore.position + Vector2.One * 29 + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(3), DustType<Content.Dusts.Ink>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, color, 0.7f);
            Lighting.AddLight(gore.position, color.ToVector3() * 0.3f);
            return true;
        }
    }
}