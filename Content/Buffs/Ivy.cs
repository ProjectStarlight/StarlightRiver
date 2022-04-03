using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Buffs
{
	public class Ivy : ModBuff
    {
        public override string Texture => AssetDirectory.Buffs + "Ivy";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ivy");
            Description.SetDefault("It's rooted in you!");
            Main.debuff[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
        }

        public override void Update(NPC NPC, ref int buffIndex)
        {
            //NPC.GetGlobalNPC<DebuffHandler>().ivy = true;
        }
    }
}
