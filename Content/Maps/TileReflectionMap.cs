
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Core;
using System.Collections.Generic;
using System.Diagnostics;
using Terraria;
using Terraria.Graphics.Shaders;

namespace StarlightRiver.Maps
{
    public class TileReflectionMap : MapPass
    {
        protected override string MapEffectName => "TileReflection";
        public override int Priority => 1;

        internal override void OnApplyShader()
        {
            MapEffect?.Shader.Parameters["PlayerMap"]?.SetValue(TargetHost.GetMap("TileReflectableMap").MapTarget);
            MapEffect?.Shader.Parameters["PlayerTarget"]?.SetValue(PlayerTarget.Target);
        }

    }

    public class TileReflectableMap : MapPass
    {
        protected override string MapEffectName => "";
        public override int Priority => 0;
    }
}