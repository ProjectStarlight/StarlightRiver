using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Dusts
{
	public class CoachGunDust : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Dust + "NeedlerDust";
            return true;
        }
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.scale *= Main.rand.NextFloat(0.8f, 2f);
            dust.frame = new Rectangle(0, 0, 34, 36);
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            Color gray = new Color(25, 25, 25);
            Color ret;
            if (dust.alpha < 80)
            {
                ret = Color.Lerp(Color.Yellow, Color.Orange, dust.alpha / 80f);
            }
            else if (dust.alpha < 140)
            {
                ret = Color.Lerp(Color.Orange, gray, (dust.alpha - 80) / 80f);
            }
            else
                ret = gray;
            return ret * ((255 - dust.alpha) / 255f);
        }

        public override bool Update(Dust dust)
        {
            if (Math.Abs(dust.velocity.X) > 3)
                dust.velocity.X *= 0.85f;
            else
                dust.velocity.X *= 0.92f;

            if (dust.velocity.Y > -2)
                dust.velocity.Y -= 0.1f;
            else
                dust.velocity.Y *= 0.92f;

            if (dust.velocity.Y > 0)
                dust.velocity.Y *= 0.85f;
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

    public class CoachGunDustTwo : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Dust + "NeedlerDustTwo";
            return true;
        }

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.scale *= Main.rand.NextFloat(0.8f, 2f);
            dust.frame = new Rectangle(0, 0, 34, 36);
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            Color gray = new Color(25, 25, 25);
            Color ret;
            if (dust.alpha < 80)
            {
                ret = Color.Lerp(Color.Yellow, Color.Orange, dust.alpha / 80f);
            }
            else if (dust.alpha < 140)
            {
                ret = Color.Lerp(Color.Orange, gray, (dust.alpha - 80) / 80f);
            }
            else
                ret = gray;
            return ret * ((255 - dust.alpha) / 255f);
        }

        public override bool Update(Dust dust)
        {
            if (Math.Abs(dust.velocity.X) > 3)
                dust.velocity.X *= 0.85f;
            else
                dust.velocity.X *= 0.92f;

            if (dust.velocity.Y > -2)
                dust.velocity.Y -= 0.1f;
            else
                dust.velocity.Y *= 0.92f;

            if (dust.velocity.Y > 3.5f)
                dust.velocity.Y = 3.5f;

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

    public class CoachGunDustThree : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Dust + "NeedlerDustThree";
            return true;
        }

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.scale *= Main.rand.NextFloat(0.8f, 2f);
            dust.frame = new Rectangle(0, 0, 34, 36);
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            Color gray = new Color(25, 25, 25);
            Color ret;
            if (dust.alpha < 40)
                ret = Color.Lerp(Color.Yellow, Color.Orange, dust.alpha / 40f);
            else if (dust.alpha < 80)
                ret = Color.Lerp(Color.Orange, gray, (dust.alpha - 40) / 40f);
            else
                ret = gray;

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

    class CoachGunDustFour : ModDust
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
            dust.velocity *= 2;
            dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.GetEffect("Effects/GlowingDust")), "GlowingDustPass");
        }

        public override bool Update(Dust dust)
        {
            if (dust.color == Color.Transparent)
                dust.position -= Vector2.One * 32 * dust.scale;

            //dust.rotation += dust.velocity.Y * 0.1f;
            dust.position += dust.velocity;
            dust.velocity *= 0.95f;
            dust.shader.UseColor(dust.color);
            dust.scale *= 0.97f;
            dust.fadeIn++;

            Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.6f);

            if (dust.fadeIn > 40)
                dust.active = false;

            return false;
        }
    }

    class CoachGunSparks : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Assets/Keys/GlowVerySoft";
            return true;
        }

        public override void OnSpawn(Dust dust)
        {
            dust.noLight = false;
            dust.noGravity = true;
            dust.scale *= 0.15f;
            dust.frame = new Rectangle(0, 0, 64, 64);
            dust.color = Color.Lerp(Color.Red, Color.Yellow, Main.rand.NextFloat());
            dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.GetEffect("Effects/GlowingDust")), "GlowingDustPass");
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return Color.Lerp(dust.color, Color.Orange, dust.fadeIn / 45f);
        }

        public override bool Update(Dust dust)
        {
            dust.shader.UseColor(dust.color);
            dust.position += dust.velocity;
            dust.velocity.Y += 0.03f;
            dust.scale *= 0.97f;

            dust.fadeIn++;

            Lighting.AddLight(dust.position, dust.color.ToVector3() * dust.scale);

            if (dust.fadeIn > 45)
                dust.active = false;
            return false;
        }
    }

    public class CoachGunDustFive : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Dust + "NeedlerDust";
            return true;
        }
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.scale *= Main.rand.NextFloat(0.8f, 2f);
            dust.frame = new Rectangle(0, 0, 34, 36);
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            Color gray = new Color(25, 25, 25);
            Color ret;
            if (dust.alpha < 80)
            {
                ret = Color.Lerp(Color.Yellow, Color.Orange, dust.alpha / 80f);
            }
            else if (dust.alpha < 140)
            {
                ret = Color.Lerp(Color.Orange, gray, (dust.alpha - 80) / 80f);
            }
            else
                ret = gray;
            return ret * ((255 - dust.alpha) / 255f);
        }

        public override bool Update(Dust dust)
        {
            if (Math.Abs(dust.velocity.X) > 7)
                dust.velocity.X *= 0.85f;
            else
                dust.velocity.X *= 0.92f;

            if (dust.velocity.Y > -2)
                dust.velocity.Y -= 0.1f;

            if (dust.velocity.Y > 3.5f)
                dust.velocity.Y = 3.5f;

            else
                dust.velocity.Y *= 0.92f;

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
    public class CoachSmoke : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Dust + "NeedlerDust";
            return true;
        }
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.scale *= Main.rand.NextFloat(0.8f, 2f);
            dust.frame = new Rectangle(0, 0, 34, 36);
            dust.rotation = Main.rand.NextFloat(6.28f);
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }

        public override bool Update(Dust dust)
        {
            dust.velocity *= 0.98f;
            dust.velocity.X *= 0.95f;

            if (dust.velocity.Y > -2)
                dust.velocity.Y -= 0.1f;

            dust.color *= 0.98f;

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
            dust.rotation += 0.01f;

            if (dust.alpha >= 255)
                dust.active = false;

            return false;
        }
    }
}