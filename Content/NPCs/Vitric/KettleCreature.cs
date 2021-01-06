using Microsoft.Xna.Framework;

using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Tiles.Vitric;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.NPCs.Vitric
{
    internal class KettleCreature : ModNPC
    {
        KettleLimb leftLeg;
        KettleLimb rightLeg;

        private bool Floating => !leftLeg.footOnGround && !rightLeg.footOnGround;
        private KettleLimb FootOffGround => FootOnGround == leftLeg ? rightLeg : leftLeg;
        private KettleLimb FootOnGround;

        private const int WALK_RADIUS = 32;

        private ref float Timer => ref npc.ai[0];
        private ref float WalkTimer => ref npc.ai[1];
        private ref float Landed => ref npc.ai[2];

        public override string Texture => "StarlightRiver/Assets/NPCs/Vitric/KettleCreature";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Kettle Kreature");
        }

        public override void SetDefaults()
        {
            npc.width = 32;
            npc.height = 32;
            npc.knockBackResist = 0.8f;
            npc.lifeMax = 100;
            npc.noGravity = true;
            npc.noTileCollide = false;
            npc.damage = 15;
            npc.aiStyle = -1;
            //npc.HitSound = SoundID.NPCHit1;
            //npc.DeathSound = SoundID.NPCDeath4;
        }

        public override void AI()
        {
            Timer++;

            if(Timer == 1) // On-Spawn
            {
                leftLeg = new KettleLimb(this, new Vector2(-10, 0));
                rightLeg = new KettleLimb(this, new Vector2(10, 0));
            }

            if(!Floating)
            {
                npc.velocity *= 0;
                npc.Center = Vector2.Lerp(leftLeg.foot, rightLeg.foot, 0.5f) + new Vector2(0, -70);

                if(leftLeg.savedPoint == default && rightLeg.savedPoint == default && Landed == 0) //landing case
                {
                    WalkTimer = 0;
                    rightLeg.savedPoint = rightLeg.foot + Vector2.UnitX * WALK_RADIUS;
                    rightLeg.foot.Y -= 5;
                    FootOnGround = leftLeg;
                    Landed = 1;
                }

                WalkTimer += 0.14f;
                FootOffGround.foot = FootOffGround.savedPoint + (Vector2.UnitX * -WALK_RADIUS * npc.direction).RotatedBy(WalkTimer * npc.direction);

                if (WalkTimer > 4.68f)
                {
                    npc.velocity.Y -= 4;
                    npc.velocity.X += 4 * npc.direction;
                    leftLeg.MoveWholeLimb(npc.velocity);
                    rightLeg.MoveWholeLimb(npc.velocity);
                    WalkTimer = 0;
                }

                if(FootOffGround.footOnGround && WalkTimer > 0.5f)
                {
                    WalkTimer = 0;
                    FootOnGround.savedPoint = FootOnGround.foot + Vector2.UnitX * WALK_RADIUS * npc.direction;
                    FootOnGround.foot.Y -= 16;
                    FootOnGround = FootOffGround;

                    npc.TargetClosest(true);
                    npc.direction = Main.player[npc.target].Center.X > npc.Center.X ? 1 : -1;
                }
            }

            else
            {
                leftLeg.MoveWholeLimb(npc.velocity);
                rightLeg.MoveWholeLimb(npc.velocity);
                npc.velocity.Y += 0.43f;
                npc.velocity.X *= 0.95f;

                if (Landed == 1)
                {
                    rightLeg.savedPoint = rightLeg.foot + Vector2.UnitX * WALK_RADIUS;
                    leftLeg.savedPoint = leftLeg.foot + Vector2.UnitX * WALK_RADIUS;
                }
            }

            leftLeg.Constrain();
            rightLeg.Constrain();
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            leftLeg.Draw(spriteBatch);
            rightLeg.Draw(spriteBatch);
        }
    }

    class KettleLimb
    {
        public Vector2 joint;
        public Vector2 foot;
        public Vector2 attachPoint;
        public Vector2 savedPoint;
        private Vector2 attachOff;

        private KettleCreature parent;
        private NPC parentNPC => parent.npc;

        private const int LIMB_LENGTH = 32;

        public bool footOnGround => Helper.PointInTile(foot);

        public KettleLimb(KettleCreature parent, Vector2 attachOff)
        {
            this.parent = parent;
            attachPoint = parent.npc.Center + attachOff;
            foot = attachPoint + new Vector2(0, 64);
            joint = attachPoint + new Vector2(0, 32);
            this.attachOff = attachOff;
        }

        public void MoveWholeLimb(Vector2 velocity)
        {
            attachPoint += velocity;
            joint += velocity;
            foot += velocity;
        }

        public void Constrain()
        {
            joint = Vector2.Lerp(foot, attachPoint, 0.5f) + Vector2.UnitY * -15;

            if (Vector2.Distance(foot, attachPoint) > 120) foot -= Vector2.Normalize(foot - attachPoint) * 3;

            attachPoint = parentNPC.Center + attachOff;
        }

        public void Draw(SpriteBatch sb)
        {
            var limbTex = GetTexture("StarlightRiver/Assets/NPCs/Vitric/KettleCreatureLimb");
            var jointTex = GetTexture("StarlightRiver/Assets/NPCs/Vitric/KettleCreatureJoint");

            sb.Draw(jointTex, joint - Main.screenPosition, null, Color.White, 0, jointTex.Size() / 2, 1, 0, 0);
            sb.Draw(jointTex, attachPoint - Main.screenPosition, null, Color.White, 0, jointTex.Size() / 2, 1, 0, 0);
            sb.Draw(jointTex, foot - Main.screenPosition, null, Color.White, 0, jointTex.Size() / 2, 1, 0, 0);

            sb.Draw(limbTex, joint - Main.screenPosition, null, Color.White, (joint - attachPoint).ToRotation() + 1.57f, Vector2.UnitX * limbTex.Width / 2, 1, 0, 0);
            sb.Draw(limbTex, foot - Main.screenPosition, null, Color.White, (foot - joint).ToRotation() + 1.57f, Vector2.UnitX * limbTex.Width / 2, 1, 0, 0);
        }
    }
}