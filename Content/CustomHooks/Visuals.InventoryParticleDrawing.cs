using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Items.CursedAccessories;
using StarlightRiver.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

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