using System;

namespace StarlightRiver.Helpers
{
	public class SplineHelper
	{
		public struct SplineData
		{
			private Vector2 startPoint;
			public Vector2 StartPoint
			{
				get => startPoint;
				set
				{
					startPoint = value;
					dist1 = ApproximateSplineLength(30, startPoint, midPoint - startPoint, midPoint, endPoint - startPoint);
					dist2 = ApproximateSplineLength(30, midPoint, endPoint - startPoint, endPoint, endPoint - midPoint);
				}
			}

			private Vector2 midPoint;
			public Vector2 MidPoint
			{
				get => midPoint;
				set
				{
					midPoint = value;
					dist1 = ApproximateSplineLength(30, startPoint, midPoint - startPoint, midPoint, endPoint - startPoint);
					dist2 = ApproximateSplineLength(30, midPoint, endPoint - startPoint, endPoint, endPoint - midPoint);
				}
			}

			private Vector2 endPoint;
			public Vector2 EndPoint
			{
				get => endPoint;
				set
				{
					endPoint = value;
					dist1 = ApproximateSplineLength(30, startPoint, midPoint - startPoint, midPoint, endPoint - startPoint);
					dist2 = ApproximateSplineLength(30, midPoint, endPoint - startPoint, endPoint, endPoint - midPoint);
				}
			}

			public float dist1;
			public float dist2;

			public SplineData(Vector2 startPoint, Vector2 midPoint, Vector2 endPoint)
			{
				this.startPoint = startPoint;
				this.midPoint = midPoint;
				this.endPoint = endPoint;
				dist1 = ApproximateSplineLength(30, startPoint, midPoint - startPoint, midPoint, endPoint - startPoint);
				dist2 = ApproximateSplineLength(30, midPoint, endPoint - startPoint, endPoint, endPoint - midPoint);
			}
		}

		public static Vector2 PointOnSpline(float progress, SplineData data)
		{
			float factor = data.dist1 / (data.dist1 + data.dist2);

			var toStart = Vector2.Normalize(data.MidPoint - data.StartPoint);
			var toEnd = Vector2.Normalize(data.EndPoint - data.MidPoint);

			var bisector = Vector2.Normalize(toStart + toEnd);

			Vector2 ab = data.EndPoint - data.StartPoint;
			Vector2 ac = data.MidPoint - data.StartPoint;
			float curve = ab.X * ac.Y - ab.Y * ac.X;
			float curveFactor = MathF.Abs(curve) / ab.Length() / (ab.Length() * 0.5f);

			float tangentLength = (data.dist1 + data.dist2) * MathF.Min(1f / curveFactor, 0.8f);
			Vector2 midTangent = bisector * tangentLength;

			if (progress < factor)
			{
				float t = progress / factor;
				return Vector2.Hermite(data.StartPoint, Vector2.Zero, data.MidPoint, midTangent, t);
			}
			else
			{
				float t = (progress - factor) / (1f - factor);
				return Vector2.Hermite(data.MidPoint, midTangent, data.EndPoint, Vector2.Zero, t);
			}
		}

		public static float TangentOfSpline(float progress, SplineData data)
		{
			return PointOnSpline(progress, data).DirectionTo(PointOnSpline(progress + 0.01f, data)).ToRotation();
		}

		public static float ApproximateSplineLength(int steps, Vector2 start, Vector2 startTan, Vector2 end, Vector2 endTan)
		{
			float total = 0;
			Vector2 prevPoint = start;
			for (int k = 0; k < steps; k++)
			{
				var testPoint = Vector2.Hermite(start, startTan, end, endTan, k / (float)steps);
				total += Vector2.Distance(prevPoint, testPoint);
				prevPoint = testPoint;
			}

			return total;
		}

		public static float ApproximateSplineLength(SplineData data)
		{
			return data.dist1 + data.dist2;
		}
	}
}
