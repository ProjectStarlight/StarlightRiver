using StarlightRiver.Items.Armor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace StarlightRiver.Core.PlayerLayers
{
    public class HelmetMaskLayer : PlayerDrawLayer
    {
        private const int HEADVANITYSLOT = 10;
        private const int HEADARMORSLOT = 0;

        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            if (drawInfo.drawPlayer.armor[HEADVANITYSLOT].IsAir && drawInfo.drawPlayer.armor[HEADARMORSLOT].ModItem is IArmorLayerDrawable) 
                (drawInfo.drawPlayer.armor[HEADARMORSLOT].ModItem as IArmorLayerDrawable).DrawArmorLayer(drawInfo);
            else if (drawInfo.drawPlayer.armor[HEADVANITYSLOT].ModItem is IArmorLayerDrawable) 
                (drawInfo.drawPlayer.armor[HEADVANITYSLOT].ModItem as IArmorLayerDrawable).DrawArmorLayer(drawInfo);
        }
    }
}
