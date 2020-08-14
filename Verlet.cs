using Microsoft.Xna.Framework;
using StarlightRiver.Keys;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.World.Generation;
using Microsoft.Xna.Framework.Graphics;

namespace StarlightRiver
{
    public class VerletChainInstance
    {
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


        //gravity
        public Vector2 forceGravity = new Vector2(0f, 1f);//x, y (positive = down)
        public float gravityStrengthMult = 1f;

        public bool customGravity = false;
        public List<Vector2> forceGravityList = new List<Vector2>();//length must match the segment count

        private void Start(Vector2 targetPosition)
        {
            Vector2 ropeStartPoint = targetPosition;

            for (int i = 0; i < segmentCount; i++)
            {
                ropeSegments.Add(new RopeSegment(ropeStartPoint));
                if((customGravity ? forceGravityList[i] : forceGravity) != Vector2.Zero)
                {
                    ropeStartPoint += Vector2.Normalize((customGravity ? forceGravityList[i] : forceGravity)) * (customDistances ? segmentDistanceList[i] : segmentDistance);
                }
                else
                {
                    ropeStartPoint.Y += (customDistances ? segmentDistanceList[i] : segmentDistance);
                }
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
                //DrawRope();
                Simulate(targetPosition);
            }
            //else if (!ChainActive && init == true)//if in-active and initalized 
            //{
            //    //Reset here
            //}
        }

        private void Simulate(Vector2 targetPosition)
        {
            for (int i = 1; i < segmentCount; i++)
            {
                RopeSegment firstSegment = ropeSegments[i];
                Vector2 velocity = (firstSegment.posNow - firstSegment.posOld) / drag;
                firstSegment.posOld = firstSegment.posNow;
                firstSegment.posNow += velocity;
                firstSegment.posNow += (customGravity ? forceGravityList[i] : forceGravity) * gravityStrengthMult;
                ropeSegments[i] = firstSegment;
            }

            for (int i = 0; i < constraintRepetitions; i++)//the amount of times Constraints are applied per update
            {
                ApplyConstraint(targetPosition);
            }
        }

        private void ApplyConstraint(Vector2 targetPosition)
        {
            RopeSegment firstSegment = ropeSegments[0];
            firstSegment.posNow = targetPosition;
            ropeSegments[0] = firstSegment;

            for (int i = 0; i < segmentCount - 1; i++)
            {
                RopeSegment firstSeg = ropeSegments[i];
                RopeSegment secondSeg = ropeSegments[i + 1];

                float segmentDist = (customDistances ? segmentDistanceList[i] : segmentDistance);

                //Vector2 distVect = (firstSeg.posNow - secondSeg.posNow);
                //float dist = (float)Math.Sqrt(distVect.X * (distVect.X + distVect.Y) * distVect.Y);

                float dist = (firstSeg.posNow - secondSeg.posNow).Length();
                float error = Math.Abs(dist - segmentDist);
                Vector2 changeDir = Vector2.Zero;

                if (dist > segmentDist)
                {
                    changeDir = Vector2.Normalize(firstSeg.posNow - secondSeg.posNow);
                }
                else if (dist < segmentDist)
                {
                    changeDir = Vector2.Normalize(secondSeg.posNow - firstSeg.posNow);
                }

                Vector2 changeAmount = changeDir * error;
                if (i != 0)
                {
                    firstSeg.posNow -= changeAmount * 0.5f;
                    ropeSegments[i] = firstSeg;
                    secondSeg.posNow += changeAmount * 0.5f;
                    ropeSegments[i + 1] = secondSeg;
                }
                else
                {
                    secondSeg.posNow += changeAmount;
                    ropeSegments[i + 1] = secondSeg;
                }
            }
        }

        public void IterateRope(Action<int> iterateMethod) //method for stuff other than drawing, only passes index
        {
            for (int i = 0; i < segmentCount; i++)
            {
                iterateMethod(i);
            }
        }

        public void DrawRope(SpriteBatch spritebatch, Action<SpriteBatch, int, Vector2> drawMethod_curPos) //current position
        {
            //Vector2[] ropePositions = new Vector2[segmentCount];
            //if (init)//some things end up calling the draw method before the update method has had a chance to run, commented out since this can be done on the side of where this method is being called
            //{
            for (int i = 0; i < segmentCount; i++)
            {
                //ropePositions[i] = this.ropeSegments[i].posNow; //original did this for an unknown reason

                drawMethod_curPos(spritebatch, i, ropeSegments[i].posNow);
            }
            //}
        }

        public void DrawRope(SpriteBatch spritebatch, Action<SpriteBatch, int, RopeSegment> drawMethod_curSeg) //current segment (has position and previous position)
        {
            for (int i = 0; i < segmentCount; i++)
            {
                drawMethod_curSeg(spritebatch, i, ropeSegments[i]);
            }
        }

        public void DrawRope(SpriteBatch spritebatch, Action<SpriteBatch, int, Vector2, Vector2, Vector2> drawMethod_curPos_prevPos_nextPos)//current position, previous point position, next point position
        {
            for (int i = 0; i < segmentCount; i++)
            {
                drawMethod_curPos_prevPos_nextPos(spritebatch, i, ropeSegments[i].posNow, (i > 0 ? ropeSegments[i - 1].posNow : Vector2.Zero), (i < segmentCount - 1 ? ropeSegments[i + 1].posNow : Vector2.Zero));
            }
        }

        public struct RopeSegment
        {
            public Vector2 posNow;
            public Vector2 posOld;

            public RopeSegment(Vector2 pos)
            {
                this.posNow = pos;
                this.posOld = pos;
            }
        }
    }
}