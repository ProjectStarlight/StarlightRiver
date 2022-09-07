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
            On.Terraria.Main.DrawBlack += DrawArtifacts;
        }

        public override void Unload()
        {
            On.Terraria.Main.DrawBlack -= DrawArtifacts;
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

        public void DrawArtifacts(On.Terraria.Main.orig_DrawBlack orig, Main self, bool force = false)
        {
            foreach (var item in TileEntity.ByID)
            {
                if (item.Value is Artifact artifact && artifact.IsOnScreen())
                {
                    artifact.Draw(Main.spriteBatch);
                }
            }

            orig(self, force);
        }
    }
}