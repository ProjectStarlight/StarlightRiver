using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Helpers;
using StarlightRiver.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace StarlightRiver.Core.VerletGenerators
{
	class RectangularBanner : VerletChain
	{
        public RectangularBanner(int SegCount, bool specialDraw, Vector2 StartPoint, int SegDistance) : base(SegCount, specialDraw, StartPoint, SegDistance) { }

        public override void PrepareStrip(float scale)
        {
            if (meshBuffer is null)
                meshBuffer = new VertexBuffer(Main.graphics.GraphicsDevice, typeof(VertexPositionColor), segmentCount * 9 - 6, BufferUsage.WriteOnly);

            VertexPositionColor[] verticies = new VertexPositionColor[segmentCount * 9 - 6];

            float rotation = (ropeSegments[0].posScreen - ropeSegments[1].posScreen).ToRotation() + (float)Math.PI / 2;

            verticies[0] = new VertexPositionColor((ropeSegments[0].posScreen + Vector2.UnitY.RotatedBy(rotation - Math.PI / 4) * -scale).Vec3().ScreenCoord(), ropeSegments[0].color);
            verticies[1] = new VertexPositionColor((ropeSegments[0].posScreen + Vector2.UnitY.RotatedBy(rotation + Math.PI / 4) * -scale).Vec3().ScreenCoord(), ropeSegments[0].color);
            verticies[2] = new VertexPositionColor((ropeSegments[1].posScreen).Vec3().ScreenCoord(), ropeSegments[1].color);

            for (int k = 1; k < segmentCount - 1; k++)
            {
                float rotation2 = (ropeSegments[k - 1].posScreen - ropeSegments[k].posScreen).ToRotation() + (float)Math.PI / 2;

                int point = k * 9 - 6;

                verticies[point] = new VertexPositionColor((ropeSegments[k].posScreen + Vector2.UnitY.RotatedBy(rotation2 - Math.PI / 4) * -scale).Vec3().ScreenCoord(), ropeSegments[k].color);
                verticies[point + 1] = new VertexPositionColor((ropeSegments[k].posScreen + Vector2.UnitY.RotatedBy(rotation2 + Math.PI / 4) * -scale).Vec3().ScreenCoord(), ropeSegments[k].color);
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
    }
}
