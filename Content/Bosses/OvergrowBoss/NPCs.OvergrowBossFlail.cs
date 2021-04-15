using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Bosses.OvergrowBoss
{
    public class OvergrowBossFlail : ModNPC, IDrawAdditive
    {
        public OvergrowBoss parent;
        public Player holder;

        public override string Texture => AssetDirectory.OvergrowItem + "ShakerBall";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("The Shaker");
            NPCID.Sets.TrailCacheLength[npc.type] = 10;
            NPCID.Sets.TrailingMode[npc.type] = 1;
        }

        public override void SetDefaults()
        {
            npc.lifeMax = 2000;
            npc.width = 64;
            npc.height = 64;
            npc.aiStyle = -1;
            npc.noGravity = true;
            npc.knockBackResist = 0;
            npc.damage = 60;
            for (int k = 0; k < npc.buffImmune.Length; k++) npc.buffImmune[k] = true;
            npc.ai[3] = 1;
        }

        public override bool CheckActive()
        {
            return false;
        }

        public override void AI()
        {
            /* AI fields:
             * 0: state
             * 1: timer
             * 2: shocked?
             * 3: chain?
             */

            if (parent == null) return; //safety check

            if (parent.npc.ai[0] <= 4) npc.ai[3] = 1;

            npc.ai[1]++; //ticks our timer

            if (npc.ai[2] == 1) //if zapped
            {
                parent.npc.ai[1] = 0; //resets the boss' timer constatnly, effectively freezing it
                parent.npc.velocity *= 0;
                parent.ResetAttack(); //also reset's their attack just incase

                if (npc.ai[1] % 5 == 0 && npc.ai[1] < 60) DrawHelper.DrawElectricity(npc.Center, parent.npc.Center, DustType<Dusts.GoldNoMovement>(), 0.5f); //draw zap effects

                if (npc.ai[1] == 60) //after 60 seconds disconnect the flail and phase the boss
                {
                    npc.velocity.Y -= 10; //launches it out of the pit
                    npc.ai[3] = 0; //cut the chain
                    parent.npc.ai[0] = (int)OvergrowBoss.OvergrowBossPhase.Stun; //phase the boss
                    parent.npc.ai[1] = 0; //reset timer on boss
                }

                if (npc.ai[1] == 80) //some things need to be on a delay
                    npc.ai[2] = 0; //no longer zapped!
            }

            if (npc.ai[0] == 1) //pick-upable
            {
                npc.life = 1;
                npc.friendly = true;
                npc.rotation += npc.velocity.X / 125f;
                if (npc.velocity.Y == 0 && npc.velocity.X > 0.3f) npc.velocity.X -= 0.3f;
                if (npc.velocity.Y == 0 && npc.velocity.X < -0.3f) npc.velocity.X += 0.3f;
                if (Math.Abs(npc.velocity.X) <= 0.3f) npc.velocity.X = 0;
                if (Main.player.Any(p => p.Hitbox.Intersects(npc.Hitbox)) && holder == null && npc.velocity == Vector2.Zero)
                    holder = Main.player.FirstOrDefault(p => p.Hitbox.Intersects(npc.Hitbox)); //the first person to walk over it picks it up
                if (holder != null)
                {
                    npc.position = holder.Center + new Vector2(-32, -100); //they hold it above their head
                    holder.bodyFrame = new Rectangle(0, 56 * 5, 40, 56); //holding animation
                    holder.AddBuff(BuffID.Cursed, 2, true); //they cant use items!
                    holder.rocketTime = 0; holder.wingTime = 0; //they cant rocket/fly!
                    holder.velocity.X *= 0.95f; //they cant move fast!

                    if (holder.controlUseItem) //but they can YEET THIS MOTHERFUCKER
                    {
                        npc.velocity = Vector2.Normalize(holder.Center - Main.MouseWorld) * -10; //in the direction they are aiming
                        holder = null;
                    }
                }

                npc.velocity.Y += 0.2f;
            }
        }

        public override bool CheckDead()
        {
            npc.dontTakeDamage = true;
            npc.life = 1;
            return false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            if (npc.life <= 1)
                for (int k = 0; k < npc.oldPos.Length; k++)
                {
                    Color color = drawColor * ((float)(npc.oldPos.Length - k) / npc.oldPos.Length) * 0.4f;
                    float scale = npc.scale * (npc.oldPos.Length - k) / npc.oldPos.Length;
                    Texture2D tex = GetTexture(Texture);

                    spriteBatch.Draw(tex, npc.oldPos[k] + npc.Size / 2 - Main.screenPosition, null, color, npc.oldRot[k], tex.Size() / 2, scale, default, default);
                }

            if (npc.ai[3] != 0)
                for (float k = 0; k <= 1; k += 1 / (Vector2.Distance(npc.Center, parent.npc.Center) / 16))
                {
                    Vector2 pos = Vector2.Lerp(npc.Center, parent.npc.Center, k) - Main.screenPosition;
                    //shake the chain when tossed
                    if ((parent.npc.ai[0] == (int)OvergrowBoss.OvergrowBossPhase.FirstAttack && (parent.npc.ai[2] == 3 || parent.npc.ai[2] == 4 || parent.npc.ai[2] == 6) ||
                        parent.npc.ai[0] == (int)OvergrowBoss.OvergrowBossPhase.FirstToss) && npc.velocity.Length() > 0)
                        pos += Vector2.Normalize(npc.Center - parent.npc.Center).RotatedBy(1.58f) * (float)Math.Sin(StarlightWorld.rottime + k * 20) * 10;

                    spriteBatch.Draw(GetTexture(AssetDirectory.OvergrowItem + "ShakerChain"), pos,
                        new Rectangle(0, 0, 8, 16), drawColor, (npc.Center - parent.npc.Center).ToRotation() + 1.58f, new Vector2(4, 8), 1, 0, 0);
                }
            return true;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            if (npc.ai[0] == 1 && holder == null && npc.velocity.X == 0)
                spriteBatch.DrawString(Main.fontMouseText, "Pick up!", npc.Center + new Vector2(Main.fontMouseText.MeasureString("Pick up!").X / -2, -50 + (float)Math.Sin(StarlightWorld.rottime) * 5) - Main.screenPosition, Color.Yellow * 0.75f);
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            if (npc.life <= 1)
                for (int k = 0; k < npc.oldPos.Length; k++)
                {
                    Color color = new Color(255, 255, 200) * 0.3f;
                    float scale = npc.scale * (npc.oldPos.Length - k) / npc.oldPos.Length * 1.1f;
                    Texture2D tex = GetTexture("StarlightRiver/Assets/Keys/Glow");

                    spriteBatch.Draw(tex, npc.oldPos[k] + npc.Size / 2 - Main.screenPosition, null, color, 0, tex.Size() / 2, scale, default, default);
                }
        }
    }
}