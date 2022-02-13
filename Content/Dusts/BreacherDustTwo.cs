using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Dusts
{
	class BreacherDustTwo : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Dust + "Aurora";
            return true;
        }
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.fadeIn = 60;
            dust.frame = new Rectangle(0, 0, 100, 100);

            dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.GetEffect("Effects/GlowingDust")), "GlowingDustPass");
            dust.shader.UseColor(Color.Transparent);
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            Vector3 col = Vector3.Lerp(dust.color.ToVector3(), Color.White.ToVector3(), dust.scale / (dust.customData is null ? 0.5f : (float)dust.customData));
            return new Color(col.X, col.Y, col.Z) * ((255 - dust.alpha) / 255f);
        }

        public override bool Update(Dust dust)
        {
            Lighting.AddLight(dust.position, dust.color.ToVector3() * dust.scale * 2.5f);

            dust.fadeIn--;
            dust.position += dust.velocity;
            dust.velocity *= 0.94f;

            dust.shader.UseColor(dust.color);
            dust.alpha += 10;
            if (dust.alpha >= 255) dust.active = false;
            return false;
        }
    }
}
