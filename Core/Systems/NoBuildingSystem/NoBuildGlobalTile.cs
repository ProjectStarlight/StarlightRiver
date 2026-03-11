using StarlightRiver.Content.Bosses.SquidBoss;
using StarlightRiver.Content.Tiles.Permafrost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Core.Systems.NoBuildingSystem;

class NoBuildGlobalTile : GlobalTile
{
	public override bool CanExplode(int i, int j, int type)
	{
		if (NoBuildSystem.IsTileProtected(i, j))
			return false;

		return base.CanExplode(i, j, type);
	}
}