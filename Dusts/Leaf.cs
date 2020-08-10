using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace StarlightRiver.Dusts
{
    public class Leaf : Corrupt
    {
        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return lightColor;
        }

        public override void OnSpawn(Dust dust)
        {
            dust.scale *= 2;
            dust.fadeIn = Main.rand.NextFloat(6.28f);
            dust.noLight = false;
        }

        public override bool Update(Dust dust)
        {
            dust.position.Y += dust.velocity.Y;
            dust.velocity.Y += 0.01f;
            dust.position.X += (float)Math.Sin(StarlightWorld.rottime + dust.fadeIn) * dust.scale * dust.velocity.X * 0.4f;
            dust.rotation = (float)Math.Sin(StarlightWorld.rottime + dust.fadeIn) * 0.5f;
            dust.scale *= 0.99f;
            dust.color *= 0.92f;
            if (dust.scale <= 0.3)
            {
                dust.active = false;
            }
            return false;
        }
    }

    public class GreenLeaf : Leaf
    {
    }
}