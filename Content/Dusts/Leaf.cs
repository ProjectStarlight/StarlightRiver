using Microsoft.Xna.Framework;
using System;
using Terraria;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Dusts
{
    public class Leaf : Corrupt
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Dust + name;
            return true;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return lightColor;
        }

        public override void OnSpawn(Dust dust)
        {
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
                dust.active = false;
            return false;
        }
    }

    public class GreenLeaf : Leaf
    {
    }

    public class GreyLeaf : Leaf
	{
        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color.MultiplyRGBA(lightColor);
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity.Y += 0.02f;
            dust.position.X += (float)Math.Sin(StarlightWorld.rottime + dust.fadeIn) * dust.scale * dust.velocity.X * 0.2f;
            dust.rotation = (float)Math.Sin(StarlightWorld.rottime + dust.fadeIn) * 0.5f;
            dust.scale *= 0.99f;
            dust.color *= 0.995f;
            dust.velocity *= 0.96f;
            if (dust.scale <= 0.3)
                dust.active = false;
            return false;
        }
    }
}