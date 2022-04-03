using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarlightRiver.Core;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Vitric
{
	class VitricNPCSpawner : PlayerTicker
	{
		public override bool Active(Player Player) => Player.GetModPlayer<BiomeHandler>().ZoneGlass;

		public override int TickFrequency => 30;

		public override void Tick(Player Player)
		{
			if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient)
				return; //only spawn for singlePlayer or on the server

			if (Main.rand.Next(4) == 0 && Main.npc.Count(n => n.active && n.type == NPCType<MagmitePassive>()) < 5)
			{
				Point16 coords = Helpers.Helper.FindTile(((Player.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(200, 500)) / 16).ToPoint16(), n => !n.active() && n.liquidType() == 1 && n.liquid > 0, 10, 2, 2);
				if (coords != Point16.Zero)
					NPC.NewNPC(coords.X * 16, coords.Y * 16, NPCType<MagmitePassive>(), 0, -1);
			}
		}
	}
}
