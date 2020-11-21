using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.CustomHooks
{
    class MatrixUpdateCancel : HookGroup
    {
        //Really risky matrix reset cancellation which may or may not kill zoom
        public override SafetyLevel Safety => SafetyLevel.Fragile;

        public override void Load()
        {
            On.Terraria.Graphics.SpriteViewMatrix.ShouldRebuild += UpdateMatrixFirst;
        }

        private bool UpdateMatrixFirst(On.Terraria.Graphics.SpriteViewMatrix.orig_ShouldRebuild orig, SpriteViewMatrix self) => StarlightRiver.Rotation == 0 && orig(self);
    }
}