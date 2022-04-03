using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.BaseTypes
{
	class GravityOrbTest : ModNPC, IDrawAdditive
    {
        public int radius = 100;
        public int attract = 100;

        public override string Texture => AssetDirectory.Debug;

        public override void SetDefaults()
        {
            NPC.lifeMax = 2;
            NPC.life = 2;
            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            NPC.aiStyle = -1;
            NPC.noGravity = true;
            NPC.width = 200;
            NPC.height = 200;
            NPC.knockBackResist = 0;
        }

        public override void AI()
        {
            for (int k = 0; k < Main.maxPlayers; k++)
            {
                Player Player = Main.player[k];
                if (!Player.active) return;
                GravityPlayer mp = Player.GetModPlayer<GravityPlayer>();

                if (Vector2.DistanceSquared(Player.Center, NPC.Center) < (radius + attract) * (radius + attract) && mp.cooldown <= 0 && mp.controller is null)
                {
                    mp.controller = this;
                    mp.angle = (Player.Center - NPC.Center).ToRotation();
                    mp.attractSpeed = GetComponent(Player.velocity, (Player.Center - NPC.Center).ToRotation()).Length() / 2f;
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

                var tex = Request<Texture2D>("StarlightRiver/Assets/Tiles/Interactive/WispSwitchGlow2").Value;
                float rad = (radius + attract) * 2 / (float)tex.Width;
                spriteBatch.Draw(tex, NPC.Center - Main.screenPosition, null, Color.Cyan * (time % rad / (rad / 4)) * 0.4f, 0, tex.Size() / 2, rad - time % rad, 0, 0);
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


		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
		{
            if (controller != null && controller.NPC.active || angleTimer > 0)
            {
                drawInfo.drawPlayer.fullRotationOrigin = drawInfo.drawPlayer.Size / 2;
                drawInfo.drawPlayer.fullRotation = realAngle + (float)Math.PI * 0.5f * angleTimer / 59f;
            }
        }

        public override void PostUpdate()
        {
            if (controller != null && controller.NPC.active)
            {
                Player.MountedCenter = controller.NPC.Center + Vector2.UnitX.RotatedBy(angle) * (controller.radius + Player.height / 2 + (controller.attract - controller.attract * (angleTimer / 60f)));
                angle += Player.velocity.X / (controller.radius + Player.height / 2);
                angle %= (float)(Math.PI * 2);

                if (Player.justJumped && angleTimer >= 60)
                {
                    controller = null;
                    Player.velocity = Vector2.UnitX.RotatedBy(angle) * 10;
                    Player.jump = 0;
                    cooldown = 30;
                }

                if (angleTimer < 60)
                {
                    angleTimer += (int)attractSpeed;
                    attractSpeed += Player.gravity * (float)Math.Max(0, Math.Sin(angle));
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
