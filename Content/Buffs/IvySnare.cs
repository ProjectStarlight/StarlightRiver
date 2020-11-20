using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Buffs
{
    public class IvySnare : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Snared");
            Description.SetDefault("You've been caught!");
            Main.debuff[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            //npc.GetGlobalNPC<DebuffHandler>().snared = true;
        }
    }
}
