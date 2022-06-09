using Microsoft.Xna.Framework;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Helpers;
using System;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Core //TODO: Move this somewhere else? not sure.
{
	class SwimPlayer : ModPlayer
    {
        int boostCD = 0;
        float targetRotation = 0;
        float realRotation = 0;
        int emergeTime = 0;
        public bool ShouldSwim { get; set; }
        public float SwimSpeed { get; set; }

        private void CheckAuroraSwimming() //checks for if hte Player should be swimming
        {
            bool canSwim = Player.grapCount <= 0 && !Player.mount.Active;
            if (canSwim)
            {
                if (Player.HasBuff(BuffType<PrismaticDrown>())) //TODO: Change this to be set on the arena instead of checking for this buff probably
                {
                    ShouldSwim = true;
                    SwimSpeed *= 0.7f;
                }

                for (int x = 0; x < 2; x++)
                    for (int y = 0; y < 3; y++)
                    {
                        int realX = (int)(Player.position.X / 16) + x;
                        int realY = (int)(Player.position.Y / 16) + y;

                        if (WorldGen.InWorld(realX, realY))
                        {
                            Tile tile = Framing.GetTileSafely(realX, realY);
                            if (tile.Get<AuroraWaterData>().HasAuroraWater) //TODO: Integrate with properly ported aurora water system
                            {
                                ShouldSwim = true;
                                SwimSpeed *= 0.7f;
                            }
                        }
                    }
            }
		}

        public override void PreUpdate()
        {
            CheckAuroraSwimming();

            if (!ShouldSwim) //reset stuff when the Player isnt swimming
            {
                if (boostCD > 0)
                {
                    boostCD = 0;
                    Player.UpdateRotation(0);
                }

                if (emergeTime <= 0) //20 frames for the Player to rotate back
                    return;
            }

            targetRotation = ShouldSwim ? Player.velocity.ToRotation() : 1.57f + 3.14f;

            realRotation %= 6.28f; //handles the rotation, ensures the Player wont randomly snap to rotation when entering/leaving swimming

            if (Math.Abs(targetRotation - realRotation) % 6.28f > 0.21f)
            {
                float Mod(float a, float b) => a % b > 0 ? a % b : a % b + b;
                if (Mod(targetRotation, 6.28f) > Mod(realRotation, 6.28f))
                    realRotation += 0.2f;
                else
                    realRotation -= 0.2f;
            }
            else realRotation = targetRotation;

            Player.fullRotationOrigin = Player.Size / 2; //so the Player rotates around their center... why is this not the default?
            Player.fullRotation = realRotation + MathHelper.PiOver2;

            if (Player.itemAnimation != 0 && Player.HeldItem.useStyle != Terraria.ID.ItemUseStyleID.Swing && Player.itemAnimation == Player.itemAnimationMax - 1) //corrects the rotation on used Items
                Player.itemRotation -= realRotation + 1.57f;

            if (!ShouldSwim) //return later so rotation logic still runs
                return;

            Player.wingTime = -1;
            emergeTime = 20; //20 frames for the Player to rotate back, reset while swimming

            if (Player.itemAnimation == 0)
                Player.bodyFrame = new Rectangle(0, 56 * (int)(1 + Main.GameUpdateCount / 10 % 5), 40, 56);

            Player.legFrame = new Rectangle(0, 56 * (int)(5 + Main.GameUpdateCount / 7 % 3), 40, 56);

            float speed = 0.2f * SwimSpeed;
            if (Player.controlRight) Player.velocity.X += speed; //there should probably be a better way of doing this?
            if (Player.controlLeft) Player.velocity.X -= speed;
            if (Player.controlDown) Player.velocity.Y += speed;
            if (Player.controlUp) Player.velocity.Y -= speed;

            //Player.velocity.Y -= 0.4125f; //this combats vanilla gravity.
            //so does this!
            Player.gravity = 0;
            Player.velocity *= 0.95f;

            if (Player.controlJump && boostCD <= 0)
                boostCD = 60;

            if (boostCD > 40)
            {
                var timer = ((60 - boostCD) - 40) / 20f;
                var angle = timer * 6.28f;
                var off = new Vector2((float)Math.Cos(angle) * 18, (float)Math.Sin(angle) * 4);
                var vel = -Player.velocity * 0.5f;
                Player.UpdateRotation(angle);
                Dust l = Dust.NewDustPerfect(Player.Center + off.RotatedBy(Player.fullRotation), Terraria.ID.DustID.Cloud, vel, 0, new Color(255, 255, 255, 10), 1 + Main.rand.NextFloat());
                Dust r =Dust.NewDustPerfect(Player.Center - off.RotatedBy(Player.fullRotation), Terraria.ID.DustID.Cloud, vel, 0, new Color(255, 255, 255, 10), 1 + Main.rand.NextFloat());
                l.noGravity = true;
                r.noGravity = true;

                Player.bodyFrame = new Rectangle(0, 0, 40, 56);
                Player.legFrame = new Rectangle(0, 0, 40, 56);

                Player.velocity += Vector2.Normalize(Player.velocity) * 0.38f * SwimSpeed;
                Player.AddBuff(Terraria.ID.BuffID.Cursed, 1, true);
            }
            else Player.UpdateRotation(0);

            if (boostCD > 0) boostCD--;
            if (emergeTime > 0) emergeTime--;
        }

        public override void ResetEffects()
        {
            ShouldSwim = false;
            SwimSpeed = 1f;
        }
    }
}
