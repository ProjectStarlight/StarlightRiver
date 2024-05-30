using StarlightRiver.Content.Configs;
using StarlightRiver.Core.Loaders.UILoading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace StarlightRiver.Content.GUI
{
	internal class ArmorChargeUI : SmartUIState
	{
		private static string message;

		public override bool Visible => ModContent.GetInstance<GUIConfig>().ObviousArmorCharge;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (!string.IsNullOrEmpty(message) && ModContent.GetInstance<GUIConfig>().ObviousArmorCharge)
				Utils.DrawBorderString(spriteBatch, message, Main.LocalPlayer.Center - Main.screenPosition + new Vector2(0, -48), Color.White, 0.8f, 0.5f, 0.5f);

			message = "";
		}

		public static void SetMessage(string message)
		{
			ArmorChargeUI.message = message;
		}
	}
}