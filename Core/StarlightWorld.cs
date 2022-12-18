using StarlightRiver.Content.Bosses.SquidBoss;
using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Content.NPCs.TownUpgrade;
using StarlightRiver.Content.Tiles.Permafrost;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Core
{
	//Larger scale TODO: This is slowly becoming a godclass, we should really do something about that
	public partial class StarlightWorld : ModSystem
	{
		public static StarlightWorld worldInstance;

		private static WorldFlags flags;

		public static float visualTimer;

		public static float Chungus;

		public static Cutaway cathedralOverlay;

		public static int timer; //I dont know why this is here and really dont want to risk removing it at this point.

		//Voidsmith
		public static Dictionary<string, bool> townUpgrades = new();

		public static Rectangle vitricBiome = new();

		public static int squidNPCProgress = 0;
		public static Rectangle squidBossArena = new();

		public static Rectangle VitricBossArena => new(vitricBiome.X + vitricBiome.Width / 2 - 59, vitricBiome.Y - 1, 108, 74); //ceiros arena

		public StarlightWorld()
		{
			worldInstance = this;
		}

		public static bool HasFlag(WorldFlags flag)
		{
			return (flags & flag) != 0;
		}

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

			WriteRectangle(writer, vitricBiome);
			WriteRectangle(writer, squidBossArena);

			WriteNPCUpgrades(writer);
		}

		public override void NetReceive(BinaryReader reader)
		{
			flags = (WorldFlags)reader.ReadInt32();

			vitricBiome = ReadRectangle(reader);
			squidBossArena = ReadRectangle(reader);

			if (CutawayHandler.cutaways.Count == 0)
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

		private Rectangle ReadRectangle(BinaryReader reader)
		{
			return new Rectangle(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
		}

		private void WriteNPCUpgrades(BinaryWriter writer)
		{
			foreach (KeyValuePair<string, bool> upgrade in townUpgrades)
			{
				writer.Write(upgrade.Key);
				writer.Write(upgrade.Value);
			}
		}

		private void ReadNPCUpgrades(BinaryReader reader)
		{
			for (int i = 0; i < townUpgrades.Count(); i++)
				townUpgrades[reader.ReadString()] = reader.ReadBoolean();
		}

		public override void PreUpdateWorld()
		{
			timer++;
			visualTimer += (float)Math.PI / 60;

			if (visualTimer >= Math.PI * 2)
				visualTimer = 0;
		}

		public override void PostUpdateWorld()
		{
			//SquidBoss arena
			if (!Main.npc.Any(n => n.active && n.type == NPCType<ArenaActor>()))
				NPC.NewNPC(new EntitySource_WorldEvent(), squidBossArena.Center.X * 16 + 8, squidBossArena.Center.Y * 16 + 56 * 16, NPCType<ArenaActor>());
		}

		public override void OnWorldLoad()
		{
			vitricBiome.X = 0;
			vitricBiome.Y = 0;

			flags = default;

			townUpgrades = new Dictionary<string, bool>();

			//Autoload NPC upgrades
			Mod Mod = StarlightRiver.Instance;
			if (Mod.Code != null)
			{
				foreach (Type type in Mod.Code.GetTypes().Where(t => t.IsSubclassOf(typeof(TownUpgrade))))
				{
					townUpgrades.Add(type.Name.Replace("Upgrade", ""), false);
				}
			}
		}

		public override void SaveWorldData(TagCompound tag)
		{
			var townTag = new TagCompound();
			foreach (KeyValuePair<string, bool> pair in townUpgrades)
				townTag.Add(pair.Key, pair.Value);

			// TODO why the hell is this throwing Collection was modified?

			tag["VitricBiomePos"] = vitricBiome.TopLeft();
			tag["VitricBiomeSize"] = vitricBiome.Size();

			tag["SquidNPCProgress"] = squidNPCProgress;
			tag["SquidBossArenaPos"] = squidBossArena.TopLeft();
			tag["SquidBossArenaSize"] = squidBossArena.Size();
			tag["PermafrostCenter"] = permafrostCenter;

			tag[nameof(flags)] = (int)flags;

			tag[nameof(townUpgrades)] = townTag;

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

			cathedralOverlay = new Cutaway(Request<Texture2D>("StarlightRiver/Assets/Bosses/SquidBoss/CathedralOver", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value, squidBossArena.TopLeft() * 16)
			{
				Inside = CheckForSquidArena
			};
			CutawayHandler.NewCutaway(cathedralOverlay);
		}

		public override void LoadWorldData(TagCompound tag)
		{
			vitricBiome.X = (int)tag.Get<Vector2>("VitricBiomePos").X;
			vitricBiome.Y = (int)tag.Get<Vector2>("VitricBiomePos").Y;
			vitricBiome.Width = (int)tag.Get<Vector2>("VitricBiomeSize").X;
			vitricBiome.Height = (int)tag.Get<Vector2>("VitricBiomeSize").Y;

			squidNPCProgress = tag.GetInt("SquidNPCProgress");
			squidBossArena.X = (int)tag.Get<Vector2>("SquidBossArenaPos").X;
			squidBossArena.Y = (int)tag.Get<Vector2>("SquidBossArenaPos").Y;
			squidBossArena.Width = (int)tag.Get<Vector2>("SquidBossArenaSize").X;
			squidBossArena.Height = (int)tag.Get<Vector2>("SquidBossArenaSize").Y;
			permafrostCenter = tag.GetInt("PermafrostCenter");

			flags = (WorldFlags)tag.GetInt(nameof(flags));

			TagCompound tag1 = tag.GetCompound(nameof(townUpgrades));
			var targetDict = new Dictionary<string, bool>();

			foreach (KeyValuePair<string, object> pair in tag1)
				targetDict.Add(pair.Key, tag1.GetBool(pair.Key));

			townUpgrades = targetDict;

			Chungus = tag.GetFloat("Chungus");

			Chungus += Main.rand.NextFloat(-0.005f, 0.01f);
			Chungus = MathHelper.Clamp(Chungus, 0, 1);

			//setup overlays
			if (Main.netMode == NetmodeID.SinglePlayer)
				CreateCutaways();

			Content.Physics.VerletChainSystem.toDraw.Clear();

			DummyTile.dummies.Clear();
		}

		public override void Unload()
		{
			cathedralOverlay = null;
			townUpgrades = null;
			genNoise = null;
		}
	}
}