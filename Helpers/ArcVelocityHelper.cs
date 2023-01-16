using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace StarlightRiver.Helpers
{
	public static class ArcVelocityHelper
	{
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
			if (MaxHeight <= 0) {
				neededYvel = -(float)Math.Sqrt(-2 * gravity * MaxHeight);
				TravelTime = (float)Math.Sqrt(-2 * MaxHeight / gravity) + (float)Math.Sqrt(2 * Math.Max(DistanceToTravel.Y - MaxHeight, 0) / gravity); //time up, then time down
			}

			else {
				neededYvel = 0;
				TravelTime = (-neededYvel + (float)Math.Sqrt(Math.Pow(neededYvel, 2) - (4 * -DistanceToTravel.Y * gravity / 2))) / (gravity); //time down
			}

			if (maxXvel != null)
				return new Vector2(MathHelper.Clamp(DistanceToTravel.X / TravelTime, -(float)maxXvel, (float)maxXvel), neededYvel);

			return new Vector2(DistanceToTravel.X / TravelTime, neededYvel);
		}

		public static Vector2 GetArcVel(this Entity ent, Vector2 targetPos, float gravity, float? minArcHeight = null, float? maxArcHeight = null, float? maxXvel = null, float? heightabovetarget = null) => GetArcVel(ent.Center, targetPos, gravity, minArcHeight, maxArcHeight, maxXvel, heightabovetarget);
	}
}
