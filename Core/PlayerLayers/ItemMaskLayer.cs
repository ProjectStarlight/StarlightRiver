using Terraria.DataStructures;

namespace StarlightRiver.Core.PlayerLayers
{
	public class ItemMaskLayer : PlayerDrawLayer
	{
		public override Position GetDefaultPosition()
		{
			return new AfterParent(PlayerDrawLayers.HeldItem);
		}

		public override void Draw(ref PlayerDrawSet drawInfo)
		{
			if (drawInfo.drawPlayer.HeldItem.ModItem is IGlowingItem)
				(drawInfo.drawPlayer.HeldItem.ModItem as IGlowingItem).DrawGlowmask(drawInfo);
		}
	}
}