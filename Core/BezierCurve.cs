using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace StarlightRiver.Core
{
	public class BezierCurve
	{
		private Vector2[] _controlPoints;

		public BezierCurve(params Vector2[] controls)
		{
			_controlPoints = controls;
		}

		/// <summary>
		/// Return a Vector2 at value T along the bezier curve.
		/// </summary>
		/// <param name="T">How far along the bezier curve to return a point.</param>
		/// <returns></returns>
		public Vector2 Evaluate(float T)
		{
			if (T < 0f)
				T = 0f;
			if (T > 1f)
				T = 1f;

			return PrivateEvaluate(_controlPoints, T);
		}

		/// <summary>
		/// Get a list of points along the bezier curve. Must be at least 2.
		/// </summary>
		/// <param name="amount">The amount of points to get.</param>
		/// <returns>A list of Vector2s representing the points.</returns>
		public List<Vector2> GetPoints(int amount)
		{
			if (amount < 2)
			{
				//throw new ArgumentException("How am I supposed to get one (or, heck, less) point on a bezier curve? You need to have more than two points specified for the number!");
				amount = 2;
			}

			float perStep = 1f / (amount - 1);

			var points = new List<Vector2>();

			for (int i = 0; i < amount; i++)
			{
				points.Add(Evaluate(perStep * i));
			}

			return points;
		}

		private Vector2 PrivateEvaluate(Vector2[] points, float T)
		{
			if (points.Length > 2)
			{
				var nextPoints = new Vector2[points.Length - 1];
				for (int k = 0; k < points.Length - 1; k++)
				{
					nextPoints[k] = Vector2.Lerp(points[k], points[k + 1], T);
				}

				return PrivateEvaluate(nextPoints, T);
			}
			else
			{
				return Vector2.Lerp(points[0], points[1], T);
			}
		}

		public Vector2 this[int x]
		{
			get => _controlPoints[x];
			set => _controlPoints[x] = value;
		}
	}
}
