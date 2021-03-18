using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.CustomHooks
{
    class ZoomOutEnabler : HookGroup
    {
        //lag at worst
        public override SafetyLevel Safety => SafetyLevel.Safe;

        public override void Load()
        {
            IL.Terraria.Main.DrawTiles += DrawZoomOut;
            IL.Terraria.Main.InitTargets_int_int += ChangeTargets;
            On.Terraria.Main.DrawTiles += OffsetDrawing;
        }

        private void OffsetDrawing(On.Terraria.Main.orig_DrawTiles orig, Main self, bool solidOnly, int waterStyleOverride)
        {
            orig(self, solidOnly, waterStyleOverride);
        }

        private void ChangeTargets(ILContext il)
        {
            var c = new ILCursor(il);

            SwapTarget(c, "tileTarget");
            SwapTarget(c, "tile2Target");
            SwapTarget(c, "wallTarget");
        }

        private void SwapTarget(ILCursor c, string name)
        {
            if (!c.TryGotoNext(MoveType.Before,
            i => i.MatchStfld<Main>(name)))
                return;

            c.Emit(OpCodes.Pop);
            c.EmitDelegate<Func<RenderTarget2D>>(returnNewTileTarget);
        }

        private RenderTarget2D returnNewTileTarget()
        {
            return new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth * 2, Main.screenHeight * 2, false, Main.graphics.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
        }

        private void DrawZoomOut(ILContext il)
        {
            var c = new ILCursor(il);

            if (!c.TryGotoNext(MoveType.After,
                i => i.MatchSub(),
                i => i.MatchLdcR4(16),
                i => i.MatchDiv(),
                i => i.MatchLdcR4(1),
                i => i.MatchSub()))
                return;

            c.EmitDelegate<Func<float, float>>((returnvalue) =>
            {
                return (int)((Main.screenPosition.X - 1600) / 16f - 1f);
            });

            /*
                IL_00B5: add
                IL_00B6: ldc.r4    16
                IL_00BB: div
                IL_00BC: conv.i4
                IL_00BD: ldc.i4.2
                IL_00BE: add
                ---> here
            */
            if (!c.TryGotoNext(MoveType.After,
                i => i.MatchAdd(),
                i => i.MatchLdcR4(16),
                i => i.MatchDiv(),
                i => i.MatchConvI4(),
                i => i.MatchLdcI4(2),
                i => i.MatchAdd()))
                return;

            c.EmitDelegate<Func<int, int>>((returnvalue) =>
            {
                return (int)((Main.screenPosition.X + Main.screenWidth + 1600) / 16f + 2);
            });

            return;
            Vector2 zero = new Vector2((float)Main.offScreenRange, (float)Main.offScreenRange);

            //var c = new ILCursor(il);

            if (!c.TryGotoNext(MoveType.After,
                i => i.MatchStloc(8)))
                return;

            c.Index++;

            c.Emit(OpCodes.Ldc_I4, (int)((Main.screenPosition.X - zero.X) / 16f - 1f));
            c.Emit(OpCodes.Stloc, 5);

            c.Emit(OpCodes.Ldc_I4, (int)((Main.screenPosition.X + (float)Main.screenWidth + zero.X) / 16f) + 2);
            c.Emit(OpCodes.Stloc, 6);

            c.Emit(OpCodes.Ldc_I4, (int)((Main.screenPosition.Y - zero.Y) / 16f - 1f));
            c.Emit(OpCodes.Stloc, 7);

            c.Emit(OpCodes.Ldc_I4, (int)((Main.screenPosition.Y + (float)Main.screenHeight + zero.Y) / 16f) + 5);
            c.Emit(OpCodes.Stloc, 8);
        }
    }
}
