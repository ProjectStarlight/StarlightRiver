using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace StarlightRiver.Physics
{
	public class VerletChainSystem : IOrderedLoadable
	{
        public static RenderTarget2D target = Main.dedServ ? null : new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth / 2, Main.screenHeight / 2, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        public static List<VerletChain> toDraw = new List<VerletChain>();

        public float Priority => 1;

		public void Load()
		{
		}

		public void Unload()
		{
            target = null;
            toDraw = null;
		}
	}

	public class VerletChain 
    {
        //static
        private static readonly BasicEffect basicEffectColor = Main.dedServ ? null : new BasicEffect(Main.graphics.GraphicsDevice) { VertexColorEnabled = true };

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
        //Chain.UpdateChain(Projectile.Center); //chain example
        #endregion

        //basic variables
        public bool Init = false;
        public bool Active = false;//check Active for most init checks, since this also covers that

        public int segmentCount;
        public int segmentDistance;

        public bool collideWithTiles;

        public int constraintRepetitions = 2;
        public float drag = 1f;
        public Vector2 forceGravity = Vector2.Zero;

        //medium variables
        public bool useStartPoint = true;
        public Vector2 startPoint = Vector2.Zero;
        public bool useEndPoint = false;
        public Vector2 endPoint = Vector2.Zero;
        public float scale = 1;

        //advanced variables
        public bool customDistances = false;
        public List<int> segmentDistances;//length must match the segment count

        public bool customGravity = false;
        public List<Vector2> forceGravities;//length must match the segment count

        public VertexBuffer meshBuffer;

        public List<RopeSegment> ropeSegments;

        public int simStartOffset = 0;
        public int simEndOffset = 0;//if zero this gets set to the segment count on start

		public VerletChain(int SegCount, bool specialDraw, Vector2 StartPoint, int SegDistance, bool CollideWithTiles = false)
        {
            segmentCount = SegCount;
            segmentDistance = SegDistance;
            startPoint = StartPoint;
            collideWithTiles = CollideWithTiles;

            if (!specialDraw)
                VerletChainSystem.toDraw.Add(this);
        }

        public VerletChain(int SegCount, bool specialDraw, Vector2? StartPoint = null, Vector2? EndPoint = null, int SegDistance = 5, Vector2? Grav = null)
        {
            segmentCount = SegCount;
            segmentDistance = SegDistance;

            forceGravity = Grav ?? Vector2.Zero;

            startPoint = StartPoint ?? Vector2.Zero;

            endPoint = EndPoint ?? Vector2.Zero;

            if (!specialDraw)
                VerletChainSystem.toDraw.Add(this);

            //Start(EndPoint != null);
        }

        public VerletChain(int SegCount, bool specialDraw, Vector2? StartPoint = null, Vector2? EndPoint = null, int SegDistance = 5, Vector2? Grav = null,
            bool CustomGravs = false, List<Vector2> SegGravs = null,
            bool CustomDists = false, List<int> SegDists = null)
        {
            segmentCount = SegCount;
            segmentDistance = SegDistance;

            forceGravity = Grav ?? (CustomGravs ? Vector2.One : Vector2.Zero);

            startPoint = StartPoint ?? Vector2.Zero;

            endPoint = EndPoint ?? Vector2.Zero;

            if (customGravity = CustomGravs)
                forceGravities = SegGravs ?? Enumerable.Repeat(forceGravity, segmentCount).ToList();

            if (customDistances = CustomDists)
                segmentDistances = SegDists ?? Enumerable.Repeat(segmentDistance, segmentCount).ToList();

            if (!specialDraw)
                VerletChainSystem.toDraw.Add(this);

            //Start(EndPoint != null);
        }

        public void Start(bool SpawnEndPoint = false)//public in case you want to reset the chain
        {
            Init = true;
            Active = true;

            if (simEndOffset == 0)
            {
                simEndOffset = segmentCount;
                if (useEndPoint)
                    simEndOffset--;
            }

            ropeSegments = new List<RopeSegment>();

            Vector2 nextRopePoint = startPoint;

            for (int i = 0; i < segmentCount; i++)
            {
                ropeSegments.Add(new RopeSegment(nextRopePoint));

                if (SpawnEndPoint)
                {
                    float distance = (int)(Vector2.Distance(startPoint, endPoint) / segmentCount);
                    nextRopePoint += Vector2.Normalize(endPoint - nextRopePoint) * distance * i;
                }
                else
                {
                    int distance = customDistances ? segmentDistances[i] : segmentDistance;
                    Vector2 spawnGrav = customGravity ? forceGravities[i] * forceGravity : forceGravity;
                    if (spawnGrav != Vector2.Zero)
                        nextRopePoint += Vector2.Normalize(spawnGrav) * distance;
                    else
                        nextRopePoint.Y += distance;
                }
            }

            if (useEndPoint)
                ropeSegments[simEndOffset].posNow = endPoint;
        }

        public void UpdateChain(Vector2 Start, Vector2 End)
        {
            endPoint = End;
            startPoint = Start;
            UpdateChain();
        }

        public void UpdateChain(Vector2 Start)
        {
            startPoint = Start;
            UpdateChain();
        }

        public void UpdateChain()
        {
            if (!Init)//hacky solution to the setdefaults problem
                Start();

            if (Active)
                Simulate();
        }

        private void Simulate()
        {
            for (int i = simStartOffset; i < simEndOffset; i++)
            {
                RopeSegment segment = ropeSegments[i];
                Vector2 velocity = (segment.posNow - segment.posOld) / drag;
                segment.posOld = segment.posNow;

                Vector2 gravityVel = customGravity ? forceGravities[i] * forceGravity : forceGravity;

                velocity = TileCollision(segment.posNow, velocity);
                gravityVel = TileCollision(segment.posNow, gravityVel);

                segment.posNow += velocity;
                segment.posNow += gravityVel;
            }

            for (int i = 0; i < constraintRepetitions; i++)//the amount of times Constraints are applied per update
            {
                if (useStartPoint)
                    ropeSegments[simStartOffset].posNow = startPoint;
                if (useEndPoint)
                   ropeSegments[simEndOffset - 1].posNow = endPoint;//if the end point clamp breaks, check this
                ApplyConstraint();
            }
        }

        private void ApplyConstraint()
        {
            for (int i = simStartOffset; i < simEndOffset - 1; i++)
            {
                float segmentDist = customDistances ? segmentDistances[i] : segmentDistance;

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
                    ropeSegments[i].posNow += TileCollision(ropeSegments[i].posNow, changeAmount * -0.5f);
                    ropeSegments[i] = ropeSegments[i];
                    ropeSegments[i + 1].posNow += TileCollision(ropeSegments[i + 1].posNow, changeAmount * 0.5f);
                    ropeSegments[i + 1] = ropeSegments[i + 1];
                }
                else
                {
                    ropeSegments[i + 1].posNow += TileCollision(ropeSegments[i + 1].posNow, changeAmount);
                    ropeSegments[i + 1] = ropeSegments[i + 1];
                }
            }
        }

        public void IterateRope(Action<int> iterateMethod) //method for stuff other than drawing, only passes index, limited to rope range
        {
            if (Active)
                for (int i = simStartOffset; i < simEndOffset; i++)
                    iterateMethod(i);
        }

        public void IterateRopeFull(Action<int> iterateMethod) //method for stuff other than drawing
        {
            if (Active)
                for (int i = 0; i < segmentCount; i++)
                    iterateMethod(i);
        }

        /// <summary>
        /// Generates the mesh for this verlet chain if rendered to the banner RenderTarget. If you dont wish to render to this target, use the DrawStrip overload instead.
        /// </summary>
        /// <param name="buffer"> the vertex buffer used as the mesh </param>
        /// <param name="offset"> the offset for the origin of the mesh </param>
        /// <param name="scale"> the scale factor this should draw with </param>
        public virtual void PrepareStrip(float scale)
        {
            if(meshBuffer is null)
                meshBuffer = new VertexBuffer(Main.graphics.GraphicsDevice, typeof(VertexPositionColor), segmentCount * 9 - 6, BufferUsage.WriteOnly);

            VertexPositionColor[] verticies = new VertexPositionColor[segmentCount * 9 - 6];

            float rotation = (ropeSegments[0].posScreen - ropeSegments[1].posScreen).ToRotation() + (float)Math.PI / 2;

            verticies[0] = new VertexPositionColor((ropeSegments[0].posScreen + Vector2.UnitY.RotatedBy(rotation - Math.PI / 4) * -5).Vec3().ScreenCoord(), ropeSegments[0].color);
            verticies[1] = new VertexPositionColor((ropeSegments[0].posScreen + Vector2.UnitY.RotatedBy(rotation + Math.PI / 4) * -5).Vec3().ScreenCoord(), ropeSegments[0].color);
            verticies[2] = new VertexPositionColor((ropeSegments[1].posScreen).Vec3().ScreenCoord(), ropeSegments[1].color);

            for (int k = 1; k < segmentCount - 1; k++)
            {
                float rotation2 = (ropeSegments[k - 1].posScreen - ropeSegments[k].posScreen).ToRotation() + (float)Math.PI / 2;

                int point = k * 9 - 6;
                int off = Math.Min(k, segmentCount - (segmentCount / 4));

                verticies[point] = new VertexPositionColor((ropeSegments[k].posScreen + Vector2.UnitY.RotatedBy(rotation2 - Math.PI / 4) * -(segmentCount - off) * scale).Vec3().ScreenCoord(), ropeSegments[k].color);
                verticies[point + 1] = new VertexPositionColor((ropeSegments[k].posScreen + Vector2.UnitY.RotatedBy(rotation2 + Math.PI / 4) * -(segmentCount - off) * scale).Vec3().ScreenCoord(), ropeSegments[k].color);
                verticies[point + 2] = new VertexPositionColor((ropeSegments[k + 1].posScreen).Vec3().ScreenCoord(), ropeSegments[k + 1].color);

                int extra = k == 1 ? 0 : 6;
                verticies[point + 3] = verticies[point];
                verticies[point + 4] = verticies[point - (3 + extra)];
                verticies[point + 5] = verticies[point - (1 + extra)];

                verticies[point + 6] = verticies[point - (2 + extra)];
                verticies[point + 7] = verticies[point + 1];
                verticies[point + 8] = verticies[point - (1 + extra)];
            }

            meshBuffer.SetData(verticies);
        }

        /// <summary>
        /// used for drawing the strip was a mesh without drawing to the render target
        /// </summary>
        /// <param name="prepareFunction"> returns the vertex buffer used as the mesh </param>
        /// <param name="effect"> the shader to apply to the mesh. Defaults to a BasicEffect </param>
        /// <param name="offset"> the vector passed to prepareFunction as an offset </param>
        public void DrawStrip(Func<Vector2, VertexBuffer> prepareFunction, Effect effect = null, Vector2 offset = default)
        {
            if (!Active || ropeSegments.Count < 1 || Main.dedServ) 
                return;

            GraphicsDevice graphics = Main.graphics.GraphicsDevice;

            var buffer = prepareFunction(offset);
            graphics.SetVertexBuffer(buffer);

            if (effect is null)
                effect = basicEffectColor;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawPrimitives(PrimitiveType.TriangleList, 0, segmentCount * 3 - 2);
            }
        }

        public void DrawStrip(float scale)
        {
            if (!Active || ropeSegments.Count < 1 || Main.dedServ) 
                return;

            GraphicsDevice graphics = Main.graphics.GraphicsDevice;

            PrepareStrip(scale);
            graphics.SetVertexBuffer(meshBuffer);

            foreach (EffectPass pass in basicEffectColor.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawPrimitives(PrimitiveType.TriangleList, 0, segmentCount * 3 - 2);
            }
        }

        public static void DrawStripsPixelated(SpriteBatch spriteBatch)
        {
            if (Main.dedServ) return;

            spriteBatch.Draw(VerletChainSystem.target, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
        }

        public void DrawRope(SpriteBatch spritebatch, Action<SpriteBatch, int, Vector2> drawMethod_curPos) //current position
        {
            if (ropeSegments is null)
                return;

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

        public Vector2 TileCollision(Vector2 pos, Vector2 vel)
        {
            if (!collideWithTiles)
                return vel;

            Vector2 newVel = Collision.noSlopeCollision(pos - (new Vector2(3, 3)), vel, 6, 6, true, true);
            Vector2 ret = new Vector2(vel.X,vel.Y);
            if (Math.Abs(newVel.X) < Math.Abs(vel.X))
                ret.X *= 0;

            if (Math.Abs(newVel.Y) < Math.Abs(vel.Y))
                ret.Y *= 0;

            return ret;
        }
    }
}