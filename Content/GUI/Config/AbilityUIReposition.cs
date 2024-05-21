using StarlightRiver.Content.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.GUI.Config
{
	internal class AbilityUIReposition : BaseUIRepositionElement
	{
		public override ref Vector2 modifying => ref ModContent.GetInstance<GUIConfig>().AbilityIconPosition;

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			var dims = GetDimensions().ToRectangle();

			if (dims.Contains(Main.MouseScreen.ToPoint()) && Main.mouseLeft)
			{
				Main.playerInventory = true;
			}
		}
	}
}
