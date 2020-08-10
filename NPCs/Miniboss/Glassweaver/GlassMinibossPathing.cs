using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.NPCs.Miniboss.Glassweaver.PathingUtils;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.NPCs.Miniboss.Glassweaver
{
    internal partial class GlassMiniboss : ModNPC
    {
        public Rectangle targetRectangle;
        private float storedVelocity; //this should be deterministic in theory? to be tested.

        //the bounce pads which the boss will use to jump when he needs to.
        private BouncePad[] pads => new BouncePad[]
        {
            new BouncePad(new Rectangle((int)spawnPos.X - 280, (int)spawnPos.Y + 200, 32, 8), RegionCenter, this, 10),
            new BouncePad(new Rectangle((int)spawnPos.X + 232, (int)spawnPos.Y + 200, 32, 8), RegionCenter, this, 10),
            new BouncePad(new Rectangle((int)spawnPos.X - 16, (int)spawnPos.Y + 300, 32, 8), RegionCenter, this, 12, true),

            new BouncePad(new Rectangle((int)spawnPos.X + 130, (int)spawnPos.Y + 84, 32, 8), RegionRight, this, 5),
            new BouncePad(new Rectangle((int)spawnPos.X - 178, (int)spawnPos.Y + 84, 32, 8), RegionLeft, this, 5),

            new BouncePad(new Rectangle((int)spawnPos.X + 80, (int)spawnPos.Y + 300, 32, 8), RegionRight, this, 8),
            new BouncePad(new Rectangle((int)spawnPos.X - 128, (int)spawnPos.Y + 300, 32, 8), RegionLeft, this, 8),

            new BouncePad(new Rectangle((int)spawnPos.X - 46, (int)spawnPos.Y + 84, 32, 8), RegionPit, this, 4, true),
            new BouncePad(new Rectangle((int)spawnPos.X + 32, (int)spawnPos.Y + 84, 32, 8), RegionPit, this, 4, true)
        };

        //debug drawing of regions and pads
        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            spriteBatch.Draw(Main.magicPixel, drawRect(RegionCenter), null, Color.Blue * 0.4f);
            spriteBatch.Draw(Main.magicPixel, drawRect(RegionLeft), null, Color.Green * 0.4f);
            spriteBatch.Draw(Main.magicPixel, drawRect(RegionRight), null, Color.Purple * 0.4f);
            spriteBatch.Draw(Main.magicPixel, drawRect(RegionPit), null, Color.Red * 0.4f);

            for (int k = 0; k < pads.Length; k++) pads[k].DebugDraw(spriteBatch);

            return true;
        }

        private Rectangle drawRect(Rectangle input) => new Rectangle(input.X - (int)Main.screenPosition.X, input.Y - (int)Main.screenPosition.Y, input.Width, input.Height);

        private Rectangle RegionCenter => new Rectangle((int)spawnPos.X - 184, (int)spawnPos.Y - 210, 352, 308);
        private Rectangle RegionLeft => new Rectangle((int)spawnPos.X - 442, (int)spawnPos.Y - 210, 258, 420);
        private Rectangle RegionRight => new Rectangle((int)spawnPos.X + 168, (int)spawnPos.Y - 210, 258, 420);
        private Rectangle RegionPit => new Rectangle((int)spawnPos.X - 184, (int)spawnPos.Y + 128, 352, 276);

        private Rectangle arena => new Rectangle((int)spawnPos.X - 442, (int)spawnPos.Y - 210, 868, 600);

        public void Jump(int strength, bool cancel)
        {
            PathingTimer++;

            if (PathingTimer == 1) storedVelocity = npc.velocity.X;

            npc.velocity.X *= 1 - (PathingTimer / 30f);

            if (PathingTimer >= 30)
            {
                if (!cancel) npc.velocity.X = storedVelocity;
                npc.velocity.Y -= strength;
                PathingTimer = 0;
            }
        }

        private void PathToTarget()
        {
            if (AttackTimer == 1)
            {
                PickTarget();
                if (AttackTimer != 0) npc.velocity.X = targetRectangle.Center.X > npc.Center.X ? 3 : -3; //if we do need to change targets, set velocity           
            }

            npc.noTileCollide = npc.velocity.Y < 0 || (npc.velocity.Y != 0 && GetRegion(npc) == RegionCenter && targetRectangle == RegionPit); //allow us to clip on the way up, also a special case here for jumping down from center => pit

            if ((npc.Hitbox.Intersects(targetRectangle) && npc.velocity.Y == 0) || AttackTimer >= 240) //extra failsafe if pathing takes longer than 4s
            {
                ResetAttack();
                npc.velocity.X = 0;
            }
        }

        private void PickTarget()
        {
            List<Player> validTargets = new List<Player>();

            for(int k = 0; k < Main.maxPlayers; k++)
                if (arena.Intersects(Main.player[k].Hitbox)) validTargets.Add(Main.player[k]);

            validTargets = Helper.RandomizeList<Player>(validTargets);

            for(int k = 0; k < validTargets.Count; k++)
            {
                Player player = validTargets[k];

                if (GetRegion(player) != GetRegion(npc)) //change if possible
                {
                    targetRectangle = GetRegion(player);
                    break;
                }
            }

            if (GetRegion(npc) == targetRectangle) ResetAttack(); //cycle attack instead of trying to move if we dont need to move!
        }

        private Rectangle GetRegion(Entity entity)
        {
            if (entity.Hitbox.Intersects(RegionCenter)) return RegionCenter;
            else if (entity.Hitbox.Intersects(RegionLeft)) return RegionLeft;
            else if (entity.Hitbox.Intersects(RegionRight)) return RegionRight;
            else if (entity.Hitbox.Intersects(RegionPit)) return RegionPit;
            else return RegionCenter;
        }
    }
}
