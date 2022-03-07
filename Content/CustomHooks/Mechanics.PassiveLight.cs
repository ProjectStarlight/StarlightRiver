using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.CustomHooks
{
	class PassiveLight : HookGroup
    {
        private static float mult = 0;
        private static bool vitricLava = false;

        //Rare method to hook but not the best finding logic. Also old code.
        public override SafetyLevel Safety => SafetyLevel.Fragile;

        public override void Load()
        {
            IL.Terraria.Lighting.PreRenderPhase += VitricLighting;
            On.Terraria.Main.Update += UpdateLightingVars; //TODO: Change to an event in modworld update hook eventually?
        }

		private void UpdateLightingVars(On.Terraria.Main.orig_Update orig, Main self, GameTime gameTime)
		{
            if (!Main.gameMenu)
            {
                mult = (0.8f + (Main.dayTime ? (float)System.Math.Sin(Main.time / Main.dayLength * 3.14f) * 0.35f : -(float)System.Math.Sin(Main.time / Main.nightLength * 3.14f) * 0.35f));
                vitricLava = Main.LocalPlayer.GetModPlayer<BiomeHandler>().ZoneGlass;
            }

            orig(self, gameTime);
		}

		public override void Unload()
        {
            IL.Terraria.Lighting.PreRenderPhase -= VitricLighting;
        }

        private delegate void ModLightingStateDelegate(float from, ref float to);
        private delegate void ModColorDelegate(int i, int j, ref float r, ref float g, ref float b);

        private void VitricLighting(ILContext il)
        {
            // Create our cursor at the start of the void PreRenderPhase() method.
            ILCursor c = new ILCursor(il);

            // We insert our emissions right before the ModifyLight call (line 1963, CIL 0x3428)
            // Get the TileLoader.ModifyLight method. Then, using it,
            // find where it's called and place the cursor right before that call instruction.

            MethodInfo ModifyLight = typeof(TileLoader).GetMethod("ModifyLight", BindingFlags.Public | BindingFlags.Static);
            c.GotoNext(i => i.MatchCall(ModifyLight));

            // Emit the values of I and J.
            /* To emit local variables, you have to know the indeces of where those variables are stored.
             * These are stated at the very top of the method, in a format like below:
             * .locals init ( 
             *      [0] = float32 FstName, 
             *      [1] = ScdName, 
             *      [2] = ThdName
             * )
            */

            c.Emit(OpCodes.Ldloc, 27); // [27] = n
            c.Emit(OpCodes.Ldloc, 29); // [29] = num17

            /* Emit the addresses of R, G, and B.
             * It's important to emit their *addresses*, because we're passing them—
             *   by reference, not by value. Under the hood, "ref" tokens—
             *   pass a pointer to the object (even for managed types),
             *   and that's what we need to do here.
            */
            c.Emit(OpCodes.Ldloca, 32); // [32] = num18
            c.Emit(OpCodes.Ldloca, 33); // [33] = num19
            c.Emit(OpCodes.Ldloca, 34); // [34] = num20

            // Consume the values of I,J and the addresses of R,G,B by calling EmitVitricDel.
            c.EmitDelegate<ModColorDelegate>(EmitVitricDel);
        }

        private static void EmitVitricDel(int i, int j, ref float r, ref float g, ref float b)
        {
            if (!WorldGen.InWorld(i, j))
                return;

            var tile = Framing.GetTileSafely(i, j);

            if (tile is null)
                return;

            // If the tile is in the vitric biome and doesn't block light, emit light.
            if (StarlightWorld.VitricBiome.Contains(i, j))
            {
                bool tileBlock = tile.active() && Main.tileBlockLight[tile.type] && !(tile.slope() != 0 || tile.halfBrick());
                bool wallBlock = Main.wallLight[tile.wall];
                bool lava = tile.liquidType() == 1;
                bool lit = Main.tileLighted[tile.type];

                if (vitricLava && lava)
                    (r, g, b) = (1, 0, 0);

                if (!tileBlock && wallBlock && !lava && !lit)
                {
                    var yOff = j - StarlightWorld.VitricBiome.Y;
                 
                    if (mult > 1)
                        mult = 1;

                    var progress = 0.5f + (yOff / ((float)StarlightWorld.VitricBiome.Height)) * 0.7f;
                    progress = MathHelper.Max(0.5f, progress);

                    r = (0.3f + (yOff > 70 ? ((yOff - 70) * 0.006f) : 0)) * progress * mult;
                    g = (0.48f + (yOff > 70 ? ((yOff - 70) * 0.0005f) : 0)) * progress * mult;
                    b = (0.65f - (yOff > 70 ? ((yOff - 70) * 0.005f) : 0)) * progress * mult;

                    if (yOff > 90 && mult < 1)
                    {
                        r += (1 - mult * mult) * (progress) * 0.6f;
                        g += (1 - mult * mult) * (progress) * 0.2f;
                    }
                }
            }
        }
    }
}