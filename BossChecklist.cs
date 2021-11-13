using StarlightRiver.Content.Items.Permafrost;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace StarlightRiver
{
	public partial class StarlightRiver : Mod
    {
        private void CallBossChecklist()
        {
            Mod bcl = ModLoader.GetMod("BossChecklist");
            if (bcl == null) return;

            List<int> SquidBossCollection = new List<int>();
            List<int> SquidBossLoot = new List<int>();
            string SquidBossInfo = "Drop Auroracle Bait into the prismatic waters of the permafrost shrine.";
            bcl.Call("AddBoss", 2.6f, ModContent.NPCType<Content.Bosses.SquidBoss.SquidBoss>(), this, "Auroracle", (Func<bool>)(() => StarlightWorld.HasFlag(WorldFlags.SquidBossDowned)), ModContent.ItemType<SquidBossSpawn>(), SquidBossCollection, SquidBossLoot, SquidBossInfo, null, "StarlightRiver/Assets/BossChecklist/SquidBoss");

            List<int> vitricMiniBossCollection = new List<int>();
            List<int> vitricMiniBossLoot = new List<int>();
            string vitricMiniBossInfo = "Talk to the glassweaver in the vitric desert.";
            bcl.Call("AddMiniBoss", 4.1f, ModContent.NPCType<Content.Bosses.GlassMiniboss.GlassMiniboss>(), this, "Glassweaver", (Func<bool>)(() => StarlightWorld.HasFlag(WorldFlags.DesertOpen)), null, vitricMiniBossCollection, vitricMiniBossLoot, vitricMiniBossInfo);

            List<int> vitricBossCollection = new List<int>()
            {
                ModContent.ItemType<Content.Tiles.Trophies.CeirosTrophyItem>(),
            };
            List<int> vitricBossLoot = new List<int>()
            {
                ModContent.ItemType<Content.Items.Vitric.VitricBossBag>(),
                ModContent.ItemType<Content.Items.Vitric.BossSpear>(),
                ModContent.ItemType<Content.Items.Vitric.RefractiveBlade>(),
                ModContent.ItemType<Content.Items.Vitric.VitricBossBow>(),
                ModContent.ItemType<Content.Items.Vitric.Needler>(),
                ModContent.ItemType<Content.Items.Misc.StaminaUp>(),
                ModContent.ItemType<Content.Items.Vitric.CeirosExpert>()
            };
            string vitricBossInfo = "Use a Glass Idol at cerios' atop the vitric temple, after breaking the crystal covering it.";
            bcl.Call("AddBoss", 4.9f, ModContent.NPCType<Content.Bosses.VitricBoss.VitricBoss>(), this, "Ceiros", (Func<bool>)(() => StarlightWorld.HasFlag(WorldFlags.VitricBossDowned)), ModContent.ItemType<Content.Items.Vitric.GlassIdol>(), vitricBossCollection, vitricBossLoot, vitricBossInfo, null, "StarlightRiver/Assets/BossChecklist/VitricBoss");
        }
    }
}
