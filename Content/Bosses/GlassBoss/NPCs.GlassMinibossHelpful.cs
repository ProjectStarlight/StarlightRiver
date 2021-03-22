using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Bosses.GlassBoss
{
    class GlassMinibossHelpful : ModNPC, IDrawAdditive
    {
        public VitricBoss parent;
        private Vector2 savedPos;

        public override string Texture => AssetDirectory.GlassBoss + Name;

        private ref float Timer => ref npc.ai[0];

        public override void SetDefaults()
        {
            npc.width = 118;
            npc.height = 152;
            npc.friendly = true;
            npc.dontTakeDamage = true;
            npc.dontTakeDamageFromHostiles = true;
            npc.immortal = true;
            npc.aiStyle = -1;
            npc.lifeMax = 1000;
            npc.noGravity = true;
        }

        public override void AI()
        {
            Timer++;
            
            if (parent is null && Timer != 1)
                npc.active = false;

            if (Timer == 1)
            {
                npc.frame = new Rectangle(0, 0, npc.width, npc.height);
                savedPos = npc.Center;
            }

            if (Timer < 40)
            {
                npc.Center = Vector2.SmoothStep(savedPos, parent.npc.Center + new Vector2(-50, -50), Timer / 40f);
                npc.frame.Y = (int)((Timer / 40f) * 9) * npc.height;
            }

            if(Timer == 75)
            {
                npc.frame.Y += npc.height;
                Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 40;
                Main.PlaySound(Terraria.ID.SoundID.Shatter, npc.Center);
                Main.PlaySound(Terraria.ID.SoundID.DD2_ExplosiveTrapExplode, npc.Center);                
            }

            if (Timer == 100)
            {
                npc.frame.Y += npc.height;
                savedPos = npc.Center;

                for (int k = 0; k < 100; k++)
                {
                    Dust d = Dust.NewDustPerfect(parent.npc.Center, ModContent.DustType<Dusts.GlassAttracted>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(20, 40), 0, default, Main.rand.NextFloat(1, 5));
                    d.customData = savedPos + new Vector2(400, 400);
                }
            }

            if (Timer == 120)
            {
                npc.frame.X += npc.width;
                npc.frame.Y = 0;
            }

            if (Timer > 120 && Timer < 140)
            {
                npc.frame.Y = (int)(( (Timer - 120) / 20f) * 5) * npc.height;
            }

            if(Timer == 140)
            {
                npc.Center = npc.Center + new Vector2(400, 400);
                npc.frame.X += npc.width;
                npc.frame.Y = 0;

                Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<PlayerShield>(), 0, 0);
            }

            if (Timer > 140 && Timer < 180)
            {
                npc.frame.Y = (int)(((Timer - 140) / 40f) * 17) * npc.height;
            }

            if (Timer > 800)
                npc.active = false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            var tex = ModContent.GetTexture(Texture);
            spriteBatch.Draw(tex, npc.Center - Main.screenPosition, npc.frame, Color.White, 0, npc.Size / 2, 1, 0, 0);

            return false;
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            var tex = ModContent.GetTexture("StarlightRiver/Assets/Keys/Glow");

            if (Timer > 75 && Timer < 90)
            {
                float progress = (Timer - 75) / 15f;
                float progress2 = 1 - progress;
                spriteBatch.Draw(tex, npc.Center + new Vector2(10, 10) - Main.screenPosition, null, Color.Yellow * progress2, 0, tex.Size() / 2, 2 + progress, default, default);
            }
        }
    }
}
