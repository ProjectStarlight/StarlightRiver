using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            string SquidBossInfo = "Drop Auroracle Bait into the prismatic waters  of the permafrost shrine.";
            bcl.Call("AddBoss", 1.6f, ModContent.NPCType<NPCs.Boss.SquidBoss.SquidBoss>(), this, "Auroracle", (Func<bool>)(() => StarlightWorld.SquidBossDowned), ModContent.ItemType<Items.Permafrost.SquidBossSpawn>(), SquidBossCollection, SquidBossLoot, SquidBossInfo, null, "StarlightRiver/NPCs/Boss/SquidBoss/BodyPreview");

            List<int> vitricMiniBossCollection = new List<int>();
            List<int> vitricMiniBossLoot = new List<int>();
            string vitricMiniBossInfo = "Talk to the glassweaver in the vitric desert.";
            bcl.Call("AddMiniBoss", 3.1f, ModContent.NPCType<NPCs.Miniboss.Glassweaver.GlassMiniboss>(), this, "Glassweaver", (Func<bool>)(() => false), null, vitricMiniBossCollection, vitricMiniBossLoot, vitricMiniBossInfo);

            List<int> vitricBossCollection = new List<int>();
            List<int> vitricBossLoot = new List<int>();
            string vitricBossInfo = "Use a Glass Idol at cerios' altar in the vitric desert, after breaking the crystal covering it.";
            bcl.Call("AddBoss", 3.9f, ModContent.NPCType<NPCs.Boss.VitricBoss.VitricBoss>(), this, "Ceiros", (Func<bool>)(() => StarlightWorld.GlassBossDowned), ModContent.ItemType<Items.Vitric.VitricOre>(), vitricBossCollection, vitricBossLoot, vitricBossInfo, null, "StarlightRiver/NPCs/Boss/VitricBoss/Preview");
        }
    }
}
