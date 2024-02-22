using Terraria.ID;

namespace StarlightRiver.Content.Items.SteampunkSet
{
	public class AncientGear : QuickMaterial
	{
		public AncientGear() : base("Ancient Gear", "", 999, 200, 2, AssetDirectory.SteampunkItem) { }

		public override void Load()
		{
			StarlightItem.ExtractinatorUseEvent += AddToExtractinatorPool;
		}

		private void AddToExtractinatorPool(int extractType, int extractinatorBlockType, ref int resultType, ref int resultStack)
		{
			if (extractType == 0 || extractType == ItemID.DesertFossil)
			{
				if (Main.rand.NextBool(20))
				{
					resultType = Type;
					resultStack = 1;
				}
			}
		}
	}
}