using System;

namespace StarlightRiver.Helpers
{
	public static class Eases
	{
		public static float EaseQuadIn(float x)
		{
			return (float)Math.Pow(x, 2);
		}

		public static float EaseQuadOut(float x)
		{
			return 1f - EaseQuadIn(1f - x);
		}

		public static float EaseQuadInOut(float x)
		{
			return (x < 0.5f) ? 
				2f * (float)Math.Pow(x, 2) : 
				-2f * (float)Math.Pow(x, 2) + 4f * x - 1f;
		}

		public static float EaseCubicIn(float x)
		{
			return (float)Math.Pow(x, 3);
		}

		public static float EaseCubicOut(float x)
		{
			return 1f - EaseCubicIn(1f - x);
		}

		public static float EaseCubicInOut(float x)
		{
			return (x < 0.5f) ? 
				4f * (float)Math.Pow(x, 3) : 
				4f * (float)Math.Pow(x, 3) - 12f * (float)Math.Pow(x, 2) + 12f * x - 3f;
		}

		public static float EaseQuarticIn(float x)
		{
			return (float)Math.Pow(x, 4);
		}

		public static float EaseQuarticOut(float x)
		{
			return 1f - EaseQuarticIn(1f - x);
		}

		public static float EaseQuarticInOut(float x)
		{
			return (x < 0.5f) ? 
				8f * (float)Math.Pow(x, 4) : 
				-8f * (float)Math.Pow(x, 4) + 32f * (float)Math.Pow(x, 3) - 48f * (float)Math.Pow(x, 2) + 32f * x - 7f;
		}

		public static float EaseQuinticIn(float x)
		{
			return (float)Math.Pow(x, 5);
		}

		public static float EaseQuinticOut(float x)
		{
			return 1f - EaseQuinticIn(1f - x);
		}

		public static float EaseQuinticInOut(float x)
		{
			return (x < 0.5f) ? 
				16f * (float)Math.Pow(x, 5) : 
				16f * (float)Math.Pow(x, 5) - 80f * (float)Math.Pow(x, 4) + 160f * (float)Math.Pow(x, 3) - 160f * (float)Math.Pow(x, 2) + 80f * x - 15f;
		}

		public static float EaseCircularIn(float x)
		{
			return 1f - (float)Math.Sqrt(1.0 - Math.Pow(x, 2));
		}

		public static float EaseCircularOut(float x)
		{
			return (float)Math.Sqrt(1.0 - Math.Pow(x - 1.0, 2));
		}

		public static float EaseCircularInOut(float x)
		{
			return (x < 0.5f) ? 
				(1f - (float)Math.Sqrt(1.0 - Math.Pow(x * 2, 2))) * 0.5f :
				(float)((Math.Sqrt(1.0 - Math.Pow(-2 * x + 2, 2)) + 1) * 0.5);
		}

		public static float EaseBackIn(float x)
		{
			float c1 = 1.70158f;
			float c3 = c1 + 1f;

			return c3 * x * x * x - c1 * x * x;
		}

		public static float EaseBackOut(float x)
		{
			float c1 = 1.70158f;
			float c3 = c1 + 1f;

			return 1 + c3 * (float)Math.Pow(x - 1, 3) + c1 * (float)Math.Pow(x - 1, 2);
		}

		public static float EaseBackInOut(float x)
		{
			float c1 = 1.70158f;
			float c2 = c1 * 1.525f;

			return x < 0.5f ?
			  (float)Math.Pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2) / 2 :
			  ((float)Math.Pow(2 * x - 2, 2) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) / 2;
		}

		/// <summary>
		/// A bezier curve like ease
		/// </summary>
		/// <param name="time"></param>
		/// <returns></returns>
		public static float BezierEase(float time)
		{
			return time * time / (2f * (time * time - time) + 1f);
		}

		/// <summary>
		/// An ease which "swoops" past 0 and 1 briefly near the ends
		/// </summary>
		/// <param name="time"></param>
		/// <returns></returns>
		public static float SwoopEase(float time)
		{
			return 3.75f * (float)Math.Pow(time, 3) - 8.5f * (float)Math.Pow(time, 2) + 5.75f * time;
		}

		/// <summary>
		/// <para>Animations are interpolated with a cubic bezier. You will define the bezier using the p1 and p2 parameters.</para>
		/// <para>This function serves as a constructor for the real interpolation function</para>
		/// <para>Use https://cubic-bezier.com/ to find appropriate parameters.</para>
		/// </summary>
		public static Func<float, float> CubicBezier(float p1x, float p1y, float p2x, float p2y)
		{
			if (p1x < 0 || p1x > 1 || p2x < 0 || p2x > 1)
			{
				throw new ArgumentException("X point parameters of cubic bezier timing function should be between values 0 and 1!");
			}

			Vector2 p0 = Vector2.Zero;
			var p1 = new Vector2(p1x, p1y);
			var p2 = new Vector2(p2x, p2y);
			Vector2 p3 = Vector2.One;

			float timing(float t)
			{
				return (float)(Math.Pow(1 - t, 3) * p0.X + 3 * Math.Pow(1 - t, 2) * t * p1.X + 3 * (1 - t) * Math.Pow(t, 2) * p2.X + Math.Pow(t, 3) * p3.X);
			}

			float progression(float x)
			{
				float time;
				if (x <= 0)
					time = 0;
				else if (x >= 1)
					time = 1;
				else
					time = BinarySolve(timing, x, 0.001f);

				return (float)(Math.Pow(1 - time, 3) * p0.Y + 3 * Math.Pow(1 - time, 2) * time * p1.Y + 3 * (1 - time) * Math.Pow(time, 2) * p2.Y + Math.Pow(time, 3) * p3.Y);
			}

			return progression;
		}

		// Binary solver for cubic bezier
		private static float BinarySolve(in Func<float, float> function, in float target, in float precision, float start = 0, float end = 1, int iteration = 0)
		{
			if (iteration > 1000)
			{
				throw new ArgumentException("Could not converge to an answer in over 1000 iterations.");
			}

			float halfway = (start + end) / 2;
			float res = function(halfway);

			if (Math.Abs(res - target) < precision)
				return halfway;
			else if (target < res)
				return BinarySolve(function, target, precision, start, halfway, iteration + 1);
			else
				return BinarySolve(function, target, precision, halfway, end, iteration + 1);
		}
	}
}