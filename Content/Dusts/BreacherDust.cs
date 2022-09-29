﻿using StarlightRiver.Core;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Dusts
{
	public class BreacherDust : ModDust
    {
        public override string Texture => AssetDirectory.Dust + Name;
        public override void OnSpawn(Dust dust)
        {
            dust.noLight = false;
        }
        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.scale *= 0.98f;
            dust.velocity *= 0.98f;
            dust.alpha += 10;
            if (dust.alpha >= 255)
                dust.active = false;
            Color color = new Color(255, 50, 180);
            Lighting.AddLight(dust.position, color.ToVector3() * 0.1f);
            return false;
        }
    }
}