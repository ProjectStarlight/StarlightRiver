using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;

namespace StarlightRiver.Helpers
{
	public static partial class Helpers
	{
		public static bool HasParameter(this Effect effect, string parameterName)
		{
			foreach (EffectParameter parameter in effect.Parameters)
			{
				if (parameter.Name == parameterName)
					return true;
			}

			return false;
		}

		public static void ActivateScreenShader(string ShaderName, Vector2 vec = default)
		{
			if (Main.netMode != NetmodeID.Server && !Filters.Scene[ShaderName].IsActive())
				Filters.Scene.Activate(ShaderName, vec);
		}

		public static ScreenShaderData GetScreenShader(string ShaderName)
		{
			return Filters.Scene[ShaderName].GetShader();
		}
	}
}
