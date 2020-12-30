using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Dusts
{
    public class Mist : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Dust + name;
            return true;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }

        public override void OnSpawn(Dust dust)
        {
            dust.scale *= Main.rand.NextFloat(0.5f, 1.2f);
            dust.fadeIn = 0;
            dust.noLight = false;
            dust.rotation = Main.rand.NextFloat(6.28f);
            dust.frame = new Rectangle(0, 0, 32, 22);
        }

        public override bool Update(Dust dust)
        {
            dust.position.X += dust.velocity.X + Main.LocalPlayer.velocity.X * -0.6f * (dust.scale * 0.01f);
            dust.position.Y += Main.LocalPlayer.velocity.Y * -0.15f * (dust.scale * 0.01f);
            dust.velocity.X = 0.9f;
            dust.position.Y += (float)Math.Sin(StarlightWorld.rottime + dust.fadeIn / 30f) * 0.4f;
            dust.scale *= 0.999f;
            dust.rotation += 0.01f;

            dust.fadeIn++;
            float alpha = dust.fadeIn / 45f - (float)Math.Pow(dust.fadeIn, 2) / 3600f;
            dust.color = new Color(200, 235, 255) * 0.4f * alpha;

            if (dust.fadeIn > 120)
                dust.active = false;
            return false;
        }
    }
}