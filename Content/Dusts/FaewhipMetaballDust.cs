using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Dusts
{
    public class FaewhipMetaballDust : Glow
    {
        public override bool Update(Dust dust)
        {
            Vector2 posToBe = (Vector2)dust.customData;
            dust.shader.UseColor(dust.color);

            if ((posToBe - dust.position).Length() < 5)
            {
                dust.position = posToBe;
                dust.velocity = Vector2.Zero;
                return false;
            }
            
            Vector2 direction = dust.position.DirectionTo(posToBe);
            dust.velocity = Vector2.Lerp(dust.velocity, direction * 4, 0.05f);
            dust.position += dust.velocity;
            if ((posToBe - dust.position).Length() < 5)
                dust.position = posToBe;
            return false;
        }
    }
}