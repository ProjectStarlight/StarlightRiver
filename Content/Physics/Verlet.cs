using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

namespace StarlightRiver.Physics
{
    public class VerletChainInstance
    {
        #region verlet chain example
        /*Chain = new VerletChainInstance //chain example
        {
            segmentCount = 8,
            segmentDistance = 32,
            constraintRepetitions = 10,//defaults to 2, but with drag this may cause the rope to get stretched
            //customDistances = true,
            //segmentDistanceList = new List<float>
            //{
            //    64f,
            //    32f,
            //    24f,
            //    24f,
            //    32f,
            //    64f,
            //    86f,
            //    176f
            //},
            drag = 1.05f,
            forceGravity = new Vector2(0f, 1f),
            gravityStrengthMult = 1f
        };*/
        //Chain.UpdateChain(projectile.Center); //chain example
        #endregion

        //base
        public bool ChainActive = true;
        public bool init = false;

        public List<RopeSegment> ropeSegments = new List<RopeSegment>();

        //distances
        public int segmentDistance = 5;

        public bool customDistances = false;
        public List<float> segmentDistanceList = new List<float>();//length must match the segment count

        //general
        public int segmentCount = 10;
        public int constraintRepetitions = 2;
        public float drag = 1;
        public float scale = 1;

        //gravity
        public Vector2 forceGravity = new Vector2(0f, 1f);//x, y (positive = down)
        public float gravityStrengthMult = 1f;

        public bool customGravity = false;
        public List<Vector2> forceGravityList = new List<Vector2>();//length must match the segment count

        public static RenderTarget2D target = Main.dedServ ? null : new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth / 2, Main.screenHeight / 2, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        public static List<VerletChainInstance> toDraw = new List<VerletChainInstance>();

        public VerletChainInstance(bool specialDraw)
        {
            if (!specialDraw) toDraw.Add(this);
        }

        private void Start(Vector2 targetPosition)
        {
            Vector2 ropeStartPoint = targetPosition;

            for (int i = 0; i < segmentCount; i++)
            {
                ropeSegments.Add(new RopeSegment(ropeStartPoint));

                if ((customGravity ? forceGravityList[i] : forceGravity) != Vector2.Zero)
                    ropeStartPoint += Vector2.Normalize(customGravity ? forceGravityList[i] : forceGravity) * (customDistances ? segmentDistanceList[i] : segmentDistance);
                else
                    ropeStartPoint.Y += customDistances ? segmentDistanceList[i] : segmentDistance;
            }
        }


        public void UpdateChain(Vector2 targetPosition)
        {
            if (ChainActive)//the below else can be renabled for this to reset the chain, or this check can be removed
            {
                if (init == false)
                {
                    Start(targetPosition); //run once
                    init = true;
                }
                Simulate(targetPosition);
            }
        }

        private void Simulate(Vector2 targetPosition)
        {
            for (int i = 1; i < segmentCount; i++)
            {
                Vector2 velocity = (ropeSegments[i].posNow - ropeSegments[i].posOld) / drag;
                ropeSegments[i].posOld = ropeSegments[i].posNow;
                ropeSegments[i].posNow += velocity;
                ropeSegments[i].posNow += (customGravity ? forceGravityList[i] : forceGravity) * gravityStrengthMult;
            }

            for (int i = 0; i < constraintRepetitions; i++)//the amount of times Constraints are applied per update
                ApplyConstraint(targetPosition);
        }

        private void ApplyConstraint(Vector2 targetPosition)
        {
            ropeSegments[0].posNow = targetPosition;

            for (int i = 0; i < segmentCount - 1; i++)
            {
                float segmentDist = customDistances ? segmentDistanceList[i] : segmentDistance;

                float dist = (ropeSegments[i].posNow - ropeSegments[i + 1].posNow).Length();
                float error = Math.Abs(dist - segmentDist);
                Vector2 changeDir = Vector2.Zero;

                if (dist > segmentDist)
                    changeDir = Vector2.Normalize(ropeSegments[i].posNow - ropeSegments[i + 1].posNow);
                else if (dist < segmentDist)
                    changeDir = Vector2.Normalize(ropeSegments[i + 1].posNow - ropeSegments[i].posNow);

                Vector2 changeAmount = changeDir * error;
                if (i != 0)
                {
                    ropeSegments[i].posNow -= changeAmount * 0.5f;
                    ropeSegments[i] = ropeSegments[i];
                    ropeSegments[i + 1].posNow += changeAmount * 0.5f;
                    ropeSegments[i + 1] = ropeSegments[i + 1];
                }
                else
                {
                    ropeSegments[i + 1].posNow += changeAmount;
                    ropeSegments[i + 1] = ropeSegments[i + 1];
                }
            }
        }

        public void IterateRope(Action<int> iterateMethod) //method for stuff other than drawing, only passes index
        {
            for (int i = 0; i < segmentCount; i++)
                iterateMethod(i);
        }

        public void PrepareStrip(out VertexBuffer buffer, Vector2 offset, float scale)
        {
            var buff = new VertexBuffer(Main.graphics.GraphicsDevice, typeof(VertexPositionColor), segmentCount * 9 - 6, BufferUsage.WriteOnly);

            VertexPositionColor[] verticies = new VertexPositionColor[segmentCount * 9 - 6];

            float rotation = (ropeSegments[0].posScreen - ropeSegments[1].posScreen).ToRotation() + (float)Math.PI / 2;

            verticies[0] = new VertexPositionColor((ropeSegments[0].posScreen + offset + Vector2.UnitY.RotatedBy(rotation - Math.PI / 4) * -5).Vec3().ScreenCoord(), ropeSegments[0].color);
            verticies[1] = new VertexPositionColor((ropeSegments[0].posScreen + offset + Vector2.UnitY.RotatedBy(rotation + Math.PI / 4) * -5).Vec3().ScreenCoord(), ropeSegments[0].color);
            verticies[2] = new VertexPositionColor((ropeSegments[1].posScreen + offset).Vec3().ScreenCoord(), ropeSegments[1].color);

            for (int k = 1; k < segmentCount - 1; k++)
            {
                float rotation2 = (ropeSegments[k - 1].posScreen - ropeSegments[k].posScreen).ToRotation() + (float)Math.PI / 2;

                int point = k * 9 - 6;
                int off = Math.Min(k, segmentCount - (segmentCount / 4));

                verticies[point] = new VertexPositionColor((ropeSegments[k].posScreen + offset + Vector2.UnitY.RotatedBy(rotation2 - Math.PI / 4) * -(segmentCount - off) * scale).Vec3().ScreenCoord(), ropeSegments[k].color);
                verticies[point + 1] = new VertexPositionColor((ropeSegments[k].posScreen + offset + Vector2.UnitY.RotatedBy(rotation2 + Math.PI / 4) * -(segmentCount - off) * scale).Vec3().ScreenCoord(), ropeSegments[k].color);
                verticies[point + 2] = new VertexPositionColor((ropeSegments[k + 1].posScreen + offset).Vec3().ScreenCoord(), ropeSegments[k + 1].color);

                int extra = k == 1 ? 0 : 6;
                verticies[point + 3] = verticies[point];
                verticies[point + 4] = verticies[point - (3 + extra)];
                verticies[point + 5] = verticies[point - (1 + extra)];

                verticies[point + 6] = verticies[point - (2 + extra)];
                verticies[point + 7] = verticies[point + 1];
                verticies[point + 8] = verticies[point - (1 + extra)];
            }

            buff.SetData(verticies);

            buffer = buff;
        }

        private static readonly BasicEffect basicEffect = Main.dedServ ? null : new BasicEffect(Main.graphics.GraphicsDevice)
        {
            VertexColorEnabled = true
        };

        public void DrawStrip(Func<Vector2, VertexBuffer> prepareFunction, Effect effect = null, Vector2 offset = default)
        {
            if (ropeSegments.Count < 1 || Main.dedServ) return;
            GraphicsDevice graphics = Main.graphics.GraphicsDevice;

            var buffer = prepareFunction(offset);
            graphics.SetVertexBuffer(buffer);

            if (effect is null)
                effect = basicEffect;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawPrimitives(PrimitiveType.TriangleList, 0, segmentCount * 3 - 2);
            }
        }

        public void DrawStrip(float scale, Vector2 offset = default)
        {
            if (ropeSegments.Count < 1 || Main.dedServ) return;
            GraphicsDevice graphics = Main.graphics.GraphicsDevice;

            PrepareStrip(out var buffer, offset, scale);
            graphics.SetVertexBuffer(buffer);

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawPrimitives(PrimitiveType.TriangleList, 0, segmentCount * 3 - 2);
            }
        }

        public static void DrawStripsPixelated(SpriteBatch spriteBatch)
        {
            if (Main.dedServ) return;

            var pos = (Main.screenLastPosition - Main.screenPosition).ToPoint16();

            spriteBatch.Draw(target, new Rectangle(pos.X, pos.Y, Main.screenWidth, Main.screenHeight), Color.White);
        }

        public void DrawRope(SpriteBatch spritebatch, Action<SpriteBatch, int, Vector2> drawMethod_curPos) //current position
        {
            for (int i = 0; i < segmentCount; i++)
                drawMethod_curPos(spritebatch, i, ropeSegments[i].posNow);
        }

        public void DrawRope(SpriteBatch spritebatch, Action<SpriteBatch, int, RopeSegment> drawMethod_curSeg) //current segment (has position and previous position)
        {
            for (int i = 0; i < segmentCount; i++)
                drawMethod_curSeg(spritebatch, i, ropeSegments[i]);
        }

        public void DrawRope(SpriteBatch spritebatch, Action<SpriteBatch, int, Vector2, Vector2, Vector2> drawMethod_curPos_prevPos_nextPos)//current position, previous point position, next point position
        {
            for (int i = 0; i < segmentCount; i++)
                drawMethod_curPos_prevPos_nextPos(spritebatch, i, ropeSegments[i].posNow, i > 0 ? ropeSegments[i - 1].posNow : Vector2.Zero, i < segmentCount - 1 ? ropeSegments[i + 1].posNow : Vector2.Zero);
        }
    }
}