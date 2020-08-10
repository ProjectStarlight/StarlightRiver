using Microsoft.Xna.Framework;
using StarlightRiver.Codex;
using StarlightRiver.Codex.Entries;
using StarlightRiver.GUI;
using StarlightRiver.Tiles;
using StarlightRiver.Tiles.Permafrost;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver
{
    public class BiomeHandler : ModPlayer
    {
        public bool ZoneGlass = false;
        public bool GlassBG = false;
        public bool ZoneVoidPre = false;
        public bool ZoneJungleCorrupt = false;
        public bool ZoneJungleBloody = false;
        public bool ZoneJungleHoly = false;
        public bool ZoneOvergrow = false;
        public bool zoneAluminum = false;
        public bool zonePermafrost = false;
        public bool zoneAshhell = false;

        public bool FountainJungleCorrupt = false;
        public bool FountainJungleBloody = false;
        public bool FountainJungleHoly = false;

        public override void UpdateBiomes()
        {
            ZoneGlass = StarlightWorld.glassTiles > 50 || StarlightWorld.VitricBiome.Contains((player.position / 16).ToPoint());
            GlassBG = StarlightWorld.VitricBiome.Contains((player.Center / 16).ToPoint()) && ZoneGlass;
            ZoneVoidPre = (StarlightWorld.voidTiles > 50);
            ZoneJungleCorrupt = (StarlightWorld.corruptJungleTiles > 50);
            ZoneJungleBloody = (StarlightWorld.bloodJungleTiles > 50);
            ZoneJungleHoly = (StarlightWorld.holyJungleTiles > 50);
            ZoneOvergrow = Main.tile[(int)(player.Center.X / 16), (int)(player.Center.Y / 16)].wall == WallType<Tiles.Overgrow.WallOvergrowGrass>() ||
                Main.tile[(int)(player.Center.X / 16), (int)(player.Center.Y / 16)].wall == WallType<Tiles.Overgrow.WallOvergrowBrick>() ||
                Main.tile[(int)(player.Center.X / 16), (int)(player.Center.Y / 16)].wall == WallType<Tiles.Overgrow.WallOvergrowInvisible>();
            zoneAluminum = StarlightWorld.aluminumTiles > 50;
            zonePermafrost = StarlightWorld.permafrostTiles > 50;
        }

        public override bool CustomBiomesMatch(Player other)
        {
            BiomeHandler modOther = other.GetModPlayer<BiomeHandler>();
            bool allMatch = true;
            allMatch &= ZoneGlass == modOther.ZoneGlass;
            allMatch &= ZoneVoidPre == modOther.ZoneVoidPre;
            allMatch &= ZoneJungleCorrupt == modOther.ZoneJungleCorrupt;
            allMatch &= ZoneJungleBloody == modOther.ZoneJungleBloody;
            allMatch &= ZoneJungleHoly == modOther.ZoneJungleHoly;
            allMatch &= ZoneOvergrow == modOther.ZoneOvergrow;
            allMatch &= zoneAluminum == modOther.zoneAluminum;
            allMatch &= zonePermafrost == modOther.zonePermafrost;
            allMatch &= zoneAshhell == modOther.zoneAshhell;
            return allMatch;
        }

        public override void CopyCustomBiomesTo(Player other)
        {
            BiomeHandler modOther = other.GetModPlayer<BiomeHandler>();
            modOther.ZoneGlass = ZoneGlass;
            modOther.ZoneVoidPre = ZoneVoidPre;
            modOther.ZoneJungleCorrupt = ZoneJungleCorrupt;
            modOther.ZoneJungleBloody = ZoneJungleBloody;
            modOther.ZoneJungleHoly = ZoneJungleHoly;
            modOther.ZoneOvergrow = ZoneOvergrow;
            modOther.zoneAluminum = zoneAluminum;
            modOther.zonePermafrost = zonePermafrost;
            modOther.zoneAshhell = zoneAshhell;
        }

        public override void SendCustomBiomes(BinaryWriter writer)
        {
            BitsByte flags = new BitsByte();
            flags[0] = ZoneGlass;
            flags[1] = ZoneVoidPre;
            flags[2] = ZoneJungleCorrupt;
            flags[3] = ZoneJungleBloody;
            flags[4] = ZoneJungleHoly;
            flags[5] = ZoneOvergrow;
            flags[6] = zoneAluminum;
            flags[7] = zonePermafrost;
            writer.Write(flags); //TODO add another BitsByte for moar biomes
        }

        public override void ReceiveCustomBiomes(BinaryReader reader)
        {
            BitsByte flags = reader.ReadByte();
            ZoneGlass = flags[0];
            ZoneVoidPre = flags[1];
            ZoneJungleCorrupt = flags[2];
            ZoneJungleBloody = flags[3];
            ZoneJungleHoly = flags[4];
            ZoneOvergrow = flags[5];
            zoneAluminum = flags[6];
            zonePermafrost = flags[7];
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

            if (ZoneVoidPre)
            {
                Overlay.state = 1;
            }
            else if (ZoneJungleCorrupt)
            {
                Overlay.state = 2;
                if (player.wet)
                {
                    player.maxFallSpeed = 999f;
                    if (player.breath != player.breathMax)
                    {
                        player.breath--;
                    }
                }
            }
            else if (ZoneJungleBloody)
            {
                Overlay.state = 3;
                if (player.wet)
                {
                    player.AddBuff(Terraria.ID.BuffID.Ichor, 600);
                }
            }
            else if (ZoneJungleHoly)
            {
                Overlay.state = (int)OverlayState.HolyJungle;
            }

            if (ZoneOvergrow && Main.rand.Next(10) == 0)
            {
                Dust.NewDustPerfect(Main.screenPosition - Vector2.One * 100 + new Vector2(Main.rand.Next(Main.screenWidth + 200), Main.rand.Next(Main.screenHeight + 200)),
                DustType<Dusts.OvergrowDust>(), Vector2.Zero, 0, new Color(255, 255, 205) * 0.05f, 2);
            }

            //Codex Unlocks
            if (ZoneGlass && player.GetModPlayer<CodexHandler>().Entries.Any(entry => entry is VitricEntry && entry.Locked))
                Helper.UnlockEntry<VitricEntry>(player);

            if (ZoneOvergrow && player.GetModPlayer<CodexHandler>().Entries.Any(entry => entry is OvergrowEntry && entry.Locked))
                Helper.UnlockEntry<OvergrowEntry>(player);

            if (zonePermafrost && player.GetModPlayer<CodexHandler>().Entries.Any(entry => entry is PermafrostEntry && entry.Locked))
                Helper.UnlockEntry<PermafrostEntry>(player);
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

        public override void TileCountsAvailable(int[] tileCounts)
        {
            glassTiles = tileCounts[TileType<Tiles.Vitric.Blocks.VitricSand>()];
            voidTiles = tileCounts[TileType<Tiles.Void.VoidBrick>()] + tileCounts[TileType<Tiles.Void.VoidStone>()];
            corruptJungleTiles = tileCounts[TileType<Tiles.JungleCorrupt.GrassJungleCorrupt>()];
            bloodJungleTiles = tileCounts[TileType<Tiles.JungleBloody.GrassJungleBloody>()];
            holyJungleTiles = tileCounts[TileType<Tiles.JungleHoly.GrassJungleHoly>()];
            aluminumTiles = tileCounts[TileType<OreAluminum>()];
            permafrostTiles = tileCounts[TileType<PermafrostIce>()];
        }

        public override void ResetNearbyTileEffects()
        {
            Main.LocalPlayer.GetModPlayer<BiomeHandler>().FountainJungleCorrupt = false;
            Main.LocalPlayer.GetModPlayer<BiomeHandler>().FountainJungleBloody = false;
            Main.LocalPlayer.GetModPlayer<BiomeHandler>().FountainJungleHoly = false;
        }
    }

    public partial class StarlightRiver : Mod
    {
        public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
        {
            if (Main.LocalPlayer.GetModPlayer<BiomeHandler>().ZoneJungleCorrupt)
            {
                tileColor = tileColor.MultiplyRGB(new Color(130, 100, 145));
                backgroundColor = backgroundColor.MultiplyRGB(new Color(130, 100, 145));
            }

            if (Main.LocalPlayer.GetModPlayer<BiomeHandler>().ZoneJungleBloody)
            {
                tileColor = tileColor.MultiplyRGB(new Color(155, 120, 90));
                backgroundColor = backgroundColor.MultiplyRGB(new Color(155, 120, 90));
            }

            if (Main.LocalPlayer.GetModPlayer<BiomeHandler>().ZoneJungleHoly)
            {
                tileColor = tileColor.MultiplyRGB(new Color(30, 60, 65));
                backgroundColor = backgroundColor.MultiplyRGB(new Color(30, 60, 65));
            }

            if (Main.LocalPlayer.GetModPlayer<BiomeHandler>().zoneAluminum)
            {
                tileColor = tileColor.MultiplyRGB(new Color(100, 150, 220));
                backgroundColor = backgroundColor.MultiplyRGB(new Color(70, 100, 120));
            }
        }
    }
}