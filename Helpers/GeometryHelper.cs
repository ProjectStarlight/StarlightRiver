using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Helpers
{
	public static class GeometryHelper
	{
		/// <summary>
		/// Gets the signed angle difference between two angles
		/// </summary>
		/// <param name="rotTo"></param>
		/// <param name="rotFrom"></param>
		/// <returns></returns>
		public static float RotationDifference(float rotTo, float rotFrom)
		{
			return ((rotTo - rotFrom) % 6.28f + 9.42f) % 6.28f - 3.14f;
		}

		public static float CompareAngle(float baseAngle, float targetAngle)
		{
			return (baseAngle - targetAngle + (float)Math.PI * 3) % MathHelper.TwoPi - (float)Math.PI;
		}

		public static float ConvertAngle(float angleIn)
		{
			return CompareAngle(0, angleIn) + (float)Math.PI;
		}

		/// <summary>
		/// Finds the centeroid point between a lsit of NPCs
		/// </summary>
		/// <param name="input">The NPCs to find the centeroid between</param>
		/// <returns>The centeroid</returns>
		public static Vector2 Centeroid(List<NPC> input) //Helper overload for NPCs for support NPCs
		{
			var centers = new List<Vector2>();

			for (int k = 0; k < input.Count; k++)
				centers.Add(input[k].Center);

			return Centeroid(centers);
		}

		/// <summary>
		/// Finds the centeroid point between a list of points
		/// </summary>
		/// <param name="input">The poitns to find the centeroid between</param>
		/// <returns>The centeroid</returns>
		public static Vector2 Centeroid(List<Vector2> input) //this gets the centeroid of the points. see: https://math.stackexchange.com/questions/1801867/finding-the-centre-of-an-abritary-set-of-points-in-two-dimensions
		{
			float sumX = 0;
			float sumY = 0;

			for (int k = 0; k < input.Count; k++)
			{
				sumX += input[k].X;
				sumY += input[k].Y;
			}

			return new Vector2(sumX / input.Count, sumY / input.Count);
		}

		/// <summary>
		/// Gets the arc velocity to get from a starting position to a target with the given gravity, and other constraints
		/// </summary>
		/// <param name="startingPos"></param>
		/// <param name="targetPos"></param>
		/// <param name="gravity"></param>
		/// <param name="minArcHeight"></param>
		/// <param name="maxArcHeight"></param>
		/// <param name="maxXvel"></param>
		/// <param name="heightabovetarget"></param>
		/// <returns></returns>
		public static Vector2 GetArcVel(Vector2 startingPos, Vector2 targetPos, float gravity, float? minArcHeight = null, float? maxArcHeight = null, float? maxXvel = null, float? heightabovetarget = null)
		{
			Vector2 DistanceToTravel = targetPos - startingPos;
			float MaxHeight = DistanceToTravel.Y - (heightabovetarget ?? 0);

			if (minArcHeight != null)
				MaxHeight = Math.Min(MaxHeight, -(float)minArcHeight);

			if (maxArcHeight != null)
				MaxHeight = Math.Max(MaxHeight, -(float)maxArcHeight);

			float TravelTime;
			float neededYvel;

			if (MaxHeight <= 0)
			{
				neededYvel = -(float)Math.Sqrt(-2 * gravity * MaxHeight);
				TravelTime = (float)Math.Sqrt(-2 * MaxHeight / gravity) + (float)Math.Sqrt(2 * Math.Max(DistanceToTravel.Y - MaxHeight, 0) / gravity); //time up, then time down
			}
			else
			{
				neededYvel = 0;
				TravelTime = (-neededYvel + (float)Math.Sqrt(Math.Pow(neededYvel, 2) - 4 * -DistanceToTravel.Y * gravity / 2)) / gravity; //time down
			}

			if (maxXvel != null)
				return new Vector2(MathHelper.Clamp(DistanceToTravel.X / TravelTime, -(float)maxXvel, (float)maxXvel), neededYvel);

			return new Vector2(DistanceToTravel.X / TravelTime, neededYvel);
		}

		/// <summary>
		/// Gets the arc velocity to get from an entities position to a target with the given gravity, and other constraints
		/// </summary>
		/// <param name="ent"></param>
		/// <param name="targetPos"></param>
		/// <param name="gravity"></param>
		/// <param name="minArcHeight"></param>
		/// <param name="maxArcHeight"></param>
		/// <param name="maxXvel"></param>
		/// <param name="heightabovetarget"></param>
		/// <returns></returns>
		public static Vector2 GetArcVel(this Entity ent, Vector2 targetPos, float gravity, float? minArcHeight = null, float? maxArcHeight = null, float? maxXvel = null, float? heightabovetarget = null)
		{
			return GetArcVel(ent.Center, targetPos, gravity, minArcHeight, maxArcHeight, maxXvel, heightabovetarget);
		}
	}
}