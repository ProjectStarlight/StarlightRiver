//Artifact general TODO:
//Better spawning method that checks to make sure the entire thing is buried
//Cultivation
//Better detour for drawing
//Uncomment buggy auroracle arena spawning


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.GUI;
using StarlightRiver.Core;
using StarlightRiver.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Archaeology
{
    public class ArchaeologyManager : ModSystem
    { 
        public override void Load()
        {
            On.Terraria.Main.DrawTiles += DrawArtifacts;
        }

        public override void Unload()
        {
            On.Terraria.Main.DrawTiles -= DrawArtifacts;
        }

        public override void PreUpdateDusts()
        {
            foreach (var item in TileEntity.ByID)
            {
                if (item.Value is Artifact artifact && artifact.IsOnScreen())
                {
                    artifact.CreateSparkles();
                }
            }
        }

        public void DrawArtifacts(On.Terraria.Main.orig_DrawTiles orig, Main self, bool solidLayer, bool forRenderTargets, bool intoRenderTargets, int waterStyleOverride = -1)
        {
            foreach (var item in TileEntity.ByID)
            {
                if (item.Value is Artifact artifact && artifact.IsOnScreen())
                {
                    artifact.Draw(Main.spriteBatch);
                }
            }
            orig(self, solidLayer, forRenderTargets, intoRenderTargets, waterStyleOverride);
        }
    }
}