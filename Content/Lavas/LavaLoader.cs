using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Lavas
{
	class LavaLoader : IOrderedLoadable
	{
		public static List<LavaStyle> lavas = new();
		public static LavaStyle ActiveStyle;

		public void Load()
		{
			StarlightPlayer.ResetEffectsEvent += UpdateActiveStyle;

			if (Main.dedServ)
				return;

			Terraria.GameContent.Liquid.IL_LiquidRenderer.DrawNormalLiquids += DrawSpecialLava;
			Terraria.GameContent.Liquid.IL_LiquidRenderer.InternalPrepareDraw += SwapLavaDrawEffects;
			//IL.Terraria.Main.DrawTiles += DrawSpecialLavaBlock;
		}

		private void UpdateActiveStyle(StarlightPlayer Player)
		{
			if (!Main.gameMenu)
				ActiveStyle = lavas.FirstOrDefault(n => n.ChooseLavaStyle());
		}

		public void Unload()
		{
			Terraria.GameContent.Liquid.IL_LiquidRenderer.DrawNormalLiquids -= DrawSpecialLava;
			Terraria.GameContent.Liquid.IL_LiquidRenderer.InternalPrepareDraw -= SwapLavaDrawEffects;
			//IL.Terraria.Main.DrawTiles -= DrawSpecialLavaBlock;

			lavas = null;
			ActiveStyle = null;
		}

		public float Priority => 1;

		private void DrawSpecialLava(ILContext il)
		{
			var c = new ILCursor(il);
			c.TryGotoNext(n => n.MatchLdloc(8), n => n.MatchLdcI4(2));
			c.Index += 2;
			c.EmitDelegate<Func<int, int>>(LavaBody);
			c.Emit(OpCodes.Stloc, 8);
			c.Emit(OpCodes.Ldloc, 8);
		}

		private int LavaBody(int arg)
		{
			if (ActiveStyle != null)
				return ActiveStyle.Slot;

			return arg;
		}

		private void DrawSpecialLavaBlock(ILContext il)
		{
			var c = new ILCursor(il);
			c.TryGotoNext(n => n.MatchLdsfld(typeof(Main), "liquidTexture"));
			c.Index += 3;
			c.Emit(OpCodes.Ldloc, 16);
			c.Emit(OpCodes.Ldloc, 15);

			c.Emit(OpCodes.Ldloc, 142);
			c.Emit(OpCodes.Ldloc, 141);
			c.Emit(OpCodes.Ldloc, 140);
			c.Emit(OpCodes.Ldloc, 143);

			c.EmitDelegate<Func<Texture2D, int, int, Tile, Tile, Tile, Tile, Texture2D>>(LavaBlockBody);
		}

		private Texture2D LavaBlockBody(Texture2D arg, int x, int y, Tile up, Tile left, Tile right, Tile down)
		{
			if (ActiveStyle is null)
				return arg;

			if (arg != Request<Texture2D>("Terraria/Liquid_1").Value)
				return arg;

			string path = ActiveStyle.texturePath;
			ActiveStyle.DrawBlockEffects(x, y, up, left, right, down);
			return Request<Texture2D>(path + "_Block").Value;
		}

		private void SwapLavaDrawEffects(ILContext il)
		{
			var c = new ILCursor(il);
			c.TryGotoNext(n => n.MatchLdsfld<Dust>("lavaBubbles"));
			c.Index += 3;

			int savedIndex = c.Index;

			c.TryGotoNext(n => n.MatchLdloc(4)); //I know this looks bad but this is reliable enough. Local 4 is ptr2 in source
			ILLabel label = il.DefineLabel(c.Next);

			c.Index = savedIndex;
			c.Emit(OpCodes.Ldloc, 66); //num25, x coordinate iteration variable
			c.Emit(OpCodes.Ldloc, 67); //num26, y coordinate iteration variable
			c.EmitDelegate<Func<int, int, bool>>(SwapLava);
			c.Emit(OpCodes.Brtrue, label);
		}

		private bool SwapLava(int x, int y)
		{
			if (ActiveStyle != null)
				return ActiveStyle.DrawEffects(x, y);

			return false;
		}
	}
}