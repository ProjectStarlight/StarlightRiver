using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Helpers
{
	public static class CommonVisualEffects
	{
		/// <summary>
		/// A flashing color on a unified timer for ability indicators
		/// </summary>
		public static Color IndicatorColor => Color.White * (float)(0.2f + 0.8f * (1 + Math.Sin(StarlightWorld.visualTimer)) / 2f);

		/// <summary>
		/// Gets a color fade similar to glass cooling. Timer is from 0 to 60
		/// </summary>
		/// <param name="time">The progress along the color animation, from 0 to 60</param>
		/// <returns></returns>
		public static Color HeatedToCoolColor(float time)
		{
			Color MoltenGlowc = Color.White;
			if (time > 30 && time < 60)
				MoltenGlowc = Color.Lerp(Color.White, Color.Orange, Math.Min((time - 30f) / 20f, 1f));
			else if (time >= 60)
				MoltenGlowc = Color.Lerp(Color.Orange, Color.Lerp(Color.Red, Color.Transparent, Math.Min((time - 60f) / 50f, 1f)), Math.Min((time - 60f) / 30f, 1f));
			return MoltenGlowc;
		}

		/// <summary>
		/// Gets an ability outline indicator color, which fades with respect to proximity to a given point
		/// </summary>
		/// <param name="minRadius">The radius at which the effect should appear at all</param>
		/// <param name="maxRadius">The radius at which the effect is at full strength</param>
		/// <param name="center">The center to base the radii on</param>
		/// <returns></returns>
		public static Color IndicatorColorProximity(int minRadius, int maxRadius, Vector2 center)
		{
			float distance = Vector2.Distance(center, Main.LocalPlayer.Center);

			if (distance > maxRadius)
				return Color.White * 0f;

			return IndicatorColor * (1 - Math.Min(1, (distance - minRadius) / (maxRadius - minRadius)));
		}
	}
}