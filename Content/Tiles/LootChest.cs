using StarlightRiver.Content.GUI;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles
{
	public abstract class LootChest : ModTile
	{
		internal virtual List<Loot> GoldLootPool { get; }
		internal virtual List<Loot> SmallLootPool { get; }

		public virtual void SafeSetDefaults() { }

		public virtual bool CanOpen(Player Player)
		{
			return true;
		}

		public override void SetStaticDefaults()
		{
			SafeSetDefaults();
			MinPick = int.MaxValue;
		}

		public override bool RightClick(int i, int j)
		{
			if (CanOpen(Main.LocalPlayer))
			{
				var smallLoot = new Loot[5];

				List<Loot> types = Helper.RandomizeList(SmallLootPool);

				for (int k = 0; k < 5; k++)
					smallLoot[k] = types[k];

				UILoader.GetUIState<LootUI>().SetItems(GoldLootPool[Main.rand.Next(GoldLootPool.Count)], smallLoot);
				UILoader.GetUIState<LootUI>().Visible = true;

				WorldGen.KillTile(i, j);
				NetMessage.SendTileSquare(Main.myPlayer, i, j, 2, 2, TileChangeType.None);

				return true;
			}

			return false;
		}
	}

	public struct Loot
	{
		public int type;
		public int count;
		public int min;
		public int max;

		public Loot(int ID, int count)
		{
			type = ID;
			this.count = count;
			min = 0;
			max = 0;
		}
		public Loot(int ID, int min, int max)
		{
			type = ID;
			this.min = min;
			this.max = max;
			count = 0;
		}

		public int GetCount() { return count == 0 ? Main.rand.Next(min, max) : count; }
	}
}
