using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Terraria;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Lavas
{
	class LavaLoader : ILoadable
    {
        public static List<LavaStyle> lavas = new List<LavaStyle>();
        public static LavaStyle activeStyle;

        public void Load()
        {
            StarlightPlayer.ResetEffectsEvent += UpdateActiveStyle;

            if (Main.dedServ)
                return;

            IL.Terraria.GameContent.Liquid.LiquidRenderer.InternalDraw += DrawSpecialLava;
            IL.Terraria.Main.DrawTiles += DrawSpecialLavaBlock;

            IL.Terraria.GameContent.Liquid.LiquidRenderer.InternalPrepareDraw += SwapLavaDrawEffects;
        }

        private void UpdateActiveStyle(StarlightPlayer player)
		{
            if(!Main.gameMenu)
                activeStyle = lavas.FirstOrDefault(n => n.ChooseLavaStyle());
		}

		public void Unload()
        {
            IL.Terraria.GameContent.Liquid.LiquidRenderer.InternalDraw -= DrawSpecialLava;
            IL.Terraria.Main.DrawTiles -= DrawSpecialLavaBlock;

            lavas.Clear();
        }

        public float Priority => 1;

        private void DrawSpecialLava(ILContext il)
        {
            var c = new ILCursor(il);
            c.TryGotoNext(n => n.MatchLdloc(8), n => n.MatchLdcI4(2));
            c.Index++;
            c.EmitDelegate<Func<int, int>>(LavaBody);
            c.Emit(OpCodes.Stloc, 8);
            c.Emit(OpCodes.Ldloc, 8);
        }

        private int LavaBody(int arg)
        {
            if (activeStyle != null)
                return activeStyle.Type;

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
            if (activeStyle is null)
                return arg;

            if (arg != GetTexture("Terraria/Liquid_1"))
                return arg;

            string path = activeStyle.texturePath;
            activeStyle.DrawBlockEffects(x, y, up, left, right, down);
            return GetTexture(path + "_Block");
        }

        private void SwapLavaDrawEffects(ILContext il)
        {
            var c = new ILCursor(il);
            c.TryGotoNext(n => n.MatchLdsfld<Dust>("lavaBubbles"));
            c.Index += 3;

            var savedIndex = c.Index;

            c.TryGotoNext(n => n.MatchLdloc(4)); //I know this looks bad but this is reliable enough. Local 4 is ptr2 in source
            var label = il.DefineLabel(c.Next);

            c.Index = savedIndex;
            c.Emit(OpCodes.Ldloc, 66); //num25, x coordinate iteration variable
            c.Emit(OpCodes.Ldloc, 67); //num26, y coordinate iteration variable
            c.EmitDelegate<Func<int, int, bool>>(SwapLava);
            c.Emit(OpCodes.Brtrue, label);
        }

        private bool SwapLava(int x, int y)
        {
            if (activeStyle != null)
                return activeStyle.DrawEffects(x, y);

            return false;
        }
    }
}
