﻿using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Dusts
{
	public class JungleEnergy : ModDust
    {
        public override string Texture => AssetDirectory.Dust + Name;

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.color.R = 200;
            dust.color.G = 255;
            dust.color.B = 80;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color * ((255 - dust.alpha) / 255f);
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.rotation += 0.05f;
            dust.color.R--;

            if (dust.fadeIn == 0) dust.alpha -= 2;
            else dust.alpha += 2;

            if (dust.alpha <= 175) dust.fadeIn = 1;

            if (dust.alpha > 255)
                dust.active = false;
            return false;
        }
    }
}