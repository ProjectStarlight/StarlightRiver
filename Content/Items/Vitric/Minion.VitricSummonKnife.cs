//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using StarlightRiver.Core;
//using StarlightRiver.Helpers;
//using System;
//using System.IO;
//using Terraria;
//using Terraria.GameContent;
//using Terraria.ID;
//using static Terraria.ModLoader.ModContent;

//namespace StarlightRiver.Content.Items.Vitric
//{
//	public class VitricSummonKnife : VitricSummonHammer
//    {
//        private bool closetoPlayer = false;
//        internal Vector2 offset;

//        public override bool? CanDamage() => offset.X > 0;

//        public override string Texture => AssetDirectory.VitricItem + Name;

//        public override void SetStaticDefaults()
//        {
//            DisplayName.SetDefault("Enchanted Vitric Weapons");
//            Main.projFrames[Projectile.type] = 1;
//            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
//        }

//        public sealed override void SetDefaults()
//        {
//            Projectile.width = 24;
//            Projectile.height = 24;
//            Projectile.tileCollide = false;
//            Projectile.friendly = true;
//            Projectile.hostile = false;
//            Projectile.minion = true;
//            Projectile.penetrate = 1;
//            Projectile.timeLeft = 60;
//            Projectile.extraUpdates = 1;
//        }

//        public override void SendExtraAI(BinaryWriter writer)
//        {
//            writer.Write((int)offset.X);
//            writer.Write((int)offset.Y);
//            writer.Write(closetoPlayer);
//        }

//        public override void ReceiveExtraAI(BinaryReader reader)
//        {
//            offset.X = reader.ReadInt32();
//            offset.Y = reader.ReadInt32();
//            closetoPlayer = reader.ReadBoolean();
//        }

//        public VitricSummonKnife()
//        {
//            strikeWhere = Projectile.Center;
//            enemySize = Vector2.One;
//        }

//        public override void DoAI()
//        {
//            Player Player = Projectile.Owner();
//            oldHitbox = new Vector2(Projectile.width, Projectile.height);
//            Projectile.tileCollide = offset.X > 0;

//            if (Projectile.localAI[0] > 600)
//                Projectile.Kill();

//            if (Projectile.localAI[0] == 1)
//            {
//                Projectile.localAI[1] = 1;
//                Projectile.rotation = Projectile.ai[0];
//                Projectile.spriteDirection = Projectile.rotation > 500 ? -1 : 1;

//                if (Projectile.rotation > 500)
//                    Projectile.rotation -= 1000;

//                Projectile.ai[0] = Main.rand.NextFloat(MathHelper.ToRadians(-20), MathHelper.ToRadians(20));

//                if (Player.Distance(Projectile.Center) < 96)
//                    closetoPlayer = true;

//                Projectile.netUpdate = true;
//            }

//            if (enemy.CanBeChasedBy())
//            {
//                strikeWhere = enemy.Center + new Vector2(enemy.velocity.X * 4, enemy.velocity.Y * 4);
//                enemySize = new Vector2(enemy.width, enemy.height);
//            }

//            Vector2 aimvector = strikeWhere - Projectile.Center;
//            float turnto = aimvector.ToRotation();

//            if (offset.X < 1)
//            {
//                Vector2 gothere;
//                float animlerp = Math.Min(Projectile.localAI[0] / 40f, 1f);

//                if (closetoPlayer)
//                {
//                    gothere = Player.Center - new Vector2(Player.direction * 32, 72) + offset * 3f;
//                    Projectile.velocity += (gothere - Projectile.Center) / 30f * animlerp;
//                    Projectile.velocity *= 0.65f;
//                }
//                else
//                {
//                    Projectile.velocity -= Vector2.Normalize(strikeWhere - Projectile.Center).RotatedBy(offset.Y * 0.2f * Projectile.spriteDirection) * animlerp * 0.10f;
//                    Projectile.velocity *= 0.92f;
//                }

//                Projectile.rotation = Projectile.rotation.AngleTowards(turnto * Projectile.spriteDirection + (Projectile.spriteDirection < 0 ? (float)Math.PI : 0), animlerp * 0.04f);

//                if ((int)Projectile.localAI[0] == 120 + (int)offset.Y)
//                {
//                    offset.X = 1;
//                    Projectile.velocity = (Projectile.rotation * Projectile.spriteDirection).ToRotationVector2() * 10f * Projectile.spriteDirection;
//                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item, (int)Projectile.position.X, (int)Projectile.position.Y, 1, 0.75f, -0.5f);
//                    Projectile.localAI[0] = 300;
//                }

//            }
//            else
//            {
//                float turnspeed = 0.04f / (1f + (Projectile.localAI[0] - 300f) / 60f);
//                Projectile.rotation = Projectile.rotation.AngleTowards(turnto * Projectile.spriteDirection + (Projectile.spriteDirection < 0 ? (float)Math.PI : 0), turnspeed);
//                Projectile.velocity = (Projectile.rotation * Projectile.spriteDirection).ToRotationVector2() * Projectile.velocity.Length() * Projectile.spriteDirection;
//            }

//        }

//        public override bool PreKill(int timeLeft)
//        {
//            int dusttype = DustType<Dusts.GlassGravity>(); //use the generics my dude.

//            for (float num315 = 0.75f; num315 < 5; num315 += 0.4f)
//            {
//                float angle = MathHelper.ToRadians(-Main.rand.Next(-30, 30));
//                Vector2 vari = new Vector2(Main.rand.NextFloat(-2f, 2), Main.rand.NextFloat(-2f, 2));
//                Dust.NewDustPerfect(Projectile.position + new Vector2(Main.rand.NextFloat(Projectile.width), Main.rand.NextFloat(Projectile.width)), dusttype, ((Projectile.velocity + vari) / num315).RotatedBy(angle), 100, default, num315 / 4f);
//            }

//            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item, (int)Projectile.Center.X, (int)Projectile.Center.Y, 27, 0.75f);
//            return true;
//        }

//        public override void Draw(SpriteBatch spriteBatch, Vector2 drawpos, Color lightColor, float aimframe)
//        {
//            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;

//            Vector2 drawOrigin = new Vector2(tex.Width / 2, tex.Height) / 2f;
//            float rotoffset = Projectile.rotation + MathHelper.ToRadians(45f);
//            spriteBatch.Draw(tex, drawpos - Main.screenPosition, VitricSummonOrb.WhiteFrame(tex.Size().ToRectangle(), false), lightColor, rotoffset * Projectile.spriteDirection, drawOrigin, Projectile.scale, Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
//            spriteBatch.Draw(tex, drawpos - Main.screenPosition, VitricSummonOrb.WhiteFrame(tex.Size().ToRectangle(), true), VitricSummonOrb.MoltenGlow(animationProgress), rotoffset * Projectile.spriteDirection, drawOrigin, Projectile.scale, Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
//        }
//    }


//}