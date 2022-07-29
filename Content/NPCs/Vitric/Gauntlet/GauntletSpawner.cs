using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace StarlightRiver.Content.NPCs.Vitric.Gauntlet
{
    public class GauntletSpawner : ModProjectile
    {
        public ref float NPCType => ref Projectile.ai[0];
        public ref float Timer => ref Projectile.ai[1];

        public override string Texture => AssetDirectory.Invisible;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            Timer++;

            if (Timer < 50)
            {
                Vector2 cinderPos = Projectile.Top + Main.rand.NextVector2Circular(95, 95);
                Dust cinder = Dust.NewDustPerfect(cinderPos, ModContent.DustType<Dusts.Cinder>(), -Vector2.UnitY.RotatedBy(cinderPos.AngleTo(Projectile.Center)) * Main.rand.NextFloat(-2, 2), 0, Bosses.GlassMiniboss.Glassweaver.GlowDustOrange, 1f);
                cinder.customData = Projectile.Center + Main.rand.NextVector2Circular(40, 40);
            }

            if (Timer > 70)
            {
                NPC.NewNPC(Entity.GetSource_Misc("SLR:GlassGauntlet"), (int)Projectile.Center.X, (int)Projectile.Center.Y, (int)NPCType);
                Projectile.Kill();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var tex = Terraria.GameContent.TextureAssets.Npc[(int)NPCType].Value;
            var fakeNPC = new NPC();
            fakeNPC.SetDefaults((int)NPCType);
            fakeNPC.FindFrame();

            var effect = Terraria.Graphics.Effects.Filters.Scene["MoltenForm"].GetShader().Shader;
            effect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Bosses/VitricBoss/ShieldMap").Value);
            effect.Parameters["uTime"].SetValue(Timer / 70f * 2);
            effect.Parameters["sourceFrame"].SetValue(new Vector4(0, 0, fakeNPC.frame.Width, fakeNPC.frame.Height));
            effect.Parameters["texSize"].SetValue(tex.Size());

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(default, BlendState.NonPremultiplied, default, default, default, effect, Main.GameViewMatrix.ZoomMatrix);

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, fakeNPC.frame, Color.White, 0, fakeNPC.frame.Size() / 2, 1, 0, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }
}
