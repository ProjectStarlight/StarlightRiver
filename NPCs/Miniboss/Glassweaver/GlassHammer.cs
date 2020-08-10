using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework.Graphics;

namespace StarlightRiver.NPCs.Miniboss.Glassweaver
{
    class GlassHammer : ModProjectile
    {
        Vector2 origin;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Woven Hammer");

        public override void SetDefaults()
        {
            projectile.width = 32;
            projectile.height = 32;
            projectile.hostile = true;
            projectile.timeLeft = 120;
            projectile.tileCollide = false;
            projectile.aiStyle = -1;
            projectile.penetrate = -1;
        }

        public override void AI()
        {
            if (projectile.timeLeft == 120) origin = projectile.Center; //sets origin when spawned

            if (projectile.timeLeft >= 60)
            {
                float radius = (120 - projectile.timeLeft) * 2;
                float rotation = -(120 - projectile.timeLeft) / 60f * 0.8f; //ai 0 is direction

                projectile.Center = origin - Vector2.UnitY.RotatedBy(rotation * projectile.ai[0]) * radius;
            }
            else if (projectile.timeLeft >= 40)
            {
                float rotation = -0.8f + (120 - projectile.timeLeft - 60) / 20f * ((float)Math.PI / 2 + 1.2f);

                projectile.Center = origin - Vector2.UnitY.RotatedBy(rotation * projectile.ai[0]) * 120;

                if (projectile.timeLeft == 40)
                {
                    Main.PlaySound(SoundID.Shatter, projectile.Center);
                    Main.LocalPlayer.GetModPlayer<Core.StarlightPlayer>().Shake += 15;

                    for (int k = 0; k < 30; k++)
                    {
                        Vector2 vector = Vector2.UnitY.RotatedByRandom((float)Math.PI / 2);
                        Dust.NewDustPerfect(projectile.Center + vector * Main.rand.NextFloat(25), DustType<Dusts.Sand>(), vector * Main.rand.NextFloat(3, 5), 150, Color.White, 0.5f);
                    }

                    Projectile.NewProjectile(projectile.Center + Vector2.UnitY * -8, Vector2.Zero, ProjectileType<Shockwave>(), 10, 0, Main.myPlayer, projectile.ai[0], 13);
                }
            }
            else if (projectile.timeLeft % 2 == 0)
            {
                Main.NewText(projectile.ai[0]);
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if(projectile.timeLeft <= 60 && projectile.timeLeft >= 40)
            {
                target.AddBuff(BuffType<Buffs.Squash>(), 180);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Rectangle frame = new Rectangle(0, 166 * (int)((120 - projectile.timeLeft) / 80f * 12), 214, 166);
            if (projectile.timeLeft <= 40) frame.Y = 12 * 166;
            spriteBatch.Draw(GetTexture(Texture), origin + new Vector2(-100, -130) - Main.screenPosition, frame, Color.White, 0, Vector2.Zero, 1, projectile.ai[0] == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
            return false;
        }
    }

    class Shockwave : ModProjectile
    {
        public override string Texture => "StarlightRiver/Tiles/Vitric/Blocks/AncientSandstone";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Shockwave");

        public override void SetDefaults()
        {
            projectile.width = 16;
            projectile.height = 16;
            projectile.hostile = true;
            projectile.timeLeft = 20;
            projectile.tileCollide = false;
            projectile.aiStyle = -1;
            projectile.penetrate = -1;
        }

        public override void AI()
        {
            projectile.velocity.Y = projectile.timeLeft <= 10 ? 1f : -1f;

            if(projectile.timeLeft == 16 && projectile.ai[1] > 0)
                Projectile.NewProjectile(new Vector2(projectile.Center.X + 16 * projectile.ai[0], projectile.Center.Y + 4), Vector2.Zero, projectile.type, projectile.damage, 0, Main.myPlayer, projectile.ai[0], projectile.ai[1] - 1);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
           spriteBatch.Draw(GetTexture(Texture), projectile.position - Main.screenPosition, new Rectangle(18, 0, 16, 16), lightColor);
            return false;
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCsAndTiles.Add(index);
        }
    }
}
