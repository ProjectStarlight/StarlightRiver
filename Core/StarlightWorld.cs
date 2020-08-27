using Microsoft.Xna.Framework;
using StarlightRiver.Keys;
using StarlightRiver.NPCs.Boss.SquidBoss;
using StarlightRiver.NPCs.TownUpgrade;
using System;
using System.Collections.Generic;
using System.IO;
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
        private static WorldFlags flags;
        
        public static Vector2 RiftLocation;

        public static float rottime;

        public static float Chungus;

        //Voidsmith
        public static Dictionary<string, bool> TownUpgrades = new Dictionary<string, bool>();

        public static List<Vector2> PureTiles = new List<Vector2> { };

        public static Rectangle VitricBiome = new Rectangle();

        public static Rectangle SquidBossArena = new Rectangle();

        //Handling Keys
        public static List<Key> Keys = new List<Key>();

        public static List<Key> KeyInventory = new List<Key>();

        public static bool HasFlag(WorldFlags flag) => (flags & flag) != 0;
        public static void Flag(WorldFlags flag) => flags |= flag;

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write((int)flags);

            WriteRectangle(writer, VitricBiome);
            WriteRectangle(writer, SquidBossArena);       
        }

        public override void NetReceive(BinaryReader reader)
        {
            flags = (WorldFlags)reader.ReadInt32();

            VitricBiome = ReadRectangle(reader);
            SquidBossArena = ReadRectangle(reader);
        }

        private void WriteRectangle(BinaryWriter writer, Rectangle rect)
        {
            writer.Write(rect.X);
            writer.Write(rect.Y);
            writer.Write(rect.Width);
            writer.Write(rect.Height);
        }

        private Rectangle ReadRectangle(BinaryReader reader) => new Rectangle(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());

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

            flags = default;

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
        }

        public override TagCompound Save()
        {
            TagCompound tag = new TagCompound();
            foreach (var pair in TownUpgrades)
                tag.Add(pair.Key, pair.Value);

            // TODO why the hell is this throwing Collection was modified?
            while (true)
                try
                {
                    return new TagCompound
                    {
                        ["VitricBiomePos"] = VitricBiome.TopLeft(),
                        ["VitricBiomeSize"] = VitricBiome.Size(),

                        ["SquidBossArenaPos"] = SquidBossArena.TopLeft(),
                        ["SquidBossArenaSize"] = SquidBossArena.Size(),

                        [nameof(flags)] = (int)flags,

                        [nameof(TownUpgrades)] = tag,

                        [nameof(PureTiles)] = PureTiles,

                        [nameof(RiftLocation)] = RiftLocation,

                        ["Chungus"] = Chungus
                    };
                }
                catch { }
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

            flags = (WorldFlags)tag.GetInt(nameof(flags));

            TagCompound tag1 = tag.GetCompound(nameof(TownUpgrades));
            Dictionary<string, bool> targetDict = new Dictionary<string, bool>();

            foreach (KeyValuePair<string, object> pair in tag1)
                targetDict.Add(pair.Key, tag1.GetBool(pair.Key));

            TownUpgrades = targetDict;

            PureTiles = (List<Vector2>)tag.GetList<Vector2>(nameof(PureTiles));

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