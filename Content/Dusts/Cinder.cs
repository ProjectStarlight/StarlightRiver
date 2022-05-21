using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Dusts
{
    //make this a particle at some point?
	public class Cinder : ModDust
    {
        public override string Texture => AssetDirectory.Dust + Name;

        public override void OnSpawn(Dust dust)
        {
            dust.frame = new Rectangle(0, 0, 74, 74);
        }

        public override Color? GetAlpha(Dust dust, Color lightColor) => new Color(dust.color.R, dust.color.G, dust.color.B, 0);

        public override bool Update(Dust dust)
        {
            dust.scale *= 0.92f;
            if (dust.scale < 0.02f)
                dust.active = false;

            if (dust.noGravity)
            {
                dust.velocity.Y -= (float)Math.Pow(Math.Sin(dust.scale), 2);
                if (Main.rand.NextBool(4))
                    dust.velocity.X += (float)Math.Cos(dust.velocity.Length());
            }
            else if (Collision.SolidTiles(dust.position, 1, 1))
                dust.velocity.Y += 0.1f;
            else
                dust.velocity *= 0.1f;
            return false;
        }
    }
}