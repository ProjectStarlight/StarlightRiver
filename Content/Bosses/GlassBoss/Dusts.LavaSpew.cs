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
    class LavaSpew : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.GlassBoss + name;
            return true;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return Color.White;
        }

        public override void OnSpawn(Dust dust)
        {
            dust.fadeIn = 0;
            dust.noLight = false;
            dust.frame = new Rectangle(0, Main.rand.NextBool() ? 0 : 72, 80, 72);
        }

        public override bool Update(Dust dust)
        {
            if(dust.fadeIn == 0)
			{
                dust.rotation = dust.velocity.ToRotation() + MathHelper.PiOver2;
                dust.position -= new Vector2(40, 36).RotatedBy(dust.rotation);
            }

            dust.frame.X = (int)(dust.fadeIn / 30f * 8) * 80;

            dust.fadeIn++;

            Lighting.AddLight(dust.position, new Vector3(1, 0.6f, 0.1f) * 0.8f);

            if (dust.fadeIn > 30)
                dust.active = false;
            return false;
        }
    }
}
