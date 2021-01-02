using Terraria.GameContent.Liquid;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics;
using MonoMod.Cil;
using StarlightRiver.Core;
using Mono.Cecil.Cil;

namespace StarlightRiver.Content.CustomHooks
{
    class VitricLava : HookGroup
    {
        public override void Load()
        {
            IL.Terraria.GameContent.Liquid.LiquidRenderer.InternalDraw += DrawSpecialLava;
        }

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
            if (Main.LocalPlayer.GetModPlayer<BiomeHandler>().ZoneGlass) return Terraria.ModLoader.ModContent.GetInstance<Waters.WaterJungleBloody>().Type;
            return arg;
        }
    }
}
