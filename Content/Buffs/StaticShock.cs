using System;
using Terraria.ID;

namespace StarlightRiver.Content.Buffs
{
	public class StaticShock : SmartBuff
	{
		public override string Texture => AssetDirectory.Buffs + "StaticShock";

		public StaticShock() : base("Static Shock", "Lowered Defense", true) { }

		public override void Update(NPC npc, ref int buffIndex)
		{
			npc.defense -= 4 + Math.Min((int)(npc.buffTime[buffIndex] * 0.1f), 5);
			if (Main.rand.NextBool(9))
			{
				Vector2 around = npc.Center + Main.rand.NextVector2Circular(npc.width, npc.height) * 1.1f;
				var d = Dust.NewDustPerfect(around, DustID.Electric, around.DirectionFrom(npc.Center), 0, Color.GhostWhite, 0.5f);
				d.noGravity = true;
			}
		}

		public override void Update(Player player, ref int buffIndex)
		{
			player.statDefense -= 4 + Math.Min(player.buffTime[buffIndex] * 1, 5);
			if (Main.rand.NextBool(9))
			{
				Vector2 around = player.Center + Main.rand.NextVector2Circular(player.width, player.height) * 1.1f;
				var d = Dust.NewDustPerfect(around, DustID.Electric, around.DirectionFrom(player.Center), 0, Color.GhostWhite, 0.5f);
				d.noGravity = true;
			}
		}
	}
}