using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core;
using StarlightRiver.Physics;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using System.Collections.Generic;

using Terraria.DataStructures;
using Terraria.GameContent;

using Terraria.Audio;

using System;
using System.Linq;
using static Terraria.ModLoader.ModContent;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader.Utilities;

namespace StarlightRiver.Content.NPCs.Snow
{
    internal class Snoobel : ModNPC
    {
        private enum Phase
        {
            Walking = 0,
            Whipping = 1,
            Reaching = 2,
            Pulling = 3,
            Deciding = 4
        }
        public override string Texture => AssetDirectory.SnowNPC + "Snoobel";

        private const int XFRAMES = 2;
        private readonly int NUM_SEGMENTS = 30;

        private Phase phase = Phase.Walking;

        private Trail trail;

        private int xFrame = 0;
        private int yFrame = 0;
        private int frameCounter = 0;

        private int attackTimer = 0;

        private VerletChain trunkChain;

        private Vector2 trunkStart => NPC.Center + new Vector2(35 * NPC.spriteDirection, 4);

        private Player target => Main.player[NPC.target];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Snoobel");
            Main.npcFrameCount[NPC.type] = 9;

            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
            {
                
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

        }

        public override void SetDefaults()
        {
            NPC.width = 38;
            NPC.height = 44;
            NPC.damage = 0;
            NPC.defense = 5;
            NPC.lifeMax = 60;
            NPC.value = 100f;
            NPC.knockBackResist = 0f;
            NPC.HitSound = SoundID.Grass;
            NPC.DeathSound = SoundID.Grass;
        }

        public override void OnSpawn(IEntitySource source)
        {
            trunkChain = new VerletChain(NUM_SEGMENTS, true, trunkStart, 4);
            trunkChain.forceGravity = new Vector2(0, 0.1f);
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return spawnInfo.Player.ZoneSnow && spawnInfo.Player.Center.Y > Main.worldSurface ? 0.2f : 0;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.UndergroundSnow,
                new FlavorTextBestiaryInfoElement("[PH] It's a snoobel! CUUUUUUUUUTE")
            });
        }

        public override void AI()
        {
            NPC.TargetClosest(true);

            switch (phase)
            {
                case Phase.Walking:
                    WalkingBehavior(); break;
                case Phase.Whipping:
                    WhippingBehavior(); break;
                case Phase.Reaching:
                    ReachingBehavior(); break;
                case Phase.Pulling:
                    PullingBehavior(); break;
                case Phase.Deciding:
                    DecidingBehavior(); break;
            }

            if (trunkChain == null)
                return;

            UpdateTrunk();

            if (!Main.dedServ)
                ManageTrail();
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;

            SpriteEffects effects = SpriteEffects.None;
            Vector2 origin = new Vector2((NPC.width * 0.75f), (NPC.height / 2) - 6);

            if (NPC.spriteDirection != 1)
                effects = SpriteEffects.FlipHorizontally;

            Vector2 slopeOffset = new Vector2(0, NPC.gfxOffY);
            Main.spriteBatch.Draw(texture, slopeOffset + NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation, origin, NPC.scale, effects, 0f);

            DrawTrunk();
            return false;
        }

        public override void FindFrame(int frameHeight)
        {
            int frameWidth = 60;
            NPC.frame = new Rectangle(frameWidth * xFrame, frameHeight * yFrame, frameWidth, frameHeight);
        }

        public override void OnKill()
        {
            if(Main.netMode != NetmodeID.Server)
            {
                
            }
        }

        private void DrawTrunk()
        {
            Main.spriteBatch.End();
            Effect effect = Terraria.Graphics.Effects.Filters.Scene["AlphaTextureTrail"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(Texture + "_Whip").Value);
            effect.Parameters["alpha"].SetValue(1);

            trail?.Render(effect);

            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, NUM_SEGMENTS - 1, new TriangularTip(20), factor => 10, factor =>
            {
                return Color.White;
            });

            List<Vector2> positions = GetTrunkPoints();
            trail.NextPosition = positions[NUM_SEGMENTS - 1];

            positions.RemoveAt(NUM_SEGMENTS - 1);
            trail.Positions = positions.ToArray();
        }

        private void WalkingBehavior()
        {
            xFrame = 1;
            frameCounter++;

            if (frameCounter % 5 == 0)
            {
                yFrame++;
                yFrame %= Main.npcFrameCount[NPC.type];
            }

            attackTimer++;
            if (attackTimer >= 400)
            {
                phase = Phase.Deciding;
                attackTimer = 0;
            }

            NPC.spriteDirection = NPC.direction = Math.Sign(target.Center.X - NPC.Center.X);

            NPC.velocity.X += NPC.direction * 0.15f;

            NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -3, 3);
        }

        private void WhippingBehavior()
        {
            phase = Phase.Walking;
        }

        private void ReachingBehavior()
        {
            phase = Phase.Walking;
        }

        private void PullingBehavior()
        {
            phase = Phase.Walking;
        }

        private void DecidingBehavior()
        {
            phase = (Phase)Main.rand.Next(4);
            switch (phase)
            {
                case Phase.Walking:
                    WalkingBehavior(); break;
                case Phase.Whipping:
                    WhippingBehavior(); break;
                case Phase.Reaching:
                    ReachingBehavior(); break;
                case Phase.Pulling:
                    PullingBehavior(); break;
            }
        }

        private void UpdateTrunk()
        {
            trunkChain.UpdateChain();

            trunkChain.startPoint = trunkStart;
            //trunkChain.ropeSegments[1].posNow = trunkStart + new Vector2(20 * NPC.spriteDirection, 0);
        }

        private List<Vector2> GetTrunkPoints()
        {
            List<Vector2> points = new List<Vector2>();

            foreach (RopeSegment ropeSegment in trunkChain.ropeSegments)
                points.Add(ropeSegment.posNow);

            return points;
        }
    }
}