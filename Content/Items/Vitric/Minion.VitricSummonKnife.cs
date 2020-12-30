using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Items.Vitric
{
    public class VitricSummonKnife : VitricSummonHammer
    {
        private bool closetoplayer = false;
        internal Vector2 offset;

        public override bool CanDamage() => offset.X > 0;

        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Enchanted Vitric Weapons");
            Main.projFrames[projectile.type] = 1;
            ProjectileID.Sets.Homing[projectile.type] = true;
        }

        public sealed override void SetDefaults()
        {
            projectile.width = 24;
            projectile.height = 24;
            projectile.tileCollide = false;
            projectile.friendly = true;
            projectile.hostile = false;
            projectile.minion = true;
            projectile.penetrate = 1;
            projectile.timeLeft = 60;
            projectile.extraUpdates = 1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write((int)offset.X);
            writer.Write((int)offset.Y);
            writer.Write(closetoplayer);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            offset.X = reader.ReadInt32();
            offset.Y = reader.ReadInt32();
            closetoplayer = reader.ReadBoolean();
        }

        public VitricSummonKnife()
        {
            strikeWhere = projectile.Center;
            enemySize = Vector2.One;
        }

        public override void DoAI()
        {
            Player player = projectile.Owner();
            oldHitbox = new Vector2(projectile.width, projectile.height);
            projectile.tileCollide = offset.X > 0;

            if (projectile.localAI[0] > 600)
                projectile.Kill();

            if (projectile.localAI[0] == 1)
            {
                projectile.localAI[1] = 1;
                projectile.rotation = projectile.ai[0];
                projectile.spriteDirection = projectile.rotation > 500 ? -1 : 1;

                if (projectile.rotation > 500)
                    projectile.rotation -= 1000;

                projectile.ai[0] = Main.rand.NextFloat(MathHelper.ToRadians(-20), MathHelper.ToRadians(20));

                if (player.Distance(projectile.Center) < 96)
                    closetoplayer = true;

                projectile.netUpdate = true;
            }

            if (Helper.IsTargetValid(enemy))
            {
                strikeWhere = enemy.Center + new Vector2(enemy.velocity.X * 4, enemy.velocity.Y * 4);
                enemySize = new Vector2(enemy.width, enemy.height);
            }

            Vector2 aimvector = strikeWhere - projectile.Center;
            float turnto = aimvector.ToRotation();

            if (offset.X < 1)
            {
                Vector2 gothere;
                float animlerp = Math.Min(projectile.localAI[0] / 40f, 1f);

                if (closetoplayer)
                {
                    gothere = player.Center - new Vector2(player.direction * 32, 72) + offset * 3f;
                    projectile.velocity += (gothere - projectile.Center) / 30f * animlerp;
                    projectile.velocity *= 0.65f;
                }
                else
                {
                    projectile.velocity -= Vector2.Normalize(strikeWhere - projectile.Center).RotatedBy(offset.Y * 0.2f * projectile.spriteDirection) * animlerp * 0.10f;
                    projectile.velocity *= 0.92f;
                }

                projectile.rotation = projectile.rotation.AngleTowards(turnto * projectile.spriteDirection + (projectile.spriteDirection < 0 ? (float)Math.PI : 0), animlerp * 0.04f);

                if ((int)projectile.localAI[0] == 120 + (int)offset.Y)
                {
                    offset.X = 1;
                    projectile.velocity = (projectile.rotation * projectile.spriteDirection).ToRotationVector2() * 10f * projectile.spriteDirection;
                    Main.PlaySound(SoundID.Item, (int)projectile.position.X, (int)projectile.position.Y, 1, 0.75f, -0.5f);
                    projectile.localAI[0] = 300;
                }

            }
            else
            {
                float turnspeed = 0.04f / (1f + (projectile.localAI[0] - 300f) / 60f);
                projectile.rotation = projectile.rotation.AngleTowards(turnto * projectile.spriteDirection + (projectile.spriteDirection < 0 ? (float)Math.PI : 0), turnspeed);
                projectile.velocity = (projectile.rotation * projectile.spriteDirection).ToRotationVector2() * projectile.velocity.Length() * projectile.spriteDirection;
            }

        }

        public override bool PreKill(int timeLeft)
        {
            int dusttype = DustType<Dusts.GlassGravity>(); //use the generics my dude.

            for (float num315 = 0.75f; num315 < 5; num315 += 0.4f)
            {
                float angle = MathHelper.ToRadians(-Main.rand.Next(-30, 30));
                Vector2 vari = new Vector2(Main.rand.NextFloat(-2f, 2), Main.rand.NextFloat(-2f, 2));
                Dust.NewDustPerfect(projectile.position + new Vector2(Main.rand.NextFloat(projectile.width), Main.rand.NextFloat(projectile.width)), dusttype, ((projectile.velocity + vari) / num315).RotatedBy(angle), 100, default, num315 / 4f);
            }

            Main.PlaySound(SoundID.Item, (int)projectile.Center.X, (int)projectile.Center.Y, 27, 0.75f);
            return true;
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 drawpos, Color lightColor, float aimframe)
        {
            Texture2D tex = Main.projectileTexture[projectile.type];

            Vector2 drawOrigin = new Vector2(tex.Width / 2, tex.Height) / 2f;
            float rotoffset = projectile.rotation + MathHelper.ToRadians(45f);
            spriteBatch.Draw(tex, drawpos - Main.screenPosition, VitricSummonOrb.WhiteFrame(tex.Size().ToRectangle(), false), lightColor, rotoffset * projectile.spriteDirection, drawOrigin, projectile.scale, projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
            spriteBatch.Draw(tex, drawpos - Main.screenPosition, VitricSummonOrb.WhiteFrame(tex.Size().ToRectangle(), true), VitricSummonOrb.MoltenGlow(animationProgress), rotoffset * projectile.spriteDirection, drawOrigin, projectile.scale, projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
        }
    }


}