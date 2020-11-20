using StarlightRiver.GUI;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Tiles
{
    public abstract class LootChest : ModTile
    {
        internal virtual List<Loot> GoldLootPool { get; }
        internal virtual List<Loot> SmallLootPool { get; }

        public virtual void SafeSetDefaults() { }

        public virtual bool CanOpen(Player player) => true;

        public override void SetDefaults()
        {
            SafeSetDefaults();
            minPick = int.MaxValue;
        }

        public override bool NewRightClick(int i, int j)
        {
            if (CanOpen(Main.LocalPlayer))
            {
                WorldGen.KillTile(i, j);
                Loot[] smallLoot = new Loot[5];

                List<Loot> types = Helper.RandomizeList<Loot>(SmallLootPool);
                for (int k = 0; k < 5; k++) smallLoot[k] = types[k];

                StarlightRiver.Instance.lootUI.SetItems(GoldLootPool[Main.rand.Next(GoldLootPool.Count)], smallLoot);
                LootUI.Visible = true;
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
