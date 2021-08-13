using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using StarlightRiver.Core;
using System;
using System.Reflection;
using System.Linq;
using Terraria;

namespace StarlightRiver.Content.CustomHooks
{
	class ZoomOutEnabler : HookGroup
    {
        //this is super hacky
        public override SafetyLevel Safety => SafetyLevel.OhGodOhFuck;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            IL.Terraria.Main.DrawTiles += DrawZoomOut;
            IL.Terraria.Main.InitTargets_int_int += ChangeTargets;

            //This is... a thing
            IL.Terraria.Lighting.AddLight_int_int_float_float_float += ResizeLighting;

            IL.Terraria.Lighting.LightTiles += ResizeLighting;
            IL.Terraria.Lighting.PreRenderPhase += ResizeLighting;
            IL.Terraria.Lighting.PreRenderPhase += ResizeOcclusion;
            IL.Terraria.Lighting.Brightness += ResizeLighting;

            IL.Terraria.Lighting.GetBlackness += ResizeLighting;

            IL.Terraria.Lighting.GetColor_int_int += ResizeLighting;
            IL.Terraria.Lighting.GetColor_int_int_Color += ResizeLighting;

            IL.Terraria.Lighting.GetColor4Slice += ResizeLighting;
            IL.Terraria.Lighting.GetColor4Slice_New_int_int_refVertexColors_Color_float += ResizeLighting;
            IL.Terraria.Lighting.GetColor4Slice_New_int_int_refVertexColors_float += ResizeLighting;

            IL.Terraria.Lighting.GetColor9Slice += ResizeLighting;

            IL.Terraria.Lighting.Initialize += ResizeLighting;

            IL.Terraria.Lighting.doColors += HackSwipes;

            On.Terraria.Main.DrawTiles += OffsetDrawing;
            On.Terraria.Lighting.PreRenderPhase += ReInit;
        }

		private void ResizeOcclusion(ILContext il)
		{
            var c = new ILCursor(il);

            c.TryGotoNext(n => n.MatchLdsfld<Lighting>("firstToLightX"));

            c.TryGotoNext(MoveType.Before, n => n.MatchStloc(5));
            c.Emit(OpCodes.Conv_R4);
            c.EmitDelegate<Func<float>>(GetMultiple);
            c.Emit(OpCodes.Mul);
            c.Emit(OpCodes.Conv_I4);

            c.TryGotoNext(MoveType.Before, n => n.MatchStloc(7));
            c.Emit(OpCodes.Conv_R4);
            c.EmitDelegate<Func<float>>(GetMultiple);
            c.Emit(OpCodes.Mul);
            c.Emit(OpCodes.Conv_I4);
        }

		private void HackSwipes(ILContext il)
		{
            var c = new ILCursor(il);

            for (int k = 0; k < 4; k++)
            {
                HackSwipe(c, "innerLoop1End");
                HackSwipe(c, "innerLoop2End");
                HackSwipe(c, "outerLoopEnd");
            }
        }

        private void HackSwipe(ILCursor c, string name)
		{
            var types = typeof(Lighting).GetTypeInfo().DeclaredNestedTypes;
            var type = types.FirstOrDefault(n => n.FullName == "Terraria.Lighting+LightingSwipeData");

            c.TryGotoNext(MoveType.Before, n => n.MatchStfld(type, name));
            c.Emit(OpCodes.Conv_R4);
            c.EmitDelegate<Func<float>>(GetMultiple);
            c.Emit(OpCodes.Mul);
            c.Emit(OpCodes.Conv_I4);
            c.EmitDelegate<Func<int, int>>(PrintValue);
        }

		private int PrintValue(int arg)
		{
            Main.NewText(arg);
            return arg;
		}

		private float GetMultiple()
		{
            return Math.Min(1f / ZoomHandler.zoomOverride, 1.8f);
        }

		private void ReInit(On.Terraria.Lighting.orig_PreRenderPhase orig)
		{
            Lighting.Initialize();
            orig();
		}

		private void ResizeLighting(ILContext il)
		{
            var c = new ILCursor(il);

            c.TryGotoNext(MoveType.After, n => n.MatchLdsfld<Main>("screenWidth"));
            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Ldc_I4, 4);
            c.EmitDelegate<Func<int, int>>(ReplaceDimension);

            c.TryGotoNext(MoveType.After, n => n.MatchLdsfld<Main>("screenHeight"));
            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Ldc_I4, 5);
            c.EmitDelegate<Func<int, int>>(ReplaceDimension);
        }

		private void OffsetDrawing(On.Terraria.Main.orig_DrawTiles orig, Main self, bool solidOnly, int waterStyleOverride)
        {
            try
            {
                orig(self, solidOnly, waterStyleOverride);
            }
            catch
            {

            }
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
  			/*
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
            *//*
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
            */
            var c = new ILCursor(il);

            if (!c.TryGotoNext(MoveType.After,
                i => i.MatchStloc(8)))
                return;

            c.Emit(OpCodes.Ldc_I4_0);
            c.EmitDelegate<Func<int, int>>(ReplaceDimension);
            c.Emit(OpCodes.Stloc, 5);

            c.Emit(OpCodes.Ldc_I4_1);
            c.EmitDelegate<Func<int, int>>(ReplaceDimension);
            c.Emit(OpCodes.Stloc, 6);

            c.Emit(OpCodes.Ldc_I4_2);
            c.EmitDelegate<Func<int, int>>(ReplaceDimension);
            c.Emit(OpCodes.Stloc, 7);

            c.Emit(OpCodes.Ldc_I4_3);
            c.EmitDelegate<Func<int, int>>(ReplaceDimension);
            c.Emit(OpCodes.Stloc, 8);
        }

        private int ReplaceDimension(int index)
		{
            Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);

            if (Main.drawToScreen)
                zero = Vector2.Zero;

            switch (index)
			{
                case 0:
                    return (int)((Main.screenPosition.X - zero.X) / 16f - 1f);

                case 1:
                    return (int)((Main.screenPosition.X + Main.screenWidth * (1f / ZoomHandler.zoomOverride) + zero.X) / 16f) + 2;

                case 2:
                    return (int)((Main.screenPosition.Y - zero.Y) / 16f - 1f);

                case 3:
                    return (int)((Main.screenPosition.Y + Main.screenHeight * (1f / ZoomHandler.zoomOverride) + zero.Y) / 16f) + 5;

                case 4:
                    return (int)(Main.screenWidth * (1f / ZoomHandler.zoomOverride));

                case 5:
                    return (int)(Main.screenHeight * (1f / ZoomHandler.zoomOverride));

                default:
                    return 0;
            }
		}
    }
}
