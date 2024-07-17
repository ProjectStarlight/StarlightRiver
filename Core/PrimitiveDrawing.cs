﻿using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;

namespace StarlightRiver.Core
{
	public class Primitives : IDisposable
	{
		private DynamicVertexBuffer vertexBuffer;
		private DynamicIndexBuffer indexBuffer;

		private readonly GraphicsDevice device;

		public bool IsDisposed { get; private set; }

		public Primitives(GraphicsDevice device, int maxVertices, int maxIndices)
		{
			this.device = device;

			if (device != null)
			{
				Main.QueueMainThreadAction(() =>
				{
					vertexBuffer = new DynamicVertexBuffer(device, typeof(VertexPositionColorTexture), maxVertices, BufferUsage.WriteOnly);
					indexBuffer = new DynamicIndexBuffer(device, IndexElementSize.SixteenBits, maxIndices, BufferUsage.WriteOnly);
				});
			}
		}

		public void Render(Effect effect)
		{
			if (vertexBuffer is null || indexBuffer is null)
				return;

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
			vertexBuffer?.SetData(0, vertices, 0, vertices.Length, VertexPositionColorTexture.VertexDeclaration.VertexStride, SetDataOptions.Discard);
		}

		public void SetIndices(short[] indices)
		{
			indexBuffer?.SetData(0, indices, 0, indices.Length, SetDataOptions.Discard);
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
		public int stayAlive = 10;

		private readonly Primitives primitives;

		private readonly int maxPointCount;

		private readonly ITrailTip tip;

		private readonly TrailWidthFunction trailWidthFunction;

		private readonly TrailColorFunction trailColorFunction;

		public bool IsDisposed { get; private set; }

		/// <summary>
		/// Array of positions that define the trail. NOTE: Positions[Positions.Length - 1] is assumed to be the start (e.g. Projectile.Center) and Positions[0] is assumed to be the end.
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

			primitives = new Primitives(device, maxPointCount * 2 + this.tip.ExtraVertices, 6 * (maxPointCount - 1) + this.tip.ExtraIndices);

			ModContent.GetInstance<TrailManager>().managed.Add(this);
		}

		private void GenerateMesh(out VertexPositionColorTexture[] vertices, out short[] indices, out int nextAvailableIndex)
		{
			var verticesTemp = new VertexPositionColorTexture[maxPointCount * 2];

			short[] indicesTemp = new short[maxPointCount * 6 - 6];

			// k = 0 indicates starting at the end of the trail (furthest from the origin of it).
			for (int k = 0; k < Positions.Length; k++)
			{
				// 1 at k = Positions.Length - 1 (start) and 0 at k = 0 (end).
				float factorAlongTrail = (float)k / (Positions.Length - 1);

				// Uses the trail width function to decide the width of the trail at this point (if no function, use 
				float width = trailWidthFunction?.Invoke(factorAlongTrail) ?? defaultWidth;

				Vector2 current = Positions[k];
				Vector2 next = k == Positions.Length - 1 ? Positions[^1] + (Positions[^1] - Positions[^2]) : Positions[k + 1];

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

				Vector2 a = current + normalPerp * width;
				Vector2 c = current - normalPerp * width;

				/* Texture coordinates are calculated such that the top-left is (0, 0) and the bottom-right is (1, 1).
                 * To achieve this, we consider the Y-coordinate of A to be 0 and that of C to be 1, while the X-coordinate is just the factor along the trail.
                 * This results in the point last in the trail having an X-coordinate of 0, and the first one having a Y-coordinate of 1.
                 */
				var texCoordA = new Vector2(factorAlongTrail, 0);
				var texCoordC = new Vector2(factorAlongTrail, 1);

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

				indicesTemp[k * 6] = (short)(k + maxPointCount);
				indicesTemp[k * 6 + 1] = (short)(k + maxPointCount + 1);
				indicesTemp[k * 6 + 2] = (short)(k + 1);
				indicesTemp[k * 6 + 3] = (short)(k + 1);
				indicesTemp[k * 6 + 4] = k;
				indicesTemp[k * 6 + 5] = (short)(k + maxPointCount);
			}

			// The next available index will be the next value after the count of points (starting at 0).
			nextAvailableIndex = verticesTemp.Length;

			vertices = verticesTemp;

			// Maybe we could use an array instead of a list for the indices, if someone figures out how to add indices to an array properly.
			indices = indicesTemp;
		}

		private void SetupMeshes()
		{
			GenerateMesh(out VertexPositionColorTexture[] mainVertices, out short[] mainIndices, out int nextAvailableIndex);

			Vector2 toNext = (NextPosition - Positions[^1]).SafeNormalize(Vector2.Zero);

			tip.GenerateMesh(Positions[^1], toNext, nextAvailableIndex, out VertexPositionColorTexture[] tipVertices, out short[] tipIndices, trailWidthFunction, trailColorFunction);

			primitives.SetVertices(mainVertices.FastUnion(tipVertices));
			primitives.SetIndices(mainIndices.FastUnion(tipIndices));
		}

		public void Render(Effect effect)
		{
			if (Positions == null || (primitives?.IsDisposed ?? true) || IsDisposed)
				return;

			SetupMeshes();

			primitives.Render(effect);

			stayAlive = 10; //Set stayalive to 10 frames as we render again, we will dispose this trail if it fails to render for 10 frames
		}

		public void Dispose()
		{
			primitives?.Dispose();
			IsDisposed = true;
		}
	}

	public class NoTip : ITrailTip
	{
		public int ExtraVertices => 0;

		public int ExtraIndices => 0;

		public void GenerateMesh(Vector2 trailTipPosition, Vector2 trailTipNormal, int startFromIndex, out VertexPositionColorTexture[] vertices, out short[] indices, TrailWidthFunction trailWidthFunction, TrailColorFunction trailColorFunction)
		{
			vertices = Array.Empty<VertexPositionColorTexture>();
			indices = Array.Empty<short>();
		}
	}

	public class TriangularTip : ITrailTip
	{
		private readonly float length;

		public int ExtraVertices => 3;

		public int ExtraIndices => 3;

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

			float width = trailWidthFunction?.Invoke(1) ?? 1;
			Vector2 a = trailTipPosition + normalPerp * width;
			Vector2 b = trailTipPosition - normalPerp * width;
			Vector2 c = trailTipPosition + trailTipNormal * length;

			Vector2 texCoordA = Vector2.UnitX;
			Vector2 texCoordB = Vector2.One;
			var texCoordC = new Vector2(1, 0.5f);//this fixes the texture being skewed off to the side

			Color colorA = trailColorFunction?.Invoke(texCoordA) ?? Color.White;
			Color colorB = trailColorFunction?.Invoke(texCoordB) ?? Color.White;
			Color colorC = trailColorFunction?.Invoke(texCoordC) ?? Color.White;

			vertices = new VertexPositionColorTexture[]
			{
				new(a.Vec3(), colorA, texCoordA),
				new(b.Vec3(), colorB, texCoordB),
				new(c.Vec3(), colorC, texCoordC)
			};

			indices = new short[]
			{
				(short)startFromIndex,
				(short)(startFromIndex + 1),
				(short)(startFromIndex + 2)
			};
		}
	}

	// Note: Every vertex in this tip is drawn twice, but the performance impact from this would be very little
	public class RoundedTip : ITrailTip
	{
		// TriCount is the amount of tris the curve should have, higher means a better circle approximation. (Keep in mind each tri is drawn twice)
		private readonly int triCount;

		// The edge vextex count is count * 2 + 1, but one extra is added for the center, and there is one extra hidden vertex.
		public int ExtraVertices => triCount * 2 + 3;

		public int ExtraIndices => triCount * 2 * 3 + 5;

		public RoundedTip(int triCount = 2)//amount of tris
		{
			this.triCount = triCount;

			if (triCount < 2)
				throw new ArgumentException($"Parameter {nameof(triCount)} cannot be less than 2.");
		}

		public void GenerateMesh(Vector2 trailTipPosition, Vector2 trailTipNormal, int startFromIndex, out VertexPositionColorTexture[] vertices, out short[] indices, TrailWidthFunction trailWidthFunction, TrailColorFunction trailColorFunction)
		{
			/*   C---D
             *  / \ / \
             * B---A---E (first layer)
             * 
             *   H---G
             *  / \ / \
             * I---A---F (second layer)
             * 
             * This tip attempts to approximate a semicircle as shown.
             * Consists of a fan of triangles which share a common center (A).
             * The higher the tri count, the more points there are.
             * Point E and F are ontop of eachother to prevent a visual seam.
             */

			/// We want an array of vertices the size of the accuracy amount plus the center.
			vertices = new VertexPositionColorTexture[ExtraVertices];

			var fanCenterTexCoord = new Vector2(1, 0.5f);

			vertices[0] = new VertexPositionColorTexture(trailTipPosition.Vec3(), (trailColorFunction?.Invoke(fanCenterTexCoord) ?? Color.White) * 0.75f, fanCenterTexCoord);

			var indicesTemp = new List<short>();

			for (int k = 0; k <= triCount; k++)
			{
				// Referring to the illustration: 0 is point B, 1 is point E, any other value represent the rotation factor of points in between.
				float rotationFactor = k / (float)triCount;

				// Rotates by pi/2 - (factor * pi) so that when the factor is 0 we get B and when it is 1 we get E.
				float angle = MathHelper.PiOver2 - rotationFactor * MathHelper.Pi;

				Vector2 circlePoint = trailTipPosition + trailTipNormal.RotatedBy(angle) * (trailWidthFunction?.Invoke(1) ?? 1);

				// Handily, the rotation factor can also be used as a texture coordinate because it is a measure of how far around the tip a point is.
				var circleTexCoord = new Vector2(rotationFactor, 1);

				// The transparency must be changed a bit so it looks right when overlapped
				Color circlePointColor = (trailColorFunction?.Invoke(new Vector2(1, 0)) ?? Color.White) * rotationFactor * 0.85f;

				vertices[k + 1] = new VertexPositionColorTexture(circlePoint.Vec3(), circlePointColor, circleTexCoord);

				//if (k == triCount)//leftover and not needed
				//{
				//    continue;
				//}

				short[] tri = new short[]
				{
                    /* Because this is a fan, we want all triangles to share a common point. This is represented by index 0 offset to the next available index.
                     * The other indices are just pairs of points around the fan. The vertex k points along the circle is just index k + 1, followed by k + 2 at the next one along.
                     * The reason these are offset by 1 is because index 0 is taken by the fan center.
                     */

                    //before the fix, I believe these being in the wrong order was what prevented it from drawing
                    (short)startFromIndex,
					(short)(startFromIndex + k + 2),
					(short)(startFromIndex + k + 1)
				};

				indicesTemp.AddRange(tri);
			}

			// These 2 forloops overlap so that 2 points share the same location, this hidden point hides a tri that acts as a transition from one UV to another
			for (int k = triCount + 1; k <= triCount * 2 + 1; k++)
			{
				// Referring to the illustration: triCount + 1 is point F, 1 is point I, any other value represent the rotation factor of points in between.
				float rotationFactor = (k - 1) / (float)triCount - 1;

				// Rotates by pi/2 - (factor * pi) so that when the factor is 0 we get B and when it is 1 we get E.
				float angle = MathHelper.PiOver2 - rotationFactor * MathHelper.Pi;

				Vector2 circlePoint = trailTipPosition + trailTipNormal.RotatedBy(-angle) * (trailWidthFunction?.Invoke(1) ?? 1);

				// Handily, the rotation factor can also be used as a texture coordinate because it is a measure of how far around the tip a point is.
				var circleTexCoord = new Vector2(rotationFactor, 0);

				// The transparency must be changed a bit so it looks right when overlapped
				Color circlePointColor = (trailColorFunction?.Invoke(new Vector2(1, 0)) ?? Color.White) * rotationFactor * 0.75f;

				vertices[k + 1] = new VertexPositionColorTexture(circlePoint.Vec3(), circlePointColor, circleTexCoord);

				// Skip last point, since there is no point to pair with it.
				if (k == triCount * 2 + 1)
					continue;

				short[] tri = new short[]
				{
                    /* Because this is a fan, we want all triangles to share a common point. This is represented by index 0 offset to the next available index.
                     * The other indices are just pairs of points around the fan. The vertex k points along the circle is just index k + 1, followed by k + 2 at the next one along.
                     * The reason these are offset by 1 is because index 0 is taken by the fan center.
                     */

                    //The order of the indices is reversed since the direction is backwards
                    (short)startFromIndex,
					(short)(startFromIndex + k + 1),
					(short)(startFromIndex + k + 2)
				};

				indicesTemp.AddRange(tri);
			}

			indices = indicesTemp.ToArray();
		}
	}

	public class TrailManager : ModSystem
	{
		public List<Trail> managed = new();

		public override void PostUpdateEverything()
		{
			foreach (Trail trail in managed)
			{
				trail.stayAlive--;

				if (trail.stayAlive <= 0)
					trail.Dispose();
			}

			managed.RemoveAll(n => n.IsDisposed);
		}
	}
}