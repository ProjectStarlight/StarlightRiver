using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria;

namespace StarlightRiver.Core
{
    public class Primitives : IDisposable
    {
        public bool IsDisposed { get; private set; }

        private readonly DynamicVertexBuffer vertexBuffer;
        private readonly DynamicIndexBuffer indexBuffer;

        private readonly GraphicsDevice device;

        public Primitives(GraphicsDevice device, int maxVertices, int maxIndices)
        {
            this.device = device;

            vertexBuffer = new DynamicVertexBuffer(device, typeof(VertexPositionColorTexture), maxVertices, BufferUsage.None);
            indexBuffer = new DynamicIndexBuffer(device, IndexElementSize.SixteenBits, maxIndices, BufferUsage.None);
        }

        public void Render(Effect effect)
        {
            device.SetVertexBuffer(vertexBuffer);
            device.Indices = indexBuffer;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexBuffer.VertexCount, 0, indexBuffer.IndexCount / 3);
            }
        }

        public void SetVertices(VertexPositionColorTexture[] vertices)
        {
            vertexBuffer.SetData(0, vertices, 0, vertices.Length, VertexPositionColorTexture.VertexDeclaration.VertexStride, SetDataOptions.Discard);
        }

        public void SetIndices(short[] indices)
        {
            indexBuffer.SetData(0, indices, 0, indices.Length, SetDataOptions.Discard);
        }

        public void Dispose()
        {
            IsDisposed = true;

            vertexBuffer?.Dispose();
            indexBuffer?.Dispose();
        }
    }

    public interface ITrailTip
    {
        int ExtraVertices { get; }

        int ExtraIndices { get; }

        void GenerateMesh(Vector2 trailTipPosition, Vector2 trailTipNormal, int startFromIndex, out VertexPositionColorTexture[] vertices, out short[] indices, TrailWidthFunction trailWidthFunction, TrailColorFunction trailColorFunction);
    }

    public delegate float TrailWidthFunction(float factorAlongTrail);

    public delegate Color TrailColorFunction(Vector2 textureCoordinates);

    public class Trail : IDisposable
    {
        private readonly Primitives primitives;

        private readonly int maxPointCount;

        private readonly ITrailTip tip;

        private readonly TrailWidthFunction trailWidthFunction;

        private readonly TrailColorFunction trailColorFunction;

        /// <summary>
        /// Array of positions that define the trail. NOTE: Positions[Positions.Length - 1] is assumed to be the start (e.g. projectile.Center) and Positions[0] is assumed to be the end.
        /// </summary>
        public Vector2[] Positions 
        {
            get => positions;
            set
            {
                if (value.Length != maxPointCount)
                {
                    throw new ArgumentException("Array of positions was a different length than the expected result!");
                }

                positions = value;
            }
        }

        private Vector2[] positions;

        /// <summary>
        /// Used in order to calculate the normal from the frontmost position, because there isn't a point after it in the original list.
        /// </summary>
        public Vector2 NextPosition { get; set; }

        private const float defaultWidth = 16;

        public Trail(GraphicsDevice device, int maxPointCount, ITrailTip tip, TrailWidthFunction trailWidthFunction, TrailColorFunction trailColorFunction)
        {
            this.tip = tip ?? new NoTip();

            this.maxPointCount = maxPointCount;

            this.trailWidthFunction = trailWidthFunction;

            this.trailColorFunction = trailColorFunction;

            /* A---B---C
             * |  /|  /|
             * D / E / F
             * |/  |/  |
             * G---H---I
             * 
             * Let D, E, F, etc. be the set of n points that define the trail.
             * Since each point generates 2 vertices, there are 2n vertices, plus the tip's count.
             * 
             * As for indices - in the region between 2 defining points there are 2 triangles.
             * The amount of regions in the whole trail are given by n - 1, so there are 2(n - 1) triangles for n points.
             * Finally, since each triangle is defined by 3 indices, there are 6(n - 1) indices, plus the tip's count.
             */

            primitives = new Primitives(device, (maxPointCount * 2) + this.tip.ExtraVertices, (6 * (maxPointCount - 1)) + this.tip.ExtraIndices);
        }

        private void GenerateMesh(out VertexPositionColorTexture[] vertices, out short[] indices, out int nextAvailableIndex)
        {
            VertexPositionColorTexture[] verticesTemp = new VertexPositionColorTexture[maxPointCount * 2];

            List<short> indicesTemp = new List<short>();

            // k = 0 indicates starting at the end of the trail (furthest from the origin of it).
            for (int k = 0; k < Positions.Length; k++)
            {
                // 1 at k = Positions.Length - 1 (start) and 0 at k = 0 (end).
                float factorAlongTrail = (float)k / (Positions.Length - 1);

                // Uses the trail width function to decide the width of the trail at this point (if no function, use 
                float width = trailWidthFunction?.Invoke(factorAlongTrail) ?? defaultWidth;

                Vector2 current = Positions[k];
                Vector2 next = k == Positions.Length - 1 ? NextPosition : Positions[k + 1];

                Vector2 normalToNext = (next - current).SafeNormalize(Vector2.Zero);
                Vector2 normalPerp = normalToNext.RotatedBy(MathHelper.PiOver2);

                /* A
                 * |
                 * B---D
                 * |
                 * C
                 * 
                 * Let B be the current point and D be the next one.
                 * A and C are calculated based on the perpendicular vector to the normal from B to D, scaled by the desired width calculated earlier.
                 */

                Vector2 a = current + (normalPerp * width);
                Vector2 c = current - (normalPerp * width);

                /* Texture coordinates are calculated such that the top-left is (0, 0) and the bottom-right is (1, 1).
                 * To achieve this, we consider the Y-coordinate of A to be 0 and that of C to be 1, while the X-coordinate is just the factor along the trail.
                 * This results in the point last in the trail having an X-coordinate of 0, and the first one having a Y-coordinate of 1.
                 */
                Vector2 texCoordA = new Vector2(factorAlongTrail, 0);
                Vector2 texCoordC = new Vector2(factorAlongTrail, 1);

                // Calculates the color for each vertex based on its texture coordinates. This acts like a very simple shader (for more complex effects you can use the actual shader).
                Color colorA = trailColorFunction?.Invoke(texCoordA) ?? Color.White;
                Color colorC = trailColorFunction?.Invoke(texCoordC) ?? Color.White;

                /* 0---1---2
                 * |  /|  /|
                 * A / B / C
                 * |/  |/  |
                 * 3---4---5
                 * 
                 * Assuming we want vertices to be indexed in this format, where A, B, C, etc. are defining points and numbers are indices of mesh points:
                 * For a given point that is k positions along the chain, we want to find its indices.
                 * These indices are given by k for the above point and k + n for the below point.
                 */

                verticesTemp[k] = new VertexPositionColorTexture(a.Vec3(), colorA, texCoordA);
                verticesTemp[k + maxPointCount] = new VertexPositionColorTexture(c.Vec3(), colorC, texCoordC);
            }

            /* Now, we have to loop through the indices to generate triangles.
             * Looping to maxPointCount - 1 brings us halfway to the end; it covers the top row (excluding the last point on the top row).
             */
            for (short k = 0; k < maxPointCount - 1; k++)
            {
                short[] tris = new short[]
                {
                    /* 0---1
                     * |  /|
                     * A / B
                     * |/  |
                     * 2---3
                     * 
                     * This illustration is the most basic set of points (where n = 2).
                     * In this, we want to make triangles (2, 3, 1) and (1, 0, 2).
                     * Generalising this, if we consider A to be k = 0 and B to be k = 1, then the indices we want are going to be (k + n, k + n + 1, k + 1) and (k + 1, k, k + n)
                     */

                    (short)(k + maxPointCount), 
                    (short)(k + maxPointCount + 1), 
                    (short)(k + 1),

                    (short)(k + 1),
                    k,
                    (short)(k + maxPointCount)
                };

                indicesTemp.AddRange(tris);
            }

            // The next available index will be the next value after the count of points (starting at 0).
            nextAvailableIndex = verticesTemp.Length;

            vertices = verticesTemp;

            // Maybe we could use an array instead of a list for the indices, if someone figures out how to add indices to an array properly.
            indices = indicesTemp.ToArray();
        }

        private void SetupMeshes()
        {
            GenerateMesh(out VertexPositionColorTexture[] mainVertices, out short[] mainIndices, out int nextAvailableIndex);

            Vector2 toNext = (NextPosition - Positions[Positions.Length - 1]).SafeNormalize(Vector2.Zero);

            tip.GenerateMesh(Positions[Positions.Length - 1], toNext, nextAvailableIndex, out VertexPositionColorTexture[] tipVertices, out short[] tipIndices, trailWidthFunction, trailColorFunction);

            primitives.SetVertices(mainVertices.FastUnion(tipVertices));
            primitives.SetIndices(mainIndices.FastUnion(tipIndices));
        }

        public void Render(Effect effect)
        {
            if (Positions == null && !(primitives?.IsDisposed ?? true)) 
            {
                return;
            }

            SetupMeshes();

            primitives.Render(effect);
        }

        public void Dispose()
        {
            primitives?.Dispose();
        }
    }

    public class NoTip : ITrailTip
    {
        public int ExtraVertices => 0;

        public int ExtraIndices => 0;

        public void GenerateMesh(Vector2 trailTipPosition, Vector2 trailTipNormal, int startFromIndex, out VertexPositionColorTexture[] vertices, out short[] indices, TrailWidthFunction trailWidthFunction, TrailColorFunction trailColorFunction)
        {
            vertices = new VertexPositionColorTexture[0];
            indices = new short[0];
        }
    }

    public class TriangularTip : ITrailTip
    {
        public int ExtraVertices => 3;

        public int ExtraIndices => 3;

        private readonly float length;

        public TriangularTip(float length)
        {
            this.length = length;
        }

        public void GenerateMesh(Vector2 trailTipPosition, Vector2 trailTipNormal, int startFromIndex, out VertexPositionColorTexture[] vertices, out short[] indices, TrailWidthFunction trailWidthFunction, TrailColorFunction trailColorFunction)
        {
            /*     C
             *    / \
             *   /   \
             *  /     \
             * A-------B
             * 
             * This tip is arranged as the above shows.
             * Consists of a single triangle with indices (0, 1, 2) offset by the next available index.
             */

            Vector2 normalPerp = trailTipNormal.RotatedBy(MathHelper.PiOver2);

            Vector2 a = trailTipPosition + (normalPerp * (trailWidthFunction?.Invoke(1) ?? 1));
            Vector2 b = trailTipPosition - (normalPerp * (trailWidthFunction?.Invoke(1) ?? 1));
            Vector2 c = trailTipPosition + (trailTipNormal * length);

            Vector2 texCoordA = Vector2.UnitX;
            Vector2 texCoordB = Vector2.One;
            Vector2 texCoordC = Vector2.One;

            Color colorA = trailColorFunction?.Invoke(texCoordA) ?? Color.White;
            Color colorB = trailColorFunction?.Invoke(texCoordB) ?? Color.White;
            Color colorC = trailColorFunction?.Invoke(texCoordC) ?? Color.White;

            vertices = new VertexPositionColorTexture[]
            {
                new VertexPositionColorTexture(a.Vec3(), colorA, texCoordA),
                new VertexPositionColorTexture(b.Vec3(), colorB, texCoordB),
                new VertexPositionColorTexture(c.Vec3(), colorC, texCoordC)
            };

            indices = new short[] 
            { 
                (short)startFromIndex, 
                (short)(startFromIndex + 1), 
                (short)(startFromIndex + 2) 
            };
        }
    }


    // WARNING: This does not work for time being. No, I don't know why. I need to fix this at a later date, but right now nothing uses it, so it's OK.
    public class RoundedTip : ITrailTip
    {
        // One more is added due to the center of the fan.
        public int ExtraVertices => accuracy + 1;

        // The number of triangles in a fan is just the number of points - 1, and then there are 3 indices per triangle.
        public int ExtraIndices => 3 * (accuracy - 1);

        // Accuracy is just the number of points we want to generate, higher means a better circle approximation.
        private readonly int accuracy;

        public RoundedTip(int accuracy)
        {
            this.accuracy = accuracy;

            if (accuracy < 3)
            {
                throw new ArgumentException($"Parameter {nameof(accuracy)} cannot be less than 3.");
            }
        }

        public void GenerateMesh(Vector2 trailTipPosition, Vector2 trailTipNormal, int startFromIndex, out VertexPositionColorTexture[] vertices, out short[] indices, TrailWidthFunction trailWidthFunction, TrailColorFunction trailColorFunction)
        {
            /*   C---D
             *  / \ / \
             * B---A---E
             * 
             * This tip attempts to approximate a semicircle as shown.
             * Consists of a fan of triangles which share a common center (A).
             * The higher the accuracy, the more points there are.
             */

            // We want an array of vertices the size of the accuracy amount plus the center.
            vertices = new VertexPositionColorTexture[accuracy + 1];

            Vector2 fanCenterTexCoord = new Vector2(1, 0.5f);

            vertices[0] = new VertexPositionColorTexture(trailTipPosition.Vec3(), trailColorFunction?.Invoke(fanCenterTexCoord) ?? Color.White, fanCenterTexCoord);

            List<short> indicesTemp = new List<short>();

            for (int k = 0; k < accuracy; k++)
            {
                // Referring to the illustration: 0 is point B, 1 is point E, any other value represent the rotation factor of points in between.
                float rotationFactor = k / (float)(accuracy - 1);

                // Rotates by pi/2 - (factor * pi) so that when the factor is 0 we get A and when it is 1 we get E.
                float angle = MathHelper.PiOver2 - (rotationFactor * MathHelper.Pi);

                Vector2 circlePoint = trailTipPosition + (trailTipNormal.RotatedBy(angle) * (trailWidthFunction?.Invoke(1) ?? 1));

                // Handily, the rotation factor can also be used as a texture coordinate because it is a measure of how far around the tip a point is.
                Vector2 circleTexCoord = new Vector2(1, rotationFactor);

                Color circlePointColor = trailColorFunction?.Invoke(circleTexCoord) ?? Color.White;

                vertices[k + 1] = new VertexPositionColorTexture(circlePoint.Vec3(), circlePointColor, circleTexCoord);

                // Skip last point, since there is no point to pair with it.
                if (k == accuracy - 1)
                {
                    continue;
                }

                short[] tri = new short[]
                {
                    /* Because this is a fan, we want all triangles to share a common point. This is represented by index 0 offset to the next available index.
                     * The other indices are just pairs of points around the fan. The vertex k points along the circle is just index k + 1, followed by k + 2 at the next one along.
                     * The reason these are offset by 1 is because index 0 is taken by the fan center.
                     */

                    (short)startFromIndex, 
                    (short)(startFromIndex + k + 1), 
                    (short)(startFromIndex + k + 2)
                };

                indicesTemp.AddRange(tri);
            }

            indices = indicesTemp.ToArray();
        }
    }
}
