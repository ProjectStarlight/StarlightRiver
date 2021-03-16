using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using StarlightRiver.Codex;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Content.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ObjectData;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;


namespace StarlightRiver.Helpers
{
    public static partial class Helper
    {
        public static bool IsTargetValid(NPC npc) => npc.active && !npc.friendly && !npc.immortal && !npc.dontTakeDamage;
        public static void Kill(this NPC npc)
        {
            bool modNPCDontDie = npc.modNPC?.CheckDead() == false;
            if (modNPCDontDie)
                return;
            npc.life = 0;
            npc.checkDead();
            npc.HitEffect();
            npc.active = false;
        }
        public static void NpcVertical(NPC npc, bool jump, int slot = 1, int jumpheight = 2) //idea: could be seperated farther
        {
            npc.ai[slot] = 0;//reset jump counter
            for (int y = 0; y < jumpheight; y++)//idea: this should have diminishing results for output jump height
            {
                Tile tileType = Framing.GetTileSafely((int)(npc.position.X / 16) + (npc.direction * 2) + 1, (int)((npc.position.Y + npc.height + 8) / 16) - y - 1);
                if ((Main.tileSolid[tileType.type] || Main.tileSolidTop[tileType.type]) && tileType.active()) //how tall the wall is
                {
                    npc.ai[slot] = (y + 1);
                }

                if (y >= npc.ai[slot] + (npc.height / 16) || (!jump && y >= 2)) //stops counting if there is room for the npc to walk under //((int)((npc.position.Y - target.position.Y) / 16) + 1)
                {
                    if (npc.HasValidTarget && jump)
                    {
                        Player target = Main.player[npc.target];
                        if (npc.ai[slot] >= ((int)((npc.position.Y - target.position.Y) / 16) + 1) - (npc.height / 16 - 1)) break;
                    }
                    else break;
                }
            }
            if (npc.ai[slot] > 0)//jump and step up
            {
                Tile tileType = Framing.GetTileSafely((int)(npc.position.X / 16) + (npc.direction * 2) + 1, (int)((npc.position.Y + npc.height + 8) / 16) - 1);
                if (npc.ai[slot] == 1 && npc.collideX)
                {
                    if (tileType.halfBrick() || (Main.tileSolid[tileType.type] && (npc.position.Y % 16 + 8) == 0))
                    {
                        npc.position.Y -= 8;//note: these just zip the npc up the block and it looks bad, need to figure out how vanilla glides them up
                        npc.velocity.X = npc.oldVelocity.X;
                    }
                    else if (Main.tileSolid[tileType.type])
                    {
                        npc.position.Y -= 16;
                        npc.velocity.X = npc.oldVelocity.X;
                    }
                }
                else if (npc.ai[slot] == 2 && (npc.position.Y % 16) == 0 && Framing.GetTileSafely((int)(npc.position.X / 16) + (npc.direction * 2) + 1, (int)((npc.position.Y + npc.height) / 16) - 1).halfBrick())
                {//note: I dislike this extra check, but couldn't find a way to avoid it
                    if (npc.collideX)
                    {
                        npc.position.Y -= 16;
                        npc.velocity.X = npc.oldVelocity.X;
                    }
                }
                else if (npc.ai[slot] > 1 && jump == true)
                {
                    npc.velocity.Y = -(3 + npc.ai[slot]);
                    if (!npc.HasValidTarget && npc.velocity.X == 0)
                    {
                        npc.ai[3]++;
                    }
                }
            }
        }
    }
}

