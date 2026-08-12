using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;

namespace StarlightRiver.Core
{
	public abstract class AdvancedEaseFunction
	{
		public abstract float Ease(float time);
	}

	public class CubicBezierEase : AdvancedEaseFunction
	{
		private readonly Func<float, float> fun;

		public CubicBezierEase(float p1x, float p1y, float p2x, float p2y)
		{
			fun = Eases.CubicBezier(p1x, p1y, p2x, p2y);
		}

		public override float Ease(float time)
		{
			return fun(time);
		}
	}

	public class ModularEaseFunction : AdvancedEaseFunction
	{
		private readonly List<EasePoint> points;

		public ModularEaseFunction()
		{
			points = new List<EasePoint>();
		}

		public void AddPoint(float x, float y, Func<float, float> function)
		{
			AddPoint(new Vector2(x, y), function);
		}

		public void AddPoint(Vector2 vector, Func<float, float> function)
		{
			if (vector.X < 0f)
				throw new ArgumentException("X value of point is not in valid range!");

			var newPoint = new EasePoint(vector, function);
			if (points.Count == 0)
			{
				points.Add(newPoint);
				return;
			}

			EasePoint last = points[^1];

			if (last.Point.X > vector.X)
				throw new ArgumentException("New point has an x value less than the previous point when it should be greater or equal");

			points.Add(newPoint);
		}

		public override float Ease(float time)
		{
			Vector2 prevPoint = Vector2.Zero;
			EasePoint usePoint = points[0];

			for (int i = 0; i < points.Count; i++)
			{
				usePoint = points[i];

				if (time <= usePoint.Point.X)
					break;

				prevPoint = usePoint.Point;
			}

			float dist = usePoint.Point.X - prevPoint.X;
			float progress = (time - prevPoint.X) / dist;

			if (progress > 1f)
				progress = 1f;

			return MathHelper.Lerp(prevPoint.Y, usePoint.Point.Y, usePoint.Function(progress));
		}

		private struct EasePoint
		{
			public Vector2 Point;
			public Func<float, float> Function;

			public EasePoint(Vector2 p, Func<float, float> func)
			{
				Point = p;
				Function = func;
			}
		}
	}
}