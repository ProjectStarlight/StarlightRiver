using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Dusts
{
    class Aurora : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.fadeIn = 60;
            dust.frame = new Rectangle(0, 0, 13, 13);
            dust.scale = 0;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            Vector3 col = Vector3.Lerp(dust.color.ToVector3(), Color.White.ToVector3(), dust.scale);
            return new Color(col.X, col.Y, col.Z);
        }

        public override bool Update(Dust dust)
        {
            Lighting.AddLight(dust.position, dust.color.ToVector3() * dust.scale);
            dust.rotation += 0.02f;

            dust.scale = dust.fadeIn / 15f - (float)Math.Pow(dust.fadeIn, 2) / 900f;
            dust.fadeIn--;

            if (dust.fadeIn <= 0) dust.active = false;
            return false;
        }
    }
}
