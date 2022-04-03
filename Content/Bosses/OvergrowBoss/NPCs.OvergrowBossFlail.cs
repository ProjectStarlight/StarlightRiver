using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

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
            NPCID.Sets.TrailCacheLength[NPC.type] = 10;
            NPCID.Sets.TrailingMode[NPC.type] = 1;
        }

        public override void SetDefaults()
        {
            NPC.lifeMax = 2000;
            NPC.width = 64;
            NPC.height = 64;
            NPC.aiStyle = -1;
            NPC.noGravity = true;
            NPC.knockBackResist = 0;
            NPC.damage = 60;
            for (int k = 0; k < NPC.buffImmune.Length; k++) NPC.buffImmune[k] = true;
            NPC.ai[3] = 1;
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

            if (parent.NPC.ai[0] <= 4) NPC.ai[3] = 1;

            NPC.ai[1]++; //ticks our timer

            if (NPC.ai[2] == 1) //if zapped
            {
                parent.NPC.ai[1] = 0; //resets the boss' timer constatnly, effectively freezing it
                parent.NPC.velocity *= 0;
                parent.ResetAttack(); //also reset's their attack just incase

                if (NPC.ai[1] % 5 == 0 && NPC.ai[1] < 60) DrawHelper.DrawElectricity(NPC.Center, parent.NPC.Center, DustType<Dusts.GoldNoMovement>(), 0.5f); //draw zap effects

                if (NPC.ai[1] == 60) //after 60 seconds disconnect the flail and phase the boss
                {
                    NPC.velocity.Y -= 10; //launches it out of the pit
                    NPC.ai[3] = 0; //cut the chain
                    parent.NPC.ai[0] = (int)OvergrowBoss.OvergrowBossPhase.Stun; //phase the boss
                    parent.NPC.ai[1] = 0; //reset timer on boss
                }

                if (NPC.ai[1] == 80) //some things need to be on a delay
                    NPC.ai[2] = 0; //no longer zapped!
            }

            if (NPC.ai[0] == 1) //pick-upable
            {
                NPC.life = 1;
                NPC.friendly = true;
                NPC.rotation += NPC.velocity.X / 125f;
                if (NPC.velocity.Y == 0 && NPC.velocity.X > 0.3f) NPC.velocity.X -= 0.3f;
                if (NPC.velocity.Y == 0 && NPC.velocity.X < -0.3f) NPC.velocity.X += 0.3f;
                if (Math.Abs(NPC.velocity.X) <= 0.3f) NPC.velocity.X = 0;
                if (Main.player.Any(p => p.Hitbox.Intersects(NPC.Hitbox)) && holder == null && NPC.velocity == Vector2.Zero)
                    holder = Main.player.FirstOrDefault(p => p.Hitbox.Intersects(NPC.Hitbox)); //the first person to walk over it picks it up
                if (holder != null)
                {
                    NPC.position = holder.Center + new Vector2(-32, -100); //they hold it above their head
                    holder.bodyFrame = new Rectangle(0, 56 * 5, 40, 56); //holding animation
                    holder.AddBuff(BuffID.Cursed, 2, true); //they cant use Items!
                    holder.rocketTime = 0; holder.wingTime = 0; //they cant rocket/fly!
                    holder.velocity.X *= 0.95f; //they cant move fast!

                    if (holder.controlUseItem) //but they can YEET THIS MOTHERFUCKER
                    {
                        NPC.velocity = Vector2.Normalize(holder.Center - Main.MouseWorld) * -10; //in the direction they are aiming
                        holder = null;
                    }
                }

                NPC.velocity.Y += 0.2f;
            }
        }

        public override bool CheckDead()
        {
            NPC.dontTakeDamage = true;
            NPC.life = 1;
            return false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            if (NPC.life <= 1)
                for (int k = 0; k < NPC.oldPos.Length; k++)
                {
                    Color color = drawColor * ((float)(NPC.oldPos.Length - k) / NPC.oldPos.Length) * 0.4f;
                    float scale = NPC.scale * (NPC.oldPos.Length - k) / NPC.oldPos.Length;
                    Texture2D tex = Request<Texture2D>(Texture).Value;

                    spriteBatch.Draw(tex, NPC.oldPos[k] + NPC.Size / 2 - Main.screenPosition, null, color, NPC.oldRot[k], tex.Size() / 2, scale, default, default);
                }

            if (NPC.ai[3] != 0)
                for (float k = 0; k <= 1; k += 1 / (Vector2.Distance(NPC.Center, parent.NPC.Center) / 16))
                {
                    Vector2 pos = Vector2.Lerp(NPC.Center, parent.NPC.Center, k) - Main.screenPosition;
                    //shake the chain when tossed
                    if ((parent.NPC.ai[0] == (int)OvergrowBoss.OvergrowBossPhase.FirstAttack && (parent.NPC.ai[2] == 3 || parent.NPC.ai[2] == 4 || parent.NPC.ai[2] == 6) ||
                        parent.NPC.ai[0] == (int)OvergrowBoss.OvergrowBossPhase.FirstToss) && NPC.velocity.Length() > 0)
                        pos += Vector2.Normalize(NPC.Center - parent.NPC.Center).RotatedBy(1.58f) * (float)Math.Sin(StarlightWorld.rottime + k * 20) * 10;

                    spriteBatch.Draw(Request<Texture2D>(AssetDirectory.OvergrowItem + "ShakerChain").Value, pos,
                        new Rectangle(0, 0, 8, 16), drawColor, (NPC.Center - parent.NPC.Center).ToRotation() + 1.58f, new Vector2(4, 8), 1, 0, 0);
                }
            return true;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            if (NPC.ai[0] == 1 && holder == null && NPC.velocity.X == 0)
                spriteBatch.DrawString(Main.fontMouseText, "Pick up!", NPC.Center + new Vector2(Main.fontMouseText.MeasureString("Pick up!").X / -2, -50 + (float)Math.Sin(StarlightWorld.rottime) * 5) - Main.screenPosition, Color.Yellow * 0.75f);
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            if (NPC.life <= 1)
                for (int k = 0; k < NPC.oldPos.Length; k++)
                {
                    Color color = new Color(255, 255, 200) * 0.3f;
                    float scale = NPC.scale * (NPC.oldPos.Length - k) / NPC.oldPos.Length * 1.1f;
                    Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Keys/Glow").Value;

                    spriteBatch.Draw(tex, NPC.oldPos[k] + NPC.Size / 2 - Main.screenPosition, null, color, 0, tex.Size() / 2, scale, default, default);
                }
        }
    }
}