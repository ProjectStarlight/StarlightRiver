using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader;

namespace StarlightRiver.Content.NPCs.BaseTypes
{
    internal abstract class MovingPlatform : ModNPC
    {
        public bool BeingStoodOn;

        public virtual void SafeSetDefaults() { }

        public virtual void SafeAI() { }

        public override bool? CanBeHitByProjectile(Projectile Projectile) => false;

        public override bool? CanBeHitByItem(Player Player, Item Item) => false;

        public override bool CheckActive() => false;

        public override void SetStaticDefaults() => DisplayName.SetDefault("");

        public virtual void SafeSendExtraAI() { }

        public virtual void SafeReceiveExtraAI() { }

        public sealed override void SetDefaults()
        {
            SafeSetDefaults();

            NPC.lifeMax = 10;
            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            NPC.noGravity = true;
            NPC.knockBackResist = 0; //very very important!!
            NPC.aiStyle = -1;
            NPC.damage = 0;
            NPC.netAlways = true;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            database.Entries.Remove(bestiaryEntry);
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

            float yDistTraveled = NPC.position.Y - prevPos.Y;

            if (NPC.velocity != Vector2.Zero && NPC.velocity.Y < -1f && yDistTraveled < NPC.velocity.Y * 1.5 && yDistTraveled > NPC.velocity.Y * 6)
            {
                //this loop outside of the normal moving platform loop in Mechanics is mainly for multiPlayer with some potential for extreme lag situations on fast platforms
                //what is happening is that when terraria skips frames (or lags in mp) they add the NPC velocity multiplied by the skipped frames up to 5x a normal frame until caught up, but only run the ai once
                //so we can end up with frames where the platform skips 5x its normal velocity likely clipping through Players since the platform is thin.
                //to solve this, the collision code takes into account the previous platform position accessed by this AI for the hitbox to cover the whole travel from previous fully processed frame.
                //only handling big upwards y movements since the horizontal skips don't seem as jarring to the user since platforms tend to be wide, and vertical down skips aren't jarring since Player drops onto platform anyway instead of clipping through.
                foreach (Player Player in Main.player)
                {
                    if (!Player.active || Player.dead || Player.GoingDownWithGrapple || Player.GetModPlayer<StarlightPlayer>().platformTimer > 0)
                        continue;

                    Rectangle PlayerRect = new Rectangle((int)Player.position.X, (int)Player.position.Y + (Player.height), Player.width, 1);
                    Rectangle NPCRect = new Rectangle((int)NPC.position.X, (int)NPC.position.Y, NPC.width, 8 + (Player.velocity.Y > 0 ? (int)Player.velocity.Y : 0) + (int)Math.Abs(yDistTraveled));

                    if (PlayerRect.Intersects(NPCRect) && Player.position.Y <= NPC.position.Y)
                    {
                        if (!Player.justJumped && Player.velocity.Y >= 0)
                        {
                            Player.velocity.Y = 0;
                            Player.position.Y = NPC.position.Y - Player.height + 4;
                            Player.position += NPC.velocity;
                        }
                    }
                }
            }

            for (int k = 0; k < Main.maxProjectiles; k++)
            {
                var proj = Main.projectile[k];

                if (!proj.active || proj.aiStyle != 7)
                    continue;

                if (proj.ai[0] != 1 && proj.timeLeft < 36000 - 3 && proj.Hitbox.Intersects(NPC.Hitbox))
                {
                    proj.ai[0] = 2;
                    proj.netUpdate = true;
                }
            }

            prevPos = NPC.position;
            BeingStoodOn = false;
        }
    }
}