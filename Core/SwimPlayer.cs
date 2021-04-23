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

        public override void PostUpdate()
        {
            if (!player.HasBuff(BuffType<PrismaticDrown>())) //TODO: Better check later
            {
                if (boostCD > 0)
                {
                    boostCD = 0;
                    player.UpdateRotation(0);
                }
                return;
            }

            player.maxFallSpeed = 0;
            player.gravity = 0;
            targetRotation = player.velocity.ToRotation();

            realRotation %= 6.28f;

            if (Math.Abs(targetRotation - realRotation) % 6.28f > 0.11f)
            {
                float mod(float a, float b) => a % b > 0 ? a % b : a % b + b;
                if (mod(targetRotation, 6.28f) > mod(realRotation, 6.28f))
                    realRotation += 0.1f;
                else
                    realRotation -= 0.1f;
            }
            else realRotation = targetRotation;

            player.fullRotation = realRotation + ((float)Math.PI / 2f);
            player.fullRotation %= 6.28f;

            player.fullRotationOrigin = player.Size / 2;

            if (player.itemAnimation == 0)
                player.bodyFrame = new Rectangle(0, 56 * (int)(1 + Main.GameUpdateCount / 10 % 5), 40, 56);

            else if (player.HeldItem.useStyle != Terraria.ID.ItemUseStyleID.SwingThrow && player.itemAnimation == player.itemAnimationMax - 1)
                player.itemRotation -= realRotation + 1.57f;

            player.legFrame = new Rectangle(0, 56 * (int)(5 + Main.GameUpdateCount / 7 % 3), 40, 56);

            if (player.controlRight) player.velocity.X += 0.2f;
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
        }
    }
}
