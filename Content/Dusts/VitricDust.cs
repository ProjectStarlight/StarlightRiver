using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Dusts
{
    public class Glass : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
        }

        public override bool Update(Dust dust)
        {
            if (dust.customData is Player)
            {
                Vector2 origin = ((Player)dust.customData).Center;

                dust.position += dust.velocity;
                dust.position += Vector2.Normalize(origin - dust.position) * 6;
                dust.velocity *= 0.94f;
                dust.rotation = (origin - dust.position).Length() * 0.1f;
                dust.scale *= 0.99f;
                if (Vector2.Distance(dust.position, origin) <= 5)
                {
                    dust.active = false;
                }
            }

            if (dust.customData is Vector2)
            {
                Vector2 origin = (Vector2)dust.customData;

                dust.position += dust.velocity;
                dust.position += Vector2.Normalize(origin - dust.position) * 6;
                dust.velocity *= 0.94f;
                dust.rotation = (origin - dust.position).Length() * 0.1f;
                dust.scale *= 0.99f;
                if (Vector2.Distance(dust.position, origin) <= 5)
                {
                    dust.active = false;
                }
            }
            return false;
        }
    }

    public class Glass2 : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Dusts/Glass";
            return base.Autoload(ref name, ref texture);
        }

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity.Y += 0.15f;
            dust.rotation += 0.1f;
            dust.scale *= 0.98f;
            if (dust.scale <= 0.2)
            {
                dust.active = false;
            }
            return false;
        }
    }

    public class Glass3 : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Dusts/Glass";
            return base.Autoload(ref name, ref texture);
        }

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.rotation += 0.12f;
            dust.scale *= 0.98f;
            if (dust.scale <= 0.2)
            {
                dust.active = false;
            }
            return false;
        }
    }
}