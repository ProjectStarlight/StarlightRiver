//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using StarlightRiver.Content.Buffs.Summon;
//using StarlightRiver.Core;
//using StarlightRiver.Helpers;
//using System;
//using Terraria;
//using Terraria.DataStructures;
//using Terraria.GameContent;
//using Terraria.ID;
//using Terraria.ModLoader;

//namespace StarlightRiver.Content.Items.Vitric //TODO: Rewrite this entire file its a hot mess
//{
//	public class VitricSummonHammer : ModProjectile
//    {
//        protected Vector2 strikeWhere;
//        protected Vector2 enemySize;
//        protected Player Player;
//        protected NPC enemy;
//        protected Vector2 oldHitbox;
//        internal float animationProgress = 0f;

//        public VitricSummonHammer()
//        {
//            strikeWhere = Projectile.Center;
//            enemySize = Vector2.One;
//        }

//        public override string Texture => AssetDirectory.VitricItem + Name;

//        public override void SetStaticDefaults()
//        {
//            DisplayName.SetDefault("Enchanted Vitric Weapons");
//            Main.projFrames[Projectile.type] = 1;
//            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
//        }

//        public override void SetDefaults()
//        {
//            Projectile.width = 48;
//            Projectile.height = 32;
//            Projectile.tileCollide = false;
//            Projectile.friendly = true;
//            Projectile.hostile = false;
//            Projectile.minion = true;
//            Projectile.penetrate = -1;
//            Projectile.timeLeft = 60;
//            Projectile.extraUpdates = 1;
//        }

//        public override bool? CanDamage() => Projectile.localAI[0] > 60;

//        public override void AI()
//        {

//            Player = Projectile.Owner();

//            if (Player.HasBuff(ModContent.BuffType<VitricSummonBuff>()))
//                Projectile.timeLeft = 2;

//            Projectile.localAI[0] += 1;
//            enemy = Main.npc[(int)Projectile.ai[1]];
//            float animationSpeed = GetType() == typeof(VitricSummonSword) ? 1.50f : GetType() == typeof(VitricSummonKnife) ? 2f : 1f;
//            animationProgress += animationSpeed / (1f + Projectile.extraUpdates);

//            DoAI();
//        }

//        public virtual void DoAI()
//        {
//            if (Projectile.localAI[0] > 300)
//            {
//                Projectile.Kill();
//                return;
//            }

//            oldHitbox = new Vector2(Projectile.width, Projectile.height);

//            if (Projectile.localAI[0] == 1)
//            {
//                Projectile.rotation = Projectile.ai[0];
//                Projectile.spriteDirection = Projectile.rotation > 500 ? -1 : 1;

//                if (Projectile.rotation > 500)
//                    Projectile.rotation -= 1000;

//                Projectile.ai[0] = 0;
//                Projectile.netUpdate = true;
//            }

//            Vector2 target;

//            SwingUp();
//            SwingDown();

//            void SwingUp()
//            {
//                if (Projectile.localAI[0] < 70)//Swing up
//                {
//                    float progress = Math.Min(Projectile.localAI[0] / 50f, 1f);
//                    if (enemy.CanBeChasedBy())
//                    {
//                        strikeWhere = enemy.Center + new Vector2(enemy.velocity.X, enemy.velocity.Y / 2f);
//                        enemySize = new Vector2(enemy.width, enemy.height);
//                    }

//                    Projectile.rotation = Projectile.rotation.AngleLerp(-MathHelper.Pi / 4f, 0.075f * progress);
//                    target = strikeWhere + new Vector2(Projectile.spriteDirection * -(75 + (float)Math.Pow(Projectile.localAI[0] * 2f, 0.80) + enemySize.X / 2f), -200);
//                    Projectile.velocity += (target - Projectile.Center) / 75f;

//                    if (Projectile.velocity.Length() > 14f * progress)
//                        Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 14 * progress;

//                    Projectile.velocity /= 1.5f;
//                }
//            }

//            void SwingDown()
//            {

//                if (Projectile.localAI[0] >= 70)//Swing Down
//                {
//                    if (enemy.CanBeChasedBy())
//                        strikeWhere.X = enemy.Center.X + enemy.velocity.X * 1.50f;

//                    Projectile.velocity.X += Math.Min(Math.Abs(Projectile.velocity.X), 0) * Projectile.spriteDirection;

//                    float progress = Math.Min((Projectile.localAI[0] - 70f) / 30f, 1f);

//                    Projectile.rotation = Projectile.rotation.AngleTowards(MathHelper.Pi / 4f, 0.075f * progress);
//                    target = strikeWhere + new Vector2(Projectile.spriteDirection * -(32 + enemySize.X / 4f), -32);
//                    Projectile.velocity.X += MathHelper.Clamp(target.X - Projectile.Center.X, -80f, 80f) / 24f;
//                    Projectile.velocity.Y += 1f;

//                    if (Projectile.velocity.Length() > 10 * progress)
//                        Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 10 * progress;

//                    Projectile.velocity.X /= 1.20f;

//                    Smash();
//                }
//            }

//            void Smash()
//            {
//                if (Projectile.Center.Y > target.Y)//Smashing!
//                {
//                    int tileTargetY = (int)(Projectile.Center.Y / 16) + 1;
//                    Point16 point = new Point16((int)((Projectile.Center.X + Projectile.width / 3f * Projectile.spriteDirection) / 16), Math.Min(Main.maxTilesY, tileTargetY));
//                    Tile tile = Framing.GetTileSafely(point.X, point.Y);

//                    if (tile != null && WorldGen.InWorld(point.X, point.Y, 1) && tile.HasTile && Main.tileSolid[tile.TileType])
//                    {
//                        Projectile.localAI[0] = 301;
//                        int dusttype = ModContent.DustType<Dusts.GlassGravity>();
//                        DustHelper.TileDust(tile, ref dusttype);

//                        int ai0 = tile.TileType;
//                        int ai1 = 16 * Projectile.spriteDirection;
//                        Vector2 tilepos16 = new Vector2(point.X, point.Y - 1) * 16;

//                        //not sure if correct source
//                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), tilepos16, Vector2.Zero, ModContent.ProjectileType<ShockwaveSummon>(), (int)(Projectile.damage * 0.25), 0, Main.myPlayer, ai0, ai1);
//                        Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 10;

//                        for (float num315 = 2f; num315 < 15; num315 += 0.50f)
//                        {
//                            float angle = MathHelper.ToRadians(-Main.rand.Next(70, 130));
//                            Vector2 vecangle = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * num315 * 3f;
//                            Vector2 position = new Vector2(Projectile.position.X + Projectile.spriteDirection * (int)(Projectile.width * 0.60), Projectile.Center.Y - Projectile.height / 2f);

//                            int num316 = Dust.NewDust(position, Projectile.width / 2, Projectile.height, dusttype, 0f, 0f, 50, default, (12f - num315) / 5f);
//                            Main.dust[num316].noGravity = true;
//                            Main.dust[num316].velocity = vecangle;
//                            Main.dust[num316].fadeIn = 0.25f;
//                        }

//                        Projectile.height += 32; Projectile.position.Y -= 16;
//                        Projectile.width += 40; Projectile.position.X -= 20;

//                    }

//                }

//            }
//        }

//        public override bool PreKill(int timeLeft)
//        {
//            int dusttype = ModContent.DustType<Dusts.GlassGravity>();

//            for (float k = 2f; k < 12; k += 0.25f)
//            {
//                float angle = MathHelper.ToRadians(-Main.rand.Next(40, 140));
//                Vector2 vecangle = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * k;
//                int index = Dust.NewDust(new Vector2(Projectile.Center.X + (Projectile.spriteDirection < 0 ? -oldHitbox.X : 0), Projectile.Center.Y - oldHitbox.Y), (int)oldHitbox.X, (int)oldHitbox.Y * 2, dusttype, 0f, 0f, 50, default, (40f - k) / 40f);
//                Main.dust[index].noGravity = true;
//                Main.dust[index].velocity = vecangle;
//                Main.dust[index].fadeIn = 0.75f;
//            }

//            Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);
//            return true;
//        }

//        public override bool PreDraw(ref Color lightColor)
//        {
//            Vector2 pos = Projectile.Center;
//            SpriteBatch spriteBatch = Main.spriteBatch;
//            for (float xx = Math.Min(1f, (Projectile.velocity.Length() - 4f) / 2f); xx > 0; xx -= 0.10f)
//            {
//                Vector2 drawPos = pos - Projectile.velocity * 6f * xx;
//                Draw(spriteBatch, drawPos, lightColor * (1f - xx) * 0.5f, xx);
//            }

//            Draw(spriteBatch, Projectile.Center, lightColor, 0);
//            return false;
//        }

//        public virtual void Draw(SpriteBatch spriteBatch, Vector2 drawpos, Color lightColor, float aimframe)
//        {
//            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;

//            Vector2 drawOrigin = new Vector2(tex.Width / 2f, tex.Height) / 2f;
//            Vector2 pos = drawpos - Main.screenPosition;
//            Color color = lightColor;
//            spriteBatch.Draw(tex, pos, VitricSummonOrb.WhiteFrame(tex.Size().ToRectangle(), false), color, Projectile.rotation * Projectile.spriteDirection, drawOrigin, Projectile.scale, Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
//            spriteBatch.Draw(tex, pos, VitricSummonOrb.WhiteFrame(tex.Size().ToRectangle(), true), VitricSummonOrb.MoltenGlow(animationProgress), Projectile.rotation * Projectile.spriteDirection, drawOrigin, Projectile.scale, Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
//        }
//    }

//    class ShockwaveSummon : Bosses.GlassMiniboss.Shockwave
//    {
//        public override string Texture => "StarlightRiver/Assets/Tiles/Vitric/AncientSandstone";

//        public override void SetStaticDefaults() => DisplayName.SetDefault("Shockwave");
//        private int TileType => (int)Projectile.ai[0];
//        private int ShockwavesLeft => (int)Projectile.ai[1];//Positive and Negitive

//        public override void SetDefaults()
//        {
//            base.SetDefaults();
//            Projectile.hostile = false;
//            Projectile.friendly = true;
//            Projectile.minion = true;
//            Projectile.timeLeft = 1060;
//            Projectile.tileCollide = true;
//            Projectile.width = 16;
//            Projectile.height = 16;
//            Projectile.idStaticNPCHitCooldown = 20;
//            Projectile.usesIDStaticNPCImmunity = true;
//        }

//        public override void AI()
//        {
//            if (Projectile.timeLeft > 1000)
//            {
//                if (Projectile.timeLeft < 1002 && Projectile.timeLeft > 80)
//                    Projectile.Kill();

//                Projectile.velocity.Y = 4f;
//            }
//            else
//            {
//                Projectile.velocity.Y = Projectile.timeLeft <= 10 ? 1f : -1f;

//                if (Projectile.timeLeft == 19 && Math.Abs(ShockwavesLeft) > 0) 
//                    Projectile.NewProjectile(Projectile.InheritSource(Projectile), new Vector2((int)Projectile.Center.X / 16 * 16 + 16 * Math.Sign(ShockwavesLeft)
//                    , (int)Projectile.Center.Y / 16 * 16 - 32),
//                    Vector2.Zero, Projectile.type, Projectile.damage, 0, Main.myPlayer, TileType, Projectile.ai[1] - Math.Sign(ShockwavesLeft));

//            }
//        }

//        public override bool PreDraw(ref Color lightColor)
//        {
//            if (Projectile.timeLeft < 21)
//                Main.spriteBatch.Draw(TextureAssets.Tile[TileType].Value, Projectile.position - Main.screenPosition, new Rectangle(18, 0, 16, 16), lightColor);

//            return false;
//        }

//        public override bool OnTileCollide(Vector2 oldVelocity)
//        {
//            if (Projectile.timeLeft > 800) 
//            {
//                Point16 point = new Point16((int)((Projectile.Center.X + Projectile.width / 3f * Projectile.spriteDirection) / 16), Math.Min(Main.maxTilesY, (int)(Projectile.Center.Y / 16) + 1));
//                Tile tile = Framing.GetTileSafely(point.X, point.Y);

//                if (tile != null && WorldGen.InWorld(point.X, point.Y, 1) && tile.HasTile && Main.tileSolid[tile.TileType])
//                {
//                    Projectile.timeLeft = 20;
//                    Projectile.ai[0] = tile.TileType;
//                    Projectile.tileCollide = false;
//                    Projectile.position.Y += 16;

//                    int dusttype = 0;

//                    DustHelper.TileDust(tile, ref dusttype);

//                    for (float num315 = 0.50f; num315 < 3; num315 += 0.25f)
//                    {
//                        float angle = MathHelper.ToRadians(-Main.rand.Next(70, 130));
//                        Vector2 vecangle = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * num315 * 3f;
//                        int num316 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, (int)(Projectile.height / 2f), dusttype, 0f, 0f, 50, default, (4f - num315) / 3f);
//                        Main.dust[num316].noGravity = true;
//                        Main.dust[num316].velocity = vecangle * 2f;
//                        Main.dust[num316].fadeIn = 0.25f;
//                    }
//                }
//            }
//            return false;
//        }
//    }

//}