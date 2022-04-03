using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Helpers;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.NPCs.Permafrost
{
	internal class StrangeLight : ModNPC
    {
        public override string Texture => "StarlightRiver/Assets/Invisible";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Strange Light");
            Main.npcFrameCount[NPC.type] = 1;
        }

        public override void SetDefaults()
        {
            NPC.width = 16;
            NPC.height = 16;
            NPC.damage = 18;
            NPC.defense = 12;
            NPC.noGravity = true;
            NPC.lifeMax = 5000;
            NPC.friendly = false;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = 500f;
            NPC.alpha = 255;
            NPC.knockBackResist = 0.2f;
        }

        //NPC.ai[0]: phase
        //NPC.ai[1]: x sin timer
        //NPC.ai[2]: y sin timer
        int numOfTentacles = 5;
        bool spawnedTentacles = false;
        int[] tentacles = new int[5];

        public override void AI()
        {
            NPC.TargetClosest(true);
            Player Player = Main.player[NPC.target];
            if (!spawnedTentacles)
            {
                spawnedTentacles = true;
                for (int i = 0; i < numOfTentacles; i++)
                    tentacles[i] = Projectile.NewProjectile(NPC.Center, Vector2.Zero, ModContent.ProjectileType<StrangeTentacle>(), NPC.damage, NPC.knockBackResist, Player.whoAmI, NPC.whoAmI, i / 4);
            }
            NPC.ai[1] += 0.05f;
            if (NPC.ai[1] > 21)
                Projectile.NewProjectile(NPC.Center, Vector2.UnitY.RotatedBy(Main.projectile[3].rotation + 1.57) * 10, ModContent.ProjectileType<StrangePredictor>(), 0, 0);
            if (NPC.ai[1] > 29.5)
                NPC.ai[1] = 0;
        }
    }
    internal class StrangePredictor : ModProjectile
    {
        public override string Texture => "StarlightRiver/Assets/Invisible";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Strange Light");
        }

        public override void SetDefaults()
        {
            Projectile.hostile = true;
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.damage = 1;
            Projectile.penetrate = -1;
            Projectile.alpha = 255;
            Projectile.timeLeft = 30;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 5;
        }

        public override void AI()
        {
            Dust.NewDustPerfect(Projectile.position, 6).noGravity = true;
        }
    }

    internal class StrangeTentacle : ModProjectile
    {
        public override string Texture => "StarlightRiver/Assets/Invisible";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Strange Light");
        }

        public override void SetDefaults()
        {
            Projectile.hostile = true;
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.damage = 1;
            Projectile.penetrate = -1;
            Projectile.alpha = 0;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        float rotationCounter = 0;
        Vector2 control1;
        Vector2 control2;

        int circleX = 0;
        int circleY = 0;
        float circleSpeed = 0.05f;
        float extend = 1;
        public override void AI()
        {
            Projectile.timeLeft = 2;
            Vector2 posToBe = Vector2.UnitY * 100;
            Player Player = Projectile.Owner();
            NPC parent = Main.npc[(int)Projectile.ai[0]];

            if (circleX == 0)
            {
                circleSpeed = Main.rand.NextFloat(0.03f, 0.06f);
                circleX = Main.rand.Next(20, 40);
                circleY = Main.rand.Next(20, 40);
            }

            Projectile.ai[1] += 0.05f;

            if (Projectile.ai[1] < 21)
                if (extend > 1f)
                    extend -= 0.2f;
                else if (Math.Abs(Projectile.rotation - (parent.Center - Player.Center).ToRotation()) > 0.1f)
                    Projectile.rotation = (parent.Center - Player.Center).ToRotation();

            if (Projectile.ai[1] > 21 && Projectile.ai[1] < 22f)
            {
                extend -= 0.03f;
                Projectile.rotation = (parent.Center - Player.Center).ToRotation();
            }

            if (Projectile.ai[1] > 28)
                extend += 0.13f;

            if (Projectile.ai[1] > 29.5)
                Projectile.ai[1] = 0;

            rotationCounter += circleSpeed;

            float speed = 0.5f;
            Vector2 circle = new Vector2(circleX * (float)Math.Sin(rotationCounter), circleY * (float)Math.Cos(rotationCounter));
            float angle = Projectile.rotation;
            posToBe *= extend;
            posToBe += circle;
            posToBe = posToBe.RotatedBy(angle + 1.57);
            control1 = new Vector2(50 * (float)Math.Sin((double)rotationCounter * 1.1f), 50 * (float)Math.Cos((double)rotationCounter * 1.1f)) + parent.position;
            control2 = posToBe.RotatedBy(Math.Sin(rotationCounter)) + parent.Center;
            posToBe += parent.Center;
            Vector2 direction = posToBe - Projectile.position;

            if (direction.Length() > 1000)
                Projectile.position = posToBe;
            else
            {
                speed *= (float)Math.Sqrt(direction.Length());
                direction.Normalize();
                direction *= speed;

                Projectile.velocity = direction;
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            NPC parent = Main.npc[(int)Projectile.ai[0]];
            if (parent.active)
            {
                Texture2D tex = Main.projectileTexture[Projectile.type];
                float dist = (Projectile.position - parent.Center).Length();
                TentacleDraw.DrawBezier(spriteBatch, lightColor, tex, Projectile.Center, parent.Center, control1, control2, tex.Height / dist / 2, Projectile.rotation);
            }
            else
                Projectile.active = false;
            return false;
        }
    }
    internal static class TentacleDraw
    {
        public static void DrawBezier(SpriteBatch spriteBatch, Color lightColor, Texture2D texture, Vector2 endpoint, Vector2 startPoint, Vector2 c1, Vector2 c2, float chainsPerUse, float rotDis = 0f)
        {
            float width = texture.Width;
            float length = (startPoint - endpoint).Length();

            for (float i = 0; i <= 1; i += chainsPerUse)
            {
                float sin = 1 + (float)Math.Sin(i * length / 10);
                float cos = 1 + (float)Math.Cos(i * length / 10);
                Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);
                Vector2 distBetween;
                float projTrueRotation;

                if (i != 0)
                {
                    float x = EX(i, startPoint.X, c1.X, c2.X, endpoint.X);
                    float y = WHY(i, startPoint.Y, c1.Y, c2.Y, endpoint.Y);
                    distBetween = new Vector2(x -
                   EX(i - chainsPerUse, startPoint.X, c1.X, endpoint.X),
                   y -
                   WHY(i - chainsPerUse, startPoint.Y, c1.Y, endpoint.Y));
                    projTrueRotation = distBetween.ToRotation() - MathHelper.PiOver2 + rotDis;
                    Main.spriteBatch.Draw(texture, new Vector2(x - Main.screenPosition.X, y - Main.screenPosition.Y),
                   new Rectangle(0, 0, texture.Width, texture.Height), color, projTrueRotation,
                   new Vector2(texture.Width * 0.5f, texture.Height * 0.5f), 1, SpriteEffects.None, 0);
                }
            }
        }
        #region os's code
        public static float EX(float t,
        float x0, float x1, float x2, float x3)
        {
            return (float)(
                x0 * Math.Pow(1 - t, 3) +
                x1 * 3 * t * Math.Pow(1 - t, 2) +
                x2 * 3 * Math.Pow(t, 2) * (1 - t) +
                x3 * Math.Pow(t, 3)
            );
        }

        public static float WHY(float t,
            float y0, float y1, float y2, float y3)
        {
            return (float)(
                 y0 * Math.Pow(1 - t, 3) +
                 y1 * 3 * t * Math.Pow(1 - t, 2) +
                 y2 * 3 * Math.Pow(t, 2) * (1 - t) +
                 y3 * Math.Pow(t, 3)
             );
        }
        public static float EX(float t,
   float x0, float x1, float x2)
        {
            return (float)(
                x0 * Math.Pow(1 - t, 2) +
                x1 * 2 * t * (1 - t) +
                x2 * Math.Pow(t, 2)
            );
        }

        public static float WHY(float t,
            float y0, float y1, float y2)
        {
            return (float)(
                y0 * Math.Pow(1 - t, 2) +
                y1 * 2 * t * (1 - t) +
                y2 * Math.Pow(t, 2)
            );
        }
        #endregion

    }
}