using StarlightRiver.Content.GUI;
using StarlightRiver.Content.Tiles.UndergroundTemple;
using StarlightRiver.Core.Loaders.UILoading;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
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

		public virtual int HoverItemIcon => ModContent.ItemType<TempleChestPlacer>();

		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
		{
			return true;
		}

		public override void SetStaticDefaults()
		{
			TileID.Sets.HasOutlines[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;
			Main.tileSpelunker[Type] = true;
			Main.tileOreFinderPriority[Type] = 500;
			SafeSetDefaults();
			MinPick = int.MaxValue;
		}

		public override void MouseOver(int i, int j)
		{
			Player player = Main.LocalPlayer;

			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = HoverItemIcon;
		}

		public override bool RightClick(int i, int j)
		{
			if (CanOpen(Main.LocalPlayer) && !UILoader.GetUIState<LootUI>().Visible)//second check makes sure that you cant open the ui when its already open
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

		public override bool CanDrop(int i, int j)
		{
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