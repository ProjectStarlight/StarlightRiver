using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.Enums;

namespace StarlightRiver.Helpers
{
	public static class TileHelper
	{
		public static AnchorData AnchorTableTop(int width, bool floor = false, int start = 0)
		{
			return new AnchorData(AnchorType.SolidWithTop | (floor ? AnchorType.SolidTile | AnchorType.Table : AnchorType.Table), width, start);
		}

		public static AnchorData AnchorFloor(int width, int start = 0)
		{
			return new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, width, start);
		}
	}
}