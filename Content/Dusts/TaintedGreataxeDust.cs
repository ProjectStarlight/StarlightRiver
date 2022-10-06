using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Dusts
{
    public class TaintedGreataxeDust : ModDust
    {
        //so basically i copied flare breacher dust and then changed the colors. revolutionary i know
        public override string Texture => AssetDirectory.Dust + "NeedlerDust";
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.scale *= Main.rand.NextFloat(0.8f, 2f);
            dust.frame = new Rectangle(0, 0, 34, 36);
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            Color gray = new Color(25, 25, 25);
            Color green = new Color(85, 220, 55);
            Color ret;

            if (dust.alpha < 60)
                ret = Color.Lerp(new Color(55, 140, 35), green, dust.alpha / 60f);
            else if (dust.alpha < 120)
                ret = Color.Lerp(green, gray, (dust.alpha - 60) / 60f);
            else
                ret = gray;
            return ret * ((255 - dust.alpha) / 255f);
        }

        public override bool Update(Dust dust)
        {
            Color gray = new Color(25, 25, 25);
            Color green = new Color(85, 220, 55);

            if (dust.alpha < 60)
                Lighting.AddLight(dust.position, Color.Lerp(new Color(55, 140, 35), green, dust.alpha / 60f).ToVector3());
            else if (dust.alpha < 120)
                Lighting.AddLight(dust.position, Color.Lerp(green, gray, (dust.alpha - 60) / 60f).ToVector3());

            if (dust.velocity.Length() > 3)
                dust.velocity *= 0.85f;
            else
                dust.velocity *= 0.92f;
            if (dust.alpha > 100)
            {
                dust.scale += 0.01f;
                dust.alpha += 3;
            }
            else
            {
                Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.1f);
                dust.scale *= 0.985f;
                dust.alpha += 6;
            }
            dust.position += dust.velocity;
            if (dust.alpha >= 255)
                dust.active = false;

            return false;
        }
    }
}
