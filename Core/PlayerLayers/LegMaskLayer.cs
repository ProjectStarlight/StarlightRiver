using StarlightRiver.Content.Items;
using Terraria.DataStructures;

namespace StarlightRiver.Core.PlayerLayers
{
	public class LegMaskLayer : PlayerDrawLayer
	{
		private const int LEGVANITYSLOT = 12;
		private const int LEGARMORSLOT = 2;
		public override Position GetDefaultPosition()
		{
			return new AfterParent(PlayerDrawLayers.Leggings);
		}

		protected override void Draw(ref PlayerDrawSet drawInfo)
		{
			if (drawInfo.drawPlayer.armor[LEGVANITYSLOT].IsAir && drawInfo.drawPlayer.armor[LEGARMORSLOT].ModItem is IArmorLayerDrawable)
				(drawInfo.drawPlayer.armor[LEGARMORSLOT].ModItem as IArmorLayerDrawable).DrawArmorLayer(drawInfo);
			else if (drawInfo.drawPlayer.armor[LEGVANITYSLOT].ModItem is IArmorLayerDrawable)
				(drawInfo.drawPlayer.armor[LEGVANITYSLOT].ModItem as IArmorLayerDrawable).DrawArmorLayer(drawInfo);
		}
	}
}
