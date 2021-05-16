using MonoMod.Cil;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.Cil;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Lavas
{
    class LavaLoader : ILoadable
    {
        public static List<LavaStyle> lavas = new List<LavaStyle>();

        public void Load()
        {
            IL.Terraria.GameContent.Liquid.LiquidRenderer.InternalDraw += DrawSpecialLava;
            IL.Terraria.Main.DrawTiles += DrawSpecialLavaBlock;

            IL.Terraria.GameContent.Liquid.LiquidRenderer.InternalPrepareDraw += SwapLavaDrawEffects;
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
            foreach(var style in lavas)
                if (style.ChooseLavaStyle) return style.Type;

            return arg;
        }

        private void DrawSpecialLavaBlock(ILContext il)
        {
            var c = new ILCursor(il);
            c.TryGotoNext(n => n.MatchLdsfld(typeof(Main), "liquidTexture"));
            c.Index += 3;
            c.EmitDelegate<Func<Texture2D, Texture2D>>(LavaBlockBody);
        }

        private Texture2D LavaBlockBody(Texture2D arg)
        {
            foreach (var style in lavas)
                if (style.ChooseLavaStyle)
                {
                    string path = "";
                    string garbage = "", garbage2 = "";
                    style.SafeAutoload(ref garbage, ref path, ref garbage2);
                    return GetTexture(path);
                }

            return arg;
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
            foreach (var style in lavas)
                if (style.ChooseLavaStyle)
                {
                    return style.DrawEffects(x, y);
                }

            return false;
        }
    }
}
