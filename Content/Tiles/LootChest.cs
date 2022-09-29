using StarlightRiver.Content.GUI;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Tiles
{
	public abstract class LootChest : ModTile
    {
        internal virtual List<Loot> GoldLootPool { get; }
        internal virtual List<Loot> SmallLootPool { get; }

        public virtual void SafeSetDefaults() { }

        public virtual bool CanOpen(Player Player) => true;

        public override void SetStaticDefaults()
        {
            SafeSetDefaults();
            MinPick = int.MaxValue;
        }

        public override bool RightClick(int i, int j)
        {
            if (CanOpen(Main.LocalPlayer))
            {
                Loot[] smallLoot = new Loot[5];

                List<Loot> types = Helper.RandomizeList(SmallLootPool);
                for (int k = 0; k < 5; k++) smallLoot[k] = types[k];

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
        public int Type;
        public int Count;
        public int Min;
        public int Max;

        public Loot(int ID, int count) { Type = ID; Count = count; Min = 0; Max = 0; }
        public Loot(int ID, int min, int max) { Type = ID; Min = min; Max = max; Count = 0; }

        public int GetCount() { return Count == 0 ? Main.rand.Next(Min, Max) : Count; }
    }
}
