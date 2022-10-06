using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;
using System;

namespace StarlightRiver.Content.Dusts.ArtifactSparkles
{
    abstract class ArtifactSparkle : ModDust
    {
        public override string Texture => AssetDirectory.ArtifactSparkles + Name;

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return Color.White;
        }

        public override void OnSpawn(Dust dust)
        {
            dust.fadeIn = 0;
            dust.noLight = false;
            dust.frame = new Rectangle(18 * Main.rand.Next(2), 0, 18, 18);
        }

        public override bool Update(Dust dust)
        {
            if (dust.customData is null)
            {
                dust.position -= new Vector2(dust.frame.Width / 2, dust.frame.Height / 2) * dust.scale;
                dust.customData = 1;
            }

            if (dust.frame.X == 18)
            {
                if (dust.alpha % 50 == 45)
                    dust.frame.Y += 18;
                dust.alpha += 5;
            }
            else
            {
                if (dust.alpha % 64 == 56)
                    dust.frame.Y += 18;
                dust.alpha += 8;
            }

            if (dust.alpha > 255)
                dust.active = false;

            dust.position += dust.velocity;
            return false;
        }
    }

    class GeodeArtifactSparkle : ModDust //These have a bit more frames so they need to be their own class
    {
        public override string Texture => AssetDirectory.ArtifactSparkles + Name;

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return Color.White;
        }

        public override void OnSpawn(Dust dust)
        {
            dust.fadeIn = 0;
            dust.noLight = false;
            dust.frame = new Rectangle(24 * Main.rand.Next(4), 0, 24, 24);
        }

        public override bool Update(Dust dust)
        {
            if (dust.customData is null)
            {
                dust.position -= new Vector2(dust.frame.Width / 2, dust.frame.Height / 2) * dust.scale;
                dust.customData = 1;
            }

            switch (dust.frame.X)
            {
                case 0:
                    if (dust.alpha % 50 == 45)
                        dust.frame.Y += 24;
                    dust.alpha += 5;
                    break;
                case 24:
                    if (dust.alpha % 64 == 56)
                        dust.frame.Y += 24;
                    dust.alpha += 8;
                    break;
                case 48:
                    if (dust.alpha % 42 == 36)
                        dust.frame.Y += 24;
                    dust.alpha += 6;
                    break;
                case 72:
                    if (dust.alpha % 50 == 45)
                        dust.frame.Y += 24;
                    dust.alpha += 5;
                    break;
            }

            if (dust.alpha > 255)
                dust.active = false;

            dust.position += dust.velocity;
            return false;
        }
    }

    class GoldArtifactSparkle : ArtifactSparkle { }

    class RedArtifactSparkle : ArtifactSparkle { }

    class LimeArtifactSparkle : ArtifactSparkle { }

    class WhiteArtifactSparkle : ArtifactSparkle { }
}
