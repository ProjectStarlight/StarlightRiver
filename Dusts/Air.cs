using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Dusts
{
    public class Air : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= 0.3f;
            dust.noGravity = true;
            dust.noLight = false;
            dust.scale *= 1.4f;
            dust.color.R = 160;
            dust.color.G = 235;
            dust.color.B = 255;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color * (1 - dust.fadeIn);
        }

        public override bool Update(Dust dust)
        {
            dust.position.Y += dust.velocity.Y * 2;
            dust.velocity.Y += 0.01f;
            dust.position.X += dust.velocity.X * 2;
            dust.rotation += 0.06f;

            dust.scale *= 0.97f;
            dust.color *= 0.995f;

            if (dust.scale < 0.4f)
            {
                dust.active = false;
            }
            return false;
        }
    }

    public class Air2 : ModDust
    {
        private int timer = 0;

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Dusts/Air";
            return base.Autoload(ref name, ref texture);
        }

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            timer = 30;
            dust.color.R = 170;
            dust.color.G = 235;
            dust.color.B = 255;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            if (dust.customData is Player player)
            {
                return dust.color * (1 - Vector2.Distance(dust.position, player.Center) / 50f);
            }
            return dust.color;
        }

        public override bool Update(Dust dust)
        {
            Player player;
            if (dust.customData is Player)
            {
                player = (Player)dust.customData;

                dust.rotation = Vector2.Distance(dust.position, player.Center) * 0.1f;
                dust.position += dust.velocity;

                dust.velocity = Vector2.Normalize(dust.position - player.Center) * -4;
                dust.scale *= 0.95f;
                timer--;
                if (timer == 0 || Vector2.Distance(dust.position, player.Center) < 1)
                {
                    dust.active = false;
                }
            }
            else
            {
                dust.velocity *= 0.95f;
            }
            return false;
        }
    }

    public class Air3 : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Dusts/Air";
            return base.Autoload(ref name, ref texture);
        }

        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= 0.3f;
            dust.noGravity = true;
            dust.noLight = false;
            dust.scale *= 1.4f;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }

        public override bool Update(Dust dust)
        {
            dust.position.Y += dust.velocity.Y * 2;
            dust.velocity.Y += 0.01f;
            dust.position.X += dust.velocity.X * 2;
            dust.rotation += 0.05f;

            dust.scale *= 0.97f;

            if (dust.scale < 0.4f)
            {
                dust.active = false;
            }
            return false;
        }
    }

    public class Air4 : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Dusts/Air";
            return base.Autoload(ref name, ref texture);
        }

        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= 0.3f;
            dust.noGravity = true;
            dust.noLight = false;
            dust.scale *= 0.8f;
            dust.color.R = 170;
            dust.color.G = 235;
            dust.color.B = 255;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }

        public override bool Update(Dust dust)
        {
            dust.rotation += 0.05f;
            dust.color *= 0.99f;

            dust.scale *= 0.98f;

            if (dust.scale < 0.4f)
            {
                dust.active = false;
            }
            return false;
        }
    }

    public class AirDash : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Dusts/Air";
            return base.Autoload(ref name, ref texture);
        }

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.fadeIn <= 0 ? new Color(170, 255, 255) * (dust.alpha / 255f) : Color.Transparent;
        }

        public override bool Update(Dust dust)
        {
            dust.fadeIn -= 3;
            dust.scale = 2f - Math.Abs(dust.fadeIn) / 30f;
            dust.alpha = 150 - (int)(Math.Abs(dust.fadeIn) / 60f * 150);

            if (dust.fadeIn <= -60) dust.active = false;
            return false;
        }
    }

    public class Gold : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Dusts/Gold";
            return base.Autoload(ref name, ref texture);
        }

        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= 0.3f;
            dust.noGravity = true;
            dust.noLight = false;
            dust.scale *= 3f;
            dust.color.R = 255;
            dust.color.G = 220;
            dust.color.B = 100;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color * ((255 - dust.alpha) / 255f);
        }

        public override bool Update(Dust dust)
        {
            dust.rotation += 0.05f;

            dust.scale *= 0.97f;
            if (dust.scale < 0.2f)
            {
                dust.active = false;
            }
            return false;
        }
    }

    public class Gold2 : Gold
    {
        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.rotation += 0.05f;

            dust.scale *= 0.92f;
            if (dust.scale < 0.3f)
            {
                dust.active = false;
            }
            return false;
        }
    }

    public class Gold3 : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Dusts/Gold";
            return base.Autoload(ref name, ref texture);
        }

        private int timer = 0;

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            timer = 30;
            dust.scale *= 2;
            dust.color.R = 255;
            dust.color.G = 220;
            dust.color.B = 100;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }

        public override bool Update(Dust dust)
        {
            Player player = Main.LocalPlayer;
            dust.rotation = Vector2.Distance(dust.position, player.Center) * 0.1f;

            if (dust.customData is int) { dust.customData = (int)dust.customData - 1; }
            dust.position += dust.velocity;

            float rot = dust.velocity.ToRotation();

            if (dust.customData != null && (int)dust.customData <= 0)
            {
                rot += (float)(Math.PI * 2) / (20 * 18);
                if (rot >= (float)Math.PI * 2)
                {
                    rot = 0;
                }

                Vector2 offset = (player.Center - dust.position);
                dust.position.X = player.Center.X + (float)Math.Cos(rot) * offset.Length();
                dust.position.Y = player.Center.Y + (float)Math.Sin(rot) * offset.Length();

                dust.velocity = Vector2.Normalize(dust.position - player.Center) * -3.9f;

                dust.scale *= 0.97f;
                timer--;

                if (timer == 0 || dust.scale <= 0.31f)
                {
                    dust.active = false;
                }
            }
            else
            {
                dust.velocity *= 0.95f;
            }
            return false;
        }
    }

    public class Gold4 : Gold
    {
        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.fadeIn++;

            dust.scale *= 0.999f;
            dust.alpha = 155 + (int)(dust.fadeIn > 300 ? (dust.fadeIn - 300) / 300 * 100 : (300 - dust.fadeIn) / 300 * 100); 

            if (dust.fadeIn > 600) dust.active = false;

            return false;
        }
    }

    public class Void : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Dusts/Void";
            return base.Autoload(ref name, ref texture);
        }

        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= 0.1f;
            dust.noGravity = true;
            dust.noLight = false;
            dust.scale *= 3f;
            dust.color.R = 130;
            dust.color.G = 20;
            dust.color.B = 235;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.rotation += 0.05f;

            dust.scale *= 0.94f;

            if (dust.scale < 0.2f)
            {
                dust.active = false;
            }
            return false;
        }
    }

    public class Void2 : Void
    {
        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= 0.1f;
            dust.velocity += new Vector2(Main.rand.Next(-100, 100) * 0.1f, Main.rand.Next(-100, 100) * 0.1f);
            dust.noGravity = true;
            dust.noLight = false;
            dust.scale *= 3f;
            dust.color.R = 130;
            dust.color.G = 20;
            dust.color.B = 235;
        }

        public override bool Update(Dust dust)
        {
            Player player = Main.LocalPlayer;
            dust.velocity = (player.Center - dust.position) / 30;
            dust.position += dust.velocity;
            dust.rotation += 0.05f;

            dust.scale *= 0.98f;

            if (dust.scale < 0.2f)
            {
                dust.active = false;
            }
            return false;
        }
    }

    public class Void3 : Void
    {
        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.rotation += 0.05f;

            dust.scale *= 0.97f;

            if (dust.scale < 1.3f)
            {
                dust.active = false;
            }
            return false;
        }
    }

    public class Void4 : Void
    {
        public override bool Update(Dust dust)
        {
            if (dust.customData is Player)
            {
                dust.position += (dust.customData as Player).velocity + dust.velocity;
                dust.rotation += 0.05f;
            }

            dust.scale *= 0.95f;

            if (dust.scale < 0.1f) { dust.active = false; }
            if (dust.scale < 0.5f) { dust.velocity *= -1.1f; }

            return false;
        }
    }
}