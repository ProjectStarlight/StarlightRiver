using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc.SoilgunFiles
{
    //these could be moved to actual dust folder idk
    public class MoltenGlassGravity : ModDust
    {
        //idk why but the sprite exists with no actual dust so
        public override string Texture => AssetDirectory.Dust + "GlassMolten";

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
                dust.active = false;
            return false;
        }
    }

    public class SandNoGravity : ModDust
    {
        public override string Texture => AssetDirectory.Dust + "Sand";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = true;
            dust.scale *= 6;
            dust.frame = new Rectangle(0, 0, 10, 10);
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }

        public override bool Update(Dust dust)
        {
            dust.color = Lighting.GetColor((int)(dust.position.X / 16), (int)(dust.position.Y / 16)).MultiplyRGB(Color.White) * 0.2f * (dust.alpha / 255f);
            dust.position += dust.velocity;
            dust.scale *= 0.9745f;
            dust.velocity *= 0.97f;
            dust.rotation += 0.1f;

            if (dust.scale <= 0.2f)
                dust.active = false;
            return false;
        }
    }
    //this should very much probably be moved to the Dust folder
    public class VitricSandDust : ModDust
    {
        public override string Texture => AssetDirectory.Dust + Name;

        public override void OnSpawn(Dust dust)
        {
            UpdateType = DustID.Sand;
        }
    }
}
