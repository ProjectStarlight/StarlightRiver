using Microsoft.Xna.Framework;
using StarlightRiver.Keys;
using StarlightRiver.NPCs.Boss.SquidBoss;
using StarlightRiver.NPCs.TownUpgrade;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.World.Generation;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver
{
    public partial class StarlightWorld : ModWorld
    {
        public static Vector2 BookSP;
        public static Vector2 DashSP;
        public static Vector2 WispSP;
        public static Vector2 PureSP;
        public static Vector2 SmashSP;

        public static Vector2 RiftLocation;

        public static bool AluminumMeteors = false;

        //Boss Flags
        public static bool DesertOpen = false;

        public static bool SquidBossOpen = false;
        public static bool SquidBossDowned = false;

        public static bool GlassBossOpen = false;
        public static bool GlassBossDowned = false;

        public static bool OvergrowBossOpen = false;
        public static bool OvergrowBossFree = false;
        public static bool OvergrowBossDowned = false;

        public static bool SealOpen = false;

        public static float Chungus = 0;

        public static float rottime = 0;

        //Voidsmith
        public static Dictionary<string, bool> TownUpgrades = new Dictionary<string, bool>();

        public static List<Vector2> PureTiles = new List<Vector2> { };

        public static Rectangle VitricBiome = new Rectangle();

        public static Rectangle SquidBossArena = new Rectangle();

        //Handling Keys
        public static List<Key> Keys = new List<Key>();

        public static List<Key> KeyInventory = new List<Key>();

        private void EbonyGen(GenerationProgress progress)
        {
            progress.Message = "Making the World Impure...";

            for (int k = 0; k < (int)((Main.maxTilesX * Main.maxTilesY) * .0015); k++)
            {
                int x = WorldGen.genRand.Next(0, Main.maxTilesX);
                int y = WorldGen.genRand.Next(0, (int)WorldGen.worldSurfaceHigh);

                if (Main.tile[x, y].type == TileID.Dirt && Math.Abs(x - Main.maxTilesX / 2) >= Main.maxTilesX / 6)
                {
                    WorldGen.TileRunner(x, y, WorldGen.genRand.Next(10, 11), 1, TileType<Tiles.OreEbony>(), false, 0f, 0f, false, true);
                }
            }
        }

        public override void PreUpdate()
        {
            rottime += (float)Math.PI / 60;
            if (rottime >= Math.PI * 2) rottime = 0;
        }

        public override void PostUpdate()
        {
            if (!Main.projectile.Any(proj => proj.type == ProjectileType<Projectiles.Ability.Purifier>()) && PureTiles != null)
                PureTiles.Clear();

            //SquidBoss arena
            if (!Main.npc.Any(n => n.active && n.type == NPCType<ArenaActor>()))
                NPC.NewNPC(SquidBossArena.Center.X * 16 + 232, SquidBossArena.Center.Y * 16 - 64, NPCType<ArenaActor>());

            //Keys
            foreach (Key key in Keys) key.Update();
        }

        public override void Initialize()
        {
            VitricBiome.X = 0;
            VitricBiome.Y = 0;

            SquidBossOpen = false;
            SquidBossDowned = false;

            DesertOpen = false;
            GlassBossOpen = false;
            GlassBossDowned = false;

            OvergrowBossDowned = false;
            OvergrowBossFree = false;
            OvergrowBossOpen = false;

            SealOpen = false;

            AluminumMeteors = false;

            TownUpgrades = new Dictionary<string, bool>();

            //Autoload NPC upgrades
            Mod mod = StarlightRiver.Instance;
            if (mod.Code != null)
            {
                foreach (Type type in mod.Code.GetTypes().Where(t => t.IsSubclassOf(typeof(TownUpgrade))))
                {
                    TownUpgrades.Add(type.Name.Replace("Upgrade", ""), false);
                }
            }

            PureTiles = new List<Vector2>();

            BookSP = Vector2.Zero;
            DashSP = Vector2.Zero;
        }

        public override TagCompound Save()
        {
            TagCompound tag = new TagCompound();
            foreach (KeyValuePair<string, bool> pair in TownUpgrades) tag.Add(pair.Key, pair.Value);

            return new TagCompound
            {
                ["VitricBiomePos"] = VitricBiome.TopLeft(),
                ["VitricBiomeSize"] = VitricBiome.Size(),

                ["SquidBossArenaPos"] = SquidBossArena.TopLeft(),
                ["SquidBossArenaSize"] = SquidBossArena.Size(),

                [nameof(SquidBossOpen)] = SquidBossOpen,
                [nameof(SquidBossDowned)] = SquidBossDowned,

                [nameof(DesertOpen)] = DesertOpen,
                [nameof(GlassBossOpen)] = GlassBossOpen,
                [nameof(GlassBossDowned)] = GlassBossDowned,

                [nameof(OvergrowBossOpen)] = OvergrowBossOpen,
                [nameof(OvergrowBossFree)] = OvergrowBossFree,
                [nameof(OvergrowBossDowned)] = OvergrowBossDowned,

                [nameof(SealOpen)] = SealOpen,

                [nameof(AluminumMeteors)] = AluminumMeteors,

                [nameof(TownUpgrades)] = tag,

                [nameof(PureTiles)] = PureTiles,

                [nameof(BookSP)] = BookSP,
                [nameof(DashSP)] = DashSP,

                [nameof(RiftLocation)] = RiftLocation,

                ["Chungus"] = Chungus
            };
        }

        public override void Load(TagCompound tag)
        {
            VitricBiome.X = (int)tag.Get<Vector2>("VitricBiomePos").X;
            VitricBiome.Y = (int)tag.Get<Vector2>("VitricBiomePos").Y;
            VitricBiome.Width = (int)tag.Get<Vector2>("VitricBiomeSize").X;
            VitricBiome.Height = (int)tag.Get<Vector2>("VitricBiomeSize").Y;

            SquidBossArena.X = (int)tag.Get<Vector2>("SquidBossArenaPos").X;
            SquidBossArena.Y = (int)tag.Get<Vector2>("SquidBossArenaPos").Y;
            SquidBossArena.Width = (int)tag.Get<Vector2>("SquidBossArenaSize").X;
            SquidBossArena.Height = (int)tag.Get<Vector2>("SquidBossArenaSize").Y;

            SquidBossOpen = tag.GetBool(nameof(SquidBossOpen));
            SquidBossDowned = tag.GetBool(nameof(SquidBossDowned));

            DesertOpen = tag.GetBool(nameof(DesertOpen));
            GlassBossOpen = tag.GetBool(nameof(GlassBossOpen));
            GlassBossDowned = tag.GetBool(nameof(GlassBossDowned));

            OvergrowBossOpen = tag.GetBool(nameof(OvergrowBossOpen));
            OvergrowBossFree = tag.GetBool(nameof(OvergrowBossFree));
            OvergrowBossDowned = tag.GetBool(nameof(OvergrowBossDowned));

            SealOpen = tag.GetBool(nameof(SealOpen));

            AluminumMeteors = tag.GetBool(nameof(AluminumMeteors));

            TagCompound tag1 = tag.GetCompound(nameof(TownUpgrades));
            Dictionary<string, bool> targetDict = new Dictionary<string, bool>();

            foreach (KeyValuePair<string, object> pair in tag1)
                targetDict.Add(pair.Key, tag1.GetBool(pair.Key));

            TownUpgrades = targetDict;

            PureTiles = (List<Vector2>)tag.GetList<Vector2>(nameof(PureTiles));

            BookSP = tag.Get<Vector2>(nameof(BookSP));
            DashSP = tag.Get<Vector2>(nameof(DashSP));

            RiftLocation = tag.Get<Vector2>(nameof(RiftLocation));

            Chungus = Main.rand.NextFloat();

            for (int k = 0; k <= PureTiles.Count - 1; k++)
                for (int i = (int)PureTiles[k].X - 16; i <= (int)PureTiles[k].X + 16; i++)
                    for (int j = (int)PureTiles[k].Y - 16; j <= (int)PureTiles[k].Y + 16; j++)
                    {
                        Projectiles.Ability.Purifier.RevertTile(i, j);
                    }              

            PureTiles.Clear();
            PureTiles = new List<Vector2> { };

            foreach (Key key in KeyInventory)
            {
                GUI.KeyInventory.keys.Add(new GUI.KeyIcon(key, false));
            }
        }
    }
}