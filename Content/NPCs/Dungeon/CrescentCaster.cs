//TODO:
//Bestiary
//Make beams "shake" more
//Make him unable to support through walls
//Spawning
//Drops
//Value
//Balance
//Gore
//Hitsound
//Deathsound

//MAYBE TODO:
//Make support targets lose barrier gradually


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Core;
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

namespace StarlightRiver.Content.NPCs.Dungeon
{
    public class CrescentCasterBolt
    {
        public NPC TargetNPC;
        public NPC Owner;

        public Vector2 MidPoint;
        public Vector2 MidPointDirection;

        public Trail trail;
        public Trail trail2;

        public float fade;

        public Color baseColor = new Color(200, 230, 255);
        public Color endColor = Color.Purple;

        public CrescentCasterBolt(NPC targetNPC, NPC owner, Vector2 midPoint, Vector2 midPointDirection, GraphicsDevice device)
        {
            TargetNPC = targetNPC;
            Owner = owner;
            MidPoint = midPoint;
            MidPointDirection = midPointDirection;
            trail = new Trail(device, 15, new TriangularTip(4), factor => 16, factor =>
            {
                if (factor.X > 0.99f)
                    return Color.Transparent;

                return new Color(160, 220, 255) * fade * 0.1f * EaseFunction.EaseCubicOut.Ease(1 - factor.X);
            });

            trail2 = new Trail(device, 15, new TriangularTip(4), factor => 3 * Main.rand.NextFloat(0.55f, 1.45f), factor =>
            {
                float progress = EaseFunction.EaseCubicOut.Ease(1 - factor.X);
                return Color.Lerp(baseColor, endColor, EaseFunction.EaseCubicIn.Ease(1 - progress)) * fade * progress;
            });

            UpdateTrailPoints();

            fade = 0;
        }

        public void UpdateTrailPoints()
        {
            trail.Positions = trail2.Positions = BezierPoints().ToArray();
            trail.NextPosition = trail2.NextPosition = TargetNPC.Center;
        }

        public List<Vector2> BezierPoints()
        {
            Vector2 startPoint = Owner.Center + new Vector2(Owner.spriteDirection * 3, -25);
            BezierCurve curve = new BezierCurve(startPoint, MidPoint, TargetNPC.Center);
            return curve.GetPoints(15);
        }

        public void Render(Effect effect)
        {
            trail?.Render(effect);
            trail2?.Render(effect);
        }
    }

    internal class CrescentCaster : ModNPC,IDrawPrimitive
    {
        public override string Texture => AssetDirectory.DungeonNPC + Name;

        private const int XFRAMES = 3;
        private readonly float ACCELERATION = 0.15f;
        private readonly float MAXSPEED = 2;

        private readonly int aiCounterReset = 800;

        private int xFrame = 0;
        private int yFrame = 0;

        private int aiCounter = 0;

        private List<NPC> supportTargets = new List<NPC>();

        private List<CrescentCasterBolt> Bolts = new List<CrescentCasterBolt>();

        private Player target => Main.player[NPC.target];

        private bool Casting => aiCounter % aiCounterReset > 400;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Crescent Caster");
            Main.npcFrameCount[NPC.type] = 10;
        }

        public override void SetDefaults()
        {
            NPC.width = 30;
            NPC.height = 48;
            NPC.damage = 0;
            NPC.defense = 5;
            NPC.lifeMax = 500;
            NPC.value = 10f;
            NPC.knockBackResist = 0.6f;
            NPC.HitSound = SoundID.Item27 with
            {
                Pitch = -0.3f
            };
            NPC.DeathSound = SoundID.Shatter;
        }

        public override void AI()
        {
            NPC.TargetClosest(true);
            aiCounter++;

            if (Casting)
                CastBehavior();
            else
                WalkBehavior();
        }

        public override void FindFrame(int frameHeight)
        {
            int frameWidth = 70;
            NPC.frame = new Rectangle(frameWidth * xFrame, frameHeight * yFrame, frameWidth, frameHeight);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;

            Vector2 origin = new Vector2(NPC.frame.Width / 2, 52);
            SpriteEffects effects = SpriteEffects.FlipHorizontally;

            if (NPC.spriteDirection != 1)
            {
                effects = SpriteEffects.None;
                origin.X = NPC.frame.Width - origin.X;
            }

            spriteBatch.Draw(tex, NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation, origin, NPC.scale, effects, 0f);
            spriteBatch.Draw(glowTex, NPC.Center - screenPos, NPC.frame, Color.White, NPC.rotation, origin, NPC.scale, effects, 0f);

            return false;
        }

        public override void OnKill()
        {
            ClearBarrierAndBolts(supportTargets);
        }

        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["LightningTrail"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.05f);
            effect.Parameters["repeats"].SetValue(1f);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
            foreach (CrescentCasterBolt bolt in Bolts)
                bolt.Render(effect);
        }

        private void CastBehavior()
        {
            var tempTargets = Main.npc.Where(x => x.active && !x.friendly && x.Distance(NPC.Center) < 500 && x != NPC).ToList(); //purpose of temptargets is to check npcs who were being supported but are no longer
            var toReduceBarrier = Main.npc.Where(x => x.active && !tempTargets.Contains(x) && supportTargets.Contains(x)).ToList();

            ClearBarrierAndBolts(toReduceBarrier);

            var toAddBolts = Main.npc.Where(x => x.active && tempTargets.Contains(x) && !supportTargets.Contains(x)).ToList();

            foreach(NPC boltNPC in toAddBolts)
            {
                Vector2 directionTo = NPC.DirectionTo(boltNPC.Center);
                Bolts.Add(new CrescentCasterBolt(boltNPC, NPC, CalculateMidpoint(boltNPC), Main.rand.NextVector2Circular(1, 1), Main.instance.GraphicsDevice));
            }

            supportTargets = tempTargets;

            UpdateBolts();

            if (supportTargets.Count == 0)
            {
                aiCounter = 390;
                WalkBehavior();
                return;
            }

            switch (xFrame)
            {
                case 0: //first frame of casting
                    xFrame = 1;
                    NPC.frameCounter = 0;
                    yFrame = 0;
                    break;
                case 1: //Winding up to cast
                    NPC.frameCounter++;

                    if (NPC.frameCounter % 4 == 0)
                    {
                        yFrame++;
                    }

                    if (yFrame == 10)
                    {
                        yFrame = 0;
                        xFrame = 2;
                        NPC.frameCounter = 0;
                    }

                    break;
                case 2: //Supporting
                    SupportBehavior();
                    break;
            }
        }

        private void UpdateBolts()
        {
            foreach(CrescentCasterBolt bolt in Bolts)
            {
                if (Main.rand.NextBool(40)) //Change bolt curve
                {
                    bolt.MidPoint = CalculateMidpoint(bolt.TargetNPC);
                    bolt.MidPointDirection = Main.rand.NextVector2Circular(1, 1);
                }
                bolt.MidPoint += bolt.MidPointDirection;

                bolt.UpdateTrailPoints();

                if (aiCounter % aiCounterReset < aiCounterReset - 10 && xFrame == 2)
                {
                    if (bolt.fade < 1)
                        bolt.fade += 0.1f;
                }
                else if(bolt.fade > 0)
                    bolt.fade -= 0.1f;

            }
        }

        private void SupportBehavior()
        {
            NPC.frameCounter++;

            if (NPC.frameCounter % 4 == 0)
            {
                yFrame++;
                yFrame %= 5;
            }

            foreach (NPC supportTarget in supportTargets)
            {
                BarrierNPC barrierNPC = supportTarget.GetGlobalNPC<BarrierNPC>();

                barrierNPC.MaxBarrier = 250;
                barrierNPC.RechargeRate = 90;
                barrierNPC.RechargeDelay = 60;
            }
        }

        private void WalkBehavior()
        {
            ClearBarrierAndBolts(supportTargets);
            supportTargets.Clear();
            xFrame = 0;
            NPC.frameCounter++;

            if (NPC.velocity.Y == 0)
            {
                if (NPC.frameCounter % 4 == 0)
                {
                    yFrame++;
                    yFrame %= 8;
                }
            }
            else
                yFrame = 7;

            int direction = Math.Sign(target.Center.X - NPC.Center.X);
            NPC.direction = NPC.spriteDirection = direction;

            NPC.velocity.X += direction * ACCELERATION;
            NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -MAXSPEED, MAXSPEED);

            if (NPC.collideX && NPC.velocity.Y == 0)
                NPC.velocity.Y = -6;
        }

        private void ClearBarrierAndBolts(List<NPC> npcs)
        {
            foreach (NPC npc in npcs)
            {
                BarrierNPC clearBarrierNPC = npc.GetGlobalNPC<BarrierNPC>();
                clearBarrierNPC.MaxBarrier = 0;
                clearBarrierNPC.RechargeRate = 0;
                clearBarrierNPC.Barrier = 0;
                clearBarrierNPC.RechargeDelay = 180;
            }

            foreach (CrescentCasterBolt bolt in Bolts.ToArray())
            {
                if (npcs.Contains(bolt.TargetNPC))
                    Bolts.Remove(bolt);
            }
        }

        private Vector2 CalculateMidpoint(NPC other)
        {
            Vector2 directionTo = NPC.DirectionTo(other.Center);
            return NPC.Center + (directionTo.RotatedBy(-Math.Sign(directionTo.X) * Main.rand.NextFloat(0.5f, 1f)) * Main.rand.NextFloat(1f, 1.5f) * NPC.Distance(other.Center));
        }
    }
}