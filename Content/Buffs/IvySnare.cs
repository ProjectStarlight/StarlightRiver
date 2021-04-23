using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Buffs
{
    public class IvySnare : ModBuff
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Buffs + name;
            return true;
        }

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
