using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.NPCs.Permafrost
{
    internal class StrangeLight : ModNPC
    {
        public override string Texture => "StarlightRiver/Assets/Invisible";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Strange Light");
            Main.npcFrameCount[npc.type] = 1;
        }

        public override void SetDefaults()
        {
            npc.width = 16;
            npc.height = 16;
            npc.damage = 18;
            npc.defense = 12;
            npc.noGravity = true;
            npc.lifeMax = 5000;
            npc.friendly = false;
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath1;
            npc.value = 500f;
            npc.alpha = 255;
            npc.knockBackResist = 0.2f;
        }

        //npc.ai[0]: phase
        //npc.ai[1]: x sin timer
        //npc.ai[2]: y sin timer
        int numOfTentacles = 5;
        bool spawnedTentacles = false;
        int[] tentacles = new int[5];

        public override void AI()
        {
            npc.TargetClosest(true);
            Player player = Main.player[npc.target];
            if (!spawnedTentacles)
            {
                spawnedTentacles = true;
                for (int i = 0; i < numOfTentacles; i++)
                    tentacles[i] = Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<StrangeTentacle>(), npc.damage, npc.knockBackResist, player.whoAmI, npc.whoAmI, i / 4);
            }
            npc.ai[1] += 0.05f;
            if (npc.ai[1] > 21)
                Projectile.NewProjectile(npc.Center, Vector2.UnitY.RotatedBy(Main.projectile[3].rotation + 1.57) * 10, ModContent.ProjectileType<StrangePredictor>(), 0, 0);
            if (npc.ai[1] > 29.5)
                npc.ai[1] = 0;
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
            projectile.hostile = true;
            projectile.width = 2;
            projectile.height = 2;
            projectile.aiStyle = -1;
            projectile.friendly = false;
            projectile.damage = 1;
            projectile.penetrate = -1;
            projectile.alpha = 255;
            projectile.timeLeft = 30;
            projectile.tileCollide = true;
            projectile.ignoreWater = true;
            projectile.extraUpdates = 5;
        }

        public override void AI()
        {
            Dust.NewDustPerfect(projectile.position, 6).noGravity = true;
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
            projectile.hostile = true;
            projectile.width = 24;
            projectile.height = 24;
            projectile.aiStyle = -1;
            projectile.friendly = false;
            projectile.damage = 1;
            projectile.penetrate = -1;
            projectile.alpha = 0;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
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
            projectile.timeLeft = 2;
            Vector2 posToBe = Vector2.UnitY * 100;
            Player player = projectile.Owner();
            NPC parent = Main.npc[(int)projectile.ai[0]];

            if (circleX == 0)
            {
                circleSpeed = Main.rand.NextFloat(0.03f, 0.06f);
                circleX = Main.rand.Next(20, 40);
                circleY = Main.rand.Next(20, 40);
            }

            projectile.ai[1] += 0.05f;

            if (projectile.ai[1] < 21)
                if (extend > 1f)
                    extend -= 0.2f;
                else if (Math.Abs(projectile.rotation - (parent.Center - player.Center).ToRotation()) > 0.1f)
                    projectile.rotation = (parent.Center - player.Center).ToRotation();

            if (projectile.ai[1] > 21 && projectile.ai[1] < 22f)
            {
                extend -= 0.03f;
                projectile.rotation = (parent.Center - player.Center).ToRotation();
            }

            if (projectile.ai[1] > 28)
                extend += 0.13f;

            if (projectile.ai[1] > 29.5)
                projectile.ai[1] = 0;

            rotationCounter += circleSpeed;

            float speed = 0.5f;
            Vector2 circle = new Vector2(circleX * (float)Math.Sin(rotationCounter), circleY * (float)Math.Cos(rotationCounter));
            float angle = projectile.rotation;
            posToBe *= extend;
            posToBe += circle;
            posToBe = posToBe.RotatedBy(angle + 1.57);
            control1 = new Vector2(50 * (float)Math.Sin((double)rotationCounter * 1.1f), 50 * (float)Math.Cos((double)rotationCounter * 1.1f)) + parent.position;
            control2 = posToBe.RotatedBy(Math.Sin(rotationCounter)) + parent.Center;
            posToBe += parent.Center;
            Vector2 direction = posToBe - projectile.position;

            if (direction.Length() > 1000)
                projectile.position = posToBe;
            else
            {
                speed *= (float)Math.Sqrt(direction.Length());
                direction.Normalize();
                direction *= speed;

                projectile.velocity = direction;
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            NPC parent = Main.npc[(int)projectile.ai[0]];
            if (parent.active)
            {
                Texture2D tex = Main.projectileTexture[projectile.type];
                float dist = (projectile.position - parent.Center).Length();
                TentacleDraw.DrawBezier(spriteBatch, lightColor, tex, projectile.Center, parent.Center, control1, control2, tex.Height / dist / 2, projectile.rotation);
            }
            else
                projectile.active = false;
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
        #region os's shit
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