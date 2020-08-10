using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Dragons
{
    internal class YoungDragon : ModMountData
    {
        public override void SetDefaults()
        {
            mountData.spawnDust = DustType<Dusts.PlayerFollowOrange>();
            mountData.buff = mod.BuffType("CarMount");
            mountData.heightBoost = 20;
            mountData.fallDamage = 0.1f;
            mountData.runSpeed = 4f;
            mountData.flightTimeMax = 0;
            mountData.fatigueMax = 0;
            mountData.jumpHeight = 12;
            mountData.acceleration = 0.06f;
            mountData.jumpSpeed = 4f;
            mountData.blockExtraJumps = true;
            mountData.totalFrames = 4;
            int[] array = new int[mountData.totalFrames];
            for (int l = 0; l < array.Length; l++)
            {
                array[l] = 16;
            }
            mountData.playerYOffsets = array;
            mountData.xOffset = 13;
            mountData.bodyFrame = 3;
            mountData.yOffset = 6;
            mountData.playerHeadOffset = 22;
            if (Main.netMode != NetmodeID.Server)
            {
                mountData.textureWidth = mountData.backTexture.Width + 20;
                mountData.textureHeight = mountData.backTexture.Height;
            }
        }

        public override void UpdateEffects(Player player)
        {
            SetDefaults();
            player.noItems = true;
            if (player.controlUseItem)
            {
                Dust.NewDustPerfect(player.Center + new Vector2(player.direction * -14, 8), DustType<Dusts.Piss>(), new Vector2(player.direction * 2, 0), 180, new Color(255, 255, 150));
            }
            if (player.controlJump && player.releaseJump && player.velocity.Y != 0 && player.GetModPlayer<DragonHandler>().jumpAgainDragon)
            {
                player.GetModPlayer<DragonHandler>().jumpAgainDragon = false;
                player.velocity.Y = -6;
            }
            if (player.controlJump && !player.GetModPlayer<DragonHandler>().jumpAgainDragon)
            {
                player.maxFallSpeed = 1.5f;
            }
            if (player.velocity.Y == 0) player.GetModPlayer<DragonHandler>().jumpAgainDragon = true;
        }

        public override bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow)
        {
            texture = GetTexture("StarlightRiver/Invisible");
            DragonData data = drawPlayer.GetModPlayer<DragonHandler>().data;
            int offX = drawPlayer.direction == -1 ? 10 : -10;
            Rectangle target = new Rectangle((int)drawPosition.X + offX, (int)drawPosition.Y, 68, 54);
            Rectangle source = GetTexture("StarlightRiver/Dragons/YoungDragonScale").Frame();
            SpriteEffects flip = drawPlayer.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            playerDrawData.Add(new DrawData(GetTexture("StarlightRiver/Dragons/YoungDragonScale"), target, source, data.scaleColor.MultiplyRGB(drawColor), 0, source.Size() / 2, flip, 0));
            playerDrawData.Add(new DrawData(GetTexture("StarlightRiver/Dragons/YoungDragonBelly"), target, source, data.bellyColor.MultiplyRGB(drawColor), 0, source.Size() / 2, flip, 0));
            playerDrawData.Add(new DrawData(GetTexture("StarlightRiver/Dragons/YoungDragonHorn"), target, source, data.hornColor.MultiplyRGB(drawColor), 0, source.Size() / 2, flip, 0));
            playerDrawData.Add(new DrawData(GetTexture("StarlightRiver/Dragons/YoungDragonEye"), target, source, data.eyeColor.MultiplyRGB(drawColor), 0, source.Size() / 2, flip, 0));
            return true;
        }
    }

    public class CarMount : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("YoungDragon");
            Description.SetDefault("Wheeeeee");
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.mount.SetMount(MountType<YoungDragon>(), player);
            player.buffTime[buffIndex] = 11;
        }
    }
}