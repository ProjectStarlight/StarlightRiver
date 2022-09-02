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
            Pulling = 2,
            Deciding = 3
        }

        public override string Texture => AssetDirectory.SnowNPC + "Snoobel";

        public const int NUM_SEGMENTS = 30;

        private readonly int WHIP_BUILDUP = 90;
        private readonly int WHIP_DURATION = 40;

        private readonly int PULL_DURATION = 80;

        private Phase phase = Phase.Walking;

        private Trail trail;
        private List<Vector2> cache = new List<Vector2>();

        private int xFrame = 0;
        private int yFrame = 0;
        private int frameCounter = 0;

        private int attackTimer = 0;

        public bool pulling = false;

        public VerletChain trunkChain;

        public bool CanHit => (attackTimer > WHIP_BUILDUP && phase == Phase.Whipping) || pulling;

        private Vector2 TrunkStart => NPC.Center + new Vector2(33 * NPC.spriteDirection, 7 + NPC.gfxOffY + NPC.velocity.Y);

        private Player Target => Main.player[NPC.target];

        public override void Load()
        {
            for (int j = 1; j <= 5; j++)
                GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, AssetDirectory.SnowNPC + "Gores/SnoobelGore" + j);
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Snoobel");
            Main.npcFrameCount[NPC.type] = 9;
        }

        public override void SetDefaults()
        {
            NPC.width = 46;
            NPC.height = 40;
            NPC.damage = 0;
            NPC.defense = 5;
            NPC.lifeMax = 120;
            NPC.value = 100f;
            NPC.knockBackResist = 0.3f;
            NPC.HitSound = SoundID.NPCHit39;
            NPC.DeathSound = SoundID.NPCDeath16;
        }

        public override void OnSpawn(IEntitySource source)
        {
            trunkChain = new VerletChain(NUM_SEGMENTS, true, TrunkStart, 2, true);
            trunkChain.forceGravity = new Vector2(0, 0.1f);
            trunkChain.simStartOffset = 0;

            Projectile proj = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<SnoobelCollider>(), (int)(40 * (Main.expertMode ? 0.5f : 1f)), 0);
            (proj.ModProjectile as SnoobelCollider).parent = NPC;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return spawnInfo.Player.ZoneSnow && spawnInfo.Player.Center.Y > Main.worldSurface ? 0.1f : 0;
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
                case Phase.Pulling:
                    PullingBehavior(); break;
                case Phase.Deciding:
                    DecidingBehavior(); break;
            }

            if (trunkChain is null)
                return;

            UpdateTrunk();

            if (!Main.dedServ)
            {
                ManageCache();
                ManageTrail();
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D bestiaryTexture = Request<Texture2D>(Texture + "_Bestiary").Value;
            Vector2 slopeOffset = new Vector2(0, NPC.gfxOffY);

            if (NPC.IsABestiaryIconDummy)
            {
                Main.spriteBatch.Draw(bestiaryTexture, slopeOffset + NPC.Center - screenPos, null, drawColor, NPC.rotation, bestiaryTexture.Size() / 2, NPC.scale, SpriteEffects.None, 0f);
                return false;
            }

            SpriteEffects effects = SpriteEffects.None;
            Vector2 origin = new Vector2((NPC.width * 0.75f), (NPC.height / 2) - 6);

            if (NPC.spriteDirection != 1)
                effects = SpriteEffects.FlipHorizontally;

            Main.spriteBatch.Draw(texture, slopeOffset + NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation, origin, NPC.scale, effects, 0f);

            DrawTrunk();
            return false;
        }

        public override void FindFrame(int frameHeight)
        {
            int frameWidth = 70;
            NPC.frame = new Rectangle(frameWidth * xFrame, (frameHeight * yFrame) + 2, frameWidth, frameHeight - 2);
        }

        public override void OnKill()
        {
            if(Main.netMode != NetmodeID.Server)
            {
                foreach (RopeSegment segment in trunkChain.ropeSegments)
                {
                    Gore.NewGoreDirect(NPC.GetSource_Death(), segment.posNow, Main.rand.NextVector2Circular(1, 1), Mod.Find<ModGore>("SnoobelGore1").Type);
                }

                for (int j = 2; j <= 5; j++)
                {
                    Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), Main.rand.NextVector2Circular(3, 3), Mod.Find<ModGore>("SnoobelGore" + j).Type);
                }
            }
        }

        private void DrawTrunk()
        {
            if (trail == null || trail == default)
                return;

            Main.spriteBatch.End();
            Effect effect = Terraria.Graphics.Effects.Filters.Scene["SnoobelTrunk"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(Texture + "_Whip").Value);
            effect.Parameters["sampleTextureEnd"].SetValue(ModContent.Request<Texture2D>(Texture + "_WhipEnd").Value);
            effect.Parameters["alpha"].SetValue(1);
            effect.Parameters["flip"].SetValue(NPC.spriteDirection == 1);

            List<Vector2> points;
            if (cache == null)
                points = GetTrunkPoints();
            else
                points = trail.Positions.ToList();

            effect.Parameters["totalLength"].SetValue(TotalLength(points));
            trail.Render(effect);

            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
        }

        private void ManageCache()
        {
            cache = new List<Vector2>();
            cache.Add(TrunkStart);

            float pointLength = TotalLength(GetTrunkPoints()) / NUM_SEGMENTS;

            float pointCounter = 0;

            int presision = 30; //This normalizes length between points so it doesnt squash super weirdly on certain parts
            for (int i = 0; i < NUM_SEGMENTS - 1; i++)
            {
                for (int j = 0; j < presision; j++)
                {
                    pointCounter += (trunkChain.ropeSegments[i].posNow - trunkChain.ropeSegments[i + 1].posNow).Length() / (float)presision;
                    while (pointCounter > pointLength)
                    {
                        float lerper = j / (float)presision;
                        cache.Add(Vector2.Lerp(trunkChain.ropeSegments[i].posNow, trunkChain.ropeSegments[i + 1].posNow, lerper));
                        pointCounter -= pointLength;
                    }
                }
            }

            while (cache.Count < NUM_SEGMENTS)
                cache.Add(trunkChain.ropeSegments[NUM_SEGMENTS - 1].posNow);

            while (cache.Count > NUM_SEGMENTS)
                cache.RemoveAt(cache.Count - 1);
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, NUM_SEGMENTS, new TriangularTip(1), factor => 7, factor =>
            {
                return Lighting.GetColor((int)(NPC.Center.X / 16), (int)(NPC.Center.Y / 16));
            });


            List<Vector2> positions = cache;
            trail.NextPosition = positions[NUM_SEGMENTS - 1];

            trail.Positions = positions.ToArray();
        }

        private void WalkingBehavior()
        {
            xFrame = 1;
            frameCounter++;

            attackTimer++;
            if (attackTimer >= 200)
            {
                phase = Phase.Deciding;
                attackTimer = 0;
            }

            float xdist = Math.Abs(Target.Center.X - NPC.Center.X);

            if (xdist > 30)
            {
                xFrame = 1;
                if (frameCounter % 5 == 0)
                {
                    yFrame++;
                    yFrame %= Main.npcFrameCount[NPC.type];
                }

                NPC.spriteDirection = NPC.direction = Math.Sign(Target.Center.X - NPC.Center.X);

                NPC.velocity.X += NPC.direction * 0.15f;
                NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -3, 3);

                if (NPC.collideX && NPC.velocity.Y == 0)
                    NPC.velocity.Y = -8;
            }
            else
            {
                xFrame = 0;
                frameCounter = 0;
                yFrame = 0;
            }
        }

        private void WhippingBehavior()
        {
            if (attackTimer == 0)
            {
                trunkChain.customGravity = true;
                trunkChain.forceGravity = Vector2.One;
                trunkChain.forceGravities = new List<Vector2>();

                int index = 0;
                foreach (RopeSegment segment in trunkChain.ropeSegments)
                {
                    float rad = 4f * ((float)(index - 6) / NUM_SEGMENTS);
                    float xForce = 0.5f* NPC.spriteDirection * (float)Math.Cos(rad);
                    float yForce = -0.25f * (float)Math.Sin(rad * 2);
                    trunkChain.forceGravities.Add(new Vector2(xForce, -0.5f));
                    index++;
                }
            }
            xFrame = 0;
            frameCounter = 0;
            yFrame = 0;
            attackTimer++;

            if (attackTimer > WHIP_BUILDUP)
            { 
                trunkChain.customGravity = false;
                trunkChain.forceGravity = new Vector2(NPC.spriteDirection, 0);
                if (attackTimer > WHIP_DURATION + WHIP_BUILDUP)
                    EndAttack();
            }
        }

        private void PullingBehavior()
        {
            xFrame = 0;
            frameCounter = 0;
            yFrame = 0;
            attackTimer++;

            if (!pulling)
            {
                trunkChain.customGravity = false;
                trunkChain.forceGravity = NPC.DirectionTo(Target.Center) * 1.1f;

                Vector2 endPos = trunkChain.ropeSegments[NUM_SEGMENTS - 1].posNow + (16 * NPC.DirectionTo(Target.Center));
                Point endPosTileGrid = new Point((int)(endPos.X / 16), (int)(endPos.Y / 16));
                Tile tile = Framing.GetTileSafely(endPosTileGrid);
                if (tile != null && tile.HasTile && Main.tileSolid[tile.TileType] && attackTimer > 6)
                {
                    pulling = true;
                    trunkChain.endPoint = endPos + (16 * NPC.DirectionTo(endPos));
                    trunkChain.useEndPoint = true;
                    trunkChain.forceGravity = Vector2.Zero;
                }

                if (attackTimer > PULL_DURATION)
                    EndAttack();
            }
            else
            {
                NPC.noGravity = true;
                Vector2 dir = trunkChain.endPoint - (TrunkStart + new Vector2(60 * NPC.spriteDirection, 0));

                if (NPC.velocity.Length() < 15)
                {
                    NPC.velocity += Vector2.Normalize(dir) * 0.3f;
                    NPC.velocity *= 1.05f;
                }

                NPC.direction = NPC.spriteDirection = Math.Sign(dir.X);

                if ((NPC.Center - trunkChain.endPoint).Length() < 50 || dir.Length() < 16 || attackTimer > PULL_DURATION * 2)
                {
                    trunkChain.ropeSegments[NUM_SEGMENTS - 1].posNow -= Vector2.Normalize(dir) * 16;
                    EndAttack();
                }
            }
        }

        private void DecidingBehavior()
        {
            attackTimer = 0;
            phase = (Phase)Main.rand.Next(2) + 1;
            //phase = Phase.Pulling;
            switch (phase)
            {
                case Phase.Whipping:
                    WhippingBehavior(); break;
                case Phase.Pulling:
                    PullingBehavior(); break;
            }
        }

        private void UpdateTrunk()
        {
            trunkChain.UpdateChain();

            trunkChain.startPoint = TrunkStart;

            if (pulling)
            {
                for (int i = 0; i < NUM_SEGMENTS; i++)
                {
                    float lerper = ((float)i / (NUM_SEGMENTS - 2));
                    Vector2 posToBe = Vector2.Lerp(trunkChain.startPoint, trunkChain.endPoint, lerper);
                    trunkChain.ropeSegments[i].posNow = Vector2.Lerp(trunkChain.ropeSegments[i].posNow, posToBe, 0.5f);
                }
            }

            int anchorLength = 4;
            int anchorStart = 0;

            for (int i = 0; i < anchorLength; i++)
            {
                float lerper = (float)Math.Pow((float)(i - anchorStart) / (anchorLength + 1 - anchorStart), 0.9f);
                if (i <= anchorStart)
                    lerper = 0;
                trunkChain.ropeSegments[i].posNow = Vector2.Lerp(TrunkStart + new Vector2(i * 5 * NPC.spriteDirection, 0), trunkChain.ropeSegments[i].posNow, lerper);
            }
        }

        private List<Vector2> GetTrunkPoints()
        {
            List<Vector2> points = new List<Vector2>();

            foreach (RopeSegment ropeSegment in trunkChain.ropeSegments)
                points.Add(ropeSegment.posNow);

            return points;
        }

        private float TotalLength(List<Vector2> points)
        {
            float ret = 0;
            for (int i = 1; i < points.Count; i++)
            {
                ret += (points[i] - points[i - 1]).Length();
            }

            return ret;
        }

        private void EndAttack()
        {
            NPC.noGravity = false;
            pulling = false;
            trunkChain.endPoint = Vector2.Zero;
            attackTimer = 0;
            trunkChain.forceGravity = new Vector2(0, 0.1f);
            trunkChain.useEndPoint = false;
            phase = Phase.Walking;
        }
    }

    public class SnoobelCollider : ModProjectile //Since NPCs dont support custom collision
    {
        public override string Texture => AssetDirectory.Assets + "Invisible";

        public NPC parent;

        public VerletChain Chain => (parent.ModNPC as Snoobel).trunkChain;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Snoobel");
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            if (parent.active)
            {
                Projectile.timeLeft = 2;
                Projectile.Center = parent.Center;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            bool ret = false;

            float collisionPoint = 0f;
            for (int i = 1; i < Snoobel.NUM_SEGMENTS; i++)
                ret |= Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Chain.ropeSegments[i].posNow, Chain.ropeSegments[i - 1].posNow, 8, ref collisionPoint);

            ret &= (parent.ModNPC as Snoobel).CanHit;
            return ret;
        }

        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
        {
            if ((parent.ModNPC as Snoobel).pulling)
                damage /= 3;
        }
    }
}