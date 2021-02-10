using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.NPCs
{
    public class DropHandler : GlobalNPC
    {
        public override void NPCLoot(NPC npc)
        {
            if (npc.type == mod.NPCType("JungleCorruptWasp"))
            {
                if (Main.rand.Next(2) == 0)
                {
                    Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, mod.ItemType("JungleCorruptSoul"));
                }
            }
        }
    }
}