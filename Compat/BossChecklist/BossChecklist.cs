using StarlightRiver.Content.Items.Permafrost;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace StarlightRiver.Compat.BossChecklist
{
    public static class BossChecklistCalls
    {
        public static void CallBossChecklist()
        {
            if (ModLoader.TryGetMod("BossChecklist", out Mod bcl))
            {
                //Auroracle
                List<int> SquidBossCollection = new List<int>();
                string SquidBossInfo = "Drop Auroracle Bait into the prismatic waters of the permafrost shrine.";
                bcl.Call("AddBoss", StarlightRiver.Instance, "Auroracle", ModContent.NPCType<Content.Bosses.SquidBoss.SquidBoss>(), 2.6f, 
                    () => StarlightWorld.HasFlag(WorldFlags.SquidBossDowned), 
                    () => true,
                    SquidBossCollection, ModContent.ItemType<SquidBossSpawn>(), SquidBossInfo, "The permafrost cathedral falls silent.");

                //Ceiros
                List<int> vitricBossCollection = new List<int>()
                {
                    ModContent.ItemType<Content.Tiles.Trophies.CeirosTrophyItem>()
                };

                string vitricBossInfo = "Use a Glass Idol at cerios' atop the vitric temple, after breaking the crystal covering it.";
                bcl.Call("AddBoss", StarlightRiver.Instance, "Ceiros", ModContent.NPCType<Content.Bosses.VitricBoss.VitricBoss>(), 4.9f,
                    () => StarlightWorld.HasFlag(WorldFlags.VitricBossDowned),
                    () => StarlightWorld.HasFlag(WorldFlags.DesertOpen),
                    vitricBossCollection, ModContent.ItemType<Content.Items.Vitric.GlassIdol>(), vitricBossInfo, "The vitric forge falls silent.", CeirosPortrait.DrawCeirosPortrait);
            }
        }
    }
}
