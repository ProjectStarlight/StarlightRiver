using StarlightRiver.Content.NPCs.Misc;
using Terraria.DataStructures;

namespace StarlightRiver.Core
{
	public partial class StarlightWorld : ModSystem
	{
		private void LootWraithGen()
		{
			foreach (Chest chest in Main.chest)
			{
				if (chest != null && WorldGen.genRand.NextBool(6))
				{
					int frameX = Framing.GetTileSafely(chest.x, chest.y).TileFrameX / 36;
					if (frameX == 11 || frameX == 1) //Golden or Frozen Chests
					{
						var npc = NPC.NewNPCDirect(new EntitySource_Misc("Loot Wraith"), chest.x * 16, chest.y * 16, ModContent.NPCType<LootWraith>());
						if (npc.ModNPC is LootWraith wraith)
						{
							wraith.xTile = chest.x;
							wraith.yTile = chest.y;
						}
					}
				}
			}
		}
	}
}