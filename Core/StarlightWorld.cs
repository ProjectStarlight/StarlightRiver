using StarlightRiver.Content.Bosses.SquidBoss;
using StarlightRiver.Content.GUI;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using System.IO;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Core
{
	public partial class StarlightWorld : ModSystem
	{
		public static StarlightWorld worldInstance;

		private static WorldFlags flags;

		public static float visualTimer;

		public static Rectangle vitricBiome = new();

		public static int squidNPCProgress = 0;
		public static Rectangle squidBossArena = new();

		public static Rectangle VitricBossArena => new(vitricBiome.X + vitricBiome.Width / 2 - 59, vitricBiome.Y - 1, 108, 74); //ceiros arena

		private static Vector2 GlassweaverArenaPos => vitricBiome.TopLeft() * 16 + new Vector2(0, 80 * 16) + new Vector2(0, 256);
		public static Rectangle GlassweaverArena => new((int)GlassweaverArenaPos.X - 35 * 16, (int)GlassweaverArenaPos.Y - 30 * 16, 70 * 16, 30 * 16);

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
		}

		public override void NetReceive(BinaryReader reader)
		{
			flags = (WorldFlags)reader.ReadInt32();

			vitricBiome = ReadRectangle(reader);
			squidBossArena = ReadRectangle(reader);
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

		public override void PreUpdateWorld()
		{
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
			RichTextBox.CloseDialogue(); //Safeguard

			vitricBiome.X = 0;
			vitricBiome.Y = 0;

			flags = default;
		}

		public override void SaveWorldData(TagCompound tag)
		{
			tag["VitricBiomePos"] = vitricBiome.TopLeft();
			tag["VitricBiomeSize"] = vitricBiome.Size();

			tag["SquidNPCProgress"] = squidNPCProgress;
			tag["SquidBossArenaPos"] = squidBossArena.TopLeft();
			tag["SquidBossArenaSize"] = squidBossArena.Size();
			tag["PermafrostCenter"] = permafrostCenter;

			tag[nameof(flags)] = (int)flags;
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

			Content.Physics.VerletChainSystem.toDraw.Clear();

			DummyTile.dummies.Clear();
		}

		public override void Unload()
		{
			genNoise = null;
		}
	}
}