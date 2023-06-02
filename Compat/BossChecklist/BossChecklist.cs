using StarlightRiver.Content.Items.Permafrost;
using System.Collections.Generic;

namespace StarlightRiver.Compat.BossChecklist
{
	public static class BossChecklistCalls
	{
		public static void CallBossChecklist()
		{
			if (ModLoader.TryGetMod("BossChecklist", out Mod bcl))
			{
				//Auroracle
				var SquidBossCollection = new List<int>();
				string SquidBossInfo = "Drop Auroracle Bait into the prismatic waters of the permafrost shrine.";
				bcl.Call("AddBoss", StarlightRiver.Instance, "Auroracle", ModContent.NPCType<Content.Bosses.SquidBoss.SquidBoss>(), 2.6f,
					() => StarlightWorld.HasFlag(WorldFlags.SquidBossDowned),
					() => true,
					SquidBossCollection, ModContent.ItemType<SquidBossSpawn>(), SquidBossInfo, "The permafrost cathedral falls silent.", AuroraclePortrait.DrawAuroraclePortrait);

				//Glassweaver
				var vitricMinibossCollection = new List<int>();
				string vitricMinibossInfo = "Challenge the glassweaver in his forge in the vitric desert.";
				bcl.Call("AddMiniBoss", StarlightRiver.Instance, "Glassweaver", ModContent.NPCType<Content.Bosses.GlassMiniboss.Glassweaver>(), 4.8999f,
					() => StarlightWorld.HasFlag(WorldFlags.DesertOpen),
					() => true,
					vitricMinibossCollection, ModContent.ItemType<Content.Items.Vitric.GlassIdol>(), vitricMinibossInfo, "The glassweaver claims victory.");

				//Ceiros
				var vitricBossCollection = new List<int>()
				{
					ModContent.ItemType<Content.Tiles.Trophies.CeirosTrophyItem>()
				};

				string vitricBossInfo = "Use a Glass Idol at cerios' atop the vitric temple, after breaking the crystal covering it.";
				bcl.Call("AddBoss", StarlightRiver.Instance, "Ceiros", ModContent.NPCType<Content.Bosses.VitricBoss.VitricBoss>(), 4.9f,
					() => StarlightWorld.HasFlag(WorldFlags.VitricBossDowned),
					() => true,
					vitricBossCollection, ModContent.ItemType<Content.Items.Vitric.GlassIdol>(), vitricBossInfo, "The vitric forge falls silent.", CeirosPortrait.DrawCeirosPortrait);

				//OG Boss
				var ogBossCollection = new List<int>();
				string ogBossInfo = "Implement";
				bcl.Call("AddBoss", StarlightRiver.Instance, "???", ModContent.NPCType<Content.NPCs.Overgrow.Crusher>(), 7f,
					() => StarlightWorld.HasFlag(WorldFlags.OvergrowBossDowned),
					() => true,
					ogBossCollection, ModContent.ItemType<Content.Items.Vitric.GlassIdol>(), ogBossInfo, "[PH]OG boss loss message");
			}
		}
	}
}