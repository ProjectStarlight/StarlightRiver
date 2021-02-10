using Microsoft.Xna.Framework;
using MonoMod.Cil;
using System;
using System.Linq;
using Terraria;
using Terraria.Graphics.Effects;

using StarlightRiver.Core;
using StarlightRiver.Content.Bosses.SquidBoss;
using StarlightRiver.Content.NPCs.BaseTypes;

namespace StarlightRiver.Content.CustomHooks
{
    class DrawUnderCathedralWater : HookGroup
    {
        //Rare method to hook but not the best finding logic, but its really just some draws so nothing should go terribly wrong.
        public override SafetyLevel Safety => SafetyLevel.Fragile;

        public override void Load()
        {
            IL.Terraria.Main.DoDraw += DrawWater;
        }

        public override void Unload()
        {
            IL.Terraria.Main.DoDraw -= DrawWater;
        }

        private void DrawWater(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.TryGotoNext(n => n.MatchLdfld<Main>("DrawCacheNPCsBehindNonSolidTiles"));
            c.Index--;

            c.EmitDelegate<DrawWaterDelegate>(DrawWater);
        }

        private delegate void DrawWaterDelegate();
        private void DrawWater()
        {
            foreach (NPC npc in Main.npc.Where(n => n.active && n.modNPC is ArenaActor))
            {
                (npc.modNPC as ArenaActor).DrawBigWindow(Main.spriteBatch);

                foreach (NPC npc2 in Main.npc.Where(n => n.active && n.modNPC is IUnderwater && !(n.modNPC is SquidBoss)))
                    (npc2.modNPC as IUnderwater).DrawUnderWater(Main.spriteBatch);

                foreach (Projectile proj in Main.projectile.Where(n => n.active && n.modProjectile is IUnderwater))
                    (proj.modProjectile as IUnderwater).DrawUnderWater(Main.spriteBatch);

                foreach (NPC npc3 in Main.npc.Where(n => n.active && n.modNPC is SquidBoss))
                    (npc3.modNPC as IUnderwater).DrawUnderWater(Main.spriteBatch);

                var effect = Filters.Scene["Waves"].GetShader().Shader;

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(default, default, default, default, default, effect, Main.GameViewMatrix.ZoomMatrix);

                effect.Parameters["uTime"].SetValue(StarlightWorld.rottime);
                effect.Parameters["power"].SetValue(0.002f + 0.0005f * (float)Math.Sin(StarlightWorld.rottime));
                effect.Parameters["speed"].SetValue(50f);

                Main.spriteBatch.Draw(CathedralTarget.CatherdalWaterTarget, Vector2.Zero - Main.LocalPlayer.velocity, Color.White);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);
            }
        }
    }
}