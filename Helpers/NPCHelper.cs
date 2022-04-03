using Terraria;


namespace StarlightRiver.Helpers
{
	public static partial class Helper
    {
        public static void Kill(this NPC NPC)
        {
            bool ModNPCDontDie = NPC.ModNPC?.CheckDead() == false;
            if (ModNPCDontDie)
                return;
            NPC.life = 0;
            NPC.checkDead();
            NPC.HitEffect();
            NPC.active = false;
        }

        public static void NpcVertical(NPC NPC, bool jump, int slot = 1, int jumpheight = 2) //idea: could be seperated farther
        {
            NPC.ai[slot] = 0;//reset jump counter
            for (int y = 0; y < jumpheight; y++)//idea: this should have diminishing results for output jump height
            {
                Tile tileType = Framing.GetTileSafely((int)(NPC.position.X / 16) + (NPC.direction * 2) + 1, (int)((NPC.position.Y + NPC.height + 8) / 16) - y - 1);
                if ((Main.tileSolid[tileType.type] || Main.tileSolidTop[tileType.type]) && tileType.active()) //how tall the wall is
                {
                    NPC.ai[slot] = (y + 1);
                }

                if (y >= NPC.ai[slot] + (NPC.height / 16) || (!jump && y >= 2)) //stops counting if there is room for the NPC to walk under //((int)((NPC.position.Y - target.position.Y) / 16) + 1)
                {
                    if (NPC.HasValidTarget && jump)
                    {
                        Player target = Main.player[NPC.target];
                        if (NPC.ai[slot] >= ((int)((NPC.position.Y - target.position.Y) / 16) + 1) - (NPC.height / 16 - 1)) break;
                    }
                    else break;
                }
            }
            if (NPC.ai[slot] > 0)//jump and step up
            {
                Tile tileType = Framing.GetTileSafely((int)(NPC.position.X / 16) + (NPC.direction * 2) + 1, (int)((NPC.position.Y + NPC.height + 8) / 16) - 1);
                if (NPC.ai[slot] == 1 && NPC.collideX)
                {
                    if (tileType.halfBrick() || (Main.tileSolid[tileType.type] && (NPC.position.Y % 16 + 8) == 0))
                    {
                        NPC.position.Y -= 8;//note: these just zip the NPC up the block and it looks bad, need to figure out how vanilla glides them up
                        NPC.velocity.X = NPC.oldVelocity.X;
                    }
                    else if (Main.tileSolid[tileType.type])
                    {
                        NPC.position.Y -= 16;
                        NPC.velocity.X = NPC.oldVelocity.X;
                    }
                }
                else if (NPC.ai[slot] == 2 && (NPC.position.Y % 16) == 0 && Framing.GetTileSafely((int)(NPC.position.X / 16) + (NPC.direction * 2) + 1, (int)((NPC.position.Y + NPC.height) / 16) - 1).halfBrick())
                {//note: I dislike this extra check, but couldn't find a way to avoid it
                    if (NPC.collideX)
                    {
                        NPC.position.Y -= 16;
                        NPC.velocity.X = NPC.oldVelocity.X;
                    }
                }
                else if (NPC.ai[slot] > 1 && jump == true)
                {
                    NPC.velocity.Y = -(3 + NPC.ai[slot]);
                    if (!NPC.HasValidTarget && NPC.velocity.X == 0)
                    {
                        NPC.ai[3]++;
                    }
                }
            }
        }
    }
}

