using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Projectiles.WeaponProjectiles
{
    internal class VitricBowProjectile : ModProjectile, IDrawAdditive
    {
        internal static int MaxCharge { get; set; } = 100;
        internal static int ChargeNeededToFire => 30;
        private float MaxAngle => MathHelper.Pi / 8f;
        private int MaxFireTime { get; set; } = 20;
        private int AddedFireBuffer { get; set; } = 0;
        private int FireRate => 6;
        private float ItemFirerate { get; set; } = default;

        public override string Texture => "StarlightRiver/Assets/Bosses/GlassBoss/VolleyTell";

        public override bool? CanHitNPC(NPC target) => false;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Enchanted Glass");

        public override void SetDefaults()
        {
            projectile.width = 16;
            projectile.height = 16;
            projectile.friendly = true;
            projectile.penetrate = 1;
            projectile.timeLeft = 600;
            projectile.ranged = true;
            projectile.arrow = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Player player = projectile.Owner();

            if (ItemFirerate == default)
                ItemFirerate = (float)player.itemTime / 30f;

            if (player.dead) projectile.Kill();
            else
            {
                if (projectile.localAI[1] > 0 || !player.channel)
                    LetGo(player);
                else
                    Charging(player);

                Holding(player);
                projectile.Center = player.Center;
            }
        }

        public void Holding(Player player)
        {

            player.itemTime = 3;
            player.itemAnimation = 3;
            player.heldProj = projectile.whoAmI;

            Vector2 mousePos = Main.MouseWorld;

            if (projectile.owner == Main.myPlayer && mousePos != projectile.Center)
            {
                Vector2 diff2 = mousePos - player.Center;
                diff2.Normalize();
                projectile.velocity = diff2 * (player.HasBuff(BuffID.Archery) ? 1.20f : 1f);
                projectile.direction = Main.MouseWorld.X > player.position.X ? 1 : -1;
                projectile.netUpdate = true;
            }

            int dir = projectile.direction;
            player.ChangeDir(projectile.direction);

            Vector2 distz = projectile.Center - player.Center;
            player.itemRotation = (float)Math.Atan2(distz.Y * dir, distz.X * dir);

        }

        public void Charging(Player player)
        {
            projectile.ai[0] = Math.Min(projectile.ai[0] + (1f / ItemFirerate), MaxCharge);
            MaxFireTime = 6 + (int)(projectile.ai[0] / 8f);

            projectile.ai[1] = MaxFireTime;
            projectile.timeLeft = MaxFireTime + AddedFireBuffer;
        }


        //This is really gross and this weapon is giving me a headache as is, I'm sorry
        public void LetGo(Player player)
        {
            bool dofire = (projectile.ai[0] < ChargeNeededToFire && projectile.localAI[1] == 1)
                || (projectile.ai[0] >= ChargeNeededToFire && projectile.localAI[1] % FireRate == 0);


            bool precharge = projectile.ai[0] < ChargeNeededToFire;
            float percent = Math.Max((projectile.ai[0] - ChargeNeededToFire) / (MaxCharge - ChargeNeededToFire), 0f);

            int timeleft = projectile.timeLeft - AddedFireBuffer;

            float partialchargepercent = (projectile.ai[0] / MaxCharge);
            float maxdelta = MaxAngle * partialchargepercent;
            float anglerot = maxdelta * (1f - (timeleft / projectile.ai[1]));

            projectile.localAI[1] += 1;

            if ((projectile.timeLeft <= MaxFireTime || precharge) && dofire)
            {
                for (int i = -1; i < 2; i += 2)
                {
                    if (i > 0 || !projectile.ignoreWater)
                    {

                        float chargefloat = 1f - (timeleft / projectile.ai[1]);
                        float speed = 4f + percent * (2f + chargefloat);
                        Vector2 velo = new Vector2(speed * projectile.velocity.Length(), 0).RotatedBy(projectile.velocity.ToRotation() + (anglerot * (projectile.ignoreWater ? 0f : i)));

                        Projectile.NewProjectile(projectile.Center, velo, ModContent.ProjectileType<VitricBowShardProjectile>(), (int)((float)projectile.damage * (1f + percent)), projectile.knockBack, projectile.owner = player.whoAmI, percent, 0f); //fire the flurry of projectiles

                        if (i > 0)
                            Main.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, projectile.Center);

                        if (projectile.ignoreWater)
                        {
                            projectile.ignoreWater = false;
                            break;
                        }
                    }
                }
            }
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            if (Main.LocalPlayer == Main.player[projectile.owner] || Main.netMode == NetmodeID.SinglePlayer)
            {
                Texture2D tex = Main.projectileTexture[projectile.type];
                float maxalpha = MathHelper.Clamp((projectile.ai[0] - ChargeNeededToFire) / 20f, 0.25f, 0.5f);
                float alpha = Math.Min(projectile.ai[0] / 60f, maxalpha) * Math.Min((float)projectile.timeLeft / 8, 1f);
                Vector2 maxspread = new Vector2(Math.Min(projectile.ai[0] / MaxCharge, 1) * 0.5f, 0.4f);

                spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, tex.Frame(), new Color(200, 255, 255) * alpha, projectile.velocity.ToRotation() + 1.57f, new Vector2(tex.Width / 2, tex.Height), maxspread, 0, 0);
            }
        }

    }


    //Hey, the boss's attack is 2 projectiles in one file, let me do it here too please?
    public class VitricBowShardProjectile : ModProjectile, IDrawAdditive
    {
        public override void SetDefaults()
        {
            projectile.hostile = false;
            projectile.friendly = true;
            projectile.tileCollide = true;
            projectile.width = 24;
            projectile.height = 24;
            projectile.arrow = true;
            projectile.ranged = true;//Whoops, now it is ranged-IDG
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = -1;
            projectile.extraUpdates = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }

        public override void AI()
        {
            for (int k = 0; k <= 1; k++)
            {
                Dust d = Dust.NewDustPerfect(projectile.Center + projectile.velocity, 264, (projectile.velocity * (Main.rand.NextFloat(-0.25f, -0.1f))).RotatedBy((k == 0) ? 0.4f : -0.4f), 0, default, 1f);
                d.noGravity = true;
            }

            projectile.velocity.Y += 0.025f;
            projectile.localAI[0] += 1;

            projectile.rotation = projectile.velocity.ToRotation();

            for (int k = projectile.oldPos.Length - 1; k >= 1; k -= 1)
            {
                projectile.oldRot[k] = projectile.oldRot[k - 1];
            }

            projectile.oldRot[0] = projectile.rotation;

            if (projectile.localAI[0] == 1)
            {
                projectile.scale = 0.5f + projectile.ai[0] / 3f;
                projectile.width = (int)((float)projectile.width * projectile.scale);
                projectile.height = (int)((float)projectile.height * projectile.scale);
                projectile.penetrate = 1 + ((int)(projectile.ai[0] * 4f));
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = Main.projectileTexture[projectile.type];
            for (int k = 0; k < projectile.oldPos.Length; k++)
            {
                spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, lightColor, projectile.rotation - MathHelper.Pi / 2f, tex.Size() / 2f, projectile.scale, 0, 0);
            }
            return false;
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.Item, (int)projectile.Center.X, (int)projectile.Center.Y, 27, 0.75f);

            for (float num315 = 0.2f; num315 < 2 + projectile.scale * 1.5f; num315 += 0.25f)
            {
                float angle = MathHelper.ToRadians(Main.rand.Next(0, 360));
                Vector2 vecangle = (new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * num315) + (projectile.velocity * num315);
                Dust num316 = Dust.NewDustPerfect(new Vector2(projectile.position.X, projectile.position.Y) + new Vector2(Main.rand.Next(projectile.width), Main.rand.Next(projectile.height)), mod.DustType("Glass2"), vecangle / 3f, 50, default, (8f - num315) / 5f);
                num316.fadeIn = 0.5f;
            }
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.GetTexture("StarlightRiver/Assets/Projectiles/GlassSpikeGlow");

            Vector2 offsets = Vector2.Normalize(projectile.velocity) * -40 - Main.screenPosition;

            Color color2 = new Color(150, 255, 255) * Math.Min(projectile.timeLeft / 100f, 1f);

            //Tried to do a thing but Additive doesn't want to play nicely with alpha, is there a way to blend with alpha?

            /*for (float k = 0; k < projectile.oldPos.Length; k++)
            {
                if ((int)k % 4 == 0)
                {
                    float alpha = ((float)projectile.oldPos.Length - k) * 0.25f;

                    Vector2 pos = projectile.oldPos[(int)k] + offsets;
                    pos += new Vector2(projectile.width, projectile.height) / 2f;

                    Color color = color2 * alpha;
                    float rot = projectile.oldRot[(int)k] - MathHelper.TwoPi * 0.375f;

                    spriteBatch.Draw(tex, pos, tex.Frame(), color, rot, tex.Size() / 2, projectile.scale*alpha, 0, 0);

                }
            }*/

            Vector2 pos2 = projectile.Center + offsets;
            float rot2 = projectile.rotation - MathHelper.TwoPi * 0.375f;

            spriteBatch.Draw(tex, pos2, tex.Frame(), color2, rot2, tex.Size() / 2, 1.2f + projectile.scale, 0, 0);

        }

    }

}