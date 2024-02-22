using StarlightRiver.Content.Biomes;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Vitric
{
	class VitricNPCSpawner : PlayerTicker
	{
		public override int TickFrequency => 30;

		public override bool Active(Player Player)
		{
			return Player.InModBiome(GetInstance<VitricDesertBiome>());
		}

		public override void Tick(Player Player)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return; //only spawn for singlePlayer or on the server

			if (Main.npc.Any(n => n.active && n.boss))
				return; //No magmites during bosses

			if (Main.rand.NextBool(4) && Main.npc.Count(n => n.active && n.type == NPCType<MagmitePassive>()) < 5)
			{
				Point16 coords = Helpers.Helper.FindTile(((Player.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(200, 500)) / 16).ToPoint16(), n => !n.HasTile && n.LiquidType == LiquidID.Lava && n.LiquidAmount > 0, 10, 2, 2);

				if (coords != Point16.Zero)
					NPC.NewNPC(NPC.GetSource_NaturalSpawn(), coords.X * 16, coords.Y * 16, NPCType<MagmitePassive>(), 0, -1);
			}

			if (Main.rand.NextBool(4) && Main.npc.Count(n => n.active && n.type == NPCType<MagmiteSmol>()) < 3)
			{
				Point16 coords = Helpers.Helper.FindTile(((Player.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(200, 500)) / 16).ToPoint16(), n => !n.HasTile && n.LiquidType == LiquidID.Lava && n.LiquidAmount > 0, 10, 2, 2);

				if (coords != Point16.Zero)
					NPC.NewNPC(NPC.GetSource_NaturalSpawn(), coords.X * 16, coords.Y * 16, NPCType<MagmiteSmol>(), 0, -1);
			}

			if (Main.rand.NextBool(4) && Main.npc.Count(n => n.active && n.type == NPCType<MagmiteLarge>()) < 1)
			{
				Point16 coords = Helpers.Helper.FindTile(((Player.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(200, 500)) / 16).ToPoint16(), n => !n.HasTile && n.LiquidType == LiquidID.Lava && n.LiquidAmount > 0, 10, 2, 2);

				if (coords != Point16.Zero)
					NPC.NewNPC(NPC.GetSource_NaturalSpawn(), coords.X * 16, coords.Y * 16, NPCType<MagmiteLarge>(), 0, -1);
			}
		}
	}
}