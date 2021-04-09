using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using StarlightRiver.Core;

namespace StarlightRiver.Content.Bosses.GlassBoss
{
    class PowerupDust : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Dust + "Stamina";
            return true;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            var curveOut = Curve(1 - dust.fadeIn / 40f);
            var color = Color.Lerp(dust.color, new Color(255, 100, 0), dust.fadeIn / 30f);
            return color * (curveOut + 0.4f);
        }

        float Curve(float input) //shrug it works, just a cubic regression for a nice looking curve
		{
            return -2.65f + 19.196f * input - 32.143f * input * input + 15.625f * input * input * input;
		}

        public override void OnSpawn(Dust dust)
        {
            dust.fadeIn = 0;
            dust.noLight = false;
        }

        public override bool Update(Dust dust)
        {
            dust.rotation += dust.velocity.Y * 0.1f;
            dust.position += dust.velocity;
            //dust.scale = Curve( 1 - dust.fadeIn / 40f);

            dust.fadeIn++;

            if (dust.fadeIn > 40)
                dust.active = false;
            return false;
        }
    }
}
