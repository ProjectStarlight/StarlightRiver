using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Starwood
{
    public class StarwoodBoomerangProjectile : ModProjectile, IDrawAdditive
    {
        public override string Texture => AssetDirectory.StarwoodItem + Name;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Starwood Boomerang");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[projectile.type] = 1;
        }

        private const int maxChargeTime = 50;//how long it takes to charge up

        private float chargeMult;//a multiplier used during charge up, used both in ai and for drawing (goes from 0 to 1)

        //These stats get scaled when empowered
        private int ScaleMult = 2;
        private Vector3 lightColor = new Vector3(0.4f, 0.2f, 0.1f);
        private int dustType = DustType<Dusts.Stamina>();
        private bool empowered = false;

        private const int MaxTimeLeft = 1200;
        private const int MaxDistTime = MaxTimeLeft - 30;
        public override void SetDefaults()
        {
            projectile.timeLeft = MaxTimeLeft;
            projectile.width = 18;
            projectile.height = 18;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.tileCollide = true;
            projectile.ignoreWater = false;
            projectile.aiStyle = -1;
        }

        public override void AI()
        {
            Player projOwner = Main.player[projectile.owner];

            projectile.rotation += 0.3f;

            if (projectile.timeLeft == MaxTimeLeft) {
                StarlightPlayer mp = Main.player[projectile.owner].GetModPlayer<StarlightPlayer>();
                if (mp.Empowered) {
                    projectile.frame = 1;
                    lightColor = new Vector3(0.1f, 0.2f, 0.4f);
                    ScaleMult = 3;
                    dustType = DustType<Dusts.BlueStamina>();
                    empowered = true; } }

            Lighting.AddLight(projectile.Center, lightColor * 0.5f);

            switch (projectile.ai[0])
            {
                case 0://flying outward
                    if (empowered) {
                        projectile.velocity += Vector2.Normalize(Main.MouseWorld - projectile.Center);
                        if (projectile.velocity.Length() > 10)//swap this for shootspeed or something
                            projectile.velocity = Vector2.Normalize(projectile.velocity) * 10; }//cap to max speed

                    if (projectile.timeLeft < MaxDistTime)
                        NextPhase(0);

                    break;

                case 1://has hit something
                    if (projOwner.controlUseItem || projectile.ai[1] >= maxChargeTime - 5)
                    {
                        if (projectile.ai[1] == 0)
                            Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/ImpactHeal"), projectile.Center);

                        chargeMult = projectile.ai[1] / (maxChargeTime + 3);
                        projectile.ai[1]++;
                        projectile.velocity *= 0.75f;
                        Lighting.AddLight(projectile.Center, lightColor * chargeMult);

                        if (projectile.ai[1] >= maxChargeTime + 3) {//reset stats and start return phase
                            projectile.position = projectile.Center;
                            projectile.width = 18;
                            projectile.height = 18;
                            projectile.Center = projectile.position;
                            for (int k = 0; k < projectile.oldPos.Length; k++)
                                projectile.oldPos[k] = projectile.position;
                            NextPhase(1); }//ai[]s reset here
                        else if (projectile.ai[1] == maxChargeTime){//change hitbox size, stays for 3 frames
                            projectile.position = projectile.Center;
                            projectile.width = 67 * ScaleMult;
                            projectile.height = 67 * ScaleMult;
                            projectile.Center = projectile.position;
                            for (int k = 0; k < projectile.oldPos.Length; k++)
                                projectile.oldPos[k] = projectile.position; }
                        else if (projectile.ai[1] == maxChargeTime - 5){//sfx
                            Helpers.DustHelper.DrawStar(projectile.Center, dustType, pointAmount: 5, mainSize: 2.25f * ScaleMult, dustDensity: 2, pointDepthMult: 0.3f);
                            Lighting.AddLight(projectile.Center, lightColor * 2);
                            Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/MagicAttack"), projectile.Center);
                            for (int k = 0; k < 50; k++)
                                Dust.NewDustPerfect(projectile.Center, dustType, Vector2.One.RotatedByRandom(6.28f) * (Main.rand.NextFloat(0.25f, 1.5f) * ScaleMult), 0, default, 1.5f); }
                    }
                    else
                        NextPhase(1); // ai[]s and damage reset here
                    break;
                case 2://heading back
                    if (Vector2.Distance(projOwner.Center, projectile.Center) < 24)
                        projectile.Kill();
                    else if (Vector2.Distance(projOwner.Center, projectile.Center) < 200)
                        projectile.velocity += Vector2.Normalize(projOwner.Center - projectile.Center) * 4;
                    else
                        projectile.velocity += Vector2.Normalize(projOwner.Center - projectile.Center);

                    if (projectile.velocity.Length() > 10)//swap this for shootspeed or something
                        projectile.velocity = Vector2.Normalize(projectile.velocity) * 10;//cap to max speed
                    break;
            }

            if (projectile.ai[0] != 1)
                if (projectile.timeLeft % 8 == 0) {
                    Main.PlaySound(SoundID.Item7, projectile.Center);
                    Dust.NewDustPerfect(projectile.Center, dustType, (projectile.velocity * 0.5f).RotatedByRandom(0.5f), Scale: Main.rand.NextFloat(0.8f, 1.5f)); }
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (projectile.ai[0] == 1) {
                if (projectile.ai[1] >= maxChargeTime - 3 && projectile.ai[1] <= maxChargeTime + 3) {
                    if (empowered) {
                        damage *= ScaleMult;
                        knockback *= ScaleMult; }
                    else {
                        damage *= ScaleMult;
                        knockback *= ScaleMult; } }
                else {
                    damage = ScaleMult;
                    knockback *= 0.1f; } }
            else if (empowered)
                damage += 3;
        }

        public override bool OnTileCollide(Vector2 oldVelocity) {
            NextPhase(0, true);
            return false; }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) => NextPhase(0, true);
        public override void OnHitPlayer(Player target, int damage, bool crit) => NextPhase(0, true);

        private Texture2D GlowingTrail => GetTexture(AssetDirectory.StarwoodItem + "StarwoodBoomerangGlowTrail");
        private Texture2D GlowingTexture => GetTexture(AssetDirectory.StarwoodItem + "StarwoodBoomerangGlow");
        private Texture2D AuraTexture => GetTexture(AssetDirectory.StarwoodItem + "Glow");//TEXTURE PATH

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Vector2 drawOrigin = new Vector2(Main.projectileTexture[projectile.type].Width * 0.5f, projectile.height * 0.5f);

            if (projectile.ai[0] != 1)
                for (int k = 0; k < projectile.oldPos.Length; k++) {
                    Color color = projectile.GetAlpha(Color.White) * ((projectile.oldPos.Length - k) / (float)projectile.oldPos.Length * 0.5f);
                    float scale = projectile.scale * (projectile.oldPos.Length - k) / projectile.oldPos.Length;

                    spriteBatch.Draw(GlowingTrail,
                    projectile.oldPos[k] + drawOrigin - Main.screenPosition,
                    new Rectangle(0, Main.projectileTexture[projectile.type].Height / 2 * projectile.frame, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height / 2),
                    color,
                    projectile.rotation,
                    new Vector2(Main.projectileTexture[projectile.type].Width / 2, Main.projectileTexture[projectile.type].Height / 4),
                    scale, default, default); }

            spriteBatch.Draw(Main.projectileTexture[projectile.type],
                projectile.Center - Main.screenPosition,
                new Rectangle(0, Main.projectileTexture[projectile.type].Height / 2 * projectile.frame, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height / 2),
                lightColor,
                projectile.rotation,
                new Vector2(Main.projectileTexture[projectile.type].Width / 2, Main.projectileTexture[projectile.type].Height / 4),
                1f, default, default);

            return false;
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Texture2D tex = AuraTexture;
            for (int k = 0; k < projectile.oldPos.Length; k++)
            {
                if (!(projectile.ai[0] == 1 && (projectile.oldPos[k] / 5).ToPoint() == (projectile.position / 5).ToPoint()))
                {
                    Color color = (empowered ? new Color(70, 90, 100) : new Color(100, 90, 60)) * ((projectile.oldPos.Length - k) / (float)projectile.oldPos.Length);
                    if (k <= 4)
                        color *= 1.2f;
                    float scale = projectile.scale * (projectile.oldPos.Length - k) / projectile.oldPos.Length * 0.8f;

                    spriteBatch.Draw(tex, projectile.oldPos[k] + projectile.Size / 2 - Main.screenPosition, null, color, 0, tex.Size() * 0.5f, scale * 0.5f, default, default);
                }
            }

            Texture2D tex2 = GetTexture(AssetDirectory.StarwoodItem + "Glow2");//a
            spriteBatch.Draw(tex2, projectile.Center - Main.screenPosition, tex2.Frame(), new Color(255, 255, 200, 75) * (projectile.ai[1] / maxChargeTime), 0, tex2.Size() * 0.5f, (-chargeMult + 1) * 1f, 0, 0);
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Color color = Color.White * (chargeMult + 0.25f);

            spriteBatch.Draw(GlowingTexture,
                projectile.Center - Main.screenPosition,
                new Rectangle(0, GlowingTexture.Height / 2 * projectile.frame, GlowingTexture.Width, GlowingTexture.Height / 2),
                color,
                projectile.rotation,
                new Vector2(GlowingTexture.Width / 2, GlowingTexture.Height / 4),
                1f, default, default);

            //Chain.DrawRope(spriteBatch, ChainDrawMethod); //chain example
        }

        /*private void ChainDrawMethod(SpriteBatch spriteBatch, int i, Vector2 position, Vector2 prevPosition, Vector2 nextPosition) //chain example
        {
            if(nextPosition != Vector2.Zero)
            {
                switch (i)
                {
                    case 0:
                        Helper.DrawLine(spriteBatch, position - Main.screenPosition, nextPosition - Main.screenPosition, worm1, Color.White, 32);
                        break;
                    case 6:
                        Helper.DrawLine(spriteBatch, position - Main.screenPosition, nextPosition - Main.screenPosition, worm3, Color.White, 32);
                        break;
                    default:
                        Helper.DrawLine(spriteBatch, position - Main.screenPosition, nextPosition - Main.screenPosition, worm2, Color.White, 32);
                        break;
                }
                //Helper.DrawLine(spriteBatch, position - Main.screenPosition, nextPosition - Main.screenPosition, Main.blackTileTexture, Color.White, (int)((-((float)i / Chain.segmentCount) + 1) * 20));
            }

            //spriteBatch.Draw(GlowingTrail,
            //    position - Main.screenPosition,
            //    new Rectangle(0, (Main.projectileTexture[projectile.type].Height / 2) * projectile.frame, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height / 2),
            //    Color.White,
            //    0f,
            //    new Vector2(GlowingTrail.Width / 2, GlowingTrail.Height / 4),
            //    0.50f, default, default);
        }*/

        #region phase change void
        private void NextPhase(int phase, bool bounce = false)
        {
            if (phase == 0 && projectile.ai[0] == phase)
            {
                if (bounce)
                    projectile.velocity = -projectile.velocity;

                projectile.tileCollide = false;
                projectile.ignoreWater = true;
                projectile.ai[0] = 1;
            }
            else if (phase == 1 && projectile.ai[0] == phase)
            {
                //projectile.damage = oldDamage / 2;//half damage on the way back
                projectile.velocity.Y += 1f;
                projectile.ai[0] = 2;
                projectile.ai[1] = 0;
            }
        }
        #endregion
    }
}
