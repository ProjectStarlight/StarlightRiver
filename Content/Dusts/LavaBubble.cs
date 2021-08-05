using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Dusts
{
	class LavaBubble : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Dust + name;
            return true;
        }
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 14, 14);
            dust.fadeIn = Main.rand.NextFloat(6.28f);
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return Color.White;
        }

        public override bool Update(Dust dust)
        {
            dust.fadeIn += 0.1f;
            dust.position += dust.velocity;
            dust.position.X += (float)Math.Sin(dust.fadeIn) * 0.25f;
            dust.velocity.Y += 0.03f;
            dust.scale *= 0.98f;

            if (dust.scale < 0.1f)
                dust.active = false;

            return false;
        }
    }
}
