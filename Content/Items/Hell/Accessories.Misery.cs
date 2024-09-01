using StarlightRiver.Content.Items.BaseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Items.Hell
{
	internal class Misery : CursedAccessory
	{
		public override string Texture => AssetDirectory.HellItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Misery's Company");
			Tooltip.SetDefault("Nearby enemies have defense and innoculation equal to yours");
		}
	}
}
