using Terraria.DataStructures;

namespace StarlightRiver.Content.Items
{
	public interface IArmorLayerDrawable
	{
		public enum SubLayer
		{
			Base = 0,
			InFront = 1,
			Behind = 2
		}

		void DrawArmorLayer(PlayerDrawSet info, SubLayer subLayer = SubLayer.Base);
	}
}