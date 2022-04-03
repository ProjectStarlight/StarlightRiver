using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Dusts;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.GameContent.Dyes;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Microsoft.Xna.Framework;
using Terraria.UI;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Graphics;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Items.Geomancer
{
    class GeoRainbowDust : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Assets/Keys/GlowVerySoft";
            return true;
        }

        public override void OnSpawn(Dust dust)
        {
            dust.color = Color.Transparent;
            dust.fadeIn = 0;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 64, 64);
            dust.velocity *= 2;
            dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.Assets.Request<Effect>("Effects/GlowingDust").Value), "GlowingDustPass");
            dust.alpha = Main.rand.Next(100);
        }

        public override bool Update(Dust dust)
        {
            if (dust.color == Color.Transparent)
                dust.position -= Vector2.One * 32 * dust.scale;

            //dust.rotation += dust.velocity.Y * 0.1f;
            dust.position += dust.velocity;

            dust.velocity *= 0.98f;

            dust.color = Main.hslToRgb(((dust.alpha / 100f) + (Main.GlobalTime * 0.4f)) % 1f, 1, 0.5f);
            dust.shader.UseColor(dust.color);

            dust.fadeIn++;

            dust.scale *= 0.96f;

            Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.4f * dust.scale);

            if (dust.fadeIn > 70)
                dust.active = false;

            return false;
        }
    }
}