using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Dusts
{
    internal class OvergrowDust : ModDust
    {
        //private readonly int time = 0;

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Dust + "PlayerFollowOrange";
            return true;
        }

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = true;
            dust.customData = 550;
            dust.scale = 1.8f;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }

        public override bool Update(Dust dust)
        {
            if (dust.customData is int)
            {
                dust.customData = (int)dust.customData - 1;
                if ((int)dust.customData == 0) dust.active = false;
                if ((int)dust.customData >= 100)
                {
                    if (dust.color.R < 100) dust.color *= 1.53f;
                    dust.scale *= 1.025f;
                }
                else
                {
                    dust.color *= 0.94f;
                    dust.scale *= 0.99f;
                }

                dust.rotation = StarlightWorld.rottime;
                dust.position.X += (float)Math.Sin(-dust.scale * 3);
                dust.position.Y += (float)Math.Cos(-dust.scale * 3);
            }
            return true;
        }
    }
}