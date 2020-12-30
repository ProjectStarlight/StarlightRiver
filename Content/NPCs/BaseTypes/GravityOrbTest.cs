using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.NPCs.BaseTypes
{
    class GravityOrbTest : ModNPC, IDrawAdditive
    {
        public int radius = 100;
        public int attract = 100;

        public override string Texture => AssetDirectory.Debug;

        public override void SetDefaults()
        {
            npc.lifeMax = 2;
            npc.life = 2;
            npc.immortal = true;
            npc.dontTakeDamage = true;
            npc.aiStyle = -1;
            npc.noGravity = true;
            npc.width = 200;
            npc.height = 200;
            npc.knockBackResist = 0;
        }

        public override void AI()
        {
            for (int k = 0; k < Main.maxPlayers; k++)
            {
                Player player = Main.player[k];
                if (!player.active) return;
                GravityPlayer mp = player.GetModPlayer<GravityPlayer>();

                if (Vector2.DistanceSquared(player.Center, npc.Center) < (radius + attract) * (radius + attract) && mp.cooldown <= 0 && mp.controller is null)
                {
                    mp.controller = this;
                    mp.angle = (player.Center - npc.Center).ToRotation();
                    mp.attractSpeed = GetComponent(player.velocity, (player.Center - npc.Center).ToRotation()).Length() / 2f;
                    if (mp.attractSpeed < 1) mp.attractSpeed = 1;
                }
            }
        }

        private Vector2 GetComponent(Vector2 vector, float angle)
        {
            float oldAngle = vector.ToRotation();
            float x = (float)Math.Cos(oldAngle) - (float)Math.Cos(angle);
            float y = (float)Math.Sin(oldAngle) - (float)Math.Sin(angle);
            return new Vector2(vector.X * x, vector.Y * y);
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            for (int k = 0; k < 6; k++)
            {
                float time = (Main.GameUpdateCount + k / (6 / 4f) * 200) / 200f;

                var tex = GetTexture("StarlightRiver/Assets/Tiles/Interactive/WispSwitchGlow2");
                float rad = (radius + attract) * 2 / (float)tex.Width;
                spriteBatch.Draw(tex, npc.Center - Main.screenPosition, null, Color.Cyan * (time % rad / (rad / 4)) * 0.4f, 0, tex.Size() / 2, rad - time % rad, 0, 0);
            }
        }
    }

    class GravityPlayer : ModPlayer
    {
        public GravityOrbTest controller = null;
        public float angle = 0;
        public int cooldown = 0;
        public float attractSpeed = 0;

        int angleTimer = 0;
        float realAngle => angleTimer / 59f * angle;


        public override void ModifyDrawInfo(ref PlayerDrawInfo drawInfo)
        {
            if (controller != null && controller.npc.active || angleTimer > 0)
            {
                drawInfo.drawPlayer.fullRotationOrigin = drawInfo.drawPlayer.Size / 2;
                drawInfo.drawPlayer.fullRotation = realAngle + (float)Math.PI * 0.5f * angleTimer / 59f;
            }
            else
            {
                drawInfo.drawPlayer.fullRotationOrigin = Vector2.Zero;
                drawInfo.drawPlayer.fullRotation = 0;
            }
        }

        public override void PostUpdate()
        {
            if (controller != null && controller.npc.active)
            {
                player.MountedCenter = controller.npc.Center + Vector2.UnitX.RotatedBy(angle) * (controller.radius + player.height / 2 + (controller.attract - controller.attract * (angleTimer / 60f)));
                angle += player.velocity.X / (controller.radius + player.height / 2);
                angle %= (float)(Math.PI * 2);

                if (player.justJumped && angleTimer >= 60)
                {
                    controller = null;
                    player.velocity = Vector2.UnitX.RotatedBy(angle) * 10;
                    player.jump = 0;
                    cooldown = 30;
                }

                if (angleTimer < 60)
                {
                    angleTimer += (int)attractSpeed;
                    attractSpeed += player.gravity * (float)Math.Max(0, Math.Sin(angle));
                }
                if (angleTimer > 60) angleTimer = 60;
            }
            else
            {
                if (angleTimer > 0) angleTimer -= 3;
                if (cooldown > 0) cooldown--;
                controller = null;
            }
        }
    }
}
