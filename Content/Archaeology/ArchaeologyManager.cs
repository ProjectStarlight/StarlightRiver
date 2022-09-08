//Artifact general TODO:
//Better spawning method that checks to make sure the entire thing is buried
//Better tilechecck call method
//Better detour for drawing
//Uncomment buggy auroracle arena spawning
//Make generic sparkle dust
//Make said generic sparkle dust affected by light
//Implement UIscaling for minimap drawing
//Saving and loading for whether an artifact is drawn on the map
//Reduce jitter on minimap drawing


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.GUI;
using StarlightRiver.Core;
using StarlightRiver.Packets;
using StarlightRiver.Helpers;
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
            On.Terraria.Main.DrawMap += DrawArtifactsOnMiniMap;
        }

        public override void Unload()
        {
            On.Terraria.Main.DrawTiles -= DrawArtifacts;
            On.Terraria.Main.DrawMap -= DrawArtifactsOnMiniMap;
        }

        public override void PostDrawFullscreenMap(ref string mouseText)
        {
            var toDraw = TileEntity.ByID.Where(x => x.Value is Artifact artifact && artifact.displayedOnMap);
            foreach (var drawable in toDraw)
            {
                var pos = drawable.Value.Position.ToVector2();
                // No, I don't know why it draws one tile to the right, but that's how it is
                Helper.DrawMirrorOnFullscreenMap((int)pos.X - 1, (int)pos.Y, true, ModContent.Request<Texture2D>(AssetDirectory.Archaeology + "DigMarker_White", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value);
            }
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

        private static void DrawArtifactsOnMiniMap(On.Terraria.Main.orig_DrawMap orig, Main self, GameTime gameTime) //Credit to GabeHasWon for this code
        {
            orig(self, gameTime);

            if (Main.mapEnabled && !Main.mapFullscreen) //Draw only on the minimap
            {
                var toDraw = TileEntity.ByID.Where(x => x.Value is Artifact artifact && artifact.displayedOnMap);

                foreach (var drawable in toDraw)
                {
                    float scale = Main.mapMinimapScale;

                    Vector2 realPos = Helper.GetMiniMapPosition(drawable.Value.Position.ToVector2(), scale);

                    scale *= Main.UIScale;

                    float drawScale = (scale * 0.5f + 1f) / 3f;
                    if (drawScale > 1f)
                        drawScale = 1f;

                    if (Helper.PointOnMinimap(realPos))
                        Helper.DrawOnMinimap((int)realPos.X, (int)realPos.Y, drawScale, ModContent.Request<Texture2D>(AssetDirectory.Archaeology + "DigMarker_White", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value);
                }
            }
        }
    }
}