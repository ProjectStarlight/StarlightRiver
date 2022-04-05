using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Starwood
{
	class StarwoodStaffProjectile : ModProjectile, IDrawAdditive
    {
        public override string Texture => AssetDirectory.StarwoodItem + Name;

        public override void SetStaticDefaults() 
        {
            DisplayName.SetDefault("Starshot");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1; 
        }

        //These stats get scaled when empowered
        private int counterScore = 1;
        private Vector3 lightColor = new Vector3(0.2f, 0.1f, 0.05f);
        private int dustType = ModContent.DustType<Dusts.Stamina>();
        private bool empowered;

        private const int MaxTimeLeft = 60;

        public override void SetDefaults()
        {
            Projectile.timeLeft = MaxTimeLeft;
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
            Projectile.rotation = Main.rand.NextFloat(4f);
            Projectile.DamageType = DamageClass.Magic;
        }

        public override void AI()
        {
            if (Projectile.timeLeft == MaxTimeLeft) 
            {
                StarlightPlayer mp = Main.player[Projectile.owner].GetModPlayer<StarlightPlayer>();

                if (mp.empowered) 
                {
                    Projectile.frame = 1;
                    lightColor = new Vector3(0.05f, 0.1f, 0.2f);
                    counterScore = 2;
                    dustType = ModContent.DustType<Dusts.BlueStamina>();
                    empowered = true; 
                } 
            }

            if (Projectile.timeLeft % 50 == Projectile.ai[1])//delay between star sounds
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item9, Projectile.Center);

            Projectile.rotation += 0.3f;
            Lighting.AddLight(Projectile.Center, lightColor);
            Projectile.velocity = Projectile.velocity.RotatedBy(Math.Sin(Projectile.timeLeft * 0.2f) * Projectile.ai[0]);
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) 
        {
            target.GetGlobalNPC<StarwoodScoreCounter>().AddScore(counterScore, Projectile.owner, damage); 
        }

        public override void Kill(int timeLeft) 
        {
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
            for (int k = 0; k < 15; k++)
                Dust.NewDustPerfect(Projectile.Center, dustType, (Projectile.velocity * 0.1f * Main.rand.NextFloat(0.8f, 0.12f)).RotatedBy(Main.rand.NextFloat(-0.15f, 0.15f)), 0, default, 1.5f); 
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var tex = TextureAssets.Projectile[Projectile.type].Value;
            var source = new Rectangle(0, TextureAssets.Projectile[Projectile.type].Value.Height / 2 * Projectile.frame, TextureAssets.Projectile[Projectile.type].Value.Width, TextureAssets.Projectile[Projectile.type].Value.Height / 2);
            var origin = new Vector2(TextureAssets.Projectile[Projectile.type].Value.Width / 2, TextureAssets.Projectile[Projectile.type].Value.Height / 4);

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, source, Color.White, Projectile.rotation, origin, 1f, default, default);

            return false;
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            for (int k = 0; k < Projectile.oldPos.Length; k++) 
            {
                Color color = (empowered ? new Color(200, 220, 255) * 0.35f : new Color(255, 255, 200) * 0.3f) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);

                if (k <= 4) 
                    color *= 1.2f;

                float scale = Projectile.scale * (Projectile.oldPos.Length - k) / Projectile.oldPos.Length * 0.8f * 0.5f;
                Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Items/Starwood/Glow").Value;//TEXTURE PATH

                spriteBatch.Draw(tex, (Projectile.oldPos[k] + Projectile.Size / 2 + Projectile.Center) * 0.5f - Main.screenPosition, null, color, 0, tex.Size() / 2, scale, default, default); 
            }
        }
    }

    class StarwoodStaffFallingStar : ModProjectile, IDrawAdditive
    {
        public override string Texture => AssetDirectory.StarwoodItem + "StarwoodStarfallProjectile";

        public override void SetStaticDefaults() 
        {
            DisplayName.SetDefault("Falling Star");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1; 
        }

        //These stats get scaled when empowered
        private float ScaleMult = 1;
        private Vector3 lightColor = new Vector3(0.2f, 0.1f, 0.05f);
        private int dustType = ModContent.DustType<Dusts.Stamina>();
        private bool empowered;

        private const int MaxTimeLeft = 600;

        public override void SetDefaults()
        {
            Projectile.timeLeft = MaxTimeLeft;
            Projectile.width = 22;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
            Projectile.rotation = Main.rand.NextFloat(4f);
        }

        public override void AI()
        {
            if (Projectile.timeLeft == MaxTimeLeft) 
            {
                StarlightPlayer mp = Main.player[Projectile.owner].GetModPlayer<StarlightPlayer>();
                if (mp.empowered) 
                {
                    Projectile.frame = 1;
                    lightColor = new Vector3(0.05f, 0.1f, 0.2f);
                    ScaleMult = 1.5f;
                    dustType = ModContent.DustType<Dusts.BlueStamina>();
                    empowered = true;
                } 
            }

            float ToTarget = (Main.npc[(int)Projectile.ai[0]].Center - Projectile.Center).ToRotation();
            float VelDirection = Projectile.velocity.ToRotation();

            if (ToTarget > 0.785f && ToTarget < 2.355f && VelDirection > 0.785f && VelDirection < 2.355f)
                Projectile.velocity = Projectile.velocity.RotatedBy((ToTarget - VelDirection) * 0.3f);

            Projectile.rotation += 0.3f;
            Lighting.AddLight(Projectile.Center, lightColor);
        }

        public override void Kill(int timeLeft) 
        {
            Helpers.DustHelper.DrawStar(Projectile.Center, dustType, pointAmount: 5, mainSize: 2f * ScaleMult, dustDensity: 1f, pointDepthMult: 0.3f);
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
            for (int k = 0; k < 50; k++)
                Dust.NewDustPerfect(Projectile.Center, dustType, Vector2.One.RotatedByRandom(6.28f) * (Main.rand.NextFloat(0.25f, 1.7f) * ScaleMult), 0, default, 1.5f); 
        }

        public override bool PreDraw(ref Color lightColor) 
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, new Rectangle(0, empowered ? 24 : 0, 22, 24), Color.White, Projectile.rotation, new Vector2(11, 12), Projectile.scale, default, default);
            return false; 
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Color color = (empowered ? new Color(200, 220, 255) * 0.35f : new Color(255, 255, 200) * 0.3f) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                if (k <= 4) color *= 1.2f;
                float scale = Projectile.scale * (Projectile.oldPos.Length - k) / Projectile.oldPos.Length * 0.8f;
                Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Items/Starwood/Glow").Value;//TEXTURE PATH

                spriteBatch.Draw(tex, (Projectile.oldPos[k] + Projectile.Size / 2 + Projectile.Center) * 0.50f - Main.screenPosition, null, color, 0, tex.Size() / 2, scale, default, default);
            }
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Color color = (empowered ? new Color(200, 220, 255) * 0.35f : new Color(255, 255, 200) * 0.3f) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                if (k <= 4) color *= 1.2f;
                float scale = Projectile.scale * (Projectile.oldPos.Length - k) / Projectile.oldPos.Length * 0.8f;
                Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Items/Starwood/StarwoodStarfallGlowTrail").Value;//TEXTURE PATH

                spriteBatch.Draw(tex, (Projectile.oldPos[k] + Projectile.Size / 2 + Projectile.Center) * 0.50f - Main.screenPosition, null, color, 0, tex.Size() / 2, scale, default, default);
            }
        }
    }

    internal class StarwoodScoreCounter : GlobalNPC
    {
        private int score = 0;
        private int resetCounter = 0;
        private int lasthitPlayer = 255;
        private int lasthitDamage = 0;
        public void AddScore(int scoreAmount, int PlayerIndex, int damage) {
            score += scoreAmount;
            resetCounter = 0;
            lasthitPlayer = PlayerIndex;
            lasthitDamage = damage; }

        public override bool InstancePerEntity => true;
        public override void PostAI(NPC npc)
        {
            if (score > 0)
            {
                resetCounter++;
                if (score >= 3)
                {
                    float rotationAmount = Main.rand.NextFloat(-0.3f, 0.3f);

                    StarlightPlayer mp = Main.player[lasthitPlayer].GetModPlayer<StarlightPlayer>();
                    float speed = (mp.empowered ? 16 : 14) * Main.rand.NextFloat(0.9f, 1.1f);

                    Vector2 position = new Vector2(npc.Center.X, npc.Center.Y - 700).RotatedBy(rotationAmount, npc.Center);
                    Vector2 velocity = (Vector2.Normalize(npc.Center + new Vector2(0, -20) - position) * speed + npc.velocity / (speed / 1.5f) * 10f) * (Math.Abs(rotationAmount) + 1f);

                    Projectile.NewProjectile(npc.GetSpawnSource_NPCHurt(), position, velocity, ModContent.ProjectileType<StarwoodStaffFallingStar>(), lasthitDamage * 3, 1, lasthitPlayer, npc.whoAmI);

                    score = 0;
                    resetCounter = 0;
                }
                else if (resetCounter > 60) 
                {
                    score = 0;
                    resetCounter = 0; 
                }
            }
        }
    }
}
