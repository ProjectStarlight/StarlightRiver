using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;

namespace StarlightRiver.Core
{
	public abstract class EaseFunction
	{
		public static readonly EaseFunction Linear = new PolynomialEase((float x) => x);

		public static readonly EaseFunction EaseQuadIn = new PolynomialEase((float x) => x * x);
		public static readonly EaseFunction EaseQuadOut = new PolynomialEase((float x) => 1f - EaseQuadIn.Ease(1f - x));
		public static readonly EaseFunction EaseQuadInOut = new PolynomialEase((float x) => (x < 0.5f) ? 2f * x * x : -2f * x * x + 4f * x - 1f);

		public static readonly EaseFunction EaseCubicIn = new PolynomialEase((float x) => x * x * x);
		public static readonly EaseFunction EaseCubicOut = new PolynomialEase((float x) => 1f - EaseCubicIn.Ease(1f - x));
		public static readonly EaseFunction EaseCubicInOut = new PolynomialEase((float x) => (x < 0.5f) ? 4f * x * x * x : 4f * x * x * x - 12f * x * x + 12f * x - 3f);

		public static readonly EaseFunction EaseQuarticIn = new PolynomialEase((float x) => x * x * x * x);
		public static readonly EaseFunction EaseQuarticOut = new PolynomialEase((float x) => 1f - EaseQuarticIn.Ease(1f - x));
		public static readonly EaseFunction EaseQuarticInOut = new PolynomialEase((float x) => (x < 0.5f) ? 8f * x * x * x * x : -8f * x * x * x * x + 32f * x * x * x - 48f * x * x + 32f * x - 7f);

		public static readonly EaseFunction EaseQuinticIn = new PolynomialEase((float x) => x * x * x * x * x);
		public static readonly EaseFunction EaseQuinticOut = new PolynomialEase((float x) => 1f - EaseQuinticIn.Ease(1f - x));
		public static readonly EaseFunction EaseQuinticInOut = new PolynomialEase((float x) => (x < 0.5f) ? 16f * x * x * x * x * x : 16f * x * x * x * x * x - 80f * x * x * x * x + 160f * x * x * x - 160f * x * x + 80f * x - 15f);

		public static readonly EaseFunction EaseCircularIn = new PolynomialEase((float x) => 1f - (float)Math.Sqrt(1.0 - Math.Pow(x, 2)));
		public static readonly EaseFunction EaseCircularOut = new PolynomialEase((float x) => (float)Math.Sqrt(1.0 - Math.Pow(x - 1.0, 2)));
		public static readonly EaseFunction EaseCircularInOut = new PolynomialEase((float x) => (x < 0.5f) ? (1f - (float)Math.Sqrt(1.0 - Math.Pow(x * 2, 2))) * 0.5f : (float)((Math.Sqrt(1.0 - Math.Pow(-2 * x + 2, 2)) + 1) * 0.5));

		public virtual float Ease(float time)
		{
			throw new NotImplementedException();
		}
	}

	public class PolynomialEase : EaseFunction
	{
		private readonly Func<float, float> fun;

		public PolynomialEase(Func<float, float> func)
		{
			fun = func;
		}

		public override float Ease(float time)
		{
			return fun(time);
		}

		//removed because not needed for spirit
		//public static EaseFunction Generate(int factor, int type)
		//{
		//}
	}

	public class CubicBezierEase : EaseFunction
	{
		private readonly Func<float, float> fun;

		public CubicBezierEase(float p1x, float p1y, float p2x, float p2y)
		{
			fun = Helper.CubicBezier(p1x, p1y, p2x, p2y);
		}

		public override float Ease(float time)
		{
			return fun(time);
		}
	}

	public class EaseBuilder : EaseFunction
	{
		private readonly List<EasePoint> points;

		public EaseBuilder()
		{
			points = new List<EasePoint>();
		}

		public void AddPoint(float x, float y, EaseFunction function)
		{
			AddPoint(new Vector2(x, y), function);
		}

		public void AddPoint(Vector2 vector, EaseFunction function)
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

			return MathHelper.Lerp(prevPoint.Y, usePoint.Point.Y, usePoint.Function.Ease(progress));
		}

		private struct EasePoint
		{
			public Vector2 Point;
			public EaseFunction Function;

			public EasePoint(Vector2 p, EaseFunction func)
			{
				Point = p;
				Function = func;
			}
		}
	}
}