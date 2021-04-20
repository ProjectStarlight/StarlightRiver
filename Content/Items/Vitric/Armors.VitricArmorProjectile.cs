using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Buffs.Summon;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using StarlightRiver.Core;
using Terraria.DataStructures;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Items.Vitric
{
    internal class VitricArmorProjectile : ModProjectile
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetDefaults()
        {
            projectile.width = 12;
            projectile.height = 18;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 30;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Enchanted Glass");
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Main.PlaySound(SoundID.Item27);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;

        public override bool CanDamage() => false;

        public override void ReceiveExtraAI(System.IO.BinaryReader reader)
        {
            projectile.localAI[0] = (float)reader.ReadDouble();
        }

        public override void SendExtraAI(System.IO.BinaryWriter writer)
        {
            writer.Write((double)projectile.localAI[0]);
        }

        public bool Regenerate()
        {
            if (projectile.ai[0] == 1)
            {
                Main.PlaySound(SoundID.Item, (int)projectile.Center.X, (int)projectile.Center.Y, 30, 0.65f, Main.rand.NextFloat(0.15f, 0.25f));
            }
            return projectile.ai[0] < 1;
        }

        public void Shatter()
        {
            if (projectile.ai[0] < 1)
            {
                for (float num315 = 0.75f; num315 < 5; num315 += 0.4f)
                {
                    float angle = MathHelper.ToRadians(-Main.rand.Next(-30, 30));
                    Vector2 vari = new Vector2(Main.rand.NextFloat(-2f, 2), Main.rand.NextFloat(-2f, 2));
                    Dust.NewDustPerfect(projectile.position + new Vector2(Main.rand.NextFloat(projectile.width), Main.rand.NextFloat(projectile.width)), ModContent.DustType<Dusts.GlassGravity>(), vari, 100, default, num315 / 3f);
                }
                Main.PlaySound(SoundID.Item, (int)projectile.Center.X, (int)projectile.Center.Y, 27, 0.65f, -Main.rand.NextFloat(0.15f, 0.75f));
            }

            projectile.ai[0] = 600;
            projectile.netUpdate = true;
        }

        public DrawData Draw()
        {
            float scale = MathHelper.Clamp(1f - projectile.ai[0] / 15f, 0, 1);
            var color = Lighting.GetColor((int)(projectile.Center.X / 16f), (int)(projectile.Center.Y / 16f));
            var tex = ModContent.GetTexture(Texture);

            return new DrawData(tex, projectile.Center - Main.screenPosition, null, color, projectile.rotation, tex.Size() / 2f, Vector2.One * scale, SpriteEffects.None, 0);
        }

        public override void AI()
        {
            projectile.ai[0] = Math.Max(projectile.ai[0] - 1, 0);
            int pos = (int)projectile.localAI[0];
            Player player = projectile.Owner();

            if (player.dead)
            {
                Shatter();
                projectile.timeLeft = 0;
            }

            if (Regenerate())
            {
                Dust.NewDust(projectile.position, projectile.width, projectile.height, ModContent.DustType<Dusts.Air>(), 0, 0, 0, default, 0.35f);
                player.AddBuff(ModContent.BuffType<Buffs.ProtectiveShard>(), 2);
            }

            projectile.ai[1] -= 1;

            if (((float)player.statLife / player.statLifeMax2) > 0.2f * pos || projectile.ai[1] < 1)
            {
                projectile.position += Vector2.Normalize(player.Center - projectile.Center) * 5;
                projectile.rotation += 0.4f;
                projectile.friendly = false;

                if (Vector2.Distance(player.Center, projectile.Center) <= 16)
                {
                    projectile.timeLeft = 0;
                }
            }
            else
            {
                projectile.timeLeft = 30;
                projectile.localAI[1] += (0.05f - pos * 0.005f) * (pos % 2 == 0 ? -1 : 1);

                if (Math.Abs(projectile.localAI[1]) >= 6.28)
                    projectile.localAI[1] = 0;

                float movex = (float)Math.Cos(projectile.localAI[1]) * 2f;
                float movey = ((float)Math.Sin(projectile.localAI[1]) / 1.5f);
                projectile.position = player.Center - new Vector2(0, player.height / 3f) + new Vector2(movex, movey) * (((5 - pos) * 6) + 10);
                projectile.rotation = projectile.localAI[1];
            }
        }
    }
}