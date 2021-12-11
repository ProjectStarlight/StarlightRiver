using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.NPCs.BaseTypes
{
	internal abstract class MovingPlatform : ModNPC
    {
        public virtual void SafeSetDefaults() { }

        public virtual void SafeAI() { }

        public override bool? CanBeHitByProjectile(Projectile projectile) => false;

        public override bool? CanBeHitByItem(Player player, Item item) => false;

        public override bool CheckActive() => false;

        public override void SetStaticDefaults() => DisplayName.SetDefault("");

        public virtual void SafeSendExtraAI() { }

        public virtual void SafeReceiveExtraAI() { }

        public sealed override void SetDefaults()
        {
            SafeSetDefaults();

            npc.lifeMax = 10;
            npc.immortal = true;
            npc.dontTakeDamage = true;
            npc.noGravity = true;
            npc.knockBackResist = 0; //very very important!!
            npc.aiStyle = -1;
            npc.damage = 0;
            npc.netAlways = true;
        }

        public override sealed void SendExtraAI(BinaryWriter writer)
        {
            SafeSendExtraAI();
        }

        public override sealed void ReceiveExtraAI(BinaryReader reader)
        {
            SafeReceiveExtraAI();
        }

        Vector2 prevPos;

        public sealed override void AI()
        {
            SafeAI();


            float yDistTraveled = npc.position.Y - prevPos.Y;

            if (npc.velocity != Vector2.Zero && Math.Abs(npc.velocity.Y) > 1f && (npc.velocity.Y < 0 && yDistTraveled < npc.velocity.Y * 1.5 && yDistTraveled > npc.velocity.Y * 6))
            {
                //this loop outside of the normal moving platform loop in Mechanics is mainly for multiplayer with some potential for extreme lag situations on fast platforms
                //what is happening is that when terraria skips frames (or lags in mp) they add the npc velocity multiplied by the skipped frames up to 5x a normal frame until caught up, but only run the ai once
                //so we can end up with frames where the platform skips 5x its normal velocity likely clipping through players since the platform is thin.
                //to solve this, the collision code takes into account the previous platform position accessed by this AI for the hitbox to cover the whole travel from previous fully processed frame.
                //only handling big upwards y movements since the horizontal skips don't seem as jarring to the user since platforms tend to be wide, and vertical down skips aren't jarring since player drops onto platform anyway instead of clipping through.
                foreach (Player player in Main.player)
                {
                    if (!player.active || player.dead || player.GoingDownWithGrapple || player.GetModPlayer<StarlightPlayer>().platformTimer > 0)
                        continue;

                    Rectangle playerRect = new Rectangle((int)player.position.X, (int)player.position.Y + (player.height), player.width, 1);
                    Rectangle npcRect = new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, 8 + (player.velocity.Y > 0 ? (int)player.velocity.Y : 0) + (int)Math.Abs(yDistTraveled));

                    if (playerRect.Intersects(npcRect) && player.position.Y <= npc.position.Y)
                    {
                        if (!player.justJumped && player.velocity.Y >= 0)
                        {
                            player.velocity.Y = 0;
                            player.position.Y = npc.position.Y - player.height + 4;
                            player.position += npc.velocity;
                        }
                    }
                }
            }
            

            for (int k = 0; k < Main.maxProjectiles; k++)
			{
                var proj = Main.projectile[k];

                if (!proj.active || proj.aiStyle != 7)
                    continue;

                if(proj.ai[0] != 1 && proj.timeLeft < 36000 - 3 && proj.Hitbox.Intersects(npc.Hitbox))
				{
                    proj.ai[0] = 2;
                    proj.netUpdate = true;
                }
			}

            prevPos = npc.position;
        }
    }
}