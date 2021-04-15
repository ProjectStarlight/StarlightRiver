using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Microsoft.Xna.Framework.Graphics;

namespace StarlightRiver.Content.Bosses.GlassBoss
{
    class PowerupDust : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Assets/Keys/GlowVerySoft";
            return true;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            var curveOut = Curve(1 - dust.fadeIn / 40f);
            var color = Color.Lerp(dust.color, new Color(255, 100, 0), dust.fadeIn / 30f);
            dust.color = color * (curveOut + 0.4f);
            return dust.color;
        }

        float Curve(float input) //shrug it works, just a cubic regression for a nice looking curve
		{
            return -2.65f + 19.196f * input - 32.143f * input * input + 15.625f * input * input * input;
		}

        public override void OnSpawn(Dust dust)
        {
            dust.color = Color.Transparent;
            dust.fadeIn = 0;
            dust.noLight = false;
            dust.scale *= 0.3f;
            dust.frame = new Rectangle(0, 0, 64, 64);

            dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.GetEffect("Effects/GlowingDust")), "GlowingDustPass");
        }

        public override bool Update(Dust dust)
        {
            if (dust.color == Color.Transparent)
                dust.position -= Vector2.One * 32 * dust.scale;

            //dust.rotation += dust.velocity.Y * 0.1f;
            dust.position += dust.velocity;

            dust.shader.UseColor(dust.color);

            dust.fadeIn++;

            Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.6f);

            if (dust.fadeIn > 40)
                dust.active = false;
            return false;
        }
    }
}
