using Terraria;

using StarlightRiver.Core;
using StarlightRiver.Content.Items.BaseTypes;

namespace StarlightRiver.Content.CustomHooks
{
    class InventoryParticleDrawing : HookGroup
    {
        //Just drawing some ParticleSystems over the inventory UI. Nothing bad.
        public override SafetyLevel Safety => SafetyLevel.Safe;

        public override void Load()
        {
            On.Terraria.Main.DrawInterface_27_Inventory += DrawInventoryParticles;
        }

        private void DrawInventoryParticles(On.Terraria.Main.orig_DrawInterface_27_Inventory orig, Main self)
        {
            orig(self);
            CursedAccessory.CursedSystem.DrawParticles(Main.spriteBatch);
        }
    }
}