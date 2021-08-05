using Microsoft.Xna.Framework.Graphics;

namespace StarlightRiver.Helpers
{
	static partial class Helpers
    {
       public static bool HasParameter(this Effect effect, string parameterName)
        {
            foreach (EffectParameter parameter in effect.Parameters)
            {
                if (parameter.Name == parameterName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
