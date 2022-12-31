using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;


namespace StarlightRiver.Content.Dusts
{
    public class LightBlueSmoke : ModDust
    {
        public override string Texture => AssetDirectory.Dust + "NeedlerDust";
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.scale *= Main.rand.NextFloat(0.8f, 2f);
            dust.frame = new Rectangle(0, 0, 34, 36);
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            Color ret;
            if (dust.alpha < 80)
                ret = Color.Lerp(Color.White, new Color(0, 255, 255), dust.alpha / 80f);
            else if (dust.alpha < 140)
                ret = Color.Lerp(new Color(0, 255, 255), new Color(0, 155, 185), (dust.alpha - 80) / 80f);
            else
                ret = new Color(0, 155, 185);

            return ret * ((255 - dust.alpha) / 255f);
        }

        public override bool Update(Dust dust)
        {
            if (dust.velocity.Length() > 3)
                dust.velocity *= 0.85f;
            else
                dust.velocity *= 0.92f;

            if (dust.alpha > 100)
            {
                dust.scale += 0.01f;
                dust.alpha += 2;
            }
            else
            {
                Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.1f);
                dust.scale *= 0.985f;
                dust.alpha += 4;
            }
            dust.position += dust.velocity;
            if (dust.alpha >= 255)
                dust.active = false;

            return false;
        }
    }

    public class LightBlueSmokeScaleDown : ModDust
    {
        public override string Texture => AssetDirectory.Dust + "NeedlerDust";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.scale *= Main.rand.NextFloat(0.8f, 2f);
            dust.frame = new Rectangle(0, 0, 34, 36);
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            Color ret;
            if (dust.alpha < 80)
                ret = Color.Lerp(Color.White, new Color(0, 255, 255), dust.alpha / 80f);
            else if (dust.alpha < 140)
                ret = Color.Lerp(new Color(0, 255, 255), new Color(0, 155, 185), (dust.alpha - 80) / 80f);
            else
                ret = new Color(0, 155, 185);

            return ret * ((255 - dust.alpha) / 255f);
        }

        public override bool Update(Dust dust)
        {
            if (dust.velocity.Length() > 3)
                dust.velocity *= 0.85f;
            else
                dust.velocity *= 0.92f;

            if (dust.alpha > 100)
            {
                dust.scale *= 0.975f;
                dust.alpha += 2;
            }
            else
            {
                Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.1f);
                dust.scale *= 0.985f;
                dust.alpha += 4;
            }
            dust.position += dust.velocity;

            if (dust.alpha >= 255)
                dust.active = false;

            return false;
        }
    }

    public class LightBlueSmokeQuickFade : ModDust
    {
        public override string Texture => AssetDirectory.Dust + "NeedlerDust";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.scale *= Main.rand.NextFloat(0.8f, 2f);
            dust.frame = new Rectangle(0, 0, 34, 36);
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            Color ret;
            if (dust.alpha < 40)
                ret = Color.Lerp(Color.White, new Color(0, 255, 255), dust.alpha / 40f);
            else if (dust.alpha < 80)
                ret = Color.Lerp(new Color(0, 255, 255), new Color(0, 155, 185), (dust.alpha - 40) / 40f);
            else
                ret = new Color(0, 155, 185);

            return ret * ((255 - dust.alpha) / 255f);
        }

        public override bool Update(Dust dust)
        {
            if (dust.velocity.Length() > 3)
                dust.velocity *= 0.85f;
            else
                dust.velocity *= 0.92f;

            if (dust.alpha > 60)
            {
                dust.scale += 0.01f;
                dust.alpha += 6;
            }
            else
            {
                Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.1f);
                dust.scale *= 0.985f;
                dust.alpha += 4;
            }

            dust.position += dust.velocity;

            if (dust.alpha >= 255)
                dust.active = false;

            return false;
        }
    }
}
