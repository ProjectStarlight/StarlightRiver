using StarlightRiver.Content.Items;
using Terraria.DataStructures;

namespace StarlightRiver.Core.PlayerLayers
{
	public class FrontArmMaskLayer : PlayerDrawLayer
	{
		private const int CHESTVANITYSLOT = 11;
		private const int CHESTARMORSLOT = 1;

		public override Position GetDefaultPosition()
		{
			return new AfterParent(PlayerDrawLayers.ArmOverItem);
		}

		protected override void Draw(ref PlayerDrawSet drawInfo)
		{
			if (drawInfo.drawPlayer.armor[CHESTVANITYSLOT].IsAir && drawInfo.drawPlayer.armor[CHESTARMORSLOT].ModItem is IArmorLayerDrawable)
				(drawInfo.drawPlayer.armor[CHESTARMORSLOT].ModItem as IArmorLayerDrawable).DrawArmorLayer(drawInfo, IArmorLayerDrawable.SubLayer.InFront);
			else if (drawInfo.drawPlayer.armor[CHESTVANITYSLOT].ModItem is IArmorLayerDrawable)
				(drawInfo.drawPlayer.armor[CHESTVANITYSLOT].ModItem as IArmorLayerDrawable).DrawArmorLayer(drawInfo, IArmorLayerDrawable.SubLayer.InFront);
		}
	}
}