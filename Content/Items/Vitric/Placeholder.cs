using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace StarlightRiver.Content.Items.Vitric
{
    public class VitricSummonOrb
    {
        public static Color MoltenGlow(float time)
        {
            Color MoltenGlowc = Color.White;
            if (time > 30 && time < 60)
                MoltenGlowc = Color.Lerp(Color.White, Color.Orange, Math.Min((time - 30f) / 20f, 1f));
            else if (time >= 60)
                MoltenGlowc = Color.Lerp(Color.Orange, Color.Lerp(Color.Red, Color.Transparent, Math.Min((time - 60f) / 50f, 1f)), Math.Min((time - 60f) / 30f, 1f));
            return MoltenGlowc;
        }

        public static Rectangle WhiteFrame(Rectangle tex, bool white)
        {
            return new Rectangle(white ? tex.Width / 2 : 0, tex.Y, tex.Width / 2, tex.Height);
        }
    }
}

