using System.Collections.Generic;
using Terraria.UI;

using StarlightRiver.Core;

namespace StarlightRiver.Core
{
    public abstract class SmartUIState : UIState
    {
        public abstract int InsertionIndex(List<GameInterfaceLayer> layers);

        public virtual bool Visible { get; set; } = false;
    }
}
