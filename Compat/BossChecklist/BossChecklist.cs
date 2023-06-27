using StarlightRiver.Content.Items.Permafrost;
using System.Collections.Generic;

namespace StarlightRiver.Compat.BossChecklist
{
	public static class BossChecklistCalls
	{
		public static void CallBossChecklist()
		{
#pragma warning disable CS8974 // Converting method group to non-delegate type
			if (ModLoader.TryGetMod("BossChecklist", out Mod bcl))
			{
				//Auroracle
				var SquidBossCollection = new List<int>();
				bcl.Call(
					"LogBoss",
					StarlightRiver.Instance,
					"Auroracle",
					2.6f,
					() => StarlightWorld.HasFlag(WorldFlags.SquidBossDowned),
					ModContent.NPCType<Content.Bosses.SquidBoss.SquidBoss>(),
					new Dictionary<string, object>()
					{
						["spawnInfo"] = LocalizationRoundabout.DefaultText("Compat.BossChecklist.Auroracle.SpawnInfo", "Drop a Suspicious Looking Offering into the prismatic waters of the Permafrost Shrine, accessed only through the coldest caves."),
						["despawnMessage"] = LocalizationRoundabout.DefaultText("Compat.BossChecklist.Auroracle.Despawn", "The frozen cathedral falls silent."),
						["spawnItems"] = ModContent.ItemType<SquidBossSpawn>(),
						["customPortrait"] = AuroraclePortrait.DrawAuroraclePortrait,
						["collectibles"] = SquidBossCollection
					});

				//Glassweaver
				var vitricMinibossCollection = new List<int>();
				bcl.Call(
					"LogBoss",
					StarlightRiver.Instance,
					"Glassweaver",
					4.8999f,
					() => StarlightWorld.HasFlag(WorldFlags.GlassweaverDowned),
					ModContent.NPCType<Content.Bosses.GlassMiniboss.Glassweaver>(),
					new Dictionary<string, object>()
					{
						["spawnInfo"] = LocalizationRoundabout.DefaultText("Compat.BossChecklist.Glassweaver.SpawnInfo", "Challenge the glassweaver in his forge, near the edge of the Vitric Desert."),
						["despawnMessage"] = LocalizationRoundabout.DefaultText("Compat.BossChecklist.Glassweaver.Despawn", "The Glassweaver claims victory."),
						["collectibles"] = vitricMinibossCollection
					});

				//Ceiros
				var vitricBossCollection = new List<int>()
				{
					ModContent.ItemType<Content.Tiles.Trophies.CeirosTrophyItem>()
				};

				bcl.Call(
					"LogBoss",
					StarlightRiver.Instance,
					"Ceiros",
					4.9f,
					() => StarlightWorld.HasFlag(WorldFlags.VitricBossDowned),
					ModContent.NPCType<Content.Bosses.VitricBoss.VitricBoss>(),
					new Dictionary<string, object>()
					{
						["spawnInfo"] = LocalizationRoundabout.DefaultText("Compat.BossChecklist.Ceiros.SpawnInfo", "Use a Glass Idol at the altar atop the Vitric Temple, after breaking the crystal covering it using Forbidden Winds."),
						["despawnMessage"] = LocalizationRoundabout.DefaultText("Compat.BossChecklist.Ceiros.Despawn", "The Vitric Forge falls silent. The Glassweaver is dissapointed."),
						["spawnItems"] = ModContent.ItemType<Content.Items.Vitric.GlassIdol>(),
						["customPortrait"] = CeirosPortrait.DrawCeirosPortrait,
						["collectibles"] = vitricBossCollection
					});
			}
#pragma warning restore CS8974 // Converting method group to non-delegate type
		}
	}
}