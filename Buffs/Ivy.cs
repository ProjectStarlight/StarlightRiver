using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Buffs
{
    public class Ivy : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Ivy");
            Description.SetDefault("It's rooted in you!");
            Main.debuff[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            //npc.GetGlobalNPC<DebuffHandler>().ivy = true;
        }
    }
}
