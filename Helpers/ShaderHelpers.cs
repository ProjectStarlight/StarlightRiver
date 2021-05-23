using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.ID;
using System.Collections.Generic;

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
