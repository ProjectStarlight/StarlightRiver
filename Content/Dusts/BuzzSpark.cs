using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;
using System;

namespace StarlightRiver.Content.Dusts
{
    class BuzzSpark : ModDust
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
            dust.fadeIn = 0;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 5, 50);

            dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.GetEffect("Effects/ShrinkingDust")), "ShrinkingDustPass");
        }

        public override bool Update(Dust dust)
        {
            if (dust.customData is null)
            {
                dust.position -= new Vector2(2.5f, 25).RotatedBy(dust.rotation) * dust.scale;
                dust.customData = 1;
            }

            dust.frame.Y++;
            dust.frame.Height --;

            dust.rotation = dust.velocity.ToRotation() + 1.57f;
            dust.position += dust.velocity;

            dust.color.G -= 8;
            dust.color.A -= 5;

            dust.velocity.X *= 0.98f;
            dust.velocity.Y *= 0.95f;

            dust.velocity.Y += 0.15f;

            float mult = 1;

            if (dust.fadeIn < 5)
                mult = dust.fadeIn / 5f;

            dust.shader.UseSecondaryColor(new Color((int)(255 * (1 - (dust.fadeIn / 20f))), 0,0) * mult);
            dust.shader.UseColor(dust.color * mult);
            dust.fadeIn++;

            Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.02f);

            if (dust.fadeIn > 30)
                dust.active = false;
            return false;
        }
    }
}
