using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.CustomHooks
{
    class JungleClentaminator : HookGroup
    {
        //I wrote this at like 4AM, dont ask me what litterally any of this does, I just know that it works. for  now.
        public override SafetyLevel Safety => SafetyLevel.OhGodOhFuck;

        public override void Load()
        {
            IL.Terraria.WorldGen.Convert += JungleGrassConvert;
            IL.Terraria.WorldGen.hardUpdateWorld += JungleGrassSpread;
        }

        public override void Unload()
        {
            IL.Terraria.WorldGen.Convert -= JungleGrassConvert;
            IL.Terraria.WorldGen.hardUpdateWorld -= JungleGrassSpread;
        }

        private void JungleGrassSpread(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            for (int k = 0; k < 3; k++)
            {
                int type;
                switch (k)
                {
                    case 0: type = TileType<Tiles.JungleBloody.GrassJungleBloody>(); break;
                    case 2: type = TileType<Tiles.JungleCorrupt.GrassJungleCorrupt>(); break;
                    case 1: type = TileType<Tiles.JungleHoly.GrassJungleHoly>(); break;
                    default: type = 2; break;
                }

                for (int n = 0; n < 2; n++)
                {
                    c.TryGotoNext(i => i.MatchLdcI4(0), i => i.MatchStfld<Tile>("type"));
                    c.Index--;
                    c.Emit(OpCodes.Pop);
                    c.Emit(OpCodes.Ldc_I4, type);
                }
            }
        }

        private void JungleGrassConvert(ILContext il) //Fun stuff.
        {
            ILCursor c = new ILCursor(il);
            for (int k = 0; k < 3; k++)
            {
                int type;
                int index;
                switch (k)
                {
                    case 0: type = TileType<Tiles.JungleBloody.GrassJungleBloody>(); break;
                    case 2: type = TileType<Tiles.JungleCorrupt.GrassJungleCorrupt>(); break;
                    case 1: type = TileType<Tiles.JungleHoly.GrassJungleHoly>(); break;
                    default: type = 2; break;
                }
                c.TryGotoNext(i => i.MatchLdsfld(typeof(TileID.Sets.Conversion).GetField("Grass")));
                c.TryGotoNext(i => i.MatchLdsfld(typeof(TileID.Sets.Conversion).GetField("Ice")));
                index = c.Index--;
                c.TryGotoPrev(i => i.MatchLdsfld(typeof(TileID.Sets.Conversion).GetField("Grass")));
                c.Index++;
                c.Emit(OpCodes.Pop);
                c.Emit(OpCodes.Ldc_I4, type);
                c.Emit(OpCodes.Ldloc_0);
                c.Emit(OpCodes.Ldloc_1);
                c.EmitDelegate<GrassConvertDelegate>(EmitGrassConvertDelegate);
                c.Emit(OpCodes.Brtrue, il.Instrs[index]);
                c.Emit(OpCodes.Ldsfld, typeof(TileID.Sets.Conversion).GetField("Grass"));
            }
            c.TryGotoNext(i => i.MatchLdfld<Tile>("wall"), i => i.MatchLdcI4(69)); //funny sex number!!!
            c.TryGotoPrev(i => i.MatchLdsfld<Main>("tile"));
            c.Index++;
            c.Emit(OpCodes.Ldc_I4, TileID.JungleGrass);
            c.Emit(OpCodes.Ldloc_0);
            c.Emit(OpCodes.Ldloc_1);
            c.EmitDelegate<GrassConvertDelegate>(EmitGrassConvertDelegate);
            c.Emit(OpCodes.Pop);
        }

        private delegate bool GrassConvertDelegate(int type, int x, int y);
        private bool EmitGrassConvertDelegate(int type, int x, int y)
        {
            Tile tile = Main.tile[x, y];
            if (tile.type == TileID.JungleGrass || tile.type == TileType<Tiles.JungleBloody.GrassJungleBloody>() || tile.type == TileType<Tiles.JungleCorrupt.GrassJungleCorrupt>() || tile.type == TileType<Tiles.JungleHoly.GrassJungleHoly>())
            {
                tile.type = (ushort)type;
                return true;
            }
            return false;
        }

    }
}