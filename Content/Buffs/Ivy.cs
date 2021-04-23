using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Buffs
{
    public class Ivy : ModBuff
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Buffs + name;
            return true;
        }

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
