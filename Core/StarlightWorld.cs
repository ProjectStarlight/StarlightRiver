﻿using Microsoft.Xna.Framework;
using StarlightRiver.Content.Bosses.SquidBoss;
using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Content.Tiles.Permafrost;
using StarlightRiver.Keys;
using StarlightRiver.NPCs.TownUpgrade;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.DataStructures;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework.Graphics;

namespace StarlightRiver.Core
{
	//Larger scale TODO: This is slowly becoming a godclass, we should really do something about that
	public partial class StarlightWorld : ModSystem
    {
        private static WorldFlags flags;

        public static Vector2 RiftLocation;

        public static float rottime;

        public static float Chungus;

        public static Cutaway cathedralOverlay;

        public static int Timer; //I dont know why this is here and really dont want to risk removing it at this point.

        //Voidsmith
        public static Dictionary<string, bool> TownUpgrades = new Dictionary<string, bool>();

        public static Rectangle VitricBiome = new Rectangle();

        public static int SquidNPCProgress = 0;
        public static Rectangle SquidBossArena = new Rectangle();

        public static Rectangle VitricBossArena => new Rectangle(VitricBiome.X + VitricBiome.Width / 2 - 59, VitricBiome.Y - 1, 108, 74); //ceiros arena

        public static StarlightWorld Instance;

        public StarlightWorld()
		{
            Instance = this;
		}

        public static bool HasFlag(WorldFlags flag) => (flags & flag) != 0;

        public static void Flag(WorldFlags flag)
        {
            flags |= flag;
            NetMessage.SendData(MessageID.WorldData);
        }

        public static void FlipFlag(WorldFlags flag)
        {
            flags ^= flag;
            NetMessage.SendData(MessageID.WorldData);
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write((int)flags);

            WriteRectangle(writer, VitricBiome);
            WriteRectangle(writer, SquidBossArena);

            WriteNPCUpgrades(writer);
        }

        public override void NetReceive(BinaryReader reader)
        {
            flags = (WorldFlags)reader.ReadInt32();

            VitricBiome = ReadRectangle(reader);
            SquidBossArena = ReadRectangle(reader);

            if(CutawayHandler.cutaways.Count == 0)
                CreateCutaways();

            ReadNPCUpgrades(reader);
        }

        private void WriteRectangle(BinaryWriter writer, Rectangle rect)
        {
            writer.Write(rect.X);
            writer.Write(rect.Y);
            writer.Write(rect.Width);
            writer.Write(rect.Height);
        }

        private Rectangle ReadRectangle(BinaryReader reader) => new Rectangle(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());

        private void WriteNPCUpgrades(BinaryWriter writer)
		{
            foreach (KeyValuePair<string, bool> upgrade in TownUpgrades)
            {
                writer.Write(upgrade.Key);
                writer.Write(upgrade.Value);
            }
		}

        private void ReadNPCUpgrades(BinaryReader reader)
		{
            for (int i = 0; i <  TownUpgrades.Count(); i++)
                TownUpgrades[reader.ReadString()] = reader.ReadBoolean();
        }

		public override void PreUpdateWorld()
		{
            Timer++;
            rottime += (float)Math.PI / 60;
            if (rottime >= Math.PI * 2) rottime = 0;
        }

        public override void PostUpdateWorld()
        {
            //SquidBoss arena
            if (!Main.npc.Any(n => n.active && n.type == NPCType<ArenaActor>()))
                NPC.NewNPC(new EntitySource_WorldEvent(), SquidBossArena.Center.X * 16 + 8, SquidBossArena.Center.Y * 16 + 56 * 16, NPCType<ArenaActor>());
        }

		public override void OnWorldLoad()
		{
            VitricBiome.X = 0;
            VitricBiome.Y = 0;

            flags = default;

            TownUpgrades = new Dictionary<string, bool>();         

            //Autoload NPC upgrades
            Mod Mod = StarlightRiver.Instance;
            if (Mod.Code != null)
            {
                foreach (Type type in Mod.Code.GetTypes().Where(t => t.IsSubclassOf(typeof(TownUpgrade))))
                {
                    TownUpgrades.Add(type.Name.Replace("Upgrade", ""), false);
                }
            }
        }

        public override void SaveWorldData(TagCompound tag)
        {
            TagCompound townTag = new TagCompound();
            foreach (var pair in TownUpgrades)
                townTag.Add(pair.Key, pair.Value);

            // TODO why the hell is this throwing Collection was modified?

            tag["VitricBiomePos"] = VitricBiome.TopLeft();
            tag["VitricBiomeSize"] = VitricBiome.Size();

            tag["SquidNPCProgress"] = SquidNPCProgress;
            tag["SquidBossArenaPos"] = SquidBossArena.TopLeft();
            tag["SquidBossArenaSize"] = SquidBossArena.Size();
            tag["PermafrostCenter"] = permafrostCenter;

            tag[nameof(flags)] = (int)flags;

            tag[nameof(TownUpgrades)] = townTag;

            tag[nameof(RiftLocation)] = RiftLocation;

            tag["Chungus"] = Chungus;
        }

        private static bool CheckForSquidArena(Player Player)
		{
            if (WorldGen.InWorld((int)Main.LocalPlayer.Center.X / 16, (int)Main.LocalPlayer.Center.Y / 16))
            {
                Tile tile = Framing.GetTileSafely((int)Main.LocalPlayer.Center.X / 16, (int)Main.LocalPlayer.Center.Y / 16);

                if (tile != null)
                {
                    return
                        tile.WallType == WallType<AuroraBrickWall>() &&
                        !Main.LocalPlayer.GetModPlayer<StarlightPlayer>().trueInvisible;
                }
            }

            return false;
        }

        public static void CreateCutaways()
		{
            //TODO: Create new overlay for this when the structure is done
            /*var templeCutaway = new Cutaway(Request<Texture2D>("StarlightRiver/Assets/Backgrounds/TempleCutaway").Value, new Vector2(VitricBiome.Center.X - 47, VitricBiome.Center.Y + 5) * 16);
            templeCutaway.inside = n => n.InModBiome(ModContent.GetInstance<VitricTempleBiome>());
            CutawayHandler.NewCutaway(templeCutaway);*/

            cathedralOverlay = new Cutaway(Request<Texture2D>("StarlightRiver/Assets/Bosses/SquidBoss/CathedralOver", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value, SquidBossArena.TopLeft() * 16);
            cathedralOverlay.inside = CheckForSquidArena;
            CutawayHandler.NewCutaway(cathedralOverlay);
        }

        public override void LoadWorldData(TagCompound tag)
        {
            VitricBiome.X = (int)tag.Get<Vector2>("VitricBiomePos").X;
            VitricBiome.Y = (int)tag.Get<Vector2>("VitricBiomePos").Y;
            VitricBiome.Width = (int)tag.Get<Vector2>("VitricBiomeSize").X;
            VitricBiome.Height = (int)tag.Get<Vector2>("VitricBiomeSize").Y;

            SquidNPCProgress = tag.GetInt("SquidNPCProgress");
            SquidBossArena.X = (int)tag.Get<Vector2>("SquidBossArenaPos").X;
            SquidBossArena.Y = (int)tag.Get<Vector2>("SquidBossArenaPos").Y;
            SquidBossArena.Width = (int)tag.Get<Vector2>("SquidBossArenaSize").X;
            SquidBossArena.Height = (int)tag.Get<Vector2>("SquidBossArenaSize").Y;
            permafrostCenter = tag.GetInt("PermafrostCenter");

            flags = (WorldFlags)tag.GetInt(nameof(flags));

            TagCompound tag1 = tag.GetCompound(nameof(TownUpgrades));
            Dictionary<string, bool> targetDict = new Dictionary<string, bool>();

            foreach (KeyValuePair<string, object> pair in tag1)
                targetDict.Add(pair.Key, tag1.GetBool(pair.Key));

            TownUpgrades = targetDict;

            RiftLocation = tag.Get<Vector2>(nameof(RiftLocation));

            Chungus = tag.GetFloat("Chungus");

            Chungus += Main.rand.NextFloat(-0.005f, 0.01f);
            Chungus = MathHelper.Clamp(Chungus, 0, 1);

            //setup overlays
            if (Main.netMode == NetmodeID.SinglePlayer)
                CreateCutaways();

            Physics.VerletChainSystem.toDraw.Clear();

            DummyTile.dummies.Clear();
        }

		public override void Unload()
		{
            cathedralOverlay = null;
            TownUpgrades = null;
            genNoise = null;
		}
	}
}