using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Dusts
{
    class Aurora : ModDust
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
            dust.fadeIn = 60;
            dust.frame = new Rectangle(0, 0, 13, 13);
            dust.scale = 0;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            Vector3 col = Vector3.Lerp(dust.color.ToVector3(), Color.White.ToVector3(), dust.scale / (dust.customData is null ? 0.5f : (float)dust.customData));
            return new Color(col.X, col.Y, col.Z) * ((255 - dust.alpha) / 255f);
        }

        public override bool Update(Dust dust)
        {
            Lighting.AddLight(dust.position, dust.color.ToVector3() * dust.scale * 0.5f);
            dust.rotation += 0.06f;

            dust.scale = (dust.fadeIn / 15f - (float)Math.Pow(dust.fadeIn, 2) / 900f) * (dust.customData is null ? 0.5f : (float)dust.customData);
            dust.fadeIn--;
            dust.position += dust.velocity * 0.25f;

            if (dust.fadeIn <= 0) dust.active = false;
            return false;
        }
    }
}
