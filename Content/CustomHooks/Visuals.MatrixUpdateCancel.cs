using Terraria.Graphics;

using StarlightRiver.Core;

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

        private bool UpdateMatrixFirst(On.Terraria.Graphics.SpriteViewMatrix.orig_ShouldRebuild orig, SpriteViewMatrix self) => orig(self);
    }
}