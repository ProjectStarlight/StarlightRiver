using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Buffs;

namespace StarlightRiver.Core //TODO: Move this somewhere else? not sure.
{
    class SwimPlayer : ModPlayer
    {
        int boostCD = 0;
        float targetRotation = 0;
        float realRotation = 0;
        bool isSwimming = false;
        int emergeTime = 0;

        private bool ShouldSwim() //checks for if hte player should be swimming
        {
            if (player.HasBuff(BuffType<PrismaticDrown>())) //TODO: Change this to be set on the arena instead of checking for this buff probably
                return true;

            for(int x = 0; x < 2; x++)
                for(int y = 0; y < 3; y++)
				{
                    int realX = (int)(player.position.X / 16) + x;
                    int realY = (int)(player.position.Y / 16) + y;

                    if (WorldGen.InWorld(realX, realY))
                    {
                        Tile tile = Framing.GetTileSafely(realX, realY);
                        if ( (tile.bTileHeader3 & 0b11100000) >> 5 == 1)
                            return true;
                    }
                }
            return false;
		}

        public override void PostUpdate()
        {
            isSwimming = ShouldSwim();

            if (!isSwimming) //reset stuff when the player isnt swimming
            {
                if (boostCD > 0)
                {
                    boostCD = 0;
                    player.UpdateRotation(0);
                }

                if (emergeTime <= 0) //20 frames for the player to rotate back
                    return;
            }

            player.maxFallSpeed = 0;
            player.gravity = 0;
            targetRotation = isSwimming ? player.velocity.ToRotation() : 1.57f + 3.14f;

            realRotation %= 6.28f; //handles the rotation, ensures the player wont randomly snap to rotation when entering/leaving swimming

            if (Math.Abs(targetRotation - realRotation) % 6.28f > 0.21f)
            {
                float mod(float a, float b) => a % b > 0 ? a % b : a % b + b;
                if (mod(targetRotation, 6.28f) > mod(realRotation, 6.28f))
                    realRotation += 0.2f;
                else
                    realRotation -= 0.2f;
            }
            else realRotation = targetRotation;

            player.fullRotation = realRotation + ((float)Math.PI / 2f);
            player.fullRotation %= 6.28f;

            player.fullRotationOrigin = player.Size / 2; //so the player rotates around their center... why is this not the default?

           if (player.itemAnimation != 0 && player.HeldItem.useStyle != Terraria.ID.ItemUseStyleID.SwingThrow && player.itemAnimation == player.itemAnimationMax - 1) //corrects the rotation on used items
                player.itemRotation -= realRotation + 1.57f;

            if (!isSwimming) //return later so rotation logic still runs
                return;

            emergeTime = 20; //20 frames for the player to rotate back, reset while swimming

            if (player.itemAnimation == 0)
                player.bodyFrame = new Rectangle(0, 56 * (int)(1 + Main.GameUpdateCount / 10 % 5), 40, 56);

            player.legFrame = new Rectangle(0, 56 * (int)(5 + Main.GameUpdateCount / 7 % 3), 40, 56);

            if (player.controlRight) player.velocity.X += 0.2f; //there should probably be a better way of doing this?
            if (player.controlLeft) player.velocity.X -= 0.2f;
            if (player.controlDown) player.velocity.Y += 0.2f;
            if (player.controlUp) player.velocity.Y -= 0.2f;

            player.velocity.Y -= 0.4125f; //this combats vanilla gravity.

            player.velocity *= 0.97f;

            if (player.controlJump && boostCD <= 0)
            {
                boostCD = 90;
            }

            if (boostCD > 60)
            {
                var timer = ((90 - boostCD) - 60) / 30f;
                var angle = timer * 6.28f;
                var off = new Vector2((float)Math.Cos(angle) * 40, (float)Math.Sin(angle) * 20);

                player.UpdateRotation(angle);
                Dust.NewDustPerfect(player.Center + off.RotatedBy(player.fullRotation), DustType<Content.Dusts.Starlight>());
                Dust.NewDustPerfect(player.Center - off.RotatedBy(player.fullRotation), DustType<Content.Dusts.Starlight>());

                player.bodyFrame = new Rectangle(0, 0, 40, 56);
                player.legFrame = new Rectangle(0, 0, 40, 56);

                player.velocity += Vector2.Normalize(player.velocity) * 0.35f;
                player.AddBuff(Terraria.ID.BuffID.Cursed, 1, true);
            }
            else player.UpdateRotation(0);

            if (boostCD > 0) boostCD--;
            if (emergeTime > 0) emergeTime--;
        }
    }
}
