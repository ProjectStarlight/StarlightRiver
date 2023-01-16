using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace StarlightRiver.Core.PlayerLayers
{
    public class ItemMaskLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new Between(PlayerDrawLayers.HeldItem, PlayerDrawLayers.ArmOverItem);

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            if (drawInfo.drawPlayer.HeldItem.ModItem is IGlowingItem) 
                (drawInfo.drawPlayer.HeldItem.ModItem as IGlowingItem).DrawGlowmask(drawInfo);
        }
    }
}
