using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Tiles.Vitric.Temple.GearPuzzle
{
    class ObjectiveGear : GearTile
    {
        public override bool NewRightClick(int i, int j)
        {
            return true;
        }
    }

    class ObjectiveGearDummy : GearTileDummy
    {
        public override void OnEngage()
        {

        }
    }
}
