using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Codex;
using StarlightRiver.Codex.Entries;
using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Core
{
	public class BiomeHandler : ModPlayer
    {
        public bool ZoneGlass = false;
        public bool ZoneGlassTemple = false;
        public bool GlassBG = false;
        public bool ZoneVoidPre = false;
        public bool ZoneJungleCorrupt = false;
        public bool ZoneJungleBloody = false;
        public bool ZoneJungleHoly = false;
        public bool ZoneOvergrow = false;
        public bool zoneAluminum = false;
        public bool ZonePermafrost = false;
        public bool ZoneAshhell = false;
        public bool ZoneHotspring = false;

        public bool FountainJungleCorrupt = false;
        public bool FountainJungleBloody = false;
        public bool FountainJungleHoly = false;
        public bool FountainVitric = false;
        //public bool FountainVitricLava = false;

        public static Rectangle GlassTempleZone => new Rectangle(StarlightWorld.VitricBiome.Center.X - 50, StarlightWorld.VitricBiome.Center.Y - 4, 101, 400);

        public override void UpdateBiomes()
        {
            ZoneGlass = StarlightWorld.glassTiles > 50 || StarlightWorld.VitricBiome.Contains((player.position / 16).ToPoint());
            ZoneGlassTemple = GlassTempleZone.Contains((player.Center / 16).ToPoint()) && Main.tile[(int)(player.Center.X / 16), (int)(player.Center.Y / 16)].wall != Terraria.ID.WallID.None;
            GlassBG = StarlightWorld.VitricBiome.Contains((player.Center / 16).ToPoint()) && ZoneGlass;
        }

        public override bool CustomBiomesMatch(Player other)
        {
            BiomeHandler modOther = other.GetModPlayer<BiomeHandler>();
            bool allMatch = true;
            allMatch &= ZoneGlass == modOther.ZoneGlass;
            return allMatch;
        }

        public override void CopyCustomBiomesTo(Player other)
        {
            BiomeHandler modOther = other.GetModPlayer<BiomeHandler>();
            modOther.ZoneGlass = ZoneGlass;
        }

        public override void SendCustomBiomes(BinaryWriter writer)
        {
            BitsByte flags = new BitsByte();
            flags[0] = ZoneGlass;
            writer.Write(flags); //TODO add another BitsByte for moar biomes
        }

        public override void ReceiveCustomBiomes(BinaryReader reader)
        {
            BitsByte flags = reader.ReadByte();
            ZoneGlass = flags[0];
        }

        public override void PreUpdate()
        {
            float distance = Vector2.Distance(Main.LocalPlayer.Center, StarlightWorld.RiftLocation);
            if (distance <= 1500)
            {
                float val = (1500 / distance - 1) * 2;
                if (val <= 1) val = 1;
                if (val >= 2.5f) val = 2.5f;
                //Lighting.brightness = 1 / val;
            }

            if (!Main.dedServ && player == Main.LocalPlayer)
            {
                if (ZoneGlass && Main.Configuration.Get<bool>("UseHeatDistortion", false))
                {
                    if (!Filters.Scene["GradientDistortion"].IsActive())
                    {
                        Filters.Scene["GradientDistortion"].GetShader().Shader.Parameters["uZoom"].SetValue(Main.GameViewMatrix.Zoom);
                        Filters.Scene.Activate("GradientDistortion").GetShader()
                            .UseOpacity(2.5f)
                            .UseIntensity(7f)
                            .UseProgress(6)
                            .UseImage(StarlightRiver.LightingBufferInstance.ScreenLightingTexture, 0);
                    }
                }
                else
                {
                    if (Filters.Scene["GradientDistortion"].IsActive())
                        Filters.Scene.Deactivate("GradientDistortion");
                }
            }

            if (ZoneGlass && player.GetModPlayer<CodexHandler>().Entries.Any(entry => entry is VitricEntry && entry.Locked))
                Helper.UnlockEntry<VitricEntry>(player);
        }

		public override void ResetEffects()
		{
            ZoneHotspring = false;
		}

        public override Texture2D GetMapBackgroundImage()
        {
            if (ZoneGlass || ZoneGlassTemple || GlassBG)
                return GetTexture(AssetDirectory.MapBackgrounds + "GlassMap");

            return null;
        }
    }

    public partial class StarlightWorld : ModWorld
    {
        public static int glassTiles;
        public static int voidTiles;
        public static int corruptJungleTiles;
        public static int bloodJungleTiles;
        public static int holyJungleTiles;
        public static int aluminumTiles;
        public static int permafrostTiles;
        public static int ashHellTiles;

        public override void TileCountsAvailable(int[] tileCounts)
        {
            glassTiles = tileCounts[mod.TileType("VitricSand")] + tileCounts[mod.TileType("AncientSandstone")];
        }

        public override void ResetNearbyTileEffects()
        {
            BiomeHandler modPlayer = Main.LocalPlayer.GetModPlayer<BiomeHandler>();
            modPlayer.FountainJungleCorrupt = false;
            modPlayer.FountainJungleBloody = false;
            modPlayer.FountainJungleHoly = false;
            modPlayer.FountainVitric = false;
        }
    }
}

namespace StarlightRiver
{
	public partial class StarlightRiver : Mod
    {
        private int AddExpansion()
        {
            return (int)Math.Floor(((Main.screenPosition.X + (Main.screenWidth * (1f / Core.ZoomHandler.ClampedExtraZoomTarget))) / 16f) + 2 - (((Main.screenPosition.X + Main.screenWidth) / 16f) + 2));
        }

        private int AddExpansionY()
        {
            return (int)Math.Floor(((Main.screenPosition.Y + (Main.screenHeight * (1f / Core.ZoomHandler.ClampedExtraZoomTarget))) / 16f) + 2 - (((Main.screenPosition.Y + Main.screenHeight) / 16f) + 2));
        }

        public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
        {
            TargetHost.Maps?.OrderedShaderPass();
            TargetHost.Maps?.OrderedRenderPassBatched(Main.spriteBatch, Main.graphics.GraphicsDevice);

            Main.screenPosition += new Vector2(AddExpansion(), AddExpansionY()) * 8;

            if(StarlightWorld.spaceEventFade > 0)
			{
                tileColor = Color.Lerp(Color.White, new Color(70, 60, 120), StarlightWorld.spaceEventFade);
                backgroundColor = Color.Lerp(Color.White, new Color(17, 15, 30), StarlightWorld.spaceEventFade);
            }
        }
    }
}