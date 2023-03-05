using StarlightRiver.Content.Items.Permafrost;
using StarlightRiver.Custom.BossChecklist;
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
				string SquidBossInfo = "$Mods.StarlightRiver.Custom.BossChecklist.SquidBossInfo";
				bcl.Call("AddBoss", StarlightRiver.Instance, "$Mods.StarlightRiver.Custom.BossChecklist.Auroracle", ModContent.NPCType<Content.Bosses.SquidBoss.SquidBoss>(), 2.6f,
					() => StarlightWorld.HasFlag(WorldFlags.SquidBossDowned),
					() => true,
					SquidBossCollection, ModContent.ItemType<SquidBossSpawn>(), SquidBossInfo, "$Mods.StarlightRiver.Custom.BossChecklist.SquidBossLossMessage", AuroraclePortrait.DrawAuroraclePortrait);

				//Glassweaver
				var vitricMinibossCollection = new List<int>();
				string vitricMinibossInfo = "$Mods.StarlightRiver.Custom.BossChecklist.vitricMinibossInfo";
				bcl.Call("AddMiniBoss", StarlightRiver.Instance, "$Mods.StarlightRiver.Custom.BossChecklist.Glassweaver", ModContent.NPCType<Content.Bosses.GlassMiniboss.Glassweaver>(), 4.8999f,
					() => StarlightWorld.HasFlag(WorldFlags.DesertOpen),
					() => true,
					vitricMinibossCollection, ModContent.ItemType<Content.Items.Vitric.GlassIdol>(), vitricMinibossInfo, "$Mods.StarlightRiver.Custom.BossChecklist.vitricMinibossLossMessage");

				//Ceiros
				var vitricBossCollection = new List<int>()
				{
					ModContent.ItemType<Content.Tiles.Trophies.CeirosTrophyItem>()
				};
				string vitricBossInfo = "$Mods.StarlightRiver.Custom.BossChecklist.vitricBossInfo";
				bcl.Call("AddBoss", StarlightRiver.Instance, "$Mods.StarlightRiver.Custom.BossChecklist.Ceiros", ModContent.NPCType<Content.Bosses.VitricBoss.VitricBoss>(), 4.9f,
					() => StarlightWorld.HasFlag(WorldFlags.VitricBossDowned),
					() => true,
					vitricBossCollection, ModContent.ItemType<Content.Items.Vitric.GlassIdol>(), vitricBossInfo, "$Mods.StarlightRiver.Custom.BossChecklist.vitricBossLossMessage", CeirosPortrait.DrawCeirosPortrait);

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
