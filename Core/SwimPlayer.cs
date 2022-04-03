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
        bool isSwimming = false;
        int emergeTime = 0;

        private bool ShouldSwim() //checks for if hte Player should be swimming
        {
            if (Player.HasBuff(BuffType<PrismaticDrown>())) //TODO: Change this to be set on the arena instead of checking for this buff probably
                return true;

            for(int x = 0; x < 2; x++)
                for(int y = 0; y < 3; y++)
				{
                    int realX = (int)(Player.position.X / 16) + x;
                    int realY = (int)(Player.position.Y / 16) + y;

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

            if (!isSwimming) //reset stuff when the Player isnt swimming
            {
                if (boostCD > 0)
                {
                    boostCD = 0;
                    Player.UpdateRotation(0);
                }

                if (emergeTime <= 0) //20 frames for the Player to rotate back
                    return;
            }

            Player.maxFallSpeed = 0;
            Player.gravity = 0;
            targetRotation = isSwimming ? Player.velocity.ToRotation() : 1.57f + 3.14f;

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

            Player.fullRotation = realRotation + ((float)Math.PI / 2f);
            Player.fullRotation %= 6.28f;

            Player.fullRotationOrigin = Player.Size / 2; //so the Player rotates around their center... why is this not the default?

           if (Player.ItemAnimation != 0 && Player.HeldItem.useStyle != Terraria.ID.ItemUseStyleID.Swing && Player.ItemAnimation == Player.ItemAnimationMax - 1) //corrects the rotation on used Items
                Player.ItemRotation -= realRotation + 1.57f;

            if (!isSwimming) //return later so rotation logic still runs
                return;

            emergeTime = 20; //20 frames for the Player to rotate back, reset while swimming

            if (Player.ItemAnimation == 0)
                Player.bodyFrame = new Rectangle(0, 56 * (int)(1 + Main.GameUpdateCount / 10 % 5), 40, 56);

            Player.legFrame = new Rectangle(0, 56 * (int)(5 + Main.GameUpdateCount / 7 % 3), 40, 56);

            if (Player.controlRight) Player.velocity.X += 0.2f; //there should probably be a better way of doing this?
            if (Player.controlLeft) Player.velocity.X -= 0.2f;
            if (Player.controlDown) Player.velocity.Y += 0.2f;
            if (Player.controlUp) Player.velocity.Y -= 0.2f;

            Player.velocity.Y -= 0.4125f; //this combats vanilla gravity.

            Player.velocity *= 0.97f;

            if (Player.controlJump && boostCD <= 0)
            {
                boostCD = 90;
            }

            if (boostCD > 60)
            {
                var timer = ((90 - boostCD) - 60) / 30f;
                var angle = timer * 6.28f;
                var off = new Vector2((float)Math.Cos(angle) * 40, (float)Math.Sin(angle) * 20);

                Player.UpdateRotation(angle);
                Dust.NewDustPerfect(Player.Center + off.RotatedBy(Player.fullRotation), DustType<Content.Dusts.Starlight>());
                Dust.NewDustPerfect(Player.Center - off.RotatedBy(Player.fullRotation), DustType<Content.Dusts.Starlight>());

                Player.bodyFrame = new Rectangle(0, 0, 40, 56);
                Player.legFrame = new Rectangle(0, 0, 40, 56);

                Player.velocity += Vector2.Normalize(Player.velocity) * 0.35f;
                Player.AddBuff(Terraria.ID.BuffID.Cursed, 1, true);
            }
            else Player.UpdateRotation(0);

            if (boostCD > 0) boostCD--;
            if (emergeTime > 0) emergeTime--;
        }
    }
}
