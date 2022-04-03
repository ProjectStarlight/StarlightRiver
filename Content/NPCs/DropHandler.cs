using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.NPCs
{
	public class DropHandler : GlobalNPC
    {
        public override void NPCLoot(NPC NPC)
        {
            if (NPC.type == Mod.NPCType("JungleCorruptWasp"))
            {
                if (Main.rand.Next(2) == 0)
                {
                    Item.NewItem((int)NPC.position.X, (int)NPC.position.Y, NPC.width, NPC.height, Mod.ItemType("JungleCorruptSoul"));
                }
            }
        }
    }
}