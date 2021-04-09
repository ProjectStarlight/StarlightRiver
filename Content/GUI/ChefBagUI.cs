using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using StarlightRiver.Content.Items.Utility;
using Terraria.ModLoader.UI.Elements;

namespace StarlightRiver.Content.GUI
{
	class ChefBagUI : SmartUIState
	{
		public ChefBag openBag = null;

		public UIGrid grid = new UIGrid();

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return 1;
		}

	}
}
