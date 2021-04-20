using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Buffs.Summon;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Items.Vitric
{
    public class VitricSummonHammer : ModProjectile
    {
        protected Vector2 strikeWhere;
        protected Vector2 enemySize;
        protected Player player;
        protected NPC enemy;
        protected Vector2 oldHitbox;
        internal float animationProgress = 0f;

        public VitricSummonHammer()
        {
            strikeWhere = projectile.Center;
            enemySize = Vector2.One;
        }

        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Enchanted Vitric Weapons");
            Main.projFrames[projectile.type] = 1;
            ProjectileID.Sets.Homing[projectile.type] = true;
        }

        public override void SetDefaults()
        {
            projectile.width = 48;
            projectile.height = 32;
            projectile.tileCollide = false;
            projectile.friendly = true;
            projectile.hostile = false;
            projectile.minion = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 60;
            projectile.extraUpdates = 1;
        }

        public override bool CanDamage() => projectile.localAI[0] > 60;

        public override void AI()
        {

            player = projectile.Owner();

            if (player.HasBuff(ModContent.BuffType<VitricSummonBuff>()))
                projectile.timeLeft = 2;

            projectile.localAI[0] += 1;
            enemy = Main.npc[(int)projectile.ai[1]];
            float animationSpeed = GetType() == typeof(VitricSummonSword) ? 1.50f : GetType() == typeof(VitricSummonKnife) ? 2f : 1f;
            animationProgress += animationSpeed / (1f + projectile.extraUpdates);

            DoAI();
        }

        public virtual void DoAI()
        {
            if (projectile.localAI[0] > 300)
            {
                projectile.Kill();
                return;
            }

            oldHitbox = new Vector2(projectile.width, projectile.height);

            if (projectile.localAI[0] == 1)
            {
                projectile.rotation = projectile.ai[0];
                projectile.spriteDirection = projectile.rotation > 500 ? -1 : 1;

                if (projectile.rotation > 500)
                    projectile.rotation -= 1000;

                projectile.ai[0] = 0;
                projectile.netUpdate = true;
            }

            Vector2 target;

            SwingUp();
            SwingDown();

            void SwingUp()
            {
                if (projectile.localAI[0] < 70)//Swing up
                {
                    float progress = Math.Min(projectile.localAI[0] / 50f, 1f);
                    if (Helper.IsTargetValid(enemy))
                    {
                        strikeWhere = enemy.Center + new Vector2(enemy.velocity.X, enemy.velocity.Y / 2f);
                        enemySize = new Vector2(enemy.width, enemy.height);
                    }

                    projectile.rotation = projectile.rotation.AngleLerp(-MathHelper.Pi / 4f, 0.075f * progress);
                    target = strikeWhere + new Vector2(projectile.spriteDirection * -(75 + (float)Math.Pow(projectile.localAI[0] * 2f, 0.80) + enemySize.X / 2f), -200);
                    projectile.velocity += (target - projectile.Center) / 75f;

                    if (projectile.velocity.Length() > 14f * progress)
                        projectile.velocity = Vector2.Normalize(projectile.velocity) * 14 * progress;

                    projectile.velocity /= 1.5f;
                }
            }

            void SwingDown()
            {

                if (projectile.localAI[0] >= 70)//Swing Down
                {
                    if (Helper.IsTargetValid(enemy))
                        strikeWhere.X = enemy.Center.X + enemy.velocity.X * 1.50f;

                    projectile.velocity.X += Math.Min(Math.Abs(projectile.velocity.X), 0) * projectile.spriteDirection;

                    float progress = Math.Min((projectile.localAI[0] - 70f) / 30f, 1f);

                    projectile.rotation = projectile.rotation.AngleTowards(MathHelper.Pi / 4f, 0.075f * progress);
                    target = strikeWhere + new Vector2(projectile.spriteDirection * -(32 + enemySize.X / 4f), -32);
                    projectile.velocity.X += MathHelper.Clamp(target.X - projectile.Center.X, -80f, 80f) / 24f;
                    projectile.velocity.Y += 1f;

                    if (projectile.velocity.Length() > 10 * progress)
                        projectile.velocity = Vector2.Normalize(projectile.velocity) * 10 * progress;

                    projectile.velocity.X /= 1.20f;

                    Smash();
                }
            }

            void Smash()
            {
                if (projectile.Center.Y > target.Y)//Smashing!
                {
                    int tileTargetY = (int)(projectile.Center.Y / 16) + 1;
                    Point16 point = new Point16((int)((projectile.Center.X + projectile.width / 3f * projectile.spriteDirection) / 16), Math.Min(Main.maxTilesY, tileTargetY));
                    Tile tile = Framing.GetTileSafely(point.X, point.Y);

                    //hard coded dust ids in worldgen.cs, still ew
                    //Tile hit!
                    if (tile != null && WorldGen.InWorld(point.X, point.Y, 1) && tile.active() && Main.tileSolid[tile.type])
                    {
                        projectile.localAI[0] = 301;
                        int dusttype = ModContent.DustType<Dusts.GlassGravity>();
                        DustHelper.TileDust(tile, ref dusttype);

                        int ai0 = tile.type;
                        int ai1 = 16 * projectile.spriteDirection;
                        Vector2 tilepos16 = new Vector2(point.X, point.Y - 1) * 16;

                        Projectile.NewProjectile(tilepos16, Vector2.Zero, ModContent.ProjectileType<ShockwaveSummon>(), (int)(projectile.damage * 0.25), 0, Main.myPlayer, ai0, ai1);
                        Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 10;

                        for (float num315 = 2f; num315 < 15; num315 += 0.50f)
                        {
                            float angle = MathHelper.ToRadians(-Main.rand.Next(70, 130));
                            Vector2 vecangle = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * num315 * 3f;
                            Vector2 position = new Vector2(projectile.position.X + projectile.spriteDirection * (int)(projectile.width * 0.60), projectile.Center.Y - projectile.height / 2f);

                            int num316 = Dust.NewDust(position, projectile.width / 2, projectile.height, dusttype, 0f, 0f, 50, default, (12f - num315) / 5f);
                            Main.dust[num316].noGravity = true;
                            Main.dust[num316].velocity = vecangle;
                            Main.dust[num316].fadeIn = 0.25f;
                        }

                        projectile.height += 32; projectile.position.Y -= 16;
                        projectile.width += 40; projectile.position.X -= 20;

                    }

                }

            }
        }

        public override bool PreKill(int timeLeft)
        {
            int dusttype = ModContent.DustType<Dusts.GlassGravity>();

            for (float k = 2f; k < 12; k += 0.25f)
            {
                float angle = MathHelper.ToRadians(-Main.rand.Next(40, 140));
                Vector2 vecangle = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * k;
                int index = Dust.NewDust(new Vector2(projectile.Center.X + (projectile.spriteDirection < 0 ? -oldHitbox.X : 0), projectile.Center.Y - oldHitbox.Y), (int)oldHitbox.X, (int)oldHitbox.Y * 2, dusttype, 0f, 0f, 50, default, (40f - k) / 40f);
                Main.dust[index].noGravity = true;
                Main.dust[index].velocity = vecangle;
                Main.dust[index].fadeIn = 0.75f;
            }

            Main.PlaySound(SoundID.Shatter, projectile.Center);
            return true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Vector2 pos = projectile.Center;

            for (float xx = Math.Min(1f, (projectile.velocity.Length() - 4f) / 2f); xx > 0; xx -= 0.10f)
            {
                Vector2 drawPos = pos - projectile.velocity * 6f * xx;
                Draw(spriteBatch, drawPos, lightColor * (1f - xx) * 0.5f, xx);
            }

            Draw(spriteBatch, projectile.Center, lightColor, 0);
            return false;
        }

        public virtual void Draw(SpriteBatch spriteBatch, Vector2 drawpos, Color lightColor, float aimframe)
        {
            Texture2D tex = Main.projectileTexture[projectile.type];

            Vector2 drawOrigin = new Vector2(tex.Width / 2f, tex.Height) / 2f;
            Vector2 pos = drawpos - Main.screenPosition;
            Color color = lightColor;
            spriteBatch.Draw(tex, pos, VitricSummonOrb.WhiteFrame(tex.Size().ToRectangle(), false), color, projectile.rotation * projectile.spriteDirection, drawOrigin, projectile.scale, projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
            spriteBatch.Draw(tex, pos, VitricSummonOrb.WhiteFrame(tex.Size().ToRectangle(), true), VitricSummonOrb.MoltenGlow(animationProgress), projectile.rotation * projectile.spriteDirection, drawOrigin, projectile.scale, projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
        }
    }

    class ShockwaveSummon : Bosses.GlassMiniboss.Shockwave
    {
        public override string Texture => "StarlightRiver/Assets/Tiles/Vitric/AncientSandstone";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Shockwave");
        private int TileType => (int)projectile.ai[0];
        private int ShockwavesLeft => (int)projectile.ai[1];//Positive and Negitive

        public override void SetDefaults()
        {
            base.SetDefaults();
            projectile.hostile = false;
            projectile.friendly = true;
            projectile.minion = true;
            projectile.timeLeft = 1060;
            projectile.tileCollide = true;
            projectile.width = 16;
            projectile.height = 16;
            projectile.idStaticNPCHitCooldown = 20;
            projectile.usesIDStaticNPCImmunity = true;
        }

        public override void AI()
        {
            if (projectile.timeLeft > 1000)
            {
                if (projectile.timeLeft < 1002 && projectile.timeLeft > 80)
                    projectile.Kill();

                projectile.velocity.Y = 4f;
            }
            else
            {
                projectile.velocity.Y = projectile.timeLeft <= 10 ? 1f : -1f;

                if (projectile.timeLeft == 19 && Math.Abs(ShockwavesLeft) > 0) //what the fuck is this. Local vars. please.
                    Projectile.NewProjectile(new Vector2((int)projectile.Center.X / 16 * 16 + 16 * Math.Sign(ShockwavesLeft)
                    , (int)projectile.Center.Y / 16 * 16 - 32),
                    Vector2.Zero, projectile.type, projectile.damage, 0, Main.myPlayer, TileType, projectile.ai[1] - Math.Sign(ShockwavesLeft));

            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (projectile.timeLeft < 21)
                spriteBatch.Draw(Main.tileTexture[TileType], projectile.position - Main.screenPosition, new Rectangle(18, 0, 16, 16), lightColor);

            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (projectile.timeLeft > 800) //smells like boilerplate. IDG-it kinda is
            {
                Point16 point = new Point16((int)((projectile.Center.X + projectile.width / 3f * projectile.spriteDirection) / 16), Math.Min(Main.maxTilesY, (int)(projectile.Center.Y / 16) + 1));
                Tile tile = Framing.GetTileSafely(point.X, point.Y);

                //hard coded dust ids in worldgen.cs, still ew
                //Tile hit!
                if (tile != null && WorldGen.InWorld(point.X, point.Y, 1) && tile.active() && Main.tileSolid[tile.type])
                {
                    projectile.timeLeft = 20;
                    projectile.ai[0] = tile.type;
                    projectile.tileCollide = false;
                    projectile.position.Y += 16;

                    int dusttype = 0;

                    DustHelper.TileDust(tile, ref dusttype);

                    for (float num315 = 0.50f; num315 < 3; num315 += 0.25f)
                    {
                        float angle = MathHelper.ToRadians(-Main.rand.Next(70, 130));
                        Vector2 vecangle = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * num315 * 3f;
                        int num316 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, (int)(projectile.height / 2f), dusttype, 0f, 0f, 50, default, (4f - num315) / 3f);
                        Main.dust[num316].noGravity = true;
                        Main.dust[num316].velocity = vecangle * 2f;
                        Main.dust[num316].fadeIn = 0.25f;
                    }
                }
            }
            return false;
        }
    }

}
