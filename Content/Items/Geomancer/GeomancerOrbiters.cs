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
    public abstract class GeoProj : ModProjectile
    {
        protected const float bigScale = 1.4f;
        protected const int STARTOFFSET = 45;
        protected float offsetLerper = 1;

        protected float glowCounter;

        protected float whiteCounter;

        protected bool released = false;
        protected float releaseCounter = 0;
        protected float extraSpin = 0f;
        public override void SetDefaults()
        {
            projectile.friendly = false;
            projectile.magic = true;
            projectile.tileCollide = false;
            projectile.Size = new Vector2(16, 16);
            projectile.penetrate = -1;
        }
        public override Color? GetAlpha(Color lightColor) => Color.White;
        public override void AI()
        {
            if (projectile.scale == bigScale)
                glowCounter += 0.02f;

            SafeAI();
            projectile.rotation = 0f;
            projectile.Center = Main.player[projectile.owner].Center + ((projectile.ai[0] + (Main.GlobalTime * 0.5f) + extraSpin).ToRotationVector2() * MathHelper.Lerp(0, STARTOFFSET, EaseFunction.EaseCubicOut.Ease(offsetLerper)));

            GeomancerPlayer modPlayer = Main.player[projectile.owner].GetModPlayer<GeomancerPlayer>();
            projectile.timeLeft = 2;
            if ((modPlayer.DiamondStored && modPlayer.RubyStored && modPlayer.EmeraldStored && modPlayer.SapphireStored && modPlayer.TopazStored && modPlayer.AmethystStored) || released)
            {
                if (whiteCounter < 1)
                    whiteCounter += 0.007f;

                releaseCounter += 0.01f;
                extraSpin += Math.Min(releaseCounter, 0.15f);
                released = true;
                if (releaseCounter > 0.5f)
                {
                    offsetLerper -= 0.015f;
                }
                if (offsetLerper <= 0)
                {
                    projectile.active = false;
                    modPlayer.AmethystStored = false;
                    modPlayer.TopazStored = false;
                    modPlayer.EmeraldStored = false;
                    modPlayer.SapphireStored = false;
                    modPlayer.RubyStored = false;
                    modPlayer.DiamondStored = false;

                    modPlayer.storedGem = StoredGem.All;
                    modPlayer.allTimer = 300;

                    for (int i = 0; i < 3; i++)
                    {
                        float angle = Main.rand.NextFloat(6.28f);
                        Dust dust = Dust.NewDustPerfect(Main.player[projectile.owner].Center + (angle.ToRotationVector2() * 20), ModContent.DustType<GeoRainbowDust>());
                        dust.scale = 1f;
                        dust.velocity = angle.ToRotationVector2() * Main.rand.NextFloat() * 4;
                    }

                }
            }
            if (!modPlayer.SetBonusActive)
                projectile.active = false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = Main.projectileTexture[projectile.type];
            if (projectile.scale == bigScale)
            {
                float progress = glowCounter % 1;
                float transparency = (float)Math.Pow(1 - progress, 2);
                float scale = 0.95f + progress;

                spriteBatch.Draw(tex, projectile.Center - Main.screenPosition + new Vector2(0, projectile.gfxOffY), null, Color.White * transparency, projectile.rotation, tex.Size() / 2, scale * projectile.scale, SpriteEffects.None, 0f);
            }
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition + new Vector2(0, projectile.gfxOffY), null, Color.White, projectile.rotation, tex.Size() / 2, projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (!released)
                return;
            Texture2D tex = ModContent.GetTexture(Texture + "_White");
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition + new Vector2(0, projectile.gfxOffY), null, Color.White * whiteCounter, projectile.rotation, tex.Size() / 2, projectile.scale, SpriteEffects.None, 0f);
        }

        protected virtual void SafeAI() { }
    }

    public class GeoAmethystProj : GeoProj
    {
        public override string Texture => AssetDirectory.GeomancerItem + "GeoAmethyst";

        protected override void SafeAI()
        {
            GeomancerPlayer modPlayer = Main.player[projectile.owner].GetModPlayer<GeomancerPlayer>();
            if (modPlayer.storedGem == StoredGem.Amethyst && !released)
            {
                projectile.scale = bigScale;
            }
            else
            {
                projectile.scale = 1;
            }
        }
    }

    public class GeoRubyProj : GeoProj
    {
        public override string Texture => AssetDirectory.GeomancerItem + "GeoRuby";

        protected override void SafeAI()
        {
            GeomancerPlayer modPlayer = Main.player[projectile.owner].GetModPlayer<GeomancerPlayer>();
            if (modPlayer.storedGem == StoredGem.Ruby && !released)
            {
                projectile.scale = bigScale;
            }
            else
            {
                projectile.scale = 1;
            }
        }
    }
    public class GeoSapphireProj : GeoProj
    {
        public override string Texture => AssetDirectory.GeomancerItem + "GeoSapphire";

        protected override void SafeAI()
        {
            GeomancerPlayer modPlayer = Main.player[projectile.owner].GetModPlayer<GeomancerPlayer>();
            if (modPlayer.storedGem == StoredGem.Sapphire && !released)
            {
                projectile.scale = bigScale;
            }
            else
            {
                projectile.scale = 1;
            }
        }
    }
    public class GeoEmeraldProj : GeoProj
    {
        public override string Texture => AssetDirectory.GeomancerItem + "GeoEmerald";

        protected override void SafeAI()
        {
            GeomancerPlayer modPlayer = Main.player[projectile.owner].GetModPlayer<GeomancerPlayer>();
            if (modPlayer.storedGem == StoredGem.Emerald && !released)
            {
                projectile.scale = bigScale;
            }
            else
            {
                projectile.scale = 1;
            }
        }
    }

    public class GeoTopazProj : GeoProj
    {
        public override string Texture => AssetDirectory.GeomancerItem + "GeoTopaz";

        protected override void SafeAI()
        {
            GeomancerPlayer modPlayer = Main.player[projectile.owner].GetModPlayer<GeomancerPlayer>();
            if (modPlayer.storedGem == StoredGem.Topaz && !released)
            {
                projectile.scale = bigScale;
            }
            else
            {
                projectile.scale = 1;
            }
        }
    }

    public class GeoDiamondProj : GeoProj
    {
        public override string Texture => AssetDirectory.GeomancerItem + "GeoDiamond";

        protected override void SafeAI()
        {
            GeomancerPlayer modPlayer = Main.player[projectile.owner].GetModPlayer<GeomancerPlayer>();
            if (modPlayer.storedGem == StoredGem.Diamond && !released)
            {
                projectile.scale = bigScale;
            }
            else
            {
                projectile.scale = 1;
            }
        }
    }
}