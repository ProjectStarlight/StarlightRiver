using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ObjectData;

namespace StarlightRiver.Content.Tiles.Crimson
{
	internal class GestaltAltar : ModTile
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Crimson/GestaltAltar";

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 5, 5, DustID.Blood, SoundID.Tink, false, new Color(200, 200, 200));
			MinPick = 101;
		}

		public override bool RightClick(int i, int j)
		{
			return base.RightClick(i, j);
		}
	}
}
