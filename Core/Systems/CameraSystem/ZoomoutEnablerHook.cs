/*using Microsoft.Xna.Framework; //TODO: Dear god x2.
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
		public static float lightingStoredSize = 1;

		//this is super hacky
		

		public override void Load()
		{
			if (Main.dedServ)
				return;

			IL.Terraria.Main.DrawTiles += DrawZoomOut;
			IL.Terraria.Main.InitTargets_int_int += ChangeTargets;

			//This is... a thing
			IL.Terraria.Lighting.AddLight_int_int_float_float_float += ResizeLighting;

			IL.Terraria.Lighting.LightTiles += ResizeLighting; //TODO: Dear god.
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

			IL.Terraria.Lighting.Initialize += ResizeLightingBig;

			IL.Terraria.Lighting.doColors += HackSwipes;

			IL.Terraria.Main.DrawBlack += ResizeLighting;
			IL.Terraria.Main.DrawWalls += ResizeLighting;
			IL.Terraria.Main.drawWaters += ResizeLighting;

			On.Terraria.Lighting.PreRenderPhase += ReInit;
			On.Terraria.Lighting.LightTiles += MoveLighting;
		}

		private void MoveLighting(On.Terraria.Lighting.orig_LightTiles orig, int firstX, int lastX, int firstY, int lastY)
		{
			orig(firstX - (int)Math.Floor(AddExpansion() / 2f), (int)Math.Floor(lastX + AddExpansion() / 2f), (int)Math.Floor(firstY - AddExpansionY() / 2f), (int)Math.Floor(lastY + AddExpansionY() / 2f));
		}

		private void ResizeLightingBig(ILContext il)
		{
			var c = new ILCursor(il);

			c.TryGotoNext(MoveType.After, n => n.MatchLdsfld<Main>("screenWidth"));
			c.Emit(OpCodes.Pop);
			c.Emit(OpCodes.Ldc_I4, 6);
			c.EmitDelegate<Func<int, int>>(ReplaceDimension);

			c.TryGotoNext(MoveType.After, n => n.MatchLdsfld<Main>("screenHeight"));
			c.Emit(OpCodes.Pop);
			c.Emit(OpCodes.Ldc_I4, 7);
			c.EmitDelegate<Func<int, int>>(ReplaceDimension);
		}

		private void ResizeOcclusion(ILContext il)
		{
			var c = new ILCursor(il);

			c.TryGotoNext(n => n.MatchLdsfld<Lighting>("firstToLightX"));

			c.TryGotoNext(MoveType.After, n => n.MatchStloc(5));
			c.Emit(OpCodes.Ldloc, 5);
			c.EmitDelegate<Func<int>>(AddExpansion);
			c.Emit(OpCodes.Add);
			c.Emit(OpCodes.Stloc, 5);


			c.TryGotoNext(MoveType.After, n => n.MatchStloc(5));
			c.Emit(OpCodes.Ldloc, 7);
			c.EmitDelegate<Func<int>>(AddExpansionY);
			c.Emit(OpCodes.Add);
			c.Emit(OpCodes.Stloc, 7);
		}

		private int AddExpansion()
		{
			return (int)Math.Floor(((Main.screenPosition.X + (Main.screenWidth * (1f / Core.ZoomHandler.ClampedExtraZoomTarget))) / 16f) + 2 - (((Main.screenPosition.X + Main.screenWidth) / 16f) + 2));
		}

		private int AddExpansionY()
		{
			return (int)Math.Floor(((Main.screenPosition.Y + (Main.screenHeight * (1f / Core.ZoomHandler.ClampedExtraZoomTarget))) / 16f) + 2 - (((Main.screenPosition.Y + Main.screenHeight) / 16f) + 2));
		}

		private void HackSwipes(ILContext il)
		{
			var c = new ILCursor(il);

			for (int k = 0; k < 4; k++)
			{
				//HackSwipe(c, "innerLoop1Start", k % 2 == 0, false); //if its X or Y should alternate every other patch
				HackSwipe(c, "innerLoop1End", k % 2 == 0, true);
				HackSwipe(c, "innerLoop2Start", k % 2 == 0, true);
				//HackSwipe(c, "innerLoop2End", k % 2 == 0, true);
				//HackSwipe(c, "outerLoopStart", k % 2 == 1, false);
				HackSwipe(c, "outerLoopEnd", k % 2 == 1, true);
			}
		}

		private void HackSwipe(ILCursor c, string name, bool y, bool add)
		{
			var types = typeof(Lighting).GetTypeInfo().DeclaredNestedTypes;
			var type = types.FirstOrDefault(n => n.FullName == "Terraria.Lighting+LightingSwipeData");

			c.TryGotoNext(MoveType.Before, n => n.MatchStfld(type, name));

			if(y)
				c.EmitDelegate<Func<int>>(AddExpansionY);

			else
				c.EmitDelegate<Func<int>>(AddExpansion);

			c.Emit(add ? OpCodes.Add : OpCodes.Sub);
		}

		private void ReInit(On.Terraria.Lighting.orig_PreRenderPhase orig)
		{
			if (ZoomHandler.ClampedExtraZoomTarget < 1 && ZoomHandler.ClampedExtraZoomTarget != lightingStoredSize)
			{
				Lighting.Initialize(true);
				lightingStoredSize = ZoomHandler.ClampedExtraZoomTarget;
			}

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

		private void ChangeTargets(ILContext il)
		{
			var c = new ILCursor(il);

			SwapTarget(c, "waterTarget");
			SwapTarget(c, "backWaterTarget");
			SwapTarget(c, "blackTarget");
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
					return (int)Math.Floor((Main.screenPosition.X - zero.X) / 16f - 1f);

				case 1:
					return (int)Math.Floor((Main.screenPosition.X + Main.screenWidth * (1f / ZoomHandler.ClampedExtraZoomTarget) + zero.X) / 16f) + 2;

				case 2:
					return (int)Math.Floor((Main.screenPosition.Y - zero.Y) / 16f - 1f);

				case 3:
					return (int)Math.Floor((Main.screenPosition.Y + Main.screenHeight * (1f / ZoomHandler.ClampedExtraZoomTarget) + zero.Y) / 16f) + 5;

				case 4:
					return (int)Math.Floor(Main.screenWidth * (1f / ZoomHandler.ClampedExtraZoomTarget));

				case 5:
					return (int)Math.Floor(Main.screenHeight * (1f / ZoomHandler.ClampedExtraZoomTarget));

				case 6:
					return (int)Math.Floor(Main.screenWidth * (1f / ZoomHandler.ClampedExtraZoomTarget)) * 2;

				case 7:
					return (int)Math.Floor(Main.screenHeight * (1f / ZoomHandler.ClampedExtraZoomTarget)) * 2;

				default:
					return 0;
			}
		}
	}

}*/
