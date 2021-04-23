using MonoMod.Cil;
using StarlightRiver.Core;
using Terraria;

namespace StarlightRiver.Content.CustomHooks
{
    class DrawUnderMoonlord : HookGroup
    {
        //Rare method to hook but not the best finding logic, but its really just some draws so nothing should go terribly wrong.
        public override SafetyLevel Safety => SafetyLevel.Fragile;

        public override void Load()
        {
            IL.Terraria.Main.DoDraw += DrawMoonlordLayer;
        }

        public override void Unload()
        {
            IL.Terraria.Main.DoDraw -= DrawMoonlordLayer;
        }

        private void DrawMoonlordLayer(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.TryGotoNext(n => n.MatchLdfld<Main>("DrawCacheNPCsMoonMoon"));
            c.Index--;

            c.EmitDelegate<DrawWindowDelegate>(EmitMoonlordLayerDel);
        }

        private delegate void DrawWindowDelegate();
        private void EmitMoonlordLayerDel()
        {
            for (int k = 0; k < Main.maxProjectiles; k++)
            {
                if (Main.projectile[k].modProjectile is IMoonlordLayerDrawable)
                    (Main.projectile[k].modProjectile as IMoonlordLayerDrawable).DrawMoonlordLayer(Main.spriteBatch);
            }

            for (int k = 0; k < Main.maxNPCs; k++)
            {
                if (Main.npc[k].modNPC is IMoonlordLayerDrawable)
                    (Main.npc[k].modNPC as IMoonlordLayerDrawable).DrawMoonlordLayer(Main.spriteBatch);
            }
        }
    }
}